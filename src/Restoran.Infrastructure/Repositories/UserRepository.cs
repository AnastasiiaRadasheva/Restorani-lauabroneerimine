using Microsoft.EntityFrameworkCore;
using Restoran.Application.Common;
using Restoran.Application.Entities;
using Restoran.Application.Interfaces;
using Restoran.Infrastructure.Data;

namespace Restoran.Infrastructure.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(AppDbContext db) : base(db) { }

    public Task<User?> GetByPasswordTokenAsync(string token) =>
        Db.Users.FirstOrDefaultAsync(u => u.PasswordToken == token);

    public Task<User?> GetByEmailAsync(string email) =>
        Db.Users.FirstOrDefaultAsync(u => u.Email == email);

    public Task<bool> EmailExistsAsync(string email) =>
        Db.Users.AnyAsync(u => u.Email == email);

    public async Task<User?> GetByPhoneAsync(string phone, int? exceptUserId = null)
    {
        if (PhoneNumber.Digits(phone).Length == 0) return null;
        // Numbreid hoitakse eri kujul (+372 5555 1234, 55551234), seega võrdleme mälus
        var withPhone = await Db.Users.Where(u => u.Phone != null && (exceptUserId == null || u.Id != exceptUserId)).ToListAsync();
        return withPhone.FirstOrDefault(u => PhoneNumber.Same(u.Phone, phone));
    }

    public Task<List<User>> SearchCustomersAsync(string search, int take)
    {
        var s = search.Trim().ToLower();
        return Db.Users
            .Where(u => u.Role == UserRole.Customer &&
                        (s == "" || u.Name.ToLower().Contains(s) || u.Email.ToLower().Contains(s) || (u.Phone != null && u.Phone.Contains(s))))
            .OrderBy(u => u.Name)
            .Take(take)
            .ToListAsync();
    }

    public Task<List<User>> GetStaffAsync() =>
        Db.Users.Where(u => u.Role == UserRole.Admin || u.Role == UserRole.Waiter).ToListAsync();
}
