using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restoran.Application.Common;
using Restoran.Application.DTOs;
using Restoran.Application.Interfaces;

namespace Restoran.Api.Controllers;

/// <summary>Broneeringud.</summary>
[ApiController]
[Route("api/reservations")]
[Produces("application/json")]
[Authorize]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _reservations;

    public ReservationsController(IReservationService reservations)
    {
        _reservations = reservations;
    }

    /// <summary>Lisateenuste kataloog (lilled, šampanja, viiuldaja …) koos hindadega.</summary>
    [AllowAnonymous]
    [HttpGet("extras")]
    [ProducesResponseType(typeof(IReadOnlyList<Restoran.Application.Services.ExtraInfo>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<Restoran.Application.Services.ExtraInfo>> Extras() =>
        Ok(Restoran.Application.Services.ExtrasCatalog.All);

    /// <summary>Kõik broneeringud koos filtri, otsingu, sortimise ja lehekülgedega (personal).</summary>
    /// <remarks>Näide: GET /api/reservations?restaurantId=1&amp;date=2026-11-20&amp;status=Active&amp;search=kati&amp;sort=date&amp;page=1&amp;pageSize=10</remarks>
    [Authorize(Roles = "Admin,Waiter")]
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ReservationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ReservationDto>>> GetAll([FromQuery] ReservationQuery query) =>
        Ok(await _reservations.GetPagedAsync(query));

    /// <summary>Minu broneeringud.</summary>
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
    ///     { "restaurantId": 1, "date": "2026-11-20", "time": "19:00", "guests": 4, "specialRequest": "Sünnipäev" }
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
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    /// <summary>Muuda broneeringut (klient saab muuta kuni 24 h enne algust).</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReservationDto>> Update(int id, UpdateReservationDto dto) =>
        Ok(await _reservations.UpdateAsync(id, dto));

    /// <summary>Tühista broneering (klient kuni 24 h enne algust).</summary>
    [HttpPatch("{id:int}/cancel")]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReservationDto>> Cancel(int id) => Ok(await _reservations.CancelAsync(id));

    /// <summary>Eeltellimus (asendab varasema eeltellimuse).</summary>
    /// <remarks>
    /// Näide:
    ///
    ///     POST /api/reservations/5/preorder
    ///     { "items": [ { "menuItemId": 3, "quantity": 2 }, { "menuItemId": 7, "quantity": 2 } ], "specialRequest": "Ilma sibulata" }
    /// </remarks>
    [HttpPost("{id:int}/preorder")]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReservationDto>> PreOrder(int id, PreOrderDto dto) =>
        Ok(await _reservations.SetPreOrderAsync(id, dto));

    /// <summary>Võltsmakse eeltellimuse eest (testkaart 4242 4242 4242 4242).</summary>
    [HttpPost("{id:int}/pay")]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReservationDto>> Pay(int id, FakePaymentDto dto) =>
        Ok(await _reservations.PayAsync(id, dto));

    /// <summary>Muuda olekut (nt klient saabus: Arrived). Ainult personal.</summary>
    [Authorize(Roles = "Admin,Waiter")]
    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReservationDto>> ChangeStatus(int id, ChangeStatusDto dto) =>
        Ok(await _reservations.ChangeStatusAsync(id, dto.Status));
}
