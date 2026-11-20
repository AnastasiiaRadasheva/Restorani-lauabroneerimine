using Microsoft.EntityFrameworkCore;
using Restoran.Application.Common;
using Restoran.Application.DTOs;
using Restoran.Application.Entities;
using Restoran.Application.Interfaces;
using Restoran.Infrastructure.Data;

namespace Restoran.Infrastructure.Repositories;

public class ReservationRepository : Repository<Reservation>, IReservationRepository
{
    public ReservationRepository(AppDbContext db) : base(db) { }

    private IQueryable<Reservation> WithDetails() => Db.Reservations
        .Include(r => r.Table).ThenInclude(t => t!.Restaurant)
        .Include(r => r.User)
        .Include(r => r.PreOrderItems).ThenInclude(p => p.MenuItem)
        .Include(r => r.Extras);

    public Task<Reservation?> GetWithDetailsAsync(int id) =>
        WithDetails().FirstOrDefaultAsync(r => r.Id == id);

    public Task<List<Reservation>> GetOccupyingAsync(IEnumerable<int> tableIds, DateTime start, DateTime end, int? excludeId = null)
    {
        var ids = tableIds.ToList();
        return Db.Reservations
            .Where(r => ids.Contains(r.TableId)
                        && r.Id != excludeId
                        && (r.Status == ReservationStatus.Active || r.Status == ReservationStatus.Arrived)
                        && r.StartTime < end && r.EndTime > start) // ajavahemikud kattuvad
            .ToListAsync();
    }

    public Task<List<Reservation>> GetByUserAsync(int userId) =>
        WithDetails().Where(r => r.UserId == userId)
            .OrderByDescending(r => r.StartTime)
            .ToListAsync();

    public async Task<PagedResult<Reservation>> GetPagedAsync(ReservationQuery query)
    {
        var q = WithDetails().AsQueryable();

        if (query.RestaurantId.HasValue) q = q.Where(r => r.Table!.RestaurantId == query.RestaurantId);
        if (query.Status.HasValue) q = q.Where(r => r.Status == query.Status);
        if (query.Date.HasValue)
        {
            var from = query.Date.Value.ToDateTime(TimeOnly.MinValue);
            var to = from.AddDays(1);
            q = q.Where(r => r.StartTime >= from && r.StartTime < to);
        }
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim().ToLower();
            q = q.Where(r => r.User!.Name.ToLower().Contains(s)
                             || r.User.Email.ToLower().Contains(s)
                             || (r.User.Phone != null && r.User.Phone.Contains(s)));
        }

        q = (query.Sort?.ToLower(), query.Desc) switch
        {
            ("guests", false) => q.OrderBy(r => r.Guests),
            ("guests", true) => q.OrderByDescending(r => r.Guests),
            ("name", false) => q.OrderBy(r => r.User!.Name),
            ("name", true) => q.OrderByDescending(r => r.User!.Name),
            ("table", false) => q.OrderBy(r => r.Table!.Number),
            ("table", true) => q.OrderByDescending(r => r.Table!.Number),
            (_, true) => q.OrderByDescending(r => r.StartTime),
            _ => q.OrderBy(r => r.StartTime)
        };

        var total = await q.CountAsync();
        var items = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync();
        return new PagedResult<Reservation> { Items = items, Page = query.Page, PageSize = query.PageSize, TotalCount = total };
    }

    public Task<List<Reservation>> GetForStatsAsync(int? restaurantId, DateOnly from, DateOnly to)
    {
        var start = from.ToDateTime(TimeOnly.MinValue);
        var end = to.ToDateTime(TimeOnly.MinValue).AddDays(1);
        var q = Db.Reservations.AsNoTracking().Where(r => r.StartTime >= start && r.StartTime < end);
        if (restaurantId.HasValue) q = q.Where(r => r.Table!.RestaurantId == restaurantId);
        return q.ToListAsync();
    }

    public Task<List<Reservation>> GetForDayAsync(int restaurantId, DateOnly date)
    {
        var from = date.ToDateTime(TimeOnly.MinValue);
        var to = from.AddDays(1);
        return Db.Reservations
            .Include(r => r.User)
            .Include(r => r.PreOrderItems)
            .Where(r => r.Table!.RestaurantId == restaurantId && r.StartTime >= from && r.StartTime < to)
            .ToListAsync();
    }
}
