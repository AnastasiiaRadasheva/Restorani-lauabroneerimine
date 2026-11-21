using System.ComponentModel.DataAnnotations;
using Restoran.Application.Common;
using Restoran.Application.Entities;
using Restoran.Application.Interfaces;

namespace Restoran.Application.Services;

/// <summary>Klienditoe vestluse sõnum (klient ↔ administraator), ei ole seotud broneeringuga.</summary>
public class SupportMessage
{
    public int Id { get; set; }
    /// <summary>Klient, kelle vestlusega on tegu.</summary>
    public int UserId { get; set; }
    public User? User { get; set; }
    public bool FromStaff { get; set; }
    public string AuthorName { get; set; } = "";
    public string Text { get; set; } = "";
    public bool ReadByStaff { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

public record SupportMessageDto(int Id, bool FromStaff, string AuthorName, string Text, DateTime CreatedAt);

/// <summary>Vestluse kokkuvõte administraatorile.</summary>
public record SupportThreadDto(int UserId, string Name, string Email, string? Phone, string LastText, DateTime LastAt, int Unread);

public class NewSupportMessageDto
{
    /// <example>Kas saan tulla koeraga?</example>
    [Required, StringLength(1000, MinimumLength = 1)]
    public string Text { get; set; } = "";

    /// <summary>Kliendi nimi, kuidas tema poole pöörduda (personali vastuses ei kasutata).</summary>
    /// <example>Kati</example>
    [StringLength(80)]
    public string? Name { get; set; }
}

public interface ISupportRepository : IRepository<SupportMessage>
{
    Task<List<SupportMessage>> GetThreadAsync(int userId);
    Task<List<SupportMessage>> GetAllWithUsersAsync();
}

public interface ISupportService
{
    Task<List<SupportMessageDto>> GetMineAsync();
    Task<SupportMessageDto> SendMineAsync(NewSupportMessageDto dto);
    Task<List<SupportThreadDto>> GetThreadsAsync();
    Task<List<SupportMessageDto>> GetThreadAsync(int userId);
    Task<SupportMessageDto> ReplyAsync(int userId, NewSupportMessageDto dto);
}

public class SupportService : ISupportService
{
    private readonly ISupportRepository _messages;
    private readonly IUserRepository _users;
    private readonly INotificationService _notifications;
    private readonly ICurrentUser _current;
    private readonly IClock _clock;

    public SupportService(ISupportRepository messages, IUserRepository users, INotificationService notifications, ICurrentUser current, IClock clock)
    {
        _messages = messages;
        _users = users;
        _notifications = notifications;
        _current = current;
        _clock = clock;
    }

    public async Task<List<SupportMessageDto>> GetMineAsync() =>
        (await _messages.GetThreadAsync(RequireUser())).Select(ToDto).ToList();

    public async Task<SupportMessageDto> SendMineAsync(NewSupportMessageDto dto)
    {
        var userId = RequireUser();
        var user = await _users.GetByIdAsync(userId) ?? throw new NotFoundException("Kasutajat ei leitud.");
        // Klient peab kirjutama oma nime, et personal teaks, kuidas tema poole pöörduda
        var name = (dto.Name ?? "").Trim();
        if (name.Length < 2)
            throw new BusinessRuleException("Palun kirjutage oma nimi.");
        return await AddAsync(userId, false, name, dto.Text);
    }

    public async Task<List<SupportThreadDto>> GetThreadsAsync()
    {
        var all = await _messages.GetAllWithUsersAsync();
        return all.GroupBy(m => m.UserId)
            .Select(g =>
            {
                var last = g.OrderBy(m => m.CreatedAt).Last();
                var u = g.First().User;
                // Nimi, mille klient ise viimati kirjutas
                var name = g.Where(m => !m.FromStaff).OrderBy(m => m.CreatedAt).LastOrDefault()?.AuthorName ?? u?.Name ?? "";
                return new SupportThreadDto(g.Key, name, u?.Email ?? "", u?.Phone, last.Text, last.CreatedAt,
                    g.Count(m => !m.FromStaff && !m.ReadByStaff));
            })
            .OrderByDescending(t => t.LastAt)
            .ToList();
    }

    public async Task<List<SupportMessageDto>> GetThreadAsync(int userId)
    {
        var list = await _messages.GetThreadAsync(userId);
        foreach (var m in list.Where(m => !m.FromStaff)) m.ReadByStaff = true;
        await _messages.SaveChangesAsync();
        return list.Select(ToDto).ToList();
    }

    /// <summary>Administraator vastab: klient saab ka teate (kelluke).</summary>
    public async Task<SupportMessageDto> ReplyAsync(int userId, NewSupportMessageDto dto)
    {
        _ = await _users.GetByIdAsync(userId) ?? throw new NotFoundException($"Kasutajat {userId} ei leitud.");
        var result = await AddAsync(userId, true, "Argent", dto.Text);
        var receiver = await _users.GetByIdAsync(userId);
        await _notifications.CreateAsync(new DTOs.CreateNotificationDto { UserId = userId, Link = "/tugi", Message = L.T(receiver?.Language, "support.reply", Short(dto.Text)) });
        return result;
    }

    private async Task<SupportMessageDto> AddAsync(int userId, bool fromStaff, string author, string text)
    {
        text = text.Trim();
        if (text.Length == 0) throw new BusinessRuleException("Sõnum on tühi.");
        var m = new SupportMessage { UserId = userId, FromStaff = fromStaff, AuthorName = author, Text = text, CreatedAt = _clock.Now, ReadByStaff = fromStaff };
        await _messages.AddAsync(m);
        await _messages.SaveChangesAsync();
        return ToDto(m);
    }

    private static string Short(string s) => s.Length > 80 ? s[..80] + "…" : s;
    private int RequireUser() => _current.UserId ?? throw new UnauthorizedException("Pole sisse logitud.");
    private static SupportMessageDto ToDto(SupportMessage m) => new(m.Id, m.FromStaff, m.AuthorName, m.Text, m.CreatedAt);
}
