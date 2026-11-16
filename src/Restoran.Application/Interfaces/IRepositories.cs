using Restoran.Application.Common;
using Restoran.Application.DTOs;
using Restoran.Application.Entities;

namespace Restoran.Application.Interfaces;

/// <summary>Üldine repositoorium.</summary>
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task AddAsync(T entity);
    void Remove(T entity);
    Task SaveChangesAsync();
}

public interface IRestaurantRepository : IRepository<Restaurant>
{
    Task<List<Restaurant>> GetAllAsync(string? search);
    Task<int> CountTablesAsync(int restaurantId);
}

public interface ITableRepository : IRepository<DiningTable>
{
    Task<List<DiningTable>> GetByRestaurantAsync(int restaurantId, bool onlyActive);
    Task<bool> NumberExistsAsync(int restaurantId, int number, int? exceptId);
    Task<bool> HasFutureReservationsAsync(int tableId);
}

public interface IMenuRepository : IRepository<MenuItem>
{
    Task<PagedResult<MenuItem>> GetPagedAsync(MenuQuery query);
    Task<List<MenuItem>> GetByIdsAsync(IEnumerable<int> ids);
    Task<bool> IsUsedInPreOrdersAsync(int menuItemId);
}
