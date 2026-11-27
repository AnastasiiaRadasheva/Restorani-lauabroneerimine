using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restoran.Application.DTOs;
using Restoran.Application.Services;

namespace Restoran.Api.Controllers;

/// <summary>Teated (nt meeldetuletused).</summary>
[ApiController]
[Route("api/notifications")]
[Produces("application/json")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notifications;

    public NotificationsController(INotificationService notifications)
    {
        _notifications = notifications;
    }

    /// <summary>Taustateenus, samm 2: saada kliendile teade (piisab broneeringu id-st).</summary>
    [Authorize(Roles = "Service,Admin")]
    [HttpPost]
    [ProducesResponseType(typeof(NotificationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NotificationDto>> Create(CreateNotificationDto dto)
    {
        var created = await _notifications.CreateAsync(dto);
        return StatusCode(StatusCodes.Status201Created, created);
    }

    /// <summary>Minu viimased teated.</summary>
    [HttpGet("my")]
    [ProducesResponseType(typeof(List<NotificationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<NotificationDto>>> GetMy() => Ok(await _notifications.GetMyAsync());

    /// <summary>Märgi teade loetuks.</summary>
    [HttpPatch("{id:int}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkRead(int id)
    {
        await _notifications.MarkReadAsync(id);
        return NoContent();
    }
}
