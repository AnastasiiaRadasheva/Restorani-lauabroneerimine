using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restoran.Application.DTOs;
using Restoran.Application.Interfaces;

namespace Restoran.Api.Controllers;

/// <summary>Laudade haldus.</summary>
[ApiController]
[Route("api/tables")]
[Produces("application/json")]
public class TablesController : ControllerBase
{
    private readonly ITableService _tables;
    private readonly IReservationService _reservations;

    public TablesController(ITableService tables, IReservationService reservations)
    {
        _tables = tables;
        _reservations = reservations;
    }

    /// <summary>Sobiva vaba laua pakkumine.</summary>
    /// <remarks>
    /// Vastuse alguses on kokkuvõte: hasFreeTable (kas on vaba laud), freeTables (mitu sobivat lauda on vaba),
    /// message (lühike tekst) ja suggestedTableNumber (pakutud laud). Seejärel kõik restorani lauad:
    /// isFree = laud on sel ajal vaba, fitsGuests = laud on piisavalt suur. Broneering kestab 2 tundi.
    ///
    /// restaurantId on kohustuslik: 1 – Vanalinn, 2 – Kalamaja, 3 – Kadriorg.
    ///
    /// Näide: GET /api/tables/available?date=2027-01-15&amp;time=19:00&amp;guests=4
    /// või GET /api/tables/available?restaurantId=2&amp;date=2027-01-15&amp;time=19:00&amp;guests=4
    /// </remarks>
    [HttpGet("available")]
    [ProducesResponseType(typeof(AvailabilityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AvailabilityDto>> Available(
        [FromQuery] int? restaurantId, [FromQuery] DateOnly date, [FromQuery] TimeOnly time, [FromQuery] int guests) =>
        Ok(await _reservations.GetAvailabilityAsync(Rid(restaurantId, required: true), date, time, guests));

    /// <summary>Vabade laudade arv iga kellaaja kohta mitmeks päevaks (broneerimise kalender).</summary>
    /// <remarks>
    /// Kui mõne kellaaja FreeTables on 0, on see aeg täis. Kui päeva kõik ajad on täis, on päev täis.
    ///
    /// Näide: GET /api/tables/slots?restaurantId=1&amp;from=2027-01-15&amp;days=30&amp;guests=2
    /// </remarks>
    [HttpGet("slots")]
    [ProducesResponseType(typeof(List<DaySlotsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<DaySlotsDto>>> Slots(
        [FromQuery] int? restaurantId, [FromQuery] DateOnly from, [FromQuery] int days, [FromQuery] int guests) =>
        Ok(await _reservations.GetSlotsAsync(Rid(restaurantId, required: true), from, days, guests));

    /// <summary>Päeva laudade plaan: lauad ja nende broneeringud (personal).</summary>
    /// <remarks>
    /// restaurantId on valikuline: kui see puudub, kasutatakse esimest restorani.
    ///
    /// Näide: GET /api/tables/plan?date=2027-01-15 või GET /api/tables/plan?restaurantId=2&amp;date=2027-01-15
    /// </remarks>
    [Authorize(Roles = "Admin,Waiter")]
    [HttpGet("plan")]
    [ProducesResponseType(typeof(TablePlanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TablePlanDto>> Plan([FromQuery] int? restaurantId, [FromQuery] DateOnly date) =>
        Ok(await _reservations.GetPlanAsync(Rid(restaurantId), date));

    /// <summary>Tühi restaurantId = esimene restoran (0 tähendab "määramata"); 0 või negatiivne kirjutatud id = restorani pole.</summary>
    private static int Rid(int? restaurantId, bool required = false) =>
        restaurantId is null
            ? (required ? throw new Restoran.Application.Common.BusinessRuleException("Valige restoran: restaurantId (1 – Vanalinn, 2 – Kalamaja, 3 – Kadriorg).") : 0)
        : restaurantId > 0 ? restaurantId.Value
        : throw new Restoran.Application.Common.NotFoundException($"Restorani id-ga {restaurantId} ei ole (id algab 1-st).");

    /// <summary>Üks laud.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TableDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TableDto>> Get(int id) => Ok(await _tables.GetByIdAsync(id));

    /// <summary>Lisa laud.</summary>
    /// <remarks>
    /// Näide:
    ///
    ///     POST /api/tables
    ///     { "restaurantId": 1, "number": 11, "seats": 4, "location": "Hall", "x": 50, "y": 10, "width": 12, "height": 12 }
    /// </remarks>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ProducesResponseType(typeof(TableDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TableDto>> Create(SaveTableDto dto)
    {
        var created = await _tables.CreateAsync(dto);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    /// <summary>Muuda lauda.</summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(TableDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TableDto>> Update(int id, SaveTableDto dto) => Ok(await _tables.UpdateAsync(id, dto));

    /// <summary>Kustuta laud (kui sellel pole tulevasi broneeringuid).</summary>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        await _tables.DeleteAsync(id);
        return NoContent();
    }
}
