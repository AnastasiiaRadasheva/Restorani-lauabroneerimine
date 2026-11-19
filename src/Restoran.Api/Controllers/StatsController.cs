using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restoran.Application.Services;

namespace Restoran.Api.Controllers;

/// <summary>Statistika (ainult Admin).</summary>
[ApiController]
[Route("api/stats")]
[Produces("application/json")]
[Authorize(Roles = "Admin")]
public class StatsController : ControllerBase
{
    private readonly IStatsService _stats;

    public StatsController(IStatsService stats)
    {
        _stats = stats;
    }

    /// <summary>Hõivatumad nädalapäevad, kellaajad ja kuupäevad.</summary>
    /// <param name="restaurantId">Restoran (tühi = kõik).</param>
    /// <param name="from">Alates (vaikimisi 60 päeva tagasi).</param>
    /// <param name="to">Kuni (vaikimisi 14 päeva ette).</param>
    [HttpGet]
    [ProducesResponseType(typeof(StatsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<StatsDto>> Get(int? restaurantId, DateOnly? from, DateOnly? to) =>
        Ok(await _stats.GetAsync(restaurantId, from, to));
}
