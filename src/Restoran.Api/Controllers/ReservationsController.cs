using Restoran.Application.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restoran.Application.Common;
using Restoran.Application.DTOs;
using Restoran.Application.Interfaces;
using Restoran.Application.Services;

namespace Restoran.Api.Controllers;

/// <summary>Broneeringud.</summary>
[ApiController]
[Route("api/reservations")]
[Produces("application/json")]
[Authorize]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _reservations;
    private readonly IBookingMessenger _messenger;
    private readonly INotificationService _notifications;
    private readonly IUserRepository _users;
    private readonly ICurrentUser _current;
    private readonly IEventService _events;

    public ReservationsController(IReservationService reservations, IBookingMessenger messenger,
        INotificationService notifications, IUserRepository users, ICurrentUser current, IEventService events)
    {
        _events = events;
        _reservations = reservations;
        _messenger = messenger;
        _notifications = notifications;
        _users = users;
        _current = current;
    }

    /// <summary>Kalender personalile: broneeringute arv päevade kaupa.</summary>
    /// <remarks>Näide: GET /api/reservations/calendar?restaurantId=1&amp;from=2027-01-01&amp;days=42</remarks>
    [Authorize(Roles = "Admin,Waiter")]
    [HttpGet("calendar")]
    [ProducesResponseType(typeof(List<CalendarDayDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CalendarDayDto>>> Calendar([FromQuery] int? restaurantId, [FromQuery] DateOnly from, [FromQuery] int days = 42) =>
        Ok(await _reservations.GetCalendarAsync(restaurantId, from, days));

    /// <summary>Broneeringu sõnumid (klient ja personal).</summary>
    [HttpGet("{id:int}/messages")]
    [ProducesResponseType(typeof(List<MessageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<MessageDto>>> Messages(int id) => Ok(await _reservations.GetMessagesAsync(id));

    /// <summary>Saada sõnum: klient restoranile või personal kliendile.</summary>
    /// <remarks>
    ///     POST /api/reservations/5/messages
    ///     { "text": "Kas tordi saaks serveerida magustoiduga?" }
    /// </remarks>
    [HttpPost("{id:int}/messages")]
    [ProducesResponseType(typeof(MessageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<MessageDto>> AddMessage(int id, NewMessageDto dto)
    {
        var message = await _reservations.AddMessageAsync(id, dto);
        var r = await _reservations.GetByIdAsync(id);
        var text = message.Text.Length > 80 ? message.Text[..80] + "…" : message.Text;
        // Personal kirjutas -> teade kliendile; klient kirjutas -> teade kogu personalile
        var receivers = message.FromStaff ? new List<int> { r.UserId } : (await _users.GetStaffAsync()).Select(u => u.Id).ToList();
        foreach (var userId in receivers.Where(u => u != _current.UserId))
        {
            // Saaja keeles
            var lang = (await _users.GetByIdAsync(userId))?.Language;
            await _notifications.CreateAsync(new CreateNotificationDto
            {
                UserId = userId,
                ReservationId = id,
                Message = message.FromStaff ? L.T(lang, "msg.toGuest", text) : L.T(lang, "msg.toStaff", r.CustomerName, r.StartTime.ToString("dd.MM HH:mm"), text)
            });
        }
        return Ok(message);
    }

    /// <summary>Kirjuta korraga kõigile päeva külalistele (sõnum läheb iga broneeringu vestlusesse + teade; soovi korral SMS ja e-kiri).</summary>
    /// <remarks>
    ///     POST /api/reservations/broadcast
    ///     { "date": "2027-01-15", "restaurantId": 1, "text": "Vabandame, restoran on suletud.", "alsoSmsEmail": true }
    /// </remarks>
    [Authorize(Roles = "Admin,Waiter")]
    [HttpPost("broadcast")]
    public async Task<ActionResult<int>> Broadcast(BroadcastDto dto)
    {
        var page = await _reservations.GetPagedAsync(new ReservationQuery { Date = dto.Date, RestaurantId = dto.RestaurantId, Page = 1, PageSize = 100 });
        var guests = page.Items.Where(r => r.Status is ReservationStatus.Active or ReservationStatus.PendingPayment or ReservationStatus.Arrived).ToList();
        var text = dto.Text.Trim();
        foreach (var r in guests)
        {
            await _reservations.AddMessageAsync(r.Id, new NewMessageDto { Text = text });
            var lang = (await _users.GetByIdAsync(r.UserId))?.Language;
            var shortText = text.Length > 80 ? text[..80] + "…" : text;
            await _notifications.CreateAsync(new CreateNotificationDto { UserId = r.UserId, ReservationId = r.Id, Message = L.T(lang, "msg.toGuest", shortText) });
            if (dto.AlsoSmsEmail) await _messenger.SendStaffMessageAsync(r.UserId, r.RestaurantName, r.StartTime, text);
            // Personal tühistab: makstud summa tagastatakse alati täies ulatuses
            if (dto.CancelAll && r.Status is ReservationStatus.Active or ReservationStatus.PendingPayment)
                await _reservations.CancelAsync(r.Id, null);
        }
        return Ok(guests.Count);
    }

    /// <summary>Muuda broneeringu lisateenuseid (lubatud ka vähem kui 24 h enne).</summary>
    /// <remarks>
    ///     PUT /api/reservations/5/extras
    ///     { "extras": ["flowers", "chair"] }
    /// </remarks>
    [HttpPut("{id:int}/extras")]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ReservationDto>> SetExtras(int id, SetExtrasDto dto)
    {
        var r = await _reservations.SetExtrasAsync(id, dto);
        await NotifyGuestOfStaffChange(r, "staffChange.extras", r.StartTime.ToString("dd.MM.yyyy HH:mm"));
        return Ok(r);
    }

    /// <summary>Lisateenuste kataloog (lilled, šampanja, viiuldaja …) koos hindadega.</summary>
    [AllowAnonymous]
    [HttpGet("extras")]
    [ProducesResponseType(typeof(IReadOnlyList<Restoran.Application.Services.ExtraInfo>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<Restoran.Application.Services.ExtraInfo>> Extras() =>
        Ok(Restoran.Application.Services.ExtrasCatalog.All);

    /// <summary>Kõik broneeringud (administraator ja teenindaja) koos filtri, otsingu, sortimise ja lehekülgedega.</summary>
    /// <remarks>Näide: GET /api/reservations?restaurantId=1&amp;date=2027-01-15&amp;status=Active&amp;search=kati&amp;sort=date&amp;page=1&amp;pageSize=10</remarks>
    [Authorize(Roles = "Admin,Waiter")]
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ReservationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ReservationDto>>> GetAll([FromQuery] ReservationQuery query) =>
        Ok(await _reservations.GetPagedAsync(query));

    /// <summary>Taustateenus, samm 1: homsed broneeringud, millele pole veel meeldetuletust saadetud.</summary>
    [Authorize(Roles = "Service,Admin")]
    [HttpGet("remind-due")]
    [ProducesResponseType(typeof(List<RemindDueDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<RemindDueDto>>> RemindDue() => Ok(await _reservations.GetRemindDueAsync());

    /// <summary>Taustateenus, samm 3: märgi broneeringu meeldetuletus saadetuks (id = remind-due vastusest).</summary>
    [Authorize(Roles = "Service,Admin")]
    [HttpPatch("{id:int}/reminded")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reminded(int id)
    {
        await _reservations.MarkRemindedAsync(id);
        return NoContent();
    }

    /// <summary>Taustateenus: kes 30 minuti jooksul ei tulnud, saab oleku "ei tulnud" ja laud vabaneb.</summary>
    [Authorize(Roles = "Service,Admin")]
    [HttpPost("mark-no-show")]
    [ProducesResponseType(typeof(NoShowResultDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<NoShowResultDto>> MarkNoShow()
    {
        // Samal ajal: gastronoomiliste õhtute maksmata registreerimised tühistatakse 24 h enne õhtut
        var unpaid = await _events.CancelUnpaidAsync();
        var result = await _reservations.MarkNoShowsAsync();
        // Kliendile teade tema keeles: maksmata broneering / registreerimine tühistati
        foreach (var u in unpaid.Concat(result.UnpaidCancelled ?? new()))
        {
            var lang = (await _users.GetByIdAsync(u.UserId))?.Language;
            await _notifications.CreateAsync(new CreateNotificationDto { UserId = u.UserId, ReservationId = u.ReservationId, Link = "/minu", Message = L.T(lang, "booking.unpaidCancelled", u.What) });
        }
        return Ok(result);
    }

    /// <summary>Minu broneeringud (klient).</summary>
    [HttpGet("my")]
    [ProducesResponseType(typeof(List<ReservationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ReservationDto>>> GetMy() => Ok(await _reservations.GetMyAsync());

    /// <summary>Üks broneering (oma või personalile kõik).</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReservationDto>> Get(int id) => Ok(await _reservations.GetByIdAsync(id));

    /// <summary>Uus broneering. Kui tableId puudub, valitakse väikseim sobiv vaba laud.</summary>
    /// <remarks>
    /// Näide:
    ///
    ///     POST /api/reservations
    ///     { "restaurantId": 1, "date": "2027-01-15", "time": "19:00", "guests": 4, "specialRequest": "Sünnipäev" }
    ///
    /// Kui laud on sel ajal juba broneeritud, tagastatakse 409 Conflict.
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReservationDto>> Create(CreateReservationDto dto)
    {
        var created = await _reservations.CreateAsync(dto);
        // Kinnitus: teade saidil, SMS (kui klient lubas) ja e-kiri tšekiga
        await _messenger.SendConfirmationAsync(created);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    /// <summary>Muuda broneeringut (klient saab muuta kuni 24 h enne algust).</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReservationDto>> Update(int id, UpdateReservationDto dto)
    {
        var r = await _reservations.UpdateAsync(id, dto);
        await NotifyGuestOfStaffChange(r, "staffChange.time", r.StartTime.ToString("dd.MM.yyyy HH:mm"), r.Guests);
        return Ok(r);
    }

    /// <summary>
    /// Kui personal muutis kliendi broneeringut, saab klient teate (oma keeles, link „Minu broneeringud“),
    /// et ta näeks muudatust kohe ka kellukeses.
    /// </summary>
    private async Task NotifyGuestOfStaffChange(ReservationDto r, string key, params object[] args)
    {
        if (!_current.IsStaff || r.UserId == _current.UserId) return;
        var lang = (await _users.GetByIdAsync(r.UserId))?.Language;
        await _notifications.CreateAsync(new CreateNotificationDto { UserId = r.UserId, ReservationId = r.Id, Link = "/minu", Message = L.T(lang, key, args) });
    }

    /// <summary>Personal: muuda külalise soove (allergiad, märkus köögile, soovid, tähtpäev).</summary>
    [Authorize(Roles = "Admin,Waiter")]
    [HttpPut("{id:int}/details")]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReservationDto>> SetDetails(int id, StaffDetailsDto dto)
    {
        var r = await _reservations.SetStaffDetailsAsync(id, dto);
        await NotifyGuestOfStaffChange(r, "staffChange.details", r.StartTime.ToString("dd.MM.yyyy HH:mm"));
        return Ok(r);
    }

    /// <summary>Tühista broneering.</summary>
    /// <remarks>
    /// Rohkem kui 24 h enne: makstud summa tagastatakse.
    /// Vähem kui 24 h enne: raha ei tagastata, klient peab saatma { "acceptNoRefund": true }.
    /// </remarks>
    [HttpPatch("{id:int}/cancel")]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReservationDto>> Cancel(int id, [FromBody(EmptyBodyBehavior = Microsoft.AspNetCore.Mvc.ModelBinding.EmptyBodyBehavior.Allow)] CancelReservationDto? dto) =>
        Ok(await _reservations.CancelAsync(id, dto));

    /// <summary>Eeltellimus: toidud oma broneeringule (asendab varasema eeltellimuse).</summary>
    /// <remarks>
    /// Roa võib anda nime järgi (name) või id järgi (menuItemId). Nimed: GET /api/menu.
    ///
    ///     POST /api/reservations/5/preorder
    ///     { "items": [ { "name": "Kõrvitsasupp", "quantity": 2 }, { "name": "Crème brûlée", "quantity": 2 } ], "specialRequest": "Ilma sibulata" }
    /// </remarks>
    [HttpPost("{id:int}/preorder")]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReservationDto>> PreOrder(int id, PreOrderDto dto)
    {
        var r = await _reservations.SetPreOrderAsync(id, dto);
        await NotifyGuestOfStaffChange(r, "staffChange.preorder", r.StartTime.ToString("dd.MM.yyyy HH:mm"));
        return Ok(r);
    }

    /// <summary>Võltsmakse eeltellimuse eest (testkaart 4242 4242 4242 4242).</summary>
    [HttpPost("{id:int}/pay")]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReservationDto>> Pay(int id, FakePaymentDto dto) =>
        Ok(await _reservations.PayAsync(id, dto));

    /// <summary>Muuda olekut: Arrived (tuli) alates tund enne algust, NoShow (ei tulnud) pärast algust. Ainult personal.</summary>
    [Authorize(Roles = "Admin,Waiter")]
    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReservationDto>> ChangeStatus(int id, ChangeStatusDto dto) =>
        Ok(await _reservations.ChangeStatusAsync(id, dto.Status));
}
