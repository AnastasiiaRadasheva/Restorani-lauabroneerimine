using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restoran.Application.DTOs;
using Restoran.Application.Interfaces;

namespace Restoran.Api.Controllers;

/// <summary>Kasutajad (ainult Admin).</summary>
[ApiController]
[Route("api/users")]
[Produces("application/json")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly IAuthService _auth;

    public UsersController(IAuthService auth)
    {
        _auth = auth;
    }

    /// <summary>Kõik kasutajad: otsing nime, e-posti või telefoni järgi või broneeringu id järgi.</summary>
    /// <remarks>
    /// Tühjade väljadega tuleb kogu nimekiri. reservations = mitu broneeringut kasutajal on.
    ///
    /// Näide: GET /api/users?search=kati või GET /api/users?reservationId=5
    /// </remarks>
    /// <param name="search">Nimi, e-post või telefon (või nende osa).</param>
    /// <param name="reservationId">Broneeringu id: näitab, kelle broneering see on.</param>
    [HttpGet]
    [ProducesResponseType(typeof(List<UserListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<UserListItemDto>>> GetAll([FromQuery] string? search, [FromQuery] int? reservationId) =>
        Ok(await _auth.GetUsersAsync(search, reservationId));
}
