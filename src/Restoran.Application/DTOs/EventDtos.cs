using System.ComponentModel.DataAnnotations;

namespace Restoran.Application.DTOs;

/// <summary>Gastronoomiline õhtu koos vabade kohtade arvuga.</summary>
public record GastroEventDto(
    int Id,
    int RestaurantId,
    string RestaurantName,
    string Title,
    string Kind,
    string Description,
    DateTime StartTime,
    decimal PricePerGuest,
    string DressCode,
    string? Rule,
    string? ImageUrl,
    int Capacity,
    int SeatsLeft,
    // Kas praegune kasutaja on registreerunud ja mitme külalisega
    int MyGuests);

/// <summary>Registreerimine õhtule.</summary>
public class JoinEventDto
{
    /// <example>2</example>
    [Range(1, 8)]
    public int Guests { get; set; } = 1;

    /// <summary>Kas külaline nõustub riietuse reegliga (kohustuslik, kui õhtul on reegel).</summary>
    public bool AcceptRules { get; set; }
}
