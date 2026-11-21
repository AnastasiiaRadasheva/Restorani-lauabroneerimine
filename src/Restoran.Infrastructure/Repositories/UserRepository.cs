using Microsoft.EntityFrameworkCore;
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
