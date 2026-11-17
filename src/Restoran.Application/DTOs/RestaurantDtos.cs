using System.ComponentModel.DataAnnotations;

namespace Restoran.Application.DTOs;

/// <summary>Restoran koos kaugusega kasutajast (kui asukoht on teada).</summary>
public record RestaurantDto(
    int Id,
    string Name,
    string Address,
    string Description,
    double Latitude,
    double Longitude,
    string OpensAt,
    string ClosesAt,
    int BarX, int BarY, int BarWidth, int BarHeight,
    int TableCount,
    double? DistanceMeters);

/// <summary>Restorani loomine või muutmine.</summary>
public class SaveRestaurantDto
{
    /// <example>Argent</example>
    [Required, StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = "";

    /// <example>Rataskaevu 8, Tallinn</example>
    [Required, StringLength(200)]
    public string Address { get; set; } = "";

    [StringLength(1000)]
    public string Description { get; set; } = "";

    /// <example>59.437</example>
    [Range(-90, 90)]
    public double Latitude { get; set; }

    /// <example>24.7428</example>
    [Range(-180, 180)]
    public double Longitude { get; set; }

    /// <summary>Avamisaeg kujul HH:mm.</summary>
    /// <example>12:00</example>
    [Required, RegularExpression(@"^\d{2}:\d{2}$")]
    public string OpensAt { get; set; } = "12:00";

    /// <summary>Sulgemisaeg kujul HH:mm.</summary>
    /// <example>23:00</example>
    [Required, RegularExpression(@"^\d{2}:\d{2}$")]
    public string ClosesAt { get; set; } = "23:00";

    [Range(0, 100)] public int BarX { get; set; }
    [Range(0, 100)] public int BarY { get; set; }
    [Range(0, 100)] public int BarWidth { get; set; }
    [Range(0, 100)] public int BarHeight { get; set; }
}
