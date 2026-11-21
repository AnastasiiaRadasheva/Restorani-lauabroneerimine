namespace Restoran.Application.Entities;

/// <summary>Lisateenus broneeringule (lilled, šampanja, viiuldaja …).</summary>
public class ReservationExtra
{
    public int Id { get; set; }
    public int ReservationId { get; set; }
    public Reservation? Reservation { get; set; }

    /// <summary>Lisateenuse kood kataloogist, nt "flowers".</summary>
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    /// <summary>Hind tellimise hetkel.</summary>
    public decimal Price { get; set; }

    /// <summary>Lisatud broneerimisel ja makstud koos broneeringuga – klient ei saa seda eemaldada.
    /// Hiljem ("Minu broneeringutes") lisatud teenuse saab klient ise eemaldada.</summary>
    public bool Locked { get; set; }
}
