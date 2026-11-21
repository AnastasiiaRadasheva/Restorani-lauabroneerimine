using Restoran.Application.DTOs;

namespace Restoran.Application.Interfaces;

/// <summary>SMS-i saatmine (Twilio, kui võtmed on seadistatud; muidu demo – logi ja fail).</summary>
public interface ISmsSender
{
    Task SendAsync(string phone, string text);
}

/// <summary>E-kirja saatmine (SMTP, kui seadistatud; muidu salvestatakse HTML kausta "outbox").</summary>
public interface IEmailSender
{
    Task SendAsync(string to, string subject, string html);
}

/// <summary>Broneeringu kinnitus kliendile: SMS (kui klient lubas) ja e-kiri tšekiga.</summary>
public interface IBookingMessenger
{
    Task SendConfirmationAsync(ReservationDto reservation);

    /// <summary>Personali tehtud broneering: kliendile makselink (SMS ja e-kiri).</summary>
    Task SendPaymentRequestAsync(ReservationDto reservation);

    /// <summary>Gastronoomilise õhtu kinnitus (makstud).</summary>
    Task SendEventConfirmationAsync(EventRegistrationDto registration);

    /// <summary>Personali tehtud õhtu registreerimine: makselink.</summary>
    Task SendEventPaymentRequestAsync(EventRegistrationDto registration);

    /// <summary>Personali loodud kliendile link parooli loomiseks (e-kiri ja SMS).</summary>
    Task SendPasswordLinkAsync(int userId);

    /// <summary>Personali sõnum kliendile SMS-i ja e-kirjaga (nt restoran on suletud).</summary>
    Task SendStaffMessageAsync(int userId, string restaurantName, DateTime startTime, string text);
}
