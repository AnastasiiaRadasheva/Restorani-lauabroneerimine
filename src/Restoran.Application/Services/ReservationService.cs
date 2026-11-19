using Microsoft.Extensions.Logging;
using Restoran.Application.Common;
using Restoran.Application.DTOs;
using Restoran.Application.Entities;
using Restoran.Application.Interfaces;
using Restoran.Application.Mapping;

namespace Restoran.Application.Services;

/// <summary>Broneerimise äriloogika.</summary>
public class ReservationService : IReservationService
{
    private readonly IReservationRepository _reservations;
    private readonly ITableRepository _tables;
    private readonly IRestaurantRepository _restaurants;
    private readonly IMenuRepository _menu;
    private readonly ICurrentUser _current;
    private readonly IClock _clock;
    private readonly ILogger<ReservationService> _logger;

    public ReservationService(
        IReservationRepository reservations,
        ITableRepository tables,
        IRestaurantRepository restaurants,
        IMenuRepository menu,
        ICurrentUser current,
        IClock clock,
        ILogger<ReservationService> logger)
    {
        _reservations = reservations;
        _tables = tables;
        _restaurants = restaurants;
        _menu = menu;
        _current = current;
        _clock = clock;
        _logger = logger;
    }

    // ---------- Vabad lauad ----------

    public async Task<AvailabilityDto> GetAvailabilityAsync(int restaurantId, DateOnly date, TimeOnly time, int guests)
    {
        var restaurant = await GetRestaurant(restaurantId);
        var (start, end) = ValidateTime(restaurant, date, time, guests);

        var tables = await _tables.GetByRestaurantAsync(restaurantId, onlyActive: true);
        var busy = await GetBusyTableIds(tables, start, end, null);
        var best = BookingRules.PickBestTable(tables, busy, guests);

        var list = tables.Select(t => new TableAvailabilityDto(
            t.Id, t.Number, t.Seats, t.Location, t.X, t.Y, t.Width, t.Height, t.Spot, t.IsRound,
            IsFree: !busy.Contains(t.Id),
            FitsGuests: t.Seats >= guests)).ToList();

        return new AvailabilityDto(restaurantId, start, end, guests, best?.Id, list);
    }

    /// <summary>
    /// Kalendri jaoks: iga päeva iga kellaaja kohta, mitu sobiva suurusega lauda on vaba.
    /// Kui vabu laudu on 0, näidatakse kellaaeg veebis läbikriipsutatuna.
    /// </summary>
    public async Task<List<DaySlotsDto>> GetSlotsAsync(int restaurantId, DateOnly from, int days, int guests)
    {
        if (days < 1 || days > 60)
            throw new BusinessRuleException("Päevade arv peab olema 1 kuni 60.");
        if (guests < 1 || guests > 20)
            throw new BusinessRuleException("Inimeste arv peab olema 1 kuni 20.");

        var restaurant = await GetRestaurant(restaurantId);
        var tables = (await _tables.GetByRestaurantAsync(restaurantId, onlyActive: true))
            .Where(t => t.Seats >= guests).ToList();
        var rangeStart = from.ToDateTime(TimeOnly.MinValue);
        var occupying = (await _reservations.GetOccupyingAsync(
                tables.Select(t => t.Id), rangeStart, rangeStart.AddDays(days), null))
            .Where(r => BookingRules.Occupies(r.Status))
            .ToList();

        var result = new List<DaySlotsDto>();
        for (int d = 0; d < days; d++)
        {
            var date = from.AddDays(d);
            var slots = new List<SlotDto>();
            // Kellaajad 30-minutilise sammuga avamisest kuni 2 h enne sulgemist
            var open = (int)restaurant.OpensAt.ToTimeSpan().TotalMinutes;
            var lastStart = (int)restaurant.ClosesAt.ToTimeSpan().TotalMinutes - (int)BookingRules.Duration.TotalMinutes;
            for (int minutes = open; minutes <= lastStart; minutes += 30)
            {
                var time = TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(minutes));
                var start = date.ToDateTime(time);
                var end = start + BookingRules.Duration;
                int free = start <= _clock.Now ? 0 : tables.Count(t => !occupying.Any(r =>
                    r.TableId == t.Id && BookingRules.Overlaps(start, end, r.StartTime, r.EndTime)));
                slots.Add(new SlotDto(time.ToString("HH:mm"), free));
            }
            result.Add(new DaySlotsDto(date, slots));
        }
        return result;
    }

    // ---------- Broneerimine ----------

    public async Task<ReservationDto> CreateAsync(CreateReservationDto dto)
    {
        var userId = RequireUser();
        var restaurant = await GetRestaurant(dto.RestaurantId);
        var (start, end) = ValidateTime(restaurant, dto.Date, dto.Time, dto.Guests);
        var table = await ChooseTable(dto.RestaurantId, dto.TableId, dto.Guests, start, end, null);

        var reservation = new Reservation
        {
            TableId = table.Id,
            UserId = userId,
            StartTime = start,
            EndTime = end,
            Guests = dto.Guests,
            SpecialRequest = Clean(dto.SpecialRequest),
            Status = ReservationStatus.Active,
            CreatedAt = _clock.Now
        };
        // Eeltellimus võib tulla kohe broneeringuga koos (veebis III samm "Gastronoomia")
        if (dto.Items is { Count: > 0 })
            await ApplyPreOrderAsync(reservation, dto.RestaurantId, dto.Items);
        await _reservations.AddAsync(reservation);
        await _reservations.SaveChangesAsync();
        _logger.LogInformation("Uus broneering {Id}: restoran {Restaurant}, laud {Table}, {Start}, {Guests} inimest",
            reservation.Id, restaurant.Name, table.Number, start, dto.Guests);

        return await GetByIdAsync(reservation.Id);
    }

    public async Task<ReservationDto> UpdateAsync(int id, UpdateReservationDto dto)
    {
        var r = await GetOwned(id);
        EnsureCustomerCanModify(r);

        var restaurantId = r.Table!.RestaurantId;
        var restaurant = await GetRestaurant(restaurantId);
        var (start, end) = ValidateTime(restaurant, dto.Date, dto.Time, dto.Guests);
        var table = await ChooseTable(restaurantId, dto.TableId ?? r.TableId, dto.Guests, start, end, r.Id, allowFallback: dto.TableId is null);

        r.TableId = table.Id;
        r.StartTime = start;
        r.EndTime = end;
        r.Guests = dto.Guests;
        r.SpecialRequest = Clean(dto.SpecialRequest);
        await _reservations.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task<ReservationDto> CancelAsync(int id)
    {
        var r = await GetOwned(id);
        if (r.Status != ReservationStatus.Active)
            throw new BusinessRuleException("Tühistada saab ainult aktiivset broneeringut.");
        EnsureCustomerCanModify(r);

        r.Status = ReservationStatus.Cancelled;
        await _reservations.SaveChangesAsync();
        _logger.LogInformation("Broneering {Id} tühistati", r.Id);
        return ToDto(r);
    }

    // ---------- Eeltellimus ja makse ----------

    public async Task<ReservationDto> SetPreOrderAsync(int id, PreOrderDto dto)
    {
        var r = await GetOwned(id);
        if (r.Status != ReservationStatus.Active)
            throw new BusinessRuleException("Eeltellimust saab lisada ainult aktiivsele broneeringule.");
        EnsureCustomerCanModify(r);
        if (r.IsPaid)
            throw new BusinessRuleException("Makstud eeltellimust ei saa muuta.");

        await ApplyPreOrderAsync(r, r.Table!.RestaurantId, dto.Items);
        if (dto.SpecialRequest is not null)
            r.SpecialRequest = Clean(dto.SpecialRequest);

        await _reservations.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<ReservationDto> PayAsync(int id, FakePaymentDto dto)
    {
        var r = await GetOwned(id);
        if (r.Status != ReservationStatus.Active)
            throw new BusinessRuleException("Maksta saab ainult aktiivse broneeringu eest.");
        if (r.IsPaid)
            throw new BusinessRuleException("Broneering on juba makstud.");
        if (r.PreOrderItems.Count == 0)
            throw new BusinessRuleException("Eeltellimus on tühi, pole midagi maksta.");

        // Võltsmakse: kontrollime ainult vormingut, kaardiandmeid ei salvestata.
        var digits = new string(dto.CardNumber.Where(char.IsDigit).ToArray());
        if (digits.Length < 12)
            throw new BusinessRuleException("Kaardi number on vigane.");

        r.IsPaid = true;
        await _reservations.SaveChangesAsync();
        _logger.LogInformation("Broneering {Id} makstud (võltsmakse), summa {Total}", r.Id, r.PreOrderItems.Sum(p => p.UnitPrice * p.Quantity));
        return ToDto(r);
    }

    // ---------- Vaatamine ----------

    public async Task<ReservationDto> GetByIdAsync(int id) => ToDto(await GetOwned(id));

    public async Task<List<ReservationDto>> GetMyAsync()
    {
        var userId = RequireUser();
        var list = await _reservations.GetByUserAsync(userId);
        return list.Select(ToDto).ToList();
    }

    public async Task<PagedResult<ReservationDto>> GetPagedAsync(ReservationQuery query)
    {
        var page = await _reservations.GetPagedAsync(query);
        return new PagedResult<ReservationDto>
        {
            Items = page.Items.Select(ToDto).ToList(),
            Page = page.Page,
            PageSize = page.PageSize,
            TotalCount = page.TotalCount
        };
    }

    // ---------- Personal ----------

    public async Task<ReservationDto> ChangeStatusAsync(int id, ReservationStatus status)
    {
        var r = await _reservations.GetWithDetailsAsync(id) ?? throw NotFound(id);
        r.Status = status;
        await _reservations.SaveChangesAsync();
        _logger.LogInformation("Broneeringu {Id} uus olek: {Status}", r.Id, status);
        return ToDto(r);
    }

    public async Task<TablePlanDto> GetPlanAsync(int restaurantId, DateOnly date)
    {
        var restaurant = await GetRestaurant(restaurantId);
        var tables = await _tables.GetByRestaurantAsync(restaurantId, onlyActive: true);
        var reservations = await _reservations.GetForDayAsync(restaurantId, date);

        var planTables = tables.Select(t => new PlanTableDto(
            t.ToDto(),
            reservations.Where(r => r.TableId == t.Id)
                .OrderBy(r => r.StartTime)
                .Select(r => new PlanReservationDto(
                    r.Id, r.StartTime, r.EndTime, r.Guests, r.Status,
                    r.User?.Name ?? "", r.PreOrderItems.Count > 0))
                .ToList())).ToList();

        return new TablePlanDto(restaurant.ToDto(tables.Count), date, planTables);
    }

    // ---------- Abimeetodid ----------

    /// <summary>Kontrollib menüüpunktid ja asendab broneeringu eeltellimuse.</summary>
    private async Task ApplyPreOrderAsync(Reservation r, int restaurantId, List<PreOrderLineDto> items)
    {
        var lines = items.Where(i => i.Quantity > 0)
            .GroupBy(i => i.MenuItemId)
            .Select(g => new { MenuItemId = g.Key, Quantity = g.Sum(x => x.Quantity) })
            .ToList();

        var menuItems = await _menu.GetByIdsAsync(lines.Select(l => l.MenuItemId));
        foreach (var line in lines)
        {
            var item = menuItems.FirstOrDefault(m => m.Id == line.MenuItemId)
                       ?? throw new NotFoundException($"Menüüpunkti {line.MenuItemId} ei leitud.");
            if (item.RestaurantId != restaurantId)
                throw new BusinessRuleException($"\"{item.Name}\" ei ole selle restorani menüüs.");
            if (!item.IsAvailable)
                throw new BusinessRuleException($"\"{item.Name}\" ei ole praegu saadaval.");
        }

        r.PreOrderItems.Clear();
        foreach (var line in lines)
        {
            var item = menuItems.First(m => m.Id == line.MenuItemId);
            r.PreOrderItems.Add(new PreOrderItem
            {
                MenuItemId = item.Id,
                MenuItem = item,
                Quantity = line.Quantity,
                UnitPrice = item.Price
            });
        }
    }

    private (DateTime Start, DateTime End) ValidateTime(Restaurant restaurant, DateOnly date, TimeOnly time, int guests)
    {
        if (guests < 1 || guests > 20)
            throw new BusinessRuleException("Inimeste arv peab olema 1 kuni 20.");

        var start = date.ToDateTime(time);
        var end = start + BookingRules.Duration;

        if (start <= _clock.Now)
            throw new BusinessRuleException("Broneeringu aeg ei saa olla minevikus.");
        if (date > DateOnly.FromDateTime(_clock.Now).AddDays(90))
            throw new BusinessRuleException("Broneerida saab kuni 90 päeva ette.");
        if (time < restaurant.OpensAt || TimeOnly.FromDateTime(end) > restaurant.ClosesAt || end.Date != start.Date)
            throw new BusinessRuleException(
                $"Restoran on avatud {restaurant.OpensAt:HH\\:mm}-{restaurant.ClosesAt:HH\\:mm}. Broneering kestab 2 tundi.");

        return (start, end);
    }

    /// <summary>Valib laua: kliendi valitud (kui vaba) või väikseima sobiva vaba laua.</summary>
    private async Task<DiningTable> ChooseTable(int restaurantId, int? tableId, int guests,
        DateTime start, DateTime end, int? excludeReservationId, bool allowFallback = false)
    {
        var tables = await _tables.GetByRestaurantAsync(restaurantId, onlyActive: true);
        var busy = await GetBusyTableIds(tables, start, end, excludeReservationId);

        if (tableId is not null)
        {
            var chosen = tables.FirstOrDefault(t => t.Id == tableId)
                         ?? throw new NotFoundException($"Lauda {tableId} selles restoranis ei leitud.");
            if (chosen.Seats < guests && !allowFallback)
                throw new BusinessRuleException($"Laud {chosen.Number} on {chosen.Seats} kohaga, see on {guests} inimesele liiga väike.");
            if (chosen.Seats >= guests && !busy.Contains(chosen.Id))
                return chosen;
            if (!allowFallback)
                throw new ConflictException($"Laud {chosen.Number} on sel ajal juba broneeritud.");
        }

        return BookingRules.PickBestTable(tables, busy, guests)
               ?? throw new ConflictException("Sellel ajal pole sobiva suurusega vaba lauda. Proovi muud aega.");
    }

    private async Task<HashSet<int>> GetBusyTableIds(List<DiningTable> tables, DateTime start, DateTime end, int? excludeId)
    {
        var occupying = await _reservations.GetOccupyingAsync(tables.Select(t => t.Id), start, end, excludeId);
        return occupying
            .Where(r => BookingRules.Occupies(r.Status) && BookingRules.Overlaps(start, end, r.StartTime, r.EndTime))
            .Select(r => r.TableId)
            .ToHashSet();
    }

    /// <summary>Broneering, mida praegune kasutaja tohib vaadata (oma või personal).</summary>
    private async Task<Reservation> GetOwned(int id)
    {
        var userId = RequireUser();
        var r = await _reservations.GetWithDetailsAsync(id) ?? throw NotFound(id);
        if (r.UserId != userId && !_current.IsStaff)
            throw new ForbiddenException("See ei ole sinu broneering.");
        return r;
    }

    private void EnsureCustomerCanModify(Reservation r)
    {
        if (_current.IsStaff) return; // personal võib alati muuta
        if (!BookingRules.CanCustomerModify(r.StartTime, _clock.Now))
            throw new BusinessRuleException("Vähem kui 24 tundi enne broneeringut ei saa seda enam muuta ega tühistada. Helista restorani.");
    }

    private async Task<Restaurant> GetRestaurant(int id) =>
        await _restaurants.GetByIdAsync(id) ?? throw new NotFoundException($"Restorani {id} ei leitud.");

    private int RequireUser() => _current.UserId ?? throw new UnauthorizedException("Pole sisse logitud.");

    private static NotFoundException NotFound(int id) => new($"Broneeringut {id} ei leitud.");

    private static string? Clean(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    private ReservationDto ToDto(Reservation r)
    {
        var items = r.PreOrderItems.Select(p => new PreOrderItemDto(
            p.MenuItemId, p.MenuItem?.Name ?? "", p.Quantity, p.UnitPrice, p.UnitPrice * p.Quantity)).ToList();

        return new ReservationDto(
            r.Id,
            r.Table?.RestaurantId ?? 0,
            r.Table?.Restaurant?.Name ?? "",
            r.Table?.Restaurant?.Address ?? "",
            r.TableId,
            r.Table?.Number ?? 0,
            r.Table?.Location ?? TableLocation.Hall,
            r.UserId,
            r.User?.Name ?? "",
            r.User?.Phone,
            r.StartTime,
            r.EndTime,
            r.Guests,
            r.Status,
            r.SpecialRequest,
            r.IsPaid,
            items.Sum(i => i.Total),
            items,
            r.Status == ReservationStatus.Active && BookingRules.CanCustomerModify(r.StartTime, _clock.Now),
            r.CreatedAt);
    }
}
