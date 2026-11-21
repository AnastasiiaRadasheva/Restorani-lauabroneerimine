using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restoran.Application.Services;

namespace Restoran.Api.Controllers;

/// <summary>Klienditoe vestlus: klient küsib, administraator vastab (kuuma liini kõrval 10:00–20:00).</summary>
[ApiController]
[Route("api/support")]
[Produces("application/json")]
[Authorize]
public class SupportController : ControllerBase
{
    private readonly ISupportService _support;

    public SupportController(ISupportService support)
    {
        _support = support;
    }

    /// <summary>Minu vestlus administraatoriga.</summary>
    [HttpGet("my")]
    public async Task<ActionResult<List<SupportMessageDto>>> Mine() => Ok(await _support.GetMineAsync());

    /// <summary>Saada küsimus administraatorile.</summary>
    [HttpPost("my")]
    public async Task<ActionResult<SupportMessageDto>> Send(NewSupportMessageDto dto) => Ok(await _support.SendMineAsync(dto));

    /// <summary>Kõik vestlused (personal).</summary>
    [Authorize(Roles = "Admin,Waiter")]
    [HttpGet("threads")]
    public async Task<ActionResult<List<SupportThreadDto>>> Threads() => Ok(await _support.GetThreadsAsync());

    /// <summary>Ühe kliendi vestlus (personal). Märgib kliendi sõnumid loetuks.</summary>
    [Authorize(Roles = "Admin,Waiter")]
    [HttpGet("threads/{userId:int}")]
    public async Task<ActionResult<List<SupportMessageDto>>> Thread(int userId) => Ok(await _support.GetThreadAsync(userId));

    /// <summary>Vasta kliendile (personal).</summary>
    [Authorize(Roles = "Admin,Waiter")]
    [HttpPost("threads/{userId:int}")]
    public async Task<ActionResult<SupportMessageDto>> Reply(int userId, NewSupportMessageDto dto) => Ok(await _support.ReplyAsync(userId, dto));
}
