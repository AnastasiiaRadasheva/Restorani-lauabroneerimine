using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restoran.Application.Services;

namespace Restoran.Api.Controllers;

/// <summary>Kalendri päevade märked personalile (nt "Olulised külalised", "Terrass suletud").</summary>
[ApiController]
[Route("api/day-notes")]
[Produces("application/json")]
[Authorize(Roles = "Admin,Waiter")]
public class DayNotesController : ControllerBase
{
    private readonly IDayNoteService _notes;

    public DayNotesController(IDayNoteService notes)
    {
        _notes = notes;
    }

    /// <summary>Märked alates kuupäevast from järgmise days päeva jooksul (kuni 62).</summary>
    [HttpGet]
    public async Task<ActionResult<List<DayNoteDto>>> Get([FromQuery] DateOnly from, [FromQuery] int days = 42, [FromQuery] int? restaurantId = null) =>
        Ok(await _notes.GetRangeAsync(from, days, restaurantId));

    /// <summary>Lisa päevale märge. Värv: ox, gold, green, blue või grey.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(DayNoteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DayNoteDto>> Add(NewDayNoteDto dto) => Ok(await _notes.AddAsync(dto));

    /// <summary>Kustuta märge.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        await _notes.DeleteAsync(id);
        return NoContent();
    }
}
