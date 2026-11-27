using System.ComponentModel.DataAnnotations;
using Restoran.Application.Common;
using Restoran.Application.DTOs;
using Restoran.Application.Entities;
using Restoran.Application.Interfaces;

namespace Restoran.Application.Services;

/// <summary>Uus klient, kelle administraator lisab telefoni või vestluse põhjal.</summary>
public class NewCustomerDto
{
    /// <summary>Kliendi keel (et/ru/en) – sõnumid saadetakse selles keeles.</summary>
    [RegularExpression("^(et|ru|en)$")]
    public string? Language { get; set; }

    /// <example>Mari Maasikas</example>
    [Required, StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = "";

    /// <example>mari@example.com</example>
    [Required, EmailAddress, StringLength(200)]
    public string Email { get; set; } = "";

    /// <example>+372 5895 1836</example>
    [RegularExpression(@"^[+\d\s-]{5,30}$", ErrorMessage = "Telefoninumber on vigane.")]
    public string? Phone { get; set; }
}

/// <summary>Personali poolt tehtud broneering: klient on kas olemasolev (CustomerId) või uus (NewCustomer).</summary>
public class StaffBookingDto : CreateReservationDto
{
    /// <summary>Kliendi kasutaja id (GET /api/users). Võib jätta tühjaks, kui on antud nimi.</summary>
    public int? CustomerId { get; set; }

    /// <summary>Kliendi nimi. Kui on antud ka id, peavad need klappima; ainult nimega leitakse klient nime järgi.</summary>
    public string? CustomerName { get; set; }

    public NewCustomerDto? NewCustomer { get; set; }
}

public interface ICustomerService
{
    Task<List<UserDto>> SearchAsync(string? search);
    Task<UserDto> GetOrCreateAsync(int? customerId, string? customerName, NewCustomerDto? dto);
    Task<UserDto> RenewPasswordLinkAsync(int customerId);
}

/// <summary>Klientide otsing ja lisamine personali jaoks.</summary>
public class CustomerService : ICustomerService
{
    private readonly IUserRepository _users;
    private readonly IPasswordService _passwords;

    public CustomerService(IUserRepository users, IPasswordService passwords)
    {
        _users = users;
        _passwords = passwords;
    }

    public async Task<List<UserDto>> SearchAsync(string? search) =>
        (await _users.SearchCustomersAsync(search ?? "", 20)).Select(ToDto).ToList();

    public async Task<UserDto> GetOrCreateAsync(int? customerId, string? customerName, NewCustomerDto? dto)
    {
        var name = customerName?.Trim();
        if (customerId is not null)
        {
            var existing = await _users.GetByIdAsync(customerId.Value) ?? throw new NotFoundException($"Kasutajat id-ga {customerId} ei leitud.");
            if (!string.IsNullOrEmpty(name) && !existing.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                throw new BusinessRuleException($"Kasutaja {customerId} on \"{existing.Name}\", mitte \"{name}\". Kontrollige id-d või nime.");
            return ToDto(existing);
        }
        if (!string.IsNullOrEmpty(name))
        {
            var found = await _users.SearchCustomersAsync(name, 20);
            var exact = found.Where(u => u.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).ToList();
            var pick = exact.Count == 1 ? exact[0] : found.Count == 1 ? found[0] : null;
            if (pick is not null) return ToDto(pick);
            throw found.Count == 0
                ? new NotFoundException($"Klienti nimega \"{name}\" ei leitud.")
                : new BusinessRuleException($"Nimi \"{name}\" sobib mitmele kliendile: {string.Join(", ", found.Select(u => $"{u.Name} (id {u.Id})"))}. Lisage customerId.");
        }
        if (dto is null) throw new BusinessRuleException("Valige klient või sisestage uue kliendi andmed.");

        var email = dto.Email.Trim().ToLowerInvariant();
        var same = await _users.GetByEmailAsync(email);
        if (same is not null) return ToDto(same); // sama e-postiga klient on juba olemas
        if (!string.IsNullOrWhiteSpace(dto.Phone) && await _users.GetByPhoneAsync(dto.Phone) is not null)
            throw new ConflictException("Selle telefoninumbriga klient on juba olemas – leidke ta otsingust.");

        var user = new User { Name = dto.Name.Trim(), Email = email, Phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim(), Role = UserRole.Customer, Language = L.Norm(dto.Language) };
        // Juhuslik parool, mida keegi ei tea: klient loob ise parooli lingi kaudu (saadetakse koos makselingiga)
        user.PasswordHash = _passwords.Hash(user, Guid.NewGuid().ToString("N"));
        user.PasswordSet = false;
        user.IssuePasswordToken(DateTime.Now);
        await _users.AddAsync(user);
        await _users.SaveChangesAsync();
        return ToDto(user);
    }

    private static UserDto ToDto(User u) => new(u.Id, u.Name, u.Email, u.Phone, u.Role, null, u.SmsOptIn, u.PasswordSet);

    /// <summary>Personal saadab kliendile uue parooli loomise lingi.</summary>
    public async Task<UserDto> RenewPasswordLinkAsync(int customerId)
    {
        var user = await _users.GetByIdAsync(customerId) ?? throw new NotFoundException("Klienti ei leitud.");
        if (user.PasswordSet) throw new BusinessRuleException("Klient on parooli juba ise loonud.");
        user.IssuePasswordToken(DateTime.Now);
        await _users.SaveChangesAsync();
        return ToDto(user);
    }
}
