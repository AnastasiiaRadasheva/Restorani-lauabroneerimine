using Microsoft.EntityFrameworkCore;
using Restoran.Application.Entities;
using Restoran.Application.Interfaces;
using Restoran.Infrastructure.Data;

namespace Restoran.Infrastructure.Repositories;

public class EventRepository : Repository<GastroEvent>, IEventRepository
{
    public EventRepository(AppDbContext db) : base(db) { }

    public Task<List<GastroEvent>> GetUpcomingAsync(DateTime from) =>
        Db.Events
            .Include(e => e.Restaurant)
            .Include(e => e.Registrations)
            .Where(e => e.StartTime >= from)
            .OrderBy(e => e.StartTime)
            .ToListAsync();

    public Task<GastroEvent?> GetWithRegistrationsAsync(int id) =>
        Db.Events
            .Include(e => e.Restaurant)
            .Include(e => e.Registrations)
            .FirstOrDefaultAsync(e => e.Id == id);
}
