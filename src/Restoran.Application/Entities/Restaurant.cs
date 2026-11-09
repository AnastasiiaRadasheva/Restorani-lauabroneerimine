namespace Restoran.Application.Entities;

public class Restaurant
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Address { get; set; } = "";
    public string Description { get; set; } = "";
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public TimeOnly OpensAt { get; set; } = new(12, 0);
    public TimeOnly ClosesAt { get; set; } = new(23, 0);

    // Baari asukoht saali plaanil (protsentides 0-100)
    public int BarX { get; set; }
    public int BarY { get; set; }
    public int BarWidth { get; set; }
    public int BarHeight { get; set; }

    public List<DiningTable> Tables { get; set; } = new();
    public List<MenuItem> MenuItems { get; set; } = new();
}
