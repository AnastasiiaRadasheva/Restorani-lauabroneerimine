namespace Restoran.Application.Entities;

public class DiningTable
{
    public int Id { get; set; }
    public int RestaurantId { get; set; }
    public Restaurant? Restaurant { get; set; }

    public int Number { get; set; }
    public int Seats { get; set; }
    public TableLocation Location { get; set; }
    public bool IsActive { get; set; } = true;

    // Asukoht plaanil protsentides (0-100), et plaan skaleeruks igale ekraanile
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; } = 12;
    public int Height { get; set; } = 12;

    public List<Reservation> Reservations { get; set; } = new();
}
