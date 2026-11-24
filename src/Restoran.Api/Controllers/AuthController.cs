using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restoran.Application.DTOs;
using Restoran.Application.Interfaces;

namespace Restoran.Api.Controllers;

/// <summary>Registreerimine ja sisselogimine.</summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    /// <summary>Uue kliendi registreerimine. Tagastab kohe JWT tokeni.</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
    {
        var result = await _auth.RegisterAsync(dto);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>Sisselogimine. Testkasutajad: admin@restoran.ee / Admin123!, waiter@restoran.ee / Waiter123!, klient@restoran.ee / Klient123!</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto) => Ok(await _auth.LoginAsync(dto));

    /// <summary>
    /// Swaggeri "Authorize" aken (OAuth2 password): e-post (username) ja parool nagu veebilehel.
    /// Tagastab sama JWT tokeni nagu /api/auth/login, kuid OAuth2 kujul (access_token).
    /// </summary>
    [HttpPost("token")]
    [Consumes("application/x-www-form-urlencoded")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> Token([FromForm] string? username, [FromForm] string? password)
    {
        var result = await _auth.LoginAsync(new LoginDto { Email = username ?? "", Password = password ?? "" });
        return Ok(new
        {
            access_token = result.Token,
            token_type = "bearer",
            expires_in = (int)Math.Max(0, (result.ExpiresAt - DateTime.UtcNow).TotalSeconds)
        });
    }

    /// <summary>Parooli loomise link: kellele see kuulub (personali loodud klient).</summary>
    [HttpGet("password/{token}")]
    public async Task<ActionResult<PasswordLinkDto>> PasswordLink(string token) => Ok(await _auth.GetPasswordLinkAsync(token));

    /// <summary>Klient loob lingi kaudu parooli ja on kohe sisse logitud.</summary>
    [HttpPost("password")]
    public async Task<ActionResult<AuthResponseDto>> SetPassword(SetPasswordDto dto) => Ok(await _auth.SetPasswordAsync(dto));

    /// <summary>Sisse loginud kasutaja andmed.</summary>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserDto>> Me() => Ok(await _auth.GetMeAsync());

    /// <summary>Muuda oma profiili: nimi, telefon, SMS-ide nõusolek ja pilt.</summary>
    [Authorize]
    [HttpPut("me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserDto>> UpdateMe(UpdateProfileDto dto) => Ok(await _auth.UpdateMeAsync(dto));
}
