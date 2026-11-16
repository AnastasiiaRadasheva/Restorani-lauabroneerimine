using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Restoran.Application.Entities;
using Restoran.Application.Interfaces;

namespace Restoran.Api.Infrastructure;

public class JwtSettings
{
    public string Key { get; set; } = "";
    public string Issuer { get; set; } = "";
    public string Audience { get; set; } = "";
    public int ExpiresHours { get; set; } = 8;
}

public class JwtTokenService : ITokenService
{
    private readonly JwtSettings _settings;

    public JwtTokenService(IConfiguration config)
    {
        _settings = config.GetSection("Jwt").Get<JwtSettings>()!;
    }

    public (string Token, DateTime ExpiresAt) CreateToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
        var expires = DateTime.UtcNow.AddHours(_settings.ExpiresHours);
        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}

/// <summary>Loeb sisse loginud kasutaja andmed HTTP päringu tokenist.</summary>
public class HttpCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _http;

    public HttpCurrentUser(IHttpContextAccessor http)
    {
        _http = http;
    }

    public int? UserId =>
        int.TryParse(_http.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    public UserRole? Role =>
        Enum.TryParse<UserRole>(_http.HttpContext?.User.FindFirstValue(ClaimTypes.Role), out var r) ? r : null;

    public bool IsStaff => Role is UserRole.Admin or UserRole.Waiter;
}
