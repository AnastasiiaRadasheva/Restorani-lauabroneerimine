namespace Restoran.Application.Entities;

/// <summary>Gastronoomiline õhtu (eriline õhtusöök kindlal kuupäeval, ainult eelregistreerimisega).</summary>
public class GastroEvent
{
    public int Id { get; set; }
    public int RestaurantId { get; set; }
    public Restaurant? Restaurant { get; set; }

    /// <summary>Lühikood (steel, white, black) näidisõhtutel – veebis tõlgitakse tekstid selle järgi. Admini loodud õhtul tühi.</summary>
    public string Code { get; set; } = "";
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
    /// <summary>Õhtule ei tohi tulla lastega.</summary>
    public bool NoChildren { get; set; }
    /// <summary>Foto aadress või väike pilt (data URL).</summary>
    public string? ImageUrl { get; set; }
    /// <summary>Mitu külalist mahub (kuni õhtu saali kohtade arvuni).</summary>
    public int Capacity { get; set; }
    /// <summary>Kui määratud, toimub õhtu ainult selles saalis (nt teisel korrusel).</summary>
    public TableLocation? Zone { get; set; }

    /// <summary>"Ainult sel õhtul" – mida saab juurde tellida (admin valib).</summary>
    public List<EventMenuItem> Items { get; set; } = new();
    public List<EventRegistration> Registrations { get; set; } = new();
}

/// <summary>Õhtu eripakkumine (nt viskisaade). Võib olla võetud restorani menüüst või lisatud eraldi.</summary>
public class EventMenuItem
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public GastroEvent? Event { get; set; }
    /// <summary>Näidisõhtute joogikood tõlke jaoks (whisky, chablis…).</summary>
    public string? Code { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    /// <summary>Kui võeti restorani menüüst.</summary>
    public int? MenuItemId { get; set; }
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
    /// <summary>Valitud laud õhtu restoranis.</summary>
    public int? TableId { get; set; }
    public DiningTable? Table { get; set; }
    /// <summary>Õhtu eripakkumised kujul "id:kogus,id:kogus".</summary>
    public string? Drinks { get; set; }
    /// <summary>Lisateenused komaga (õhtul lubatud ainult šampanja ja lilled).</summary>
    public string? Extras { get; set; }
    /// <summary>Kokku: osalustasu + eripakkumised + lisateenused.</summary>
    public decimal Total { get; set; }
    /// <summary>Kas külaline nõustus riietuse (ja lastega seotud) reegliga.</summary>
    public bool AcceptedRules { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>Active (makstud), PendingPayment (admin registreeris, ootab makset) või Cancelled.</summary>
    public ReservationStatus Status { get; set; } = ReservationStatus.Active;
    public bool IsPaid { get; set; }
    /// <summary>Makselingi kood (ainult personali tehtud registreerimisel).</summary>
    public string? PaymentToken { get; set; }
    public DateTime? CancelledAt { get; set; }
    public decimal? RefundAmount { get; set; }
}
