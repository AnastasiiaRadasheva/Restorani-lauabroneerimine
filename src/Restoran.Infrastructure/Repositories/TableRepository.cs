using Microsoft.EntityFrameworkCore;
using Restoran.Application.Entities;
using Restoran.Application.Interfaces;
using Restoran.Infrastructure.Data;

namespace Restoran.Infrastructure.Repositories;

public class TableRepository : Repository<DiningTable>, ITableRepository
{
    public TableRepository(AppDbContext db) : base(db) { }

    public async Task<List<DiningTable>> GetByRestaurantAsync(int restaurantId, bool onlyActive)
    {
        var q = Db.Tables.Where(t => t.RestaurantId == restaurantId);
        if (onlyActive) q = q.Where(t => t.IsActive);
        return await q.OrderBy(t => t.Number).ToListAsync();
    }

    public Task<bool> NumberExistsAsync(int restaurantId, int number, int? exceptId) =>
        Db.Tables.AnyAsync(t => t.RestaurantId == restaurantId && t.Number == number && t.Id != exceptId);

    public Task<bool> HasFutureReservationsAsync(int tableId) =>
        Db.Reservations.AnyAsync(r => r.TableId == tableId
                                      && r.StartTime > DateTime.Now
                                      && r.Status == ReservationStatus.Active);
}
