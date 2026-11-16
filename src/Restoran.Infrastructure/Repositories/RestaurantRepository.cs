using Microsoft.EntityFrameworkCore;
using Restoran.Application.Entities;
using Restoran.Application.Interfaces;
using Restoran.Infrastructure.Data;

namespace Restoran.Infrastructure.Repositories;

public class RestaurantRepository : Repository<Restaurant>, IRestaurantRepository
{
    public RestaurantRepository(AppDbContext db) : base(db) { }

    public async Task<List<Restaurant>> GetAllAsync(string? search)
    {
        var q = Db.Restaurants.Include(r => r.Tables).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            q = q.Where(r => r.Name.ToLower().Contains(s) || r.Address.ToLower().Contains(s));
        }
        return await q.ToListAsync();
    }

    public Task<int> CountTablesAsync(int restaurantId) =>
        Db.Tables.CountAsync(t => t.RestaurantId == restaurantId && t.IsActive);
}
