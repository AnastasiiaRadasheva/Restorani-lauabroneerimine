namespace Restoran.Application.Entities;

/// <summary>Gastronoomiline õhtu (eriline õhtusöök kindlal kuupäeval, ainult eelregistreerimisega).</summary>
public class GastroEvent
{
    public int Id { get; set; }
    public int RestaurantId { get; set; }
    public Restaurant? Restaurant { get; set; }

    public string Title { get; set; } = "";
    /// <summary>Õhtu liik, nt "Degustatsiooniõhtusöök".</summary>
    public string Kind { get; set; } = "";
    public string Description { get; set; } = "";
    public DateTime StartTime { get; set; }
    public decimal PricePerGuest { get; set; }
    /// <summary>Riietus, nt "smart elegant".</summary>
    public string DressCode { get; set; } = "";
    /// <summary>Range reegel. Kui see on olemas, peab külaline sellega registreerimisel nõustuma.</summary>
    public string? Rule { get; set; }
    public string? ImageUrl { get; set; }
    /// <summary>Mitu külalist mahub.</summary>
    public int Capacity { get; set; }

    public List<EventRegistration> Registrations { get; set; } = new();
}

/// <summary>Külalise registreerimine gastronoomilisele õhtule.</summary>
public class EventRegistration
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public GastroEvent? Event { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }

    public int Guests { get; set; }
    /// <summary>Kas külaline nõustus riietuse reegliga.</summary>
    public bool AcceptedRules { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
