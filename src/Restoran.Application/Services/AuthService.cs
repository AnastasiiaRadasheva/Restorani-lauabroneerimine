using Restoran.Application.Common;
using Restoran.Application.DTOs;
using Restoran.Application.Entities;
using Restoran.Application.Interfaces;

namespace Restoran.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IPasswordService _passwords;
    private readonly ITokenService _tokens;
    private readonly ICurrentUser _current;

    public AuthService(IUserRepository users, IPasswordService passwords, ITokenService tokens, ICurrentUser current)
    {
        _users = users;
        _passwords = passwords;
        _tokens = tokens;
        _current = current;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        var email = dto.Email.Trim().ToLower();
        if (await _users.EmailExistsAsync(email))
            throw new ConflictException("Selle e-postiga kasutaja on juba olemas.");

        var user = new User
        {
            Name = dto.Name.Trim(),
            Email = email,
            Phone = dto.Phone?.Trim(),
            Role = UserRole.Customer
        };
        user.PasswordHash = _passwords.Hash(user, dto.Password);

        await _users.AddAsync(user);
        await _users.SaveChangesAsync();
        return CreateResponse(user);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _users.GetByEmailAsync(dto.Email.Trim().ToLower());
        if (user is null || !_passwords.Verify(user, dto.Password))
            throw new UnauthorizedException("Vale e-post või parool.");
        return CreateResponse(user);
    }

    public async Task<UserDto> GetMeAsync()
    {
        var id = _current.UserId ?? throw new UnauthorizedException("Pole sisse logitud.");
        var user = await _users.GetByIdAsync(id) ?? throw new NotFoundException("Kasutajat ei leitud.");
        return ToDto(user);
    }

    private AuthResponseDto CreateResponse(User user)
    {
        var (token, expires) = _tokens.CreateToken(user);
        return new AuthResponseDto(token, expires, ToDto(user));
    }

    private static UserDto ToDto(User u) => new(u.Id, u.Name, u.Email, u.Phone, u.Role);
}
