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
    int X, int Y, int Width, int Height);

/// <summary>Laua loomine või muutmine.</summary>
public class SaveTableDto
{
    /// <example>1</example>
    [Range(1, int.MaxValue)]
    public int RestaurantId { get; set; }

    /// <summary>Laua number restoranis.</summary>
    /// <example>11</example>
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

    /// <summary>Asukoht plaanil protsentides.</summary>
    [Range(0, 100)] public int X { get; set; }
    [Range(0, 100)] public int Y { get; set; }
    [Range(1, 100)] public int Width { get; set; } = 12;
    [Range(1, 100)] public int Height { get; set; } = 12;
}
