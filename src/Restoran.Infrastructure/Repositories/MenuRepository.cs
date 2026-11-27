using Microsoft.EntityFrameworkCore;
using Restoran.Application.Common;
using Restoran.Application.DTOs;
using Restoran.Application.Entities;
using Restoran.Application.Interfaces;
using Restoran.Infrastructure.Data;

namespace Restoran.Infrastructure.Repositories;

public class MenuRepository : Repository<MenuItem>, IMenuRepository
{
    public MenuRepository(AppDbContext db) : base(db) { }

    public async Task<PagedResult<MenuItem>> GetPagedAsync(MenuQuery query)
    {
        var q = Db.MenuItems.AsQueryable();

        // Filtrid
        if (query.RestaurantId.HasValue) q = q.Where(m => m.RestaurantId == query.RestaurantId);
        if (!string.IsNullOrWhiteSpace(query.Category)) q = q.Where(m => m.Category == query.Category);
        if (query.OnlyAvailable == true) q = q.Where(m => m.IsAvailable);

        // Ilma allergeenita (sildid on kujul "milk,nuts")
        foreach (var w in (query.Without ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var tag = "," + w.ToLower() + ",";
            q = q.Where(m => m.Tags == null || !("," + m.Tags + ",").Contains(tag));
        }

        // Otsing
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim().ToLower();
            q = q.Where(m => m.Name.ToLower().Contains(s) || m.Description.ToLower().Contains(s));
        }

        // Sortimine (SQLite ei sordi decimal välja, seepärast hind -> double)
        q = (query.Sort?.ToLower(), query.Desc) switch
        {
            ("price", false) => q.OrderBy(m => (double)m.Price),
            ("price", true) => q.OrderByDescending(m => (double)m.Price),
            ("name", true) => q.OrderByDescending(m => m.Name),
            ("category", false) => q.OrderBy(m => m.Category).ThenBy(m => m.Name),
            ("category", true) => q.OrderByDescending(m => m.Category).ThenBy(m => m.Name),
            ("name", false) => q.OrderBy(m => m.Name),
            // Vaikimisi menüü järjekord: külmad road, supid, pearoad, mereannid, setid, magustoidud, veinid
            _ => q.OrderBy(m => m.Category == "Külmad road" ? 0 : m.Category == "Supid" ? 1 : m.Category == "Pearoad" ? 2
                              : m.Category == "Mereannid" ? 3 : m.Category == "Setid" ? 4 : m.Category == "Magustoidud" ? 5
                              : m.Category == "Veinid" ? 6 : 7).ThenBy(m => (double)m.Price)
        };

        var total = await q.CountAsync();
        var items = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync();
        return new PagedResult<MenuItem> { Items = items, Page = query.Page, PageSize = query.PageSize, TotalCount = total };
    }

    public Task<List<MenuItem>> GetByIdsAsync(IEnumerable<int> ids)
    {
        var list = ids.Distinct().ToList();
        return Db.MenuItems.Where(m => list.Contains(m.Id)).ToListAsync();
    }

    public Task<bool> IsUsedInPreOrdersAsync(int menuItemId) =>
        Db.PreOrderItems.AnyAsync(p => p.MenuItemId == menuItemId);
}
