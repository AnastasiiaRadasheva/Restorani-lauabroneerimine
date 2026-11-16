using Restoran.Application.Common;
using Restoran.Application.DTOs;

namespace Restoran.Application.Interfaces;

public interface IRestaurantService
{
    /// <summary>Kõik restoranid; kui asukoht on antud, sorditakse kauguse järgi.</summary>
    Task<List<RestaurantDto>> GetAllAsync(string? search, double? lat, double? lng);
    Task<RestaurantDto> GetByIdAsync(int id);
    Task<RestaurantDto> CreateAsync(SaveRestaurantDto dto);
    Task<RestaurantDto> UpdateAsync(int id, SaveRestaurantDto dto);
    Task DeleteAsync(int id);
}

public interface ITableService
{
    Task<List<TableDto>> GetByRestaurantAsync(int restaurantId);
    Task<TableDto> GetByIdAsync(int id);
    Task<TableDto> CreateAsync(SaveTableDto dto);
    Task<TableDto> UpdateAsync(int id, SaveTableDto dto);
    Task DeleteAsync(int id);
}

public interface IMenuService
{
    Task<PagedResult<MenuItemDto>> GetPagedAsync(MenuQuery query);
    Task<MenuItemDto> GetByIdAsync(int id);
    Task<MenuItemDto> CreateAsync(SaveMenuItemDto dto);
    Task<MenuItemDto> UpdateAsync(int id, SaveMenuItemDto dto);
    Task DeleteAsync(int id);
}
