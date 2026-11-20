using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restoran.Application.DTOs;
using Restoran.Application.Interfaces;

namespace Restoran.Api.Controllers;

/// <summary>Gastronoomilised õhtud (erilised õhtusöögid).</summary>
[ApiController]
[Route("api/events")]
[Produces("application/json")]
public class EventsController : ControllerBase
{
    private readonly IEventService _events;

    public EventsController(IEventService events)
    {
        _events = events;
    }

    /// <summary>Tulevased õhtud koos vabade kohtadega.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<GastroEventDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<GastroEventDto>>> GetUpcoming() => Ok(await _events.GetUpcomingAsync());

    /// <summary>Registreeru õhtule.</summary>
    /// <remarks>
    /// Kui õhtul on riietuse reegel, peab acceptRules olema true.
    ///
    ///     POST /api/events/2/join
    ///     { "guests": 2, "acceptRules": true }
    /// </remarks>
    [Authorize]
    [HttpPost("{id:int}/join")]
    [ProducesResponseType(typeof(GastroEventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<GastroEventDto>> Join(int id, JoinEventDto dto) => Ok(await _events.JoinAsync(id, dto));

    /// <summary>Tühista oma registreerimine.</summary>
    [Authorize]
    [HttpDelete("{id:int}/join")]
    [ProducesResponseType(typeof(GastroEventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GastroEventDto>> Leave(int id) => Ok(await _events.LeaveAsync(id));
}
