using System.Text.Json;
using Microsoft.Extensions.Logging;
using Restoran.Application.Common;
using Restoran.Application.DTOs;
using Restoran.Application.Entities;
using Restoran.Application.Interfaces;

namespace Restoran.Application.Services;

/// <summary>
/// Gastronoomilised õhtud: registreerimine (laud, eripakkumised, lisateenused), makse ja tühistamine,
/// admini jaoks õhtute loomine ja muutmine.
/// Reeglid:
///  - registreeruda saab kuni 12 tundi enne õhtu algust;
///  - ise registreerudes makstakse kohe; personali tehtud registreerimine ootab makset (makselink);
///  - maksmata registreerimine tühistatakse 24 tundi enne õhtut;
///  - tühistamisel rohkem kui 24 h enne tagastatakse makstud summa;
///  - admin saab uue õhtu panna kõige varem 2 nädala pärast.
/// </summary>
public class EventService : IEventService
{
    public static readonly TimeSpan JoinDeadline = TimeSpan.FromHours(12);
    public static readonly TimeSpan PayDeadline = TimeSpan.FromHours(24);
    public const int MinDaysAhead = 14;

    private readonly IEventRepository _events;
    private readonly ITableRepository _tables;
    private readonly IReservationRepository _reservations;
    private readonly IRestaurantRepository _restaurants;
    private readonly IMenuRepository _menu;
    private readonly ICurrentUser _current;
    private readonly IClock _clock;
    private readonly ILogger<EventService> _logger;

    public EventService(IEventRepository events, ITableRepository tables, IReservationRepository reservations,
        IRestaurantRepository restaurants, IMenuRepository menu, ICurrentUser current, IClock clock, ILogger<EventService> logger)
    {
        _events = events;
        _tables = tables;
        _reservations = reservations;
        _restaurants = restaurants;
        _menu = menu;
        _current = current;
        _clock = clock;
        _logger = logger;
    }

    // ---------- Vaatamine ----------

    public async Task<List<GastroEventDto>> GetUpcomingAsync() =>
        (await _events.GetUpcomingAsync(_clock.Now)).Select(ToDto).ToList();

    public async Task<List<GastroEventDto>> GetAllForStaffAsync() =>
        (await _events.GetUpcomingAsync(_clock.Now.AddDays(-30))).Select(ToDto).ToList();

    /// <summary>Õhtu restorani lauad (õhtu saalis): millised on juba võetud.</summary>
    public async Task<List<EventTableDto>> GetTablesAsync(int id)
    {
        var e = await Find(id);
        var tables = (await _tables.GetByRestaurantAsync(e.RestaurantId, onlyActive: true))
            .Where(t => e.Zone is null || t.Location == e.Zone).ToList();
        var taken = Active(e).Where(r => r.TableId != null && r.UserId != _current.UserId).Select(r => r.TableId!.Value).ToHashSet();
        taken.UnionWith(await BookedTableIds(e));
        return tables.Select(t => new EventTableDto(t.Id, t.Number, t.Seats, t.Location, t.X, t.Y, t.Width, t.Height, t.Spot, t.IsRound,
            taken.Contains(t.Id))).ToList();
    }

    public async Task<List<EventRegistrationDto>> GetMyRegistrationsAsync()
    {
        var userId = _current.UserId ?? throw new UnauthorizedException("Pole sisse logitud.");
        return (await _events.GetRegistrationsOfUserAsync(userId)).Select(ToDto).ToList();
    }

    public async Task<List<EventRegistrationDto>> GetRegistrationsAsync(int eventId)
    {
        if (!_current.IsStaff) throw new ForbiddenException("Ainult personalile.");
        var e = await Find(eventId);
        foreach (var r in e.Registrations) r.Event = e;
        return e.Registrations.OrderBy(r => r.Status).ThenBy(r => r.CreatedAt).Select(ToDto).ToList();
    }

    // ---------- Registreerimine ----------

    /// <summary>Klient registreerub ise ja maksab kohe.</summary>
    public async Task<EventRegistrationDto> JoinAsync(int id, JoinEventDto dto)
    {
        var userId = _current.UserId ?? throw new UnauthorizedException("Pole sisse logitud.");
        if (_current.IsStaff || _current.Role == UserRole.Service)
            throw new ForbiddenException("Personal ei registreeru õhtule enda nimele.");
        var e = await Find(id);
        var reg = await BuildRegistrationAsync(e, userId, dto);

        if (reg.Total > 0)
        {
            CheckCard(dto.Card);
            reg.IsPaid = true;
        }
        reg.Status = ReservationStatus.Active;
        await _events.SaveChangesAsync();
        _logger.LogInformation("Kasutaja {User} registreerus õhtule {Event} ({Guests} külalist), makstud {Total} €", userId, e.Title, reg.Guests, reg.Total);
        reg.Event = e;
        return ToDto(reg);
    }

    /// <summary>Personal registreerib kliendi telefoni teel: registreerimine ootab makset.</summary>
    public async Task<EventRegistrationDto> JoinForCustomerAsync(int id, int customerId, JoinEventDto dto)
    {
        if (!_current.IsStaff) throw new ForbiddenException("Kliendi nimel saab registreerida ainult personal.");
        var e = await Find(id);
        var reg = await BuildRegistrationAsync(e, customerId, dto);
        if (reg.Total > 0)
        {
            reg.Status = ReservationStatus.PendingPayment;
            reg.PaymentToken = Guid.NewGuid().ToString("N");
        }
        await _events.SaveChangesAsync();
        _logger.LogInformation("Personal registreeris kliendi {User} õhtule {Event}", customerId, e.Title);
        reg.Event = e;
        return ToDto(reg);
    }

    private async Task<EventRegistration> BuildRegistrationAsync(GastroEvent e, int userId, JoinEventDto dto)
    {
        if (e.StartTime - JoinDeadline <= _clock.Now)
            throw new BusinessRuleException("Registreerimine on lõppenud – registreeruda saab kuni 12 tundi enne õhtu algust.");
        if ((e.Rule is not null || e.NoChildren) && !dto.AcceptRules)
            throw new BusinessRuleException("Registreerimiseks tuleb nõustuda õhtu reeglitega.");
        if (Active(e).Any(r => r.UserId == userId))
            throw new ConflictException("Olete sellele õhtule juba registreerunud.");
        if (SeatsLeft(e) < dto.Guests)
            throw new ConflictException($"Vabu kohti on alles {SeatsLeft(e)}.");

        // Laud: õhtu saalis, piisavalt suur ja vaba
        if (dto.TableId is not null)
        {
            var tables = await _tables.GetByRestaurantAsync(e.RestaurantId, onlyActive: true);
            var table = tables.FirstOrDefault(t => t.Id == dto.TableId && (e.Zone is null || t.Location == e.Zone))
                        ?? throw new NotFoundException("Lauda selles restoranis ei leitud.");
            if (table.Seats < dto.Guests)
                throw new BusinessRuleException($"Laud {table.Number} on {table.Seats} kohaga.");
            if (Active(e).Any(r => r.TableId == table.Id && r.UserId != userId) || (await BookedTableIds(e)).Contains(table.Id))
                throw new ConflictException($"Laud {table.Number} on juba võetud. Valige teine laud.");
        }

        // Eripakkumised ainult selle õhtu valikust; hind ja nimi salvestatakse (kui admin hiljem muudab, jääb tšekk samaks)
        var lines = new List<EventLineDto>();
        foreach (var line in dto.Drinks ?? new())
        {
            var item = e.Items.FirstOrDefault(i => i.Id == line.ItemId)
                       ?? throw new BusinessRuleException($"Jooki \"{line.ItemId}\" sellel õhtul ei pakuta.");
            lines.Add(new EventLineDto(item.Id, item.Code, item.Name, line.Quantity, item.Price));
        }
        var extras = (dto.Extras ?? new()).Distinct().ToList();
        foreach (var x in extras)
            if (!EventCatalog.AllowedExtras.Contains(x))
                throw new BusinessRuleException("Õhtul saab lisada ainult pudeli šampanjat ja lillekimbu.");
        decimal extrasTotal = extras.Sum(x => ExtrasCatalog.Find(x)?.Price ?? 0);

        // Varem tühistatud registreerimine kasutatakse uuesti (üks rida külalise kohta)
        var reg = e.Registrations.FirstOrDefault(r => r.UserId == userId);
        if (reg is null)
        {
            reg = new EventRegistration { UserId = userId };
            e.Registrations.Add(reg);
        }
        reg.Guests = dto.Guests;
        reg.TableId = dto.TableId;
        reg.Drinks = lines.Count > 0 ? JsonSerializer.Serialize(lines) : null;
        reg.Extras = extras.Count > 0 ? string.Join(',', extras) : null;
        reg.Total = e.PricePerGuest * dto.Guests + lines.Sum(l => l.Price * l.Quantity) + extrasTotal;
        reg.AcceptedRules = dto.AcceptRules;
        reg.CreatedAt = _clock.Now;
        reg.Status = ReservationStatus.Active;
        reg.IsPaid = false;
        reg.PaymentToken = null;
        reg.CancelledAt = null;
        reg.RefundAmount = null;
        return reg;
    }

    // ---------- Makse ja tühistamine ----------

    public async Task<EventRegistrationDto> PayRegistrationAsync(int registrationId, FakePaymentDto card)
    {
        var reg = await FindRegistration(registrationId);
        if (reg.UserId != _current.UserId && !_current.IsStaff) throw new ForbiddenException("See ei ole sinu broneering.");
        return await PayAsync(reg, card);
    }

    public async Task<EventRegistrationDto> GetByPaymentTokenAsync(string token) =>
        ToDto(await _events.GetRegistrationByTokenAsync(token) ?? throw new NotFoundException("Makselinki ei leitud või see on aegunud."));

    public async Task<EventRegistrationDto> PayByTokenAsync(string token, FakePaymentDto card) =>
        await PayAsync(await _events.GetRegistrationByTokenAsync(token) ?? throw new NotFoundException("Makselinki ei leitud või see on aegunud."), card);

    private async Task<EventRegistrationDto> PayAsync(EventRegistration reg, FakePaymentDto card)
    {
        if (reg.Status != ReservationStatus.PendingPayment)
            throw new BusinessRuleException("See broneering on juba kinnitatud või tühistatud.");
        if (reg.Event!.StartTime <= _clock.Now)
            throw new BusinessRuleException("Külastuse aeg on möödas – makselink ei kehti enam.");
        CheckCard(card);
        reg.IsPaid = true;
        reg.Status = ReservationStatus.Active;
        await _events.SaveChangesAsync();
        _logger.LogInformation("Õhtu registreerimine {Id} makstud", reg.Id);
        return ToDto(reg);
    }

    public async Task<EventRegistrationDto> CancelRegistrationAsync(int registrationId, CancelReservationDto? dto)
    {
        var reg = await FindRegistration(registrationId);
        if (reg.UserId != _current.UserId && !_current.IsStaff) throw new ForbiddenException("See ei ole sinu broneering.");
        if (reg.Status is not (ReservationStatus.Active or ReservationStatus.PendingPayment))
            throw new BusinessRuleException("Tühistada saab ainult aktiivset broneeringut.");

        var paid = reg.IsPaid ? reg.Total : 0m;
        bool inTime = reg.Event!.StartTime - _clock.Now > PayDeadline;
        if (!inTime && !_current.IsStaff && paid > 0 && dto?.AcceptNoRefund != true)
            throw new BusinessRuleException("Külastuseni on vähem kui 24 tundi: makstud summat ei tagastata. Tühistamiseks kinnitage, et olete sellega nõus.");

        reg.Status = ReservationStatus.Cancelled;
        reg.CancelledAt = _clock.Now;
        reg.RefundAmount = inTime || _current.IsStaff ? paid : 0m;
        await _events.SaveChangesAsync();
        return ToDto(reg);
    }

    public async Task<List<UnpaidCancelledDto>> CancelUnpaidAsync()
    {
        var now = _clock.Now;
        var list = await _events.GetPendingBeforeAsync(now + PayDeadline) ?? new List<EventRegistration>();
        var cancelled = new List<UnpaidCancelledDto>();
        foreach (var reg in list)
        {
            // Tavaliselt tuleb maksta 24 h enne õhtut; kui personal registreeris hiljem, on aega kuni 12 h enne
            var start = reg.Event!.StartTime;
            var deadline = reg.CreatedAt < start - PayDeadline ? start - PayDeadline : start - JoinDeadline;
            if (now < deadline) continue;
            reg.Status = ReservationStatus.Cancelled;
            reg.CancelledAt = now;
            reg.RefundAmount = 0;
            cancelled.Add(new UnpaidCancelledDto(reg.UserId, null, $"{reg.Event.Title} {start:dd.MM HH:mm}"));
        }
        if (cancelled.Count > 0) await _events.SaveChangesAsync();
        return cancelled;
    }

    // ---------- Admin: õhtute haldus ----------

    public async Task<GastroEventDto> CreateAsync(SaveEventDto dto)
    {
        RequireAdmin();
        var e = new GastroEvent();
        await ApplyAsync(e, dto, isNew: true);
        await _events.AddAsync(e);
        await _events.SaveChangesAsync();
        _logger.LogInformation("Uus gastronoomiline õhtu {Title} ({Start})", e.Title, e.StartTime);
        return ToDto(await Find(e.Id));
    }

    public async Task<GastroEventDto> UpdateAsync(int id, SaveEventDto dto)
    {
        RequireAdmin();
        var e = await Find(id);
        await ApplyAsync(e, dto, isNew: false);
        await _events.SaveChangesAsync();
        return ToDto(await Find(id));
    }

    public async Task DeleteAsync(int id)
    {
        RequireAdmin();
        var e = await Find(id);
        if (Active(e).Any())
            throw new BusinessRuleException("Õhtule on registreerunud külalisi – tühistage enne nende registreerimised.");
        _events.Remove(e);
        await _events.SaveChangesAsync();
    }

    private async Task ApplyAsync(GastroEvent e, SaveEventDto dto, bool isNew)
    {
        var restaurant = await _restaurants.GetByIdAsync(dto.RestaurantId)
                         ?? throw new NotFoundException($"Restorani {dto.RestaurantId} ei leitud.");

        // Uue õhtu (või uue kuupäeva) saab panna kõige varem 2 nädala pärast
        if ((isNew || dto.StartTime != e.StartTime) && dto.StartTime.Date < _clock.Now.Date.AddDays(MinDaysAhead))
            throw new BusinessRuleException($"Õhtu kuupäev peab olema vähemalt {MinDaysAhead} päeva pärast.");
        var time = TimeOnly.FromDateTime(dto.StartTime);
        var last = BookingRules.LastArrival(restaurant.OpensAt, restaurant.ClosesAt);
        bool overnight = restaurant.ClosesAt < restaurant.OpensAt;
        bool timeOk = overnight ? time >= restaurant.OpensAt || time <= last : time >= restaurant.OpensAt && time <= last;
        if (!timeOk)
            throw new BusinessRuleException($"Õhtu algus peab jääma lahtiolekuaja sisse ({restaurant.OpensAt:HH\\:mm}–{last:HH\\:mm}).");

        // Kohti ei saa olla rohkem kui saalis laudade taga
        var seats = (await _tables.GetByRestaurantAsync(restaurant.Id, onlyActive: true))
            .Where(t => dto.Zone is null || t.Location == dto.Zone).Sum(t => t.Seats);
        if (dto.Capacity > seats)
            throw new BusinessRuleException($"Selles saalis on ainult {seats} kohta.");
        var registered = Active(e).Sum(r => r.Guests);
        if (dto.Capacity < registered)
            throw new BusinessRuleException($"Registreerunud on juba {registered} külalist – kohti ei saa olla vähem.");
        var dayStart = BookingRules.DayStart(dto.StartTime);
        var zoneTables = (await _tables.GetByRestaurantAsync(restaurant.Id, onlyActive: false))
            .Where(t => dto.Zone is null || t.Location == dto.Zone).Select(t => t.Id).ToList();
        var booked = (await _reservations.GetOccupyingAsync(zoneTables, dayStart, dayStart.AddDays(1), null) ?? new List<Reservation>())
            .Count(r => BookingRules.Occupies(r.Status));
        if (booked > 0)
            throw new BusinessRuleException($"Sel päeval on selles saalis juba {booked} tavalist broneeringut. Valige teine päev või saal.");
        if (!isNew && dto.RestaurantId != e.RestaurantId && Active(e).Any())
            throw new BusinessRuleException("Õhtule on registreerunud külalisi – restorani ei saa muuta.");

        // Kas admin muutis tekste? Kui muutis ainult kuupäeva, hinda vms, jääb tõlge (kood) alles
        string Clean(string? s) => (s ?? "").Trim();
        var textChanged = !isNew && (Clean(dto.Title) != e.Title || Clean(dto.Kind) != e.Kind || Clean(dto.Description) != e.Description
            || Clean(dto.DressCode) != e.DressCode || Clean(dto.Rule) != Clean(e.Rule) || Clean(dto.Program) != Clean(e.Program));

        e.RestaurantId = restaurant.Id;
        e.Title = dto.Title.Trim();
        e.Kind = dto.Kind.Trim();
        e.Description = dto.Description.Trim();
        e.Program = string.IsNullOrWhiteSpace(dto.Program) ? null : dto.Program.Trim();
        e.TranslationsJson = EventTexts.Serialize(dto.Translations);
        e.StartTime = dto.StartTime;
        e.PricePerGuest = dto.PricePerGuest;
        e.DressCode = dto.DressCode.Trim();
        e.Rule = string.IsNullOrWhiteSpace(dto.Rule) ? null : dto.Rule.Trim();
        e.NoChildren = dto.NoChildren;
        e.ImageUrl = string.IsNullOrWhiteSpace(dto.ImageUrl) ? null : dto.ImageUrl;
        e.Capacity = dto.Capacity;
        e.Zone = dto.Zone;
        // Admin muutis teksti: edaspidi näidatakse admini teksti, mitte näidisõhtu tõlget
        if (textChanged) e.Code = "";

        // Eripakkumised: menüüst võetud roog (nimi, kirjeldus, hind ja foto menüüst) või uus rida
        var menuIds = dto.Items.Where(i => i.MenuItemId != null).Select(i => i.MenuItemId!.Value).ToList();
        var menuItems = menuIds.Count > 0 ? await _menu.GetByIdsAsync(menuIds) : new List<MenuItem>();
        var items = new List<EventMenuItem>();
        foreach (var i in dto.Items)
        {
            var m = i.MenuItemId is null ? null : menuItems.FirstOrDefault(x => x.Id == i.MenuItemId && x.RestaurantId == restaurant.Id)
                    ?? throw new BusinessRuleException($"Menüüpunkti {i.MenuItemId} ei leitud.");
            var name = (i.Name ?? m?.Name ?? "").Trim();
            if (name.Length < 2) throw new BusinessRuleException("Eripakkumise nimi on liiga lühike.");
            var price = i.Price ?? m?.Price ?? 0;
            var existing = e.Items.FirstOrDefault(x => x.MenuItemId == i.MenuItemId && x.Name == name);
            items.Add(existing ?? new EventMenuItem());
            var item = items[^1];
            item.MenuItemId = m?.Id;
            item.Name = name;
            item.Description = (i.Description ?? m?.Description ?? "").Trim();
            item.Price = price;
            item.ImageUrl = string.IsNullOrWhiteSpace(i.ImageUrl) ? m?.ImageUrl : i.ImageUrl;
        }
        e.Items.RemoveAll(x => !items.Contains(x));
        foreach (var item in items.Where(x => !e.Items.Contains(x))) e.Items.Add(item);
    }

    // ---------- Abi ----------

    private void RequireAdmin()
    {
        if (!_current.IsStaff) throw new ForbiddenException("Õhtuid saab hallata ainult admin.");
    }

    private static void CheckCard(FakePaymentDto? card)
    {
        var digits = new string((card?.CardNumber ?? "").Where(char.IsDigit).ToArray());
        if (digits.Length < 12) throw new BusinessRuleException("Kaardi number on vigane.");
    }

    /// <summary>Lauad, mis on õhtu päeval tavalise broneeringuga hõivatud (laud on hõivatud kogu päeva).</summary>
    private async Task<List<int>> BookedTableIds(GastroEvent e)
    {
        var dayStart = BookingRules.DayStart(e.StartTime);
        var tables = await _tables.GetByRestaurantAsync(e.RestaurantId, onlyActive: true) ?? new List<DiningTable>();
        var list = await _reservations.GetOccupyingAsync(tables.Select(t => t.Id), dayStart, dayStart.AddDays(1), null) ?? new List<Reservation>();
        return list.Where(r => BookingRules.Occupies(r.Status)).Select(r => r.TableId).ToList();
    }

    private async Task<GastroEvent> Find(int id) =>
        await _events.GetWithRegistrationsAsync(id) ?? throw new NotFoundException($"Õhtut {id} ei leitud.");

    private async Task<EventRegistration> FindRegistration(int id) =>
        await _events.GetRegistrationAsync(id) ?? throw new NotFoundException("Te pole sellele õhtule registreerunud.");

    /// <summary>Kehtivad registreerimised (tühistatud ei võta kohti ega laudu).</summary>
    private static IEnumerable<EventRegistration> Active(GastroEvent e) =>
        e.Registrations.Where(r => r.Status != ReservationStatus.Cancelled);

    private static int SeatsLeft(GastroEvent e) => Math.Max(0, e.Capacity - Active(e).Sum(r => r.Guests));

    private GastroEventDto ToDto(GastroEvent e)
    {
        var mine = Active(e).FirstOrDefault(r => r.UserId == _current.UserId);
        return new(
            e.Id, e.RestaurantId, e.Restaurant?.Name ?? "", e.Code, e.Title, e.Kind, e.Description, e.Program, e.StartTime,
            e.PricePerGuest, e.DressCode, e.Rule, e.NoChildren, e.ImageUrl, e.Capacity, SeatsLeft(e),
            e.Zone?.ToString(), e.StartTime - JoinDeadline <= _clock.Now,
            mine?.Id, mine?.Guests ?? 0, mine?.Table?.Number, mine?.Total ?? 0, mine?.Status.ToString(), mine?.IsPaid ?? false,
            e.Items.OrderBy(i => i.Id).Select(i => new EventItemDto(i.Id, i.Code, L.Word(i.Name, _current.Language), L.Word(i.Description, _current.Language), i.Price, i.ImageUrl, i.MenuItemId)).ToList(),
            EventTexts.Parse(e.TranslationsJson));
    }

    private EventRegistrationDto ToDto(EventRegistration r)
    {
        var e = r.Event!;
        var lines = (string.IsNullOrEmpty(r.Drinks) ? new List<EventLineDto>() : ReadLines(r.Drinks))
            .Select(l => l with { Name = L.Word(l.Name, _current.Language) }).ToList();
        return new(
            r.Id, e.Id, e.Code, e.Title, e.Kind, e.StartTime, e.Restaurant?.Name ?? "", e.Restaurant?.Address ?? "", e.ImageUrl,
            r.Guests, r.Table?.Number, r.Table?.Location.ToString(), e.PricePerGuest, lines,
            string.IsNullOrEmpty(r.Extras) ? new List<string>() : r.Extras.Split(',').ToList(),
            r.Total, r.Status.ToString(), r.IsPaid, r.PaymentToken, r.CancelledAt, r.RefundAmount,
            e.StartTime - _clock.Now > PayDeadline,
            r.UserId, r.User?.Name ?? "", r.User?.Email ?? "", r.User?.Phone,
            EventTexts.Parse(e.TranslationsJson));
    }

    private static List<EventLineDto> ReadLines(string text)
    {
        try { return JsonSerializer.Deserialize<List<EventLineDto>>(text) ?? new(); }
        catch (JsonException) { return new(); }
    }
}
