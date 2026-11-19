namespace Restoran.Application.Entities;

/// <summary>Sõnum broneeringu kohta: klient kirjutab restoranile, personal vastab.</summary>
public class ReservationMessage
{
    public int Id { get; set; }
    public int ReservationId { get; set; }
    public Reservation? Reservation { get; set; }

    /// <summary>True, kui kirjutas personal (metrdotel), false – klient.</summary>
    public bool FromStaff { get; set; }
    public string AuthorName { get; set; } = "";
    public string Text { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
