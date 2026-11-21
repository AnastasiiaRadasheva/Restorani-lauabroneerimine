using System.ComponentModel.DataAnnotations;
using Restoran.Application.Common;

namespace Restoran.Application.DTOs;

public record MenuItemDto(
    int Id,
    int RestaurantId,
    string Name,
    string Description,
    string Category,
    decimal Price,
    bool IsAvailable,
    string? ImageUrl);

/// <summary>Menüüpunkti loomine või muutmine.</summary>
public class SaveMenuItemDto
{
    /// <example>1</example>
    [Range(1, int.MaxValue)]
    public int RestaurantId { get; set; }

    /// <example>Seenesupp</example>
    [Required, StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = "";

    /// <example>Metsaseentest koorene supp</example>
    [StringLength(500)]
    public string Description { get; set; } = "";

    /// <example>Supid</example>
    [Required, StringLength(50)]
    public string Category { get; set; } = "";

    /// <example>6.50</example>
    [Range(0.01, 10000)]
    public decimal Price { get; set; }

    public bool IsAvailable { get; set; } = true;

    /// <summary>Foto aadress (valikuline).</summary>
    /// <example>/img/menu/austrid.jpg</example>
    [StringLength(300)]
    public string? ImageUrl { get; set; }
}

/// <summary>Menüü otsing, filter, sortimine (name, price, category) ja leheküljed.</summary>
public class MenuQuery : PageQuery
{
    /// <summary>Restorani id.</summary>
    public int? RestaurantId { get; set; }

    /// <summary>Kategooria (nt Supid, Pearoad).</summary>
    public string? Category { get; set; }

    /// <summary>Kas näidata ainult saadaval olevaid.</summary>
    public bool? OnlyAvailable { get; set; }
}
