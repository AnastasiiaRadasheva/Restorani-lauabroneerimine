using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restoran.Application.DTOs;
using Restoran.Application.Interfaces;
using Restoran.Application.Services;

namespace Restoran.Api.Controllers;

/// <summary>Gastronoomilised õhtud: vaatamine, registreerimine, makse, tühistamine ja (admin) haldus.</summary>
[ApiController]
[Route("api/events")]
[Produces("application/json")]
public class EventsController : ControllerBase
{
    private readonly IEventService _events;
    private readonly ICustomerService _customers;
    private readonly IBookingMessenger _messenger;
    private readonly INotificationService _notifications;
    private readonly IUserRepository _users;

    public EventsController(IEventService events, ICustomerService customers, IBookingMessenger messenger,
        INotificationService notifications, IUserRepository users)
    {
        _events = events;
        _customers = customers;
        _messenger = messenger;
        _notifications = notifications;
        _users = users;
    }

    /// <summary>Tulevased õhtud.</summary>
    [HttpGet]
    public async Task<ActionResult<List<GastroEventDto>>> GetUpcoming() => Ok(await _events.GetUpcomingAsync());

    /// <summary>Personalile: tulevased ja viimase 30 päeva õhtud.</summary>
    [Authorize(Roles = "Admin,Waiter")]
    [HttpGet("all")]
    public async Task<ActionResult<List<GastroEventDto>>> GetAll() => Ok(await _events.GetAllForStaffAsync());

    /// <summary>Õhtu saali lauad ja kas need on võetud.</summary>
    [HttpGet("{id:int}/tables")]
    public async Task<ActionResult<List<EventTableDto>>> Tables(int id) => Ok(await _events.GetTablesAsync(id));

    /// <summary>Klient registreerub ise ja maksab kohe (kaart on kohustuslik, kui on midagi maksta).</summary>
    [Authorize]
    [HttpPost("{id:int}/join")]
    [ProducesResponseType(typeof(EventRegistrationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EventRegistrationDto>> Join(int id, JoinEventDto dto)
    {
        var r = await _events.JoinAsync(id, dto);
        await _messenger.SendEventConfirmationAsync(r);
        return Ok(r);
    }

    /// <summary>Personal registreerib kliendi (telefoni teel). Klient saab makselingi.</summary>
    [Authorize(Roles = "Admin,Waiter")]
    [HttpPost("{id:int}/join-for-customer")]
    public async Task<ActionResult<EventRegistrationDto>> JoinForCustomer(int id, StaffEventJoinDto dto)
    {
        var customer = await _customers.GetOrCreateAsync(dto.CustomerId, dto.NewCustomer);
        var r = await _events.JoinForCustomerAsync(id, customer.Id, dto);
        if (r.Status == "PendingPayment") await _messenger.SendEventPaymentRequestAsync(r);
        else await _messenger.SendEventConfirmationAsync(r);
        return Ok(r);
    }

    /// <summary>Minu registreerimised ("Minu broneeringud").</summary>
    [Authorize]
    [HttpGet("registrations/my")]
    public async Task<ActionResult<List<EventRegistrationDto>>> My() => Ok(await _events.GetMyRegistrationsAsync());

    /// <summary>Õhtu külalised (personal).</summary>
    [Authorize(Roles = "Admin,Waiter")]
    [HttpGet("{id:int}/registrations")]
    public async Task<ActionResult<List<EventRegistrationDto>>> Registrations(int id) => Ok(await _events.GetRegistrationsAsync(id));

    /// <summary>Makset ootava registreerimise maksmine (sisse loginud klient).</summary>
    [Authorize]
    [HttpPost("registrations/{id:int}/pay")]
    public async Task<ActionResult<EventRegistrationDto>> Pay(int id, FakePaymentDto card)
    {
        var r = await _events.PayRegistrationAsync(id, card);
        await AfterPaymentAsync(r);
        return Ok(r);
    }

    /// <summary>Registreerimise tühistamine (klient või personal).</summary>
    [Authorize]
    [HttpPatch("registrations/{id:int}/cancel")]
    public async Task<ActionResult<EventRegistrationDto>> Cancel(int id, CancelReservationDto? dto) =>
        Ok(await _events.CancelRegistrationAsync(id, dto));

    /// <summary>Makselink (sisselogimist pole vaja).</summary>
    [AllowAnonymous]
    [HttpGet("payments/{token}")]
    public async Task<ActionResult<EventRegistrationDto>> ByToken(string token) => Ok(await _events.GetByPaymentTokenAsync(token));

    [AllowAnonymous]
    [HttpPost("payments/{token}")]
    public async Task<ActionResult<EventRegistrationDto>> PayByToken(string token, FakePaymentDto card)
    {
        var r = await _events.PayByTokenAsync(token, card);
        await AfterPaymentAsync(r);
        return Ok(r);
    }

    /// <summary>Uus õhtu (admin). Kuupäev vähemalt 2 nädala pärast.</summary>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<GastroEventDto>> Create(SaveEventDto dto) => Ok(await _events.CreateAsync(dto));

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<GastroEventDto>> Update(int id, SaveEventDto dto) => Ok(await _events.UpdateAsync(id, dto));

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _events.DeleteAsync(id);
        return NoContent();
    }

    /// <summary>Pärast makset: kliendile kinnitus, personalile teade.</summary>
    private async Task AfterPaymentAsync(EventRegistrationDto r)
    {
        await _messenger.SendEventConfirmationAsync(r);
        foreach (var staff in await _users.GetStaffAsync())
            await _notifications.CreateAsync(new CreateNotificationDto
            {
                UserId = staff.Id,
                Link = "/admin?tab=events",
                Message = L.T(staff.Language, "staff.paid", r.CustomerName, $"{r.EventTitle} {r.StartTime:dd.MM HH:mm}")
            });
    }
}
