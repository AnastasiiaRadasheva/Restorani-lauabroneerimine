using Restoran.Application.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restoran.Application.DTOs;
using Restoran.Application.Interfaces;
using Restoran.Application.Services;

namespace Restoran.Api.Controllers;

/// <summary>
/// Broneering telefoni või vestluse põhjal: administraator sisestab kliendi andmed ja broneerib.
/// Broneering jääb olekusse "ootab makset" ja kliendile saadetakse makselink (SMS ja e-kiri).
/// Kui klient maksab, kinnitatakse broneering automaatselt.
/// </summary>
[ApiController]
[Produces("application/json")]
public class StaffBookingController : ControllerBase
{
    private readonly ICustomerService _customers;
    private readonly IReservationService _reservations;
    private readonly IBookingMessenger _messenger;
    private readonly INotificationService _notifications;
    private readonly IUserRepository _users;

    public StaffBookingController(ICustomerService customers, IReservationService reservations, IBookingMessenger messenger,
        INotificationService notifications, IUserRepository users)
    {
        _customers = customers;
        _reservations = reservations;
        _messenger = messenger;
        _notifications = notifications;
        _users = users;
    }

    /// <summary>Klientide otsing nime, e-posti või telefoni järgi (personal).</summary>
    [Authorize(Roles = "Admin,Waiter")]
    [HttpGet("api/customers")]
    public async Task<ActionResult<List<UserDto>>> Search([FromQuery] string? search) => Ok(await _customers.SearchAsync(search));

    /// <summary>Saada kliendile uus link parooli loomiseks (kui ta pole veel parooli loonud).</summary>
    [Authorize(Roles = "Admin,Waiter")]
    [HttpPost("api/customers/{id:int}/password-link")]
    public async Task<ActionResult<UserDto>> PasswordLink(int id)
    {
        var user = await _customers.RenewPasswordLinkAsync(id);
        await _messenger.SendPasswordLinkAsync(id);
        return Ok(user);
    }

    /// <summary>Broneering kliendi nimel (admin/teenindaja): customerId = kes (GET /api/users), ülejäänu nagu tavalisel broneeringul.</summary>
    /// <remarks>Kaarti pole vaja: kui on midagi maksta, saab klient makselingi, muidu kohe kinnituse.</remarks>
    [Authorize(Roles = "Admin,Waiter")]
    [HttpPost("api/reservations/for-customer")]
    [Tags("Reservations")] // Swaggeris broneeringute grupis
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReservationDto>> CreateForCustomer(StaffBookingDto dto)
    {
        var customer = await _customers.GetOrCreateAsync(dto.CustomerId, dto.NewCustomer);
        var r = await _reservations.CreatePendingForUserAsync(customer.Id, dto);
        // Midagi maksta pole -> klient saab kohe kinnituse; muidu makselingi
        if (r.Status == ReservationStatus.PendingPayment) await _messenger.SendPaymentRequestAsync(r);
        else await _messenger.SendConfirmationAsync(r);
        return Ok(r);
    }

    /// <summary>Makselingi järgi broneeringu andmed (sisselogimist pole vaja).</summary>
    [AllowAnonymous]
    [HttpGet("api/payments/{token}")]
    public async Task<ActionResult<ReservationDto>> ByToken(string token) => Ok(await _reservations.GetByPaymentTokenAsync(token));

    /// <summary>Makse lingi kaudu (võltsmakse). Pärast makset on broneering kinnitatud ja klient saab kinnituse.</summary>
    [AllowAnonymous]
    [HttpPost("api/payments/{token}")]
    public async Task<ActionResult<ReservationDto>> Pay(string token, FakePaymentDto dto)
    {
        var r = await _reservations.PayByTokenAsync(token, dto);
        await _messenger.SendConfirmationAsync(r);
        // Personal näeb kohe, et makse tuli
        foreach (var staff in await _users.GetStaffAsync())
            await _notifications.CreateAsync(new CreateNotificationDto
            {
                UserId = staff.Id,
                ReservationId = r.Id,
                Message = L.T(staff.Language, "staff.paid", r.CustomerName, $"{r.StartTime:dd.MM HH:mm}, {r.RestaurantName}")
            });
        return Ok(r);
    }
}
