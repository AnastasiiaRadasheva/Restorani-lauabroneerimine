namespace Restoran.Application.Entities;

public class PreOrderItem
{
    public int Id { get; set; }
    public int ReservationId { get; set; }
    public Reservation? Reservation { get; set; }
    public int MenuItemId { get; set; }
    public MenuItem? MenuItem { get; set; }

    public int Quantity { get; set; }
    /// <summary>Hind tellimise hetkel (kui menüü hind muutub, vana tellimus ei muutu).</summary>
    public decimal UnitPrice { get; set; }
}
