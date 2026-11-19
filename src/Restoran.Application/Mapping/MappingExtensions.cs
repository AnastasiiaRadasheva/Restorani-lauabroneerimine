using Restoran.Application.DTOs;
using Restoran.Application.Entities;

namespace Restoran.Application.Mapping;

/// <summary>Olemite teisendamine DTO-deks. Väljapoole ei anta kunagi olemeid.</summary>
public static class MappingExtensions
{
    public static RestaurantDto ToDto(this Restaurant r, int tableCount, double? distance = null) => new(
        r.Id, r.Name, r.Address, r.Description, r.Latitude, r.Longitude,
        r.OpensAt.ToString("HH:mm"), r.ClosesAt.ToString("HH:mm"),
        r.BarX, r.BarY, r.BarWidth, r.BarHeight,
        tableCount, distance is null ? null : Math.Round(distance.Value));

    public static void Apply(this Restaurant r, SaveRestaurantDto d)
    {
        r.Name = d.Name.Trim();
        r.Address = d.Address.Trim();
        r.Description = d.Description.Trim();
        r.Latitude = d.Latitude;
        r.Longitude = d.Longitude;
        r.OpensAt = TimeOnly.Parse(d.OpensAt);
        r.ClosesAt = TimeOnly.Parse(d.ClosesAt);
        r.BarX = d.BarX; r.BarY = d.BarY; r.BarWidth = d.BarWidth; r.BarHeight = d.BarHeight;
    }

    public static TableDto ToDto(this DiningTable t) => new(
        t.Id, t.RestaurantId, t.Number, t.Seats, t.Location, t.IsActive, t.X, t.Y, t.Width, t.Height, t.Spot, t.IsRound);

    public static void Apply(this DiningTable t, SaveTableDto d)
    {
        t.RestaurantId = d.RestaurantId;
        t.Number = d.Number;
        t.Seats = d.Seats;
        t.Location = d.Location;
        t.IsActive = d.IsActive;
        t.X = d.X; t.Y = d.Y; t.Width = d.Width; t.Height = d.Height;
        t.Spot = string.IsNullOrWhiteSpace(d.Spot) ? null : d.Spot.Trim();
        t.IsRound = d.IsRound;
    }

    public static MenuItemDto ToDto(this MenuItem m) => new(
        m.Id, m.RestaurantId, m.Name, m.Description, m.Category, m.Price, m.IsAvailable);

    public static void Apply(this MenuItem m, SaveMenuItemDto d)
    {
        m.RestaurantId = d.RestaurantId;
        m.Name = d.Name.Trim();
        m.Description = d.Description.Trim();
        m.Category = d.Category.Trim();
        m.Price = d.Price;
        m.IsAvailable = d.IsAvailable;
    }
}
