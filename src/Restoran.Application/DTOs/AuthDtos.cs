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
public record UserDto(int Id, string Name, string Email, string? Phone, UserRole Role, string? AvatarUrl, bool SmsOptIn, bool PasswordSet = true);

/// <summary>Parooli loomise lingi info (kellele link kuulub).</summary>
public record PasswordLinkDto(string Name, string Email);

/// <summary>Parooli loomine lingi kaudu (personali loodud klient).</summary>
public class SetPasswordDto
{
    [Required] public string Token { get; set; } = "";
    /// <example>Parool123!</example>
    [Required, StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = "";
}

/// <summary>Profiili muutmine.</summary>
public class UpdateProfileDto
{
    /// <example>Kati Klient</example>
    [Required, StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = "";

    /// <example>+372 5895 1836</example>
    [RegularExpression(@"^[+\d\s-]{5,30}$", ErrorMessage = "Telefoninumber on vigane.")]
    public string? Phone { get; set; }

    /// <summary>Uus e-post (tühi = jätta muutmata). Edaspidi logitakse sisse uue e-postiga.</summary>
    /// <example>kati.uus@example.com</example>
    [EmailAddress, StringLength(200)]
    public string? Email { get; set; }

    /// <summary>Kas saata broneeringu kinnitus SMS-iga.</summary>
    public bool SmsOptIn { get; set; } = true;

    /// <summary>Profiilipilt data-URL kujul (kuni ~300 KB). Tühi = jätta muutmata, "-" = eemaldada.</summary>
    public string? AvatarUrl { get; set; }
}

/// <summary>Sisselogimise vastus: JWT token ja kasutaja.</summary>
public record AuthResponseDto(string Token, DateTime ExpiresAt, UserDto User);
