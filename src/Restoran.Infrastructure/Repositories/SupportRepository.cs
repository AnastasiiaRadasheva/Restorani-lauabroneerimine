using Microsoft.EntityFrameworkCore;
using Restoran.Application.Services;
using Restoran.Infrastructure.Data;

namespace Restoran.Infrastructure.Repositories;

public class SupportRepository : Repository<SupportMessage>, ISupportRepository
{
    public SupportRepository(AppDbContext db) : base(db) { }

    public Task<List<SupportMessage>> GetThreadAsync(int userId) =>
        Db.SupportMessages.Where(m => m.UserId == userId).OrderBy(m => m.CreatedAt).ToListAsync();

    public Task<List<SupportMessage>> GetAllWithUsersAsync() =>
        Db.SupportMessages.Include(m => m.User).ToListAsync();
}
