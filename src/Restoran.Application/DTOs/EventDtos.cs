using System.ComponentModel.DataAnnotations;

namespace Restoran.Application.DTOs;

/// <summary>Õhtu eripakkumine ("ainult sel õhtul").</summary>
public record EventItemDto(int Id, string? Code, string Name, string Description, decimal Price, string? ImageUrl, int? MenuItemId);

/// <summary>Gastronoomiline õhtu koos vabade kohtade arvuga.</summary>
public record GastroEventDto(
    int Id,
    int RestaurantId,
    string RestaurantName,
    string Code,
    string Title,
    string Kind,
    string Description,
    DateTime StartTime,
    decimal PricePerGuest,
    string DressCode,
    string? Rule,
    bool NoChildren,
    string? ImageUrl,
    int Capacity,
    int SeatsLeft,
    string? Zone,
    // Registreerimine sulgub 12 tundi enne algust
    bool RegistrationClosed,
    // Praeguse kasutaja registreerimine (kui on)
    int? MyRegistrationId,
    int MyGuests,
    int? MyTableNumber,
    decimal MyTotal,
    string? MyStatus,
    bool MyIsPaid,
    List<EventItemDto> Items);

/// <summary>Laud õhtu plaanil: kas see on juba võetud.</summary>
public record EventTableDto(
    int Id, int Number, int Seats, Entities.TableLocation Location,
    int X, int Y, int Width, int Height, string? Spot, bool IsRound, bool IsTaken);

/// <summary>Registreerimise rida: eripakkumine ja kogus.</summary>
public record EventLineDto(int ItemId, string? Code, string Name, int Quantity, decimal Price);

/// <summary>Registreerimine "Minu broneeringutes" ja personali nimekirjas.</summary>
public record EventRegistrationDto(
    int Id,
    int EventId,
    string EventCode,
    string EventTitle,
    string Kind,
    DateTime StartTime,
    string RestaurantName,
    string RestaurantAddress,
    string? ImageUrl,
    int Guests,
    int? TableNumber,
    string? TableLocation,
    decimal PricePerGuest,
    List<EventLineDto> Lines,
    List<string> Extras,
    decimal Total,
    string Status,
    bool IsPaid,
    string? PaymentToken,
    DateTime? CancelledAt,
    decimal? RefundAmount,
    // Tühistamisel tagastatakse raha (rohkem kui 24 h enne)
    bool FreeCancel,
    int UserId,
    string CustomerName,
    string CustomerEmail,
    string? CustomerPhone);

public class EventDrinkLineDto
{
    /// <summary>Eripakkumise id (GastroEventDto.Items).</summary>
    /// <example>1</example>
    [Range(1, int.MaxValue)] public int ItemId { get; set; }
    /// <example>2</example>
    [Range(1, 20)] public int Quantity { get; set; } = 1;
}

/// <summary>Registreerimine õhtule. Ise registreerudes makstakse kohe.</summary>
public class JoinEventDto
{
    /// <example>2</example>
    [Range(1, 8)]
    public int Guests { get; set; } = 1;

    /// <summary>Valitud laud (vt GET /api/events/{id}/tables).</summary>
    /// <example>3</example>
    public int? TableId { get; set; }

    /// <summary>Õhtu eripakkumised.</summary>
    public List<EventDrinkLineDto>? Drinks { get; set; }

    /// <summary>Lisateenused: ainult "bottle" ja "flowers".</summary>
    public List<string>? Extras { get; set; }

    /// <summary>Kas külaline nõustub riietuse ja lastega seotud reegliga (kohustuslik, kui õhtul on reegel).</summary>
    public bool AcceptRules { get; set; }

    /// <summary>Kaart (kohustuslik, kui külaline registreerub ise ja on midagi maksta).</summary>
    public FakePaymentDto? Card { get; set; }
}

/// <summary>Personal registreerib kliendi (telefoni teel): klient saab makselingi.</summary>
public class StaffEventJoinDto : JoinEventDto
{
    public int? CustomerId { get; set; }
    public Services.NewCustomerDto? NewCustomer { get; set; }
}

/// <summary>Eripakkumine õhtu salvestamisel: kas restorani menüüst (MenuItemId) või uus.</summary>
public class SaveEventItemDto
{
    public int? MenuItemId { get; set; }
    [StringLength(120)] public string? Name { get; set; }
    [StringLength(300)] public string? Description { get; set; }
    [Range(0, 1000)] public decimal? Price { get; set; }
    [StringLength(300)] public string? ImageUrl { get; set; }
}

/// <summary>Õhtu loomine või muutmine (admin).</summary>
public class SaveEventDto
{
    [Range(1, int.MaxValue)] public int RestaurantId { get; set; }
    /// <example>Trühvliõhtu</example>
    [Required, StringLength(100, MinimumLength = 2)] public string Title { get; set; } = "";
    [StringLength(100)] public string Kind { get; set; } = "";
    [StringLength(1000)] public string Description { get; set; } = "";
    public DateTime StartTime { get; set; }
    [Range(0, 10000)] public decimal PricePerGuest { get; set; }
    [StringLength(100)] public string DressCode { get; set; } = "";
    [StringLength(1000)] public string? Rule { get; set; }
    public bool NoChildren { get; set; }
    /// <summary>Foto aadress või väike pilt (data URL, kuni ~500 KB).</summary>
    [StringLength(700_000)] public string? ImageUrl { get; set; }
    [Range(1, 500)] public int Capacity { get; set; }
    /// <summary>Hall, Terrace, Upstairs või tühi (kogu restoran).</summary>
    public Entities.TableLocation? Zone { get; set; }
    public List<SaveEventItemDto> Items { get; set; } = new();
}
