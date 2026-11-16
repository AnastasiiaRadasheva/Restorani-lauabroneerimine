using Restoran.Application.Common;
using Restoran.Application.DTOs;
using Restoran.Application.Entities;
using Restoran.Application.Interfaces;
using Restoran.Application.Mapping;

namespace Restoran.Application.Services;

public class MenuService : IMenuService
{
    private readonly IMenuRepository _menu;
    private readonly IRestaurantRepository _restaurants;

    public MenuService(IMenuRepository menu, IRestaurantRepository restaurants)
    {
        _menu = menu;
        _restaurants = restaurants;
    }

    public async Task<PagedResult<MenuItemDto>> GetPagedAsync(MenuQuery query)
    {
        var page = await _menu.GetPagedAsync(query);
        return new PagedResult<MenuItemDto>
        {
            Items = page.Items.Select(m => m.ToDto()).ToList(),
            Page = page.Page,
            PageSize = page.PageSize,
            TotalCount = page.TotalCount
        };
    }

    public async Task<MenuItemDto> GetByIdAsync(int id) => (await Find(id)).ToDto();

    public async Task<MenuItemDto> CreateAsync(SaveMenuItemDto dto)
    {
        await EnsureRestaurant(dto.RestaurantId);
        var m = new MenuItem();
        m.Apply(dto);
        await _menu.AddAsync(m);
        await _menu.SaveChangesAsync();
        return m.ToDto();
    }

    public async Task<MenuItemDto> UpdateAsync(int id, SaveMenuItemDto dto)
    {
        var m = await Find(id);
        await EnsureRestaurant(dto.RestaurantId);
        m.Apply(dto);
        await _menu.SaveChangesAsync();
        return m.ToDto();
    }

    public async Task DeleteAsync(int id)
    {
        var m = await Find(id);
        if (await _menu.IsUsedInPreOrdersAsync(id))
            throw new ConflictException("Toit on eeltellimustes kasutusel. Märgi see mitte saadaval olevaks.");
        _menu.Remove(m);
        await _menu.SaveChangesAsync();
    }

    private async Task<MenuItem> Find(int id) =>
        await _menu.GetByIdAsync(id) ?? throw new NotFoundException($"Menüüpunkti {id} ei leitud.");

    private async Task EnsureRestaurant(int restaurantId)
    {
        _ = await _restaurants.GetByIdAsync(restaurantId)
            ?? throw new NotFoundException($"Restorani {restaurantId} ei leitud.");
    }
}
