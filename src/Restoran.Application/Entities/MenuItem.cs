namespace Restoran.Application.Entities;

public class MenuItem
{
    public int Id { get; set; }
    public int RestaurantId { get; set; }
    public Restaurant? Restaurant { get; set; }

    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Category { get; set; } = "";
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; } = true;
    /// <summary>Foto aadress, nt /img/menu/austrid.jpg.</summary>
    public string? ImageUrl { get; set; }
}
