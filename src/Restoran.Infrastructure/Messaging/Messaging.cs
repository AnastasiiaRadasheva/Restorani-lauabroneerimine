using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Restoran.Application.DTOs;
using Restoran.Application.Interfaces;
using Restoran.Application.Services;

namespace Restoran.Infrastructure.Messaging;

/// <summary>
/// SMS Twilio kaudu. Seadistus appsettings.json: "Sms": { "AccountSid", "AuthToken", "From" }.
/// Kui võtmeid pole, on demorežiim: SMS kirjutatakse logisse ja kausta outbox (päris SMS-i ei saadeta).
/// </summary>
public class SmsSender : ISmsSender
{
    private static readonly HttpClient _http = new();
    private readonly IConfiguration _config;
    private readonly ILogger<SmsSender> _logger;
    private readonly string _outbox = Path.Combine(Directory.GetCurrentDirectory(), "outbox");

    public SmsSender(IConfiguration config, ILogger<SmsSender> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendAsync(string phone, string text)
    {
        var to = Normalize(phone);
        var sid = _config["Sms:AccountSid"];
        var token = _config["Sms:AuthToken"];
        var from = _config["Sms:From"];
        if (string.IsNullOrWhiteSpace(sid) || string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(from))
        {
            Directory.CreateDirectory(_outbox);
            await File.WriteAllTextAsync(Path.Combine(_outbox, $"sms-{DateTime.Now:yyyyMMdd-HHmmss}-{to.TrimStart('+')}.txt"), $"Kellele: {to}\n\n{text}\n");
            _logger.LogInformation("SMS (demo) → {Phone}: {Text}", to, text);
            return;
        }

        var request = new HttpRequestMessage(HttpMethod.Post, $"https://api.twilio.com/2010-04-01/Accounts/{sid}/Messages.json")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string> { ["To"] = to, ["From"] = from, ["Body"] = text })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{sid}:{token}")));
        var response = await _http.SendAsync(request);
        if (response.IsSuccessStatusCode) _logger.LogInformation("SMS saadetud → {Phone}", to);
        else _logger.LogWarning("SMS-i saatmine ebaõnnestus ({Status}): {Body}", response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    /// <summary>Eesti number ilma suunakoodita (nt 58951836) → +37258951836.</summary>
    public static string Normalize(string phone)
    {
        var digits = new string(phone.Where(c => char.IsDigit(c) || c == '+').ToArray());
        if (digits.StartsWith('+')) return digits;
        if (digits.StartsWith("372")) return "+" + digits;
        return "+372" + digits;
    }
}

/// <summary>
/// E-kiri. Seadistus "Email": { "Host", "Port", "User", "Password", "From" } (nt Gmail + rakenduse parool).
/// Kui seadistust pole, salvestatakse kiri HTML-failina kausta outbox – selle saab brauseris avada.
/// </summary>
public class EmailSender : IEmailSender
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailSender> _logger;
    private readonly string _outbox = Path.Combine(Directory.GetCurrentDirectory(), "outbox");

    public EmailSender(IConfiguration config, ILogger<EmailSender> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendAsync(string to, string subject, string html)
    {
        var host = _config["Email:Host"];
        if (string.IsNullOrWhiteSpace(host))
        {
            Directory.CreateDirectory(_outbox);
            var file = Path.Combine(_outbox, $"email-{DateTime.Now:yyyyMMdd-HHmmss}-{to.Replace('@', '_')}.html");
            await File.WriteAllTextAsync(file, html);
            _logger.LogInformation("E-kiri (demo) → {To}: {Subject}. Fail: {File}", to, subject, file);
            return;
        }

        using var client = new SmtpClient(host, int.TryParse(_config["Email:Port"], out var p) ? p : 587)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(_config["Email:User"], _config["Email:Password"])
        };
        using var message = new MailMessage(_config["Email:From"] ?? _config["Email:User"]!, to, subject, html) { IsBodyHtml = true };
        await client.SendMailAsync(message);
        _logger.LogInformation("E-kiri saadetud → {To}: {Subject}", to, subject);
    }
}

/// <summary>Broneeringu kinnitus: SMS (kui klient lubas) + ilus e-kiri tšekiga + teade saidil.</summary>
public class BookingMessenger : IBookingMessenger
{
    private static readonly CultureInfo Et = new("et-EE");
    private readonly ISmsSender _sms;
    private readonly IEmailSender _email;
    private readonly IUserRepository _users;
    private readonly INotificationService _notifications;
    private readonly ILogger<BookingMessenger> _logger;
    private readonly IConfiguration _config;

    public BookingMessenger(ISmsSender sms, IEmailSender email, IUserRepository users, INotificationService notifications,
        ILogger<BookingMessenger> logger, IConfiguration config)
    {
        _config = config;
        _sms = sms;
        _email = email;
        _users = users;
        _notifications = notifications;
        _logger = logger;
    }

    public async Task SendConfirmationAsync(ReservationDto r)
    {
        try
        {
            var user = await _users.GetByIdAsync(r.UserId);
            if (user is null) return;
            var lang = user.Language;
            var text = L.T(lang, "booking.confirmed", r.RestaurantName, r.RestaurantAddress, L.When(r.StartTime, lang), r.TableNumber, r.Guests, r.CustomerName);

            await _notifications.CreateAsync(new CreateNotificationDto { UserId = user.Id, ReservationId = r.Id, Message = text });
            if (user.SmsOptIn && !string.IsNullOrWhiteSpace(user.Phone))
                await _sms.SendAsync(user.Phone, text + PasswordText(user));
            await _email.SendAsync(user.Email, L.T(lang, "booking.subject", r.StartTime.ToString("dd.MM HH:mm")),
                WithExtra(ReceiptHtml(r, lang, null), PasswordHtml(user)));
        }
        catch (Exception ex)
        {
            // Kinnituse saatmise viga ei tohi broneeringut tühistada
            _logger.LogWarning(ex, "Broneeringu {Id} kinnituse saatmine ebaõnnestus", r.Id);
        }
    }

    public async Task SendPaymentRequestAsync(ReservationDto r)
    {
        try
        {
            var user = await _users.GetByIdAsync(r.UserId);
            if (user is null || r.PaymentToken is null) return;
            var lang = user.Language;
            var link = $"{BaseUrl}/maksa/{r.PaymentToken}";
            var text = L.T(lang, "booking.payRequest", r.RestaurantName, L.When(r.StartTime, lang), r.Guests, link);
            await _notifications.CreateAsync(new CreateNotificationDto { UserId = user.Id, ReservationId = r.Id, Message = text });
            if (!string.IsNullOrWhiteSpace(user.Phone)) await _sms.SendAsync(user.Phone, text + PasswordText(user));
            await _email.SendAsync(user.Email, L.T(lang, "booking.paySubject"), WithExtra(ReceiptHtml(r, lang, link), PasswordHtml(user)));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Makselingi saatmine ebaõnnestus (broneering {Id})", r.Id);
        }
    }

    public async Task SendEventConfirmationAsync(EventRegistrationDto r)
    {
        try
        {
            var user = await _users.GetByIdAsync(r.UserId);
            if (user is null) return;
            var lang = user.Language;
            var table = r.TableNumber is null ? "" : L.T(lang, "event.tableSuffix", r.TableNumber);
            var text = L.T(lang, "event.confirmed", r.EventTitle, r.RestaurantName, L.When(r.StartTime, lang), table, r.Guests, r.Total.ToString("0.##"));
            await _notifications.CreateAsync(new CreateNotificationDto { UserId = user.Id, Link = "/minu", Message = text });
            if (user.SmsOptIn && !string.IsNullOrWhiteSpace(user.Phone)) await _sms.SendAsync(user.Phone, text + PasswordText(user));
            await _email.SendAsync(user.Email, $"Argent – {r.EventTitle} {r.StartTime:dd.MM}", WithExtra(EventHtml(r, lang, null), PasswordHtml(user)));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Õhtu kinnituse saatmine ebaõnnestus ({Id})", r.Id);
        }
    }

    public async Task SendEventPaymentRequestAsync(EventRegistrationDto r)
    {
        try
        {
            var user = await _users.GetByIdAsync(r.UserId);
            if (user is null || r.PaymentToken is null) return;
            var lang = user.Language;
            var link = $"{BaseUrl}/maksa/ohtu/{r.PaymentToken}";
            var text = L.T(lang, "event.payRequest", r.EventTitle, L.When(r.StartTime, lang), r.Guests, r.Total.ToString("0.##"), link);
            await _notifications.CreateAsync(new CreateNotificationDto { UserId = user.Id, Link = "/minu", Message = text });
            if (!string.IsNullOrWhiteSpace(user.Phone)) await _sms.SendAsync(user.Phone, text + PasswordText(user));
            await _email.SendAsync(user.Email, L.T(lang, "event.paySubject"), WithExtra(EventHtml(r, lang, link), PasswordHtml(user)));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Õhtu makselingi saatmine ebaõnnestus ({Id})", r.Id);
        }
    }

    private string BaseUrl => (_config["Web:BaseUrl"] ?? "http://localhost:5173").TrimEnd('/');

    /// <summary>SMS-i lisa: link parooli loomiseks (ainult personali loodud kliendil).</summary>
    private string PasswordText(Restoran.Application.Entities.User user) =>
        user.HasValidPasswordToken(DateTime.Now) ? L.T(user.Language, "account.passwordText", $"{BaseUrl}/parool/{user.PasswordToken}") : "";

    /// <summary>E-kirja lisa: nupp parooli loomiseks.</summary>
    private string PasswordHtml(Restoran.Application.Entities.User user) =>
        user.HasValidPasswordToken(DateTime.Now)
            ? $"<p style=\"text-align:center;font:14px Georgia,serif;margin-top:24px\">{E(L.T(user.Language, "account.passwordHtml"))}</p><p style=\"text-align:center\"><a href=\"{BaseUrl}/parool/{user.PasswordToken}\" style=\"display:inline-block;padding:12px 26px;border:1px solid #0c0d0e;color:#0c0d0e;font:600 12px Georgia,serif;letter-spacing:3px;text-decoration:none\">{L.T(user.Language, "account.passwordButton")}</a></p>"
            : "";

    private static string WithExtra(string html, string extra) =>
        extra.Length == 0 ? html : html.Replace("</body>", extra + "</body>");

    private static string E(string s) => WebUtility.HtmlEncode(s);

    public async Task SendPasswordLinkAsync(int userId)
    {
        try
        {
            var user = await _users.GetByIdAsync(userId);
            if (user is null || !user.HasValidPasswordToken(DateTime.Now)) return;
            var lang = user.Language;
            var text = L.T(lang, "account.created") + PasswordText(user);
            if (!string.IsNullOrWhiteSpace(user.Phone)) await _sms.SendAsync(user.Phone, text);
            var html = "<html><body style=\"background:#ecedef;padding:30px\"><table width=\"560\" align=\"center\" style=\"background:#fff;padding:30px;font:15px Georgia,serif;color:#0c0d0e\"><tr><td style=\"text-align:center;letter-spacing:8px;font-size:26px\">ARGENT</td></tr>"
                       + $"<tr><td style=\"text-align:center;padding-top:16px\">{E(L.T(lang, "hello", user.Name))}</td></tr></table></body></html>";
            await _email.SendAsync(user.Email, L.T(lang, "account.passwordSubject"), WithExtra(html, PasswordHtml(user)));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Parooli lingi saatmine ebaõnnestus (kasutaja {Id})", userId);
        }
    }

    public async Task SendStaffMessageAsync(int userId, string restaurantName, DateTime startTime, string text)
    {
        try
        {
            var user = await _users.GetByIdAsync(userId);
            if (user is null) return;
            var lang = user.Language;
            var when = L.When(startTime, lang);
            if (!string.IsNullOrWhiteSpace(user.Phone)) await _sms.SendAsync(user.Phone, L.T(lang, "broadcast.sms", restaurantName, when, text));
            var html = "<html><body style=\"background:#ecedef;padding:30px\"><table width=\"560\" align=\"center\" style=\"background:#fff;padding:30px;font:15px Georgia,serif;color:#0c0d0e\">"
                       + "<tr><td style=\"text-align:center;letter-spacing:8px;font-size:26px\">ARGENT</td></tr>"
                       + $"<tr><td style=\"padding-top:16px\">{E(L.T(lang, "hello", user.Name))}</td></tr>"
                       + $"<tr><td style=\"padding-top:6px;color:#5d6266\">{E(restaurantName)} · {E(when)}</td></tr>"
                       + $"<tr><td style=\"padding:18px 0;font-size:17px;white-space:pre-line\">{E(text)}</td></tr>"
                       + $"<tr><td style=\"font:13px Georgia,serif;color:#5d6266;text-align:center\">{E(L.T(lang, "mail.questions"))}: +372 600 0000 · argent@restoran.ee</td></tr></table></body></html>";
            await _email.SendAsync(user.Email, L.T(lang, "broadcast.subject", startTime.ToString("dd.MM")), html);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Personali sõnumi saatmine ebaõnnestus (kasutaja {Id})", userId);
        }
    }

    /// <summary>Õhtu tšekk või makselink e-kirjas (kliendi keeles).</summary>
    private static string EventHtml(EventRegistrationDto r, string lang, string? link)
    {
        var rows = $"<tr><td>{E(L.T(lang, "mail.eventFee", r.Guests))}</td><td align=\"right\">{r.PricePerGuest * r.Guests:0.##} €</td></tr>"
                   + string.Concat(r.Lines.Select(l => $"<tr><td>{l.Quantity} × {E(L.Word(l.Name, lang))}</td><td align=\"right\">{l.Price * l.Quantity:0.##} €</td></tr>"));
        var button = link is null
            ? $"<p style=\"font:14px Georgia,serif\">{E(L.T(lang, "mail.eventSee"))}</p>"
            : $"<a href=\"{link}\" style=\"display:inline-block;padding:14px 28px;background:#5a0f1c;color:#fff;font:600 13px Georgia,serif;letter-spacing:3px;text-decoration:none\">{L.T(lang, "mail.payButton")}</a><p style=\"font:13px Georgia,serif;color:#5a0f1c\">{E(L.T(lang, "mail.eventPayNote"))}</p>";
        var tableText = r.TableNumber is null ? "" : $" · {E(L.T(lang, "mail.table"))} {r.TableNumber}";
        return "<html><body style=\"background:#ecedef;padding:30px\"><table width=\"560\" align=\"center\" style=\"background:#fff;padding:30px;font:15px Georgia,serif;color:#0c0d0e\">"
               + "<tr><td style=\"text-align:center;letter-spacing:8px;font-size:26px\">ARGENT</td></tr>"
               + $"<tr><td style=\"text-align:center;padding:10px 0 20px\"><b style=\"font-size:22px\">{E(r.EventTitle)}</b><br>{E(r.RestaurantName)}, {E(r.RestaurantAddress)}<br>{E(L.When(r.StartTime, lang))}{tableText}</td></tr>"
               + $"<tr><td><table width=\"100%\">{rows}<tr><td style=\"border-top:1px solid #ccc;padding-top:8px\"><b>{L.T(lang, "mail.total")}</b></td><td align=\"right\" style=\"border-top:1px solid #ccc;padding-top:8px\"><b>{r.Total:0.##} €</b></td></tr></table></td></tr>"
               + $"<tr><td style=\"padding-top:20px;text-align:center\">{button}</td></tr></table></body></html>";
    }

    /// <summary>E-kirja tšekk Argenti stiilis (tabelid ja inline stiilid, et töötaks e-posti programmides), kliendi keeles.
    /// Kui antud on makselink, lisatakse nupp "Kinnita ja maksa".</summary>
    private static string ReceiptHtml(ReservationDto r, string lang, string? payLink)
    {
        string Row(string k, string v) =>
            $"<tr><td style=\"padding:6px 0;font:600 11px Georgia,serif;letter-spacing:3px;text-transform:uppercase;color:#5d6266\">{E(k)}</td>" +
            $"<td style=\"padding:6px 0;text-align:right;font:16px Georgia,serif;color:#0c0d0e\">{E(v)}</td></tr>";
        var lines = new StringBuilder();
        foreach (var p in r.PreOrder) lines.Append(Row($"{p.Quantity}×", $"{L.Word(p.Name, lang)} · {p.Total:0.##} €"));
        foreach (var x in r.Extras) lines.Append(Row(L.T(lang, "mail.extra"), $"{L.Word(x.Name, lang)} · {x.Price:0.##} €"));
        if (r.SeatFee > 0) lines.Append(Row(L.T(lang, "mail.seat"), $"{r.SeatFee:0.##} €"));
        var greeting = payLink is null ? L.T(lang, "mail.waiting", r.CustomerName) : L.T(lang, "mail.bookedFor", r.CustomerName);
        var pay = payLink is null ? "" :
            $"<tr><td style=\"padding-top:20px;text-align:center\"><a href=\"{payLink}\" style=\"display:inline-block;padding:14px 28px;background:#5a0f1c;color:#fff;font:600 13px Georgia,serif;letter-spacing:3px;text-decoration:none\">{L.T(lang, "mail.payButton")}</a><p style=\"font:13px Georgia,serif;color:#5a0f1c\">{E(L.T(lang, "mail.payNote"))}</p></td></tr>";

        return $@"<!doctype html><html><body style=""margin:0;background:#eceef0;padding:32px 12px"">
<table role=""presentation"" width=""100%""><tr><td align=""center"">
<table role=""presentation"" width=""480"" style=""background:#ffffff;padding:36px;border-top:4px solid #0c0d0e"">
<tr><td style=""text-align:center;font:34px Georgia,serif;letter-spacing:12px;color:#0c0d0e"">ARGENT</td></tr>
<tr><td style=""text-align:center;font:italic 15px Georgia,serif;color:#5d6266;padding-bottom:18px"">{E(L.T(lang, "mail.tagline"))}</td></tr>
<tr><td style=""font:22px Georgia,serif;color:#0c0d0e;padding:12px 0"">{E(greeting)}</td></tr>
<tr><td><table width=""100%"" style=""border-top:1px dashed #a9aeb2;border-bottom:1px dashed #a9aeb2;padding:10px 0"">
{Row(L.T(lang, "mail.where"), $"{r.RestaurantName}, {r.RestaurantAddress}")}
{Row(L.T(lang, "mail.when"), L.When(r.StartTime, lang))}
{Row(L.T(lang, "mail.table"), r.TableNumber.ToString())}
{Row(L.T(lang, "mail.guests"), r.Guests.ToString())}
{Row(L.T(lang, "mail.for"), r.CustomerName)}
{lines}
</table></td></tr>
<tr><td style=""padding-top:16px;font:600 13px Georgia,serif;letter-spacing:3px;color:#0c0d0e"">{L.T(lang, "mail.total")} <span style=""float:right;font:400 30px Georgia,serif"">{r.Total:0.##} €</span></td></tr>
{pay}
<tr><td style=""padding-top:24px;font:13px Georgia,serif;color:#5d6266;text-align:center"">{E(L.T(lang, "mail.footer"))}<br>{E(L.T(lang, "mail.questions"))}: +372 600 0000 (10:00–20:00) · argent@restoran.ee</td></tr>
</table></td></tr></table></body></html>";
    }
}
