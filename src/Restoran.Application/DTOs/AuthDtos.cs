using System.ComponentModel.DataAnnotations;
using Restoran.Application.Entities;

namespace Restoran.Application.DTOs;

/// <summary>Registreerimine (uus kasutaja saab rolli Customer).</summary>
public class RegisterDto
{
    /// <example>Mari Maasikas</example>
    [Required, StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = "";

    /// <example>mari@example.com</example>
    [Required, EmailAddress, StringLength(200)]
    public string Email { get; set; } = "";

    /// <example>+372 5555 5555</example>
    [Phone, StringLength(30)]
    public string? Phone { get; set; }

    /// <example>Parool123!</example>
    [Required, StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = "";
}

/// <summary>Sisselogimine.</summary>
public class LoginDto
{
    /// <example>klient@restoran.ee</example>
    [Required, EmailAddress]
    public string Email { get; set; } = "";

    /// <example>Klient123!</example>
    [Required]
    public string Password { get; set; } = "";
}

/// <summary>Kasutaja andmed ilma paroolita.</summary>
public record UserDto(int Id, string Name, string Email, string? Phone, UserRole Role);

/// <summary>Sisselogimise vastus: JWT token ja kasutaja.</summary>
public record AuthResponseDto(string Token, DateTime ExpiresAt, UserDto User);
