using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restoran.Application.DTOs;
using Restoran.Application.Interfaces;

namespace Restoran.Api.Controllers;

/// <summary>Sõnumid broneeringute kohta (kõik sisseloginud).</summary>
[ApiController]
[Route("api/messages")]
[Produces("application/json")]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly IReservationService _reservations;

    public MessagesController(IReservationService reservations)
    {
        _reservations = reservations;
    }

    /// <summary>Vestlused: klient näeb oma, administraator ja teenindaja kõiki.</summary>
    /// <remarks>
    /// Iga rida on üks broneering: viimane sõnum, sõnumite arv ja kas restoran on vastanud.
    /// unanswered=true – ainult vastamata (viimane sõnum on kliendilt). Vestlust vaata ja vasta: /api/reservations/{id}/messages.
    /// </remarks>
    /// <param name="unanswered">true = ainult vastamata vestlused.</param>
    [HttpGet]
    [ProducesResponseType(typeof(List<MessageThreadDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<MessageThreadDto>>> Threads([FromQuery] bool unanswered = false) =>
        Ok(await _reservations.GetMessageThreadsAsync(unanswered));
}
