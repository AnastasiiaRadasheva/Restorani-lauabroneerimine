using System.ComponentModel.DataAnnotations;
using Restoran.Application.Common;
using Restoran.Application.Entities;

namespace Restoran.Application.DTOs;

public record PreOrderItemDto(int MenuItemId, string Name, int Quantity, decimal UnitPrice, decimal Total);

/// <summary>Lisateenus broneeringus.</summary>
public record ExtraDto(string Code, string Name, decimal Price);

public record ReservationDto(
    int Id,
    int RestaurantId,
    string RestaurantName,
    string RestaurantAddress,
    int TableId,
    int TableNumber,
    TableLocation TableLocation,
    int UserId,
    string CustomerName,
    string? CustomerPhone,
    DateTime StartTime,
    DateTime EndTime,
    int Guests,
    ReservationStatus Status,
    string? SpecialRequest,
    bool IsPaid,
    decimal PreOrderTotal,
    List<PreOrderItemDto> PreOrder,
    // CanModify: kas klient saab broneeringut veel muuta (rohkem kui 24 h alguseni)
    bool CanModify,
    DateTime CreatedAt,
    // Õhtu detailid
    Occasion Occasion,
    ChefGift Gift,
    HostType HostType,
    string? HostName,
    AddressForm AddressForm,
    string ServiceLanguage,
    decimal SeatFee,
    List<ExtraDto> Extras,
    decimal ExtrasTotal,
    // Kokku: eeltellimus + lisateenused + laua valimise tasu
    decimal Total,
    int MessageCount,
    // Kas viimane sõnum on personalilt (kliendile "uus vastus")
    bool LastMessageFromStaff,
    DateTime? CancelledAt,
    // Tagastatav summa pärast tühistamist (null = pole tühistatud)
    decimal? RefundAmount);

/// <summary>Tühistamine. Vähem kui 24 h enne peab klient kinnitama, et raha ei tagastata.</summary>
public class CancelReservationDto
{
    /// <summary>Klient on nõus, et makstud summat ei tagastata (vajalik alla 24 h enne).</summary>
    public bool AcceptNoRefund { get; set; }
}

/// <summary>Sõnum broneeringu kohta.</summary>
public record MessageDto(int Id, bool FromStaff, string AuthorName, string Text, DateTime CreatedAt);

/// <summary>Uus sõnum.</summary>
public class NewMessageDto
{
    /// <example>Kas tordi saaks serveerida magustoiduga?</example>
    [Required, StringLength(1000, MinimumLength = 1)]
    public string Text { get; set; } = "";
}

/// <summary>Lisateenuste muutmine olemasoleval broneeringul.</summary>
public class SetExtrasDto
{
    /// <example>["flowers", "chair"]</example>
    [Required]
    public List<string> Extras { get; set; } = new();
}

/// <summary>Uus broneering. Kui TableId puudub, valib süsteem sobiva laua ise.</summary>
public class CreateReservationDto
{
    /// <example>1</example>
    [Range(1, int.MaxValue)]
    public int RestaurantId { get; set; }

    /// <summary>Kuupäev kujul yyyy-MM-dd.</summary>
    /// <example>2026-11-20</example>
    [Required]
    public DateOnly Date { get; set; }

    /// <summary>Kellaaeg kujul HH:mm.</summary>
    /// <example>19:00</example>
    [Required]
    public TimeOnly Time { get; set; }

    /// <example>4</example>
    [Range(1, 20)]
    public int Guests { get; set; }

    /// <summary>Kliendi valitud laud. Kui tühi, pakub süsteem väikseima sobiva vaba laua.</summary>
    public int? TableId { get; set; }

    /// <summary>Erisoovid (nt sünnipäev, tort, eriline õhtusöök).</summary>
    /// <example>Sünnipäev, palun küünlad tordile</example>
    [StringLength(500)]
    public string? SpecialRequest { get; set; }

    /// <summary>Eeltellimus koos broneeringuga (valikuline).</summary>
    public List<PreOrderLineDto>? Items { get; set; }

    /// <summary>Kas klient valis laua ise plaanilt (siis lisandub laua valimise tasu).</summary>
    public bool ChosenByGuest { get; set; }

    /// <example>Birthday</example>
    [EnumDataType(typeof(Occasion))]
    public Occasion Occasion { get; set; }

    /// <summary>Peakoka kingitus (ainult kui on tähtpäev).</summary>
    /// <example>Cake</example>
    [EnumDataType(typeof(ChefGift))]
    public ChefGift Gift { get; set; }

    [EnumDataType(typeof(HostType))]
    public HostType HostType { get; set; }

    /// <summary>Peakülalise nimi, kui HostType = Other.</summary>
    [StringLength(100)]
    public string? HostName { get; set; }

    [EnumDataType(typeof(AddressForm))]
    public AddressForm AddressForm { get; set; }

    /// <summary>Teeninduse keel: et, ru või en.</summary>
    /// <example>et</example>
    [RegularExpression("^(et|ru|en)$", ErrorMessage = "Keel peab olema et, ru või en.")]
    public string ServiceLanguage { get; set; } = "et";

    /// <summary>Lisateenuste koodid (vt GET /api/reservations/extras).</summary>
    /// <example>["flowers"]</example>
    public List<string>? Extras { get; set; }
}

/// <summary>Broneeringu muutmine (lubatud rohkem kui 24 h enne algust).</summary>
public class UpdateReservationDto
{
    [Required] public DateOnly Date { get; set; }
    [Required] public TimeOnly Time { get; set; }
    [Range(1, 20)] public int Guests { get; set; }
    public int? TableId { get; set; }
    [StringLength(500)] public string? SpecialRequest { get; set; }
}

public class PreOrderLineDto
{
    /// <example>3</example>
    [Range(1, int.MaxValue)]
    public int MenuItemId { get; set; }

    /// <example>2</example>
    [Range(1, 50)]
    public int Quantity { get; set; }
}

/// <summary>Eeltellimus: asendab broneeringu senise eeltellimuse.</summary>
public class PreOrderDto
{
    [Required]
    public List<PreOrderLineDto> Items { get; set; } = new();

    /// <summary>Soovid köögile.</summary>
    [StringLength(500)]
    public string? SpecialRequest { get; set; }
}

/// <summary>Võltsmakse (päris raha ei liigu, kaardiandmeid ei salvestata).</summary>
public class FakePaymentDto
{
    /// <example>Kati Klient</example>
    [Required, StringLength(100)]
    public string CardHolder { get; set; } = "";

    /// <summary>Testkaart, nt 4242 4242 4242 4242.</summary>
    /// <example>4242 4242 4242 4242</example>
    [Required, RegularExpression(@"^[0-9 ]{12,23}$", ErrorMessage = "Kaardi number on vigane.")]
    public string CardNumber { get; set; } = "";

    /// <example>12/28</example>
    [Required, RegularExpression(@"^(0[1-9]|1[0-2])/\d{2}$", ErrorMessage = "Kehtivus peab olema kujul KK/AA.")]
    public string Expiry { get; set; } = "";

    /// <example>123</example>
    [Required, RegularExpression(@"^\d{3}$", ErrorMessage = "CVC on 3 numbrit.")]
    public string Cvc { get; set; } = "";
}

public class ChangeStatusDto
{
    /// <example>Arrived</example>
    [EnumDataType(typeof(ReservationStatus))]
    public ReservationStatus Status { get; set; }
}

/// <summary>Broneeringute filter personalile.</summary>
public class ReservationQuery : PageQuery
{
    public int? RestaurantId { get; set; }

    /// <summary>Päev kujul yyyy-MM-dd.</summary>
    public DateOnly? Date { get; set; }

    public ReservationStatus? Status { get; set; }
}

/// <summary>Laud koos infoga, kas see on valitud ajal vaba.</summary>
public record TableAvailabilityDto(
    int Id,
    int Number,
    int Seats,
    TableLocation Location,
    int X, int Y, int Width, int Height,
    string? Spot,
    bool IsRound,
    bool IsFree,
    bool FitsGuests);

/// <summary>Vabade laudade vastus: pakutud laud ja kõik lauad plaani jaoks.</summary>
public record AvailabilityDto(
    int RestaurantId,
    DateTime StartTime,
    DateTime EndTime,
    int Guests,
    int? SuggestedTableId,
    List<TableAvailabilityDto> Tables);

/// <summary>Üks kellaaeg ja mitu sobivat lauda on siis vaba (0 = täis).</summary>
public record SlotDto(string Time, int FreeTables);

/// <summary>Ühe päeva kellaajad broneerimise kalendri jaoks.</summary>
public record DaySlotsDto(DateOnly Date, List<SlotDto> Slots);

/// <summary>Päeva plaani üks broneering.</summary>
public record PlanReservationDto(
    int Id,
    DateTime StartTime,
    DateTime EndTime,
    int Guests,
    ReservationStatus Status,
    string CustomerName,
    bool HasPreOrder);

public record PlanTableDto(TableDto Table, List<PlanReservationDto> Reservations);

/// <summary>Päeva laudade plaan.</summary>
public record TablePlanDto(RestaurantDto Restaurant, DateOnly Date, List<PlanTableDto> Tables);
