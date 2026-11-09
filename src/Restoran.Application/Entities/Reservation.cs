namespace Restoran.Application.Entities;

public class Reservation
{
    public int Id { get; set; }
    public int TableId { get; set; }
    public DiningTable? Table { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int Guests { get; set; }
    public ReservationStatus Status { get; set; } = ReservationStatus.Active;

    /// <summary>Kliendi soovid (nt eriline õhtusöök, tort, aknaalune laud).</summary>
    public string? SpecialRequest { get; set; }
    public bool IsPaid { get; set; }
    public bool Reminded { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public List<PreOrderItem> PreOrderItems { get; set; } = new();
}
