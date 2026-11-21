using Restoran.Application.Common;
using Restoran.Application.DTOs;
using Restoran.Application.Entities;
using Restoran.Application.Interfaces;
using Restoran.Application.Mapping;

namespace Restoran.Application.Services;

public class MenuService : IMenuService
{
    private readonly ICurrentUser _current;
    private readonly IMenuRepository _menu;
    private readonly IRestaurantRepository _restaurants;
    private readonly IReservationRepository _reservations;
    private readonly INotificationService _notifications;
    private readonly IClock _clock;

    public MenuService(IMenuRepository menu, IRestaurantRepository restaurants, IReservationRepository reservations,
        INotificationService notifications, IClock clock, ICurrentUser current)
    {
        _current = current;
        _reservations = reservations;
        _notifications = notifications;
        _clock = clock;
        _menu = menu;
        _restaurants = restaurants;
    }

    public async Task<PagedResult<MenuItemDto>> GetPagedAsync(MenuQuery query)
    {
        var page = await _menu.GetPagedAsync(query);
        return new PagedResult<MenuItemDto>
        {
            Items = page.Items.Select(m => query.Raw ? m.ToDto() : Localize(m.ToDto())).ToList(),
            Page = page.Page,
            PageSize = page.PageSize,
            TotalCount = page.TotalCount
        };
    }

    public async Task<MenuItemDto> GetByIdAsync(int id) => Localize((await Find(id)).ToDto());

    /// <summary>Näidismenüü nimi, kirjeldus ja kategooria saidi keeles (admini lisatud tekst jääb muutmata).</summary>
    private MenuItemDto Localize(MenuItemDto d) =>
        d with { Name = L.Word(d.Name, _current.Language), Description = L.Word(d.Description, _current.Language), Category = L.Word(d.Category, _current.Language) };

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
        bool becameUnavailable = m.IsAvailable && !dto.IsAvailable;
        m.Apply(dto);
        await _menu.SaveChangesAsync();

        // Kui eeltellitud roog ei ole enam saadaval, saavad külalised vabanduse ja kingituse
        if (becameUnavailable)
        {
            foreach (var r in await _reservations.GetActiveWithMenuItemAsync(m.Id, _clock.Now))
            {
                await _notifications.CreateAsync(new CreateNotificationDto
                {
                    UserId = r.UserId,
                    ReservationId = r.Id,
                    Message = L.T(r.User?.Language, "dish.unavailable", L.Word(m.Name, r.User?.Language), r.StartTime.ToString("dd.MM"))
                });
            }
        }
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
