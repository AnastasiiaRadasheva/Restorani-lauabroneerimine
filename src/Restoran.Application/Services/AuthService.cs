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
    private readonly IBookingMessenger _messenger;

    public AuthService(IUserRepository users, IPasswordService passwords, ITokenService tokens, ICurrentUser current, IBookingMessenger messenger)
    {
        _messenger = messenger;
        _users = users;
        _passwords = passwords;
        _tokens = tokens;
        _current = current;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        var email = dto.Email.Trim().ToLower();
        var existing = await _users.GetByEmailAsync(email);
        if (existing is not null)
        {
            // Konto lõi personal (broneering telefoni teel) – saadame lingi parooli loomiseks, broneeringud jäävad alles
            if (!existing.PasswordSet)
            {
                if (!existing.HasValidPasswordToken(DateTime.Now)) existing.IssuePasswordToken(DateTime.Now);
                await _users.SaveChangesAsync();
                await _messenger.SendPasswordLinkAsync(existing.Id);
                throw new ConflictException("Selle e-postiga on meil juba konto. Saatsime Teie e-postile lingi parooli loomiseks.");
            }
            throw new ConflictException("Selle e-postiga kasutaja on juba olemas.");
        }
        // Üks inimene – üks konto: sama telefoninumbriga teist kontot luua ei saa
        if (!string.IsNullOrWhiteSpace(dto.Phone) && await _users.GetByPhoneAsync(dto.Phone) is not null)
            throw new ConflictException("Selle telefoninumbriga konto on juba olemas. Logige sisse oma kontoga.");

        var user = new User
        {
            Name = dto.Name.Trim(),
            Email = email,
            Phone = dto.Phone?.Trim(),
            Role = UserRole.Customer,
            Language = L.Norm(_current.Language)
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
        await RememberLanguage(user);
        return CreateResponse(user);
    }

    public async Task<PasswordLinkDto> GetPasswordLinkAsync(string token)
    {
        var user = await FindByPasswordToken(token);
        return new PasswordLinkDto(user.Name, user.Email);
    }

    /// <summary>Klient loob lingi kaudu parooli ja on kohe sisse logitud.</summary>
    public async Task<AuthResponseDto> SetPasswordAsync(SetPasswordDto dto)
    {
        var user = await FindByPasswordToken(dto.Token);
        user.PasswordHash = _passwords.Hash(user, dto.Password);
        user.PasswordSet = true;
        user.Language = L.Norm(_current.Language);
        user.PasswordToken = null;
        user.PasswordTokenExpires = null;
        await _users.SaveChangesAsync();
        return CreateResponse(user);
    }

    private async Task<User> FindByPasswordToken(string token)
    {
        var user = await _users.GetByPasswordTokenAsync(token);
        if (user is null || user.PasswordSet || user.PasswordTokenExpires <= DateTime.Now)
            throw new NotFoundException("Link parooli loomiseks ei kehti enam. Küsige uus link restoranist.");
        return user;
    }

    public async Task<UserDto> GetMeAsync()
    {
        var id = _current.UserId ?? throw new UnauthorizedException("Pole sisse logitud.");
        var user = await _users.GetByIdAsync(id) ?? throw new NotFoundException("Kasutajat ei leitud.");
        await RememberLanguage(user);
        return ToDto(user);
    }

    /// <summary>Kasutaja saidi keel salvestatakse – teated ja e-kirjad tulevad selles keeles.</summary>
    private async Task RememberLanguage(User user)
    {
        var lang = L.Norm(_current.Language);
        if (user.Language == lang) return;
        user.Language = lang;
        await _users.SaveChangesAsync();
    }

    public async Task<UserDto> UpdateMeAsync(UpdateProfileDto dto)
    {
        var id = _current.UserId ?? throw new UnauthorizedException("Pole sisse logitud.");
        var user = await _users.GetByIdAsync(id) ?? throw new NotFoundException("Kasutajat ei leitud.");
        if (!string.IsNullOrWhiteSpace(dto.Phone) && !PhoneNumber.Same(dto.Phone, user.Phone)
            && await _users.GetByPhoneAsync(dto.Phone, user.Id) is not null)
            throw new ConflictException("See telefoninumber on juba teise konto juures.");
        var email = dto.Email?.Trim().ToLower();
        if (!string.IsNullOrEmpty(email) && email != user.Email)
        {
            if (await _users.EmailExistsAsync(email))
                throw new ConflictException("See e-post on juba teise konto juures.");
            user.Email = email;
        }
        user.Name = dto.Name.Trim();
        user.Phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim();
        user.SmsOptIn = dto.SmsOptIn;
        if (dto.AvatarUrl == "-") user.AvatarUrl = null;
        else if (!string.IsNullOrEmpty(dto.AvatarUrl))
        {
            if (!dto.AvatarUrl.StartsWith("data:image/") || dto.AvatarUrl.Length > 400_000)
                throw new BusinessRuleException("Pilt peab olema väike (kuni 300 KB) JPEG või PNG.");
            user.AvatarUrl = dto.AvatarUrl;
        }
        await _users.SaveChangesAsync();
        return ToDto(user);
    }

    private AuthResponseDto CreateResponse(User user)
    {
        var (token, expires) = _tokens.CreateToken(user);
        return new AuthResponseDto(token, expires, ToDto(user));
    }

    private static UserDto ToDto(User u) => new(u.Id, u.Name, u.Email, u.Phone, u.Role, u.AvatarUrl, u.SmsOptIn, u.PasswordSet);
}
