using System.ComponentModel.DataAnnotations;
using Restoran.Application.Common;
using Restoran.Application.Entities;
using Restoran.Application.Interfaces;

namespace Restoran.Application.Services;

/// <summary>
/// Personali märge kalendri päevale, nt "Olulised külalised" või "Remont terrassil".
/// RestaurantId null = kehtib kõigile restoranidele.
/// </summary>
public class DayNote
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public int? RestaurantId { get; set; }
    public Restaurant? Restaurant { get; set; }
    public string Text { get; set; } = "";
    /// <summary>Märke värv kalendris: ox, gold, green, blue, grey.
    /// Erijuhud: "important" – päev on käsitsi märgitud oluliseks, "normal" – automaatne "oluline" eemaldatud.</summary>
    public string Color { get; set; } = "ox";
    public string AuthorName { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

public record DayNoteDto(int Id, DateOnly Date, int? RestaurantId, string Text, string Color, string AuthorName);

public class NewDayNoteDto
{
    /// <example>2026-12-05</example>
    public DateOnly Date { get; set; }
    /// <example>1</example>
    public int? RestaurantId { get; set; }
    /// <example>Olulised külalised</example>
    [Required, StringLength(80, MinimumLength = 2)]
    public string Text { get; set; } = "";
    /// <example>gold</example>
    public string Color { get; set; } = "ox";
}

public interface IDayNoteRepository : IRepository<DayNote>
{
    Task<List<DayNote>> GetRangeAsync(DateOnly from, DateOnly to, int? restaurantId);
}

public interface IDayNoteService
{
    Task<List<DayNoteDto>> GetRangeAsync(DateOnly from, int days, int? restaurantId);
    Task<DayNoteDto> AddAsync(NewDayNoteDto dto);
    Task DeleteAsync(int id);
}

public class DayNoteService : IDayNoteService
{
    public static readonly string[] Colors = ["ox", "gold", "green", "blue", "grey", "important", "normal"];

    private readonly IDayNoteRepository _notes;
    private readonly IUserRepository _users;
    private readonly ICurrentUser _current;

    public DayNoteService(IDayNoteRepository notes, IUserRepository users, ICurrentUser current)
    {
        _notes = notes;
        _users = users;
        _current = current;
    }

    public async Task<List<DayNoteDto>> GetRangeAsync(DateOnly from, int days, int? restaurantId)
    {
        if (days is < 1 or > 62) throw new BusinessRuleException("Päevade arv peab olema 1 kuni 62.");
        var list = await _notes.GetRangeAsync(from, from.AddDays(days - 1), restaurantId);
        return list.Select(ToDto).ToList();
    }

    public async Task<DayNoteDto> AddAsync(NewDayNoteDto dto)
    {
        var text = dto.Text.Trim();
        if (text.Length < 2) throw new BusinessRuleException("Märge on liiga lühike.");
        if (!Colors.Contains(dto.Color)) throw new BusinessRuleException("Tundmatu värv.");
        var user = _current.UserId is int id ? await _users.GetByIdAsync(id) : null;
        var note = new DayNote
        {
            Date = dto.Date,
            RestaurantId = dto.RestaurantId,
            Text = text,
            Color = dto.Color,
            AuthorName = user?.Name ?? "",
        };
        await _notes.AddAsync(note);
        await _notes.SaveChangesAsync();
        return ToDto(note);
    }

    public async Task DeleteAsync(int id)
    {
        var note = await _notes.GetByIdAsync(id) ?? throw new NotFoundException($"Märget {id} ei leitud.");
        _notes.Remove(note);
        await _notes.SaveChangesAsync();
    }

    private static DayNoteDto ToDto(DayNote n) => new(n.Id, n.Date, n.RestaurantId, n.Text, n.Color, n.AuthorName);
}
