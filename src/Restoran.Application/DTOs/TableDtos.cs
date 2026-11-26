using System.ComponentModel.DataAnnotations;
using Restoran.Application.Entities;

namespace Restoran.Application.DTOs;

public record TableDto(
    int Id,
    int RestaurantId,
    int Number,
    int Seats,
    TableLocation Location,
    bool IsActive,
    int X, int Y, int Width, int Height,
    string? Spot,
    bool IsRound);

/// <summary>Laua loomine või muutmine.</summary>
public class SaveTableDto
{
    /// <example>1</example>
    [Range(1, int.MaxValue)]
    public int RestaurantId { get; set; }

    /// <summary>Laua number restoranis.</summary>
    /// <example>99</example>
    [Range(1, 999)]
    public int Number { get; set; }

    /// <summary>Kohtade arv.</summary>
    /// <example>4</example>
    [Range(1, 20)]
    public int Seats { get; set; }

    /// <summary>Hall (saal) või Terrace (terrass).</summary>
    /// <example>Hall</example>
    [EnumDataType(typeof(TableLocation))]
    public TableLocation Location { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>Asukoht plaanil protsentides (vasakult).</summary>
    /// <example>48</example>
    [Range(0, 100)] public int X { get; set; }

    /// <summary>Asukoht plaanil protsentides (ülevalt).</summary>
    /// <example>4</example>
    [Range(0, 100)] public int Y { get; set; }

    /// <summary>Laius plaanil protsentides.</summary>
    /// <example>8</example>
    [Range(1, 100)] public int Width { get; set; } = 12;

    /// <summary>Kõrgus plaanil protsentides.</summary>
    /// <example>8</example>
    [Range(1, 100)] public int Height { get; set; } = 12;

    /// <summary>Kirjeldus plaanil.</summary>
    /// <example>akna all</example>
    [StringLength(60)]
    public string? Spot { get; set; }

    /// <summary>Kas laud on ümmargune.</summary>
    public bool IsRound { get; set; }
}
