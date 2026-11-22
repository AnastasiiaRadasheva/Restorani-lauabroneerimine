using Microsoft.EntityFrameworkCore;
using Restoran.Application.Services;
using Restoran.Infrastructure.Data;

namespace Restoran.Infrastructure.Repositories;

public class DayNoteRepository : Repository<DayNote>, IDayNoteRepository
{
    public DayNoteRepository(AppDbContext db) : base(db) { }

    /// <summary>Märked vahemikus; restorani valides ka kõigile restoranidele kehtivad märked.</summary>
    public Task<List<DayNote>> GetRangeAsync(DateOnly from, DateOnly to, int? restaurantId) =>
        Db.DayNotes
            .Where(n => n.Date >= from && n.Date <= to)
            .Where(n => restaurantId == null || n.RestaurantId == null || n.RestaurantId == restaurantId)
            .OrderBy(n => n.Date).ThenBy(n => n.Id)
            .ToListAsync();
}
