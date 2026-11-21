using Restoran.Application.DTOs;
using Restoran.Application.Entities;

namespace Restoran.Application.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByPasswordTokenAsync(string token);
    Task<bool> EmailExistsAsync(string email);
    Task<List<User>> SearchCustomersAsync(string search, int take);
    Task<List<User>> GetStaffAsync();
}

/// <summary>JWT tokeni loomine.</summary>
public interface ITokenService
{
    (string Token, DateTime ExpiresAt) CreateToken(User user);
}

/// <summary>Praegu sisse loginud kasutaja (tuleb JWT tokenist).</summary>
public interface ICurrentUser
{
    int? UserId { get; }
    UserRole? Role { get; }
    bool IsStaff { get; }
    /// <summary>Saidi keel päisest Accept-Language (et/ru/en).</summary>
    string Language { get; }
}

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
    Task<UserDto> GetMeAsync();
    Task<UserDto> UpdateMeAsync(UpdateProfileDto dto);
    Task<PasswordLinkDto> GetPasswordLinkAsync(string token);
    Task<AuthResponseDto> SetPasswordAsync(SetPasswordDto dto);
}
