using Restoran.Application.DTOs;
using Restoran.Application.Entities;

namespace Restoran.Application.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<bool> EmailExistsAsync(string email);
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
}

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
    Task<UserDto> GetMeAsync();
}
