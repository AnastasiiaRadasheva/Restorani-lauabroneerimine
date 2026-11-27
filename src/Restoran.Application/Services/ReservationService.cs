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
    private readonly IRealtimeNotifier _realtime;

    public ReservationService(
        IReservationRepository reservations,
        ITableRepository tables,
        IRestaurantRepository restaurants,
        IMenuRepository menu,
        ICurrentUser current,
        IClock clock,
        ILogger<ReservationService> logger,
        IRealtimeNotifier realtime)
    {
        _reservations = reservations;
        _tables = tables;
        _restaurants = restaurants;
        _menu = menu;
        _current = current;
        _clock = clock;
        _logger = logger;
        _realtime = realtime;
    }

    // ---------- Vabad lauad ----------

    public async Task<AvailabilityDto> GetAvailabilityAsync(int restaurantId, DateOnly date, TimeOnly time, int guests)
    {
        restaurantId = await DefaultRestaurant(restaurantId);
        var restaurant = await GetRestaurant(restaurantId);
        var (start, end) = ValidateTime(restaurant, date, time, guests);

        var tables = await _tables.GetByRestaurantAsync(restaurantId, onlyActive: true);
        var busy = await GetBusyTableIds(tables, start, end, null);
        var best = BookingRules.PickBestTable(tables, busy, guests);

        var list = tables.Select(t => new TableAvailabilityDto(
            t.Id, t.Number, t.Seats, t.Location, t.X, t.Y, t.Width, t.Height, t.Spot, t.IsRound,
            IsFree: !busy.Contains(t.Id),
            FitsGuests: t.Seats >= guests)).ToList();

        // Kokkuvõte: mitu sobivat (piisavalt suurt) lauda on vaba
        var free = list.Count(t => t.IsFree && t.FitsGuests);
        var message = best is null
            ? L.T(_current.Language, "avail.none", guests)
            : L.T(_current.Language, "avail.free", free, guests, best.Number);
        return new AvailabilityDto(restaurantId, start, end, guests, best is not null, free, message, best?.Number, best?.Id, list);
    }

    /// <summary>
    /// Kalendri jaoks: iga päeva iga kellaaja kohta, mitu sobiva suurusega lauda on vaba.
    /// Kui vabu laudu on 0, näidatakse kellaaeg veebis läbikriipsutatuna.
    /// </summary>
    public async Task<List<DaySlotsDto>> GetSlotsAsync(int restaurantId, DateOnly from, int days, int guests)
    {
        restaurantId = await DefaultRestaurant(restaurantId);
        if (days < 1 || days > BookingRules.MaxDaysAhead + 7)
            throw new BusinessRuleException("Päevade arv peab olema 1 kuni 190.");
        if (guests < 1 || guests > 20)
            throw new BusinessRuleException("Inimeste arv peab olema 1 kuni 20.");

        var restaurant = await GetRestaurant(restaurantId);
        var tables = (await _tables.GetByRestaurantAsync(restaurantId, onlyActive: true))
            .Where(t => t.Seats >= guests).ToList();
        var rangeStart = from.ToDateTime(TimeOnly.MinValue);
        var occupying = (await _reservations.GetOccupyingAsync(
                tables.Select(t => t.Id), rangeStart, rangeStart.AddDays(days + 1), null))
            .Where(r => BookingRules.Occupies(r.Status))
            .ToList();

        // Gastronoomilise õhtu päeval on tavaline broneerimine õhtu saalis (või kogu restoranis) suletud
        var rangeDayStart = BookingRules.DayStart(from.ToDateTime(new TimeOnly(12, 0)));
        var events = await _reservations.GetEventsAsync(restaurantId, rangeDayStart, rangeDayStart.AddDays(days)) ?? new List<GastroEvent>();

        var result = new List<DaySlotsDto>();
        for (int d = 0; d < days; d++)
        {
            var date = from.AddDays(d);
            var slots = new List<SlotDto>();
            var dayEvents = events.Where(e => DateOnly.FromDateTime(BookingRules.DayStart(e.StartTime)) == date).ToList();
            // Saabumise ajad 30-minutilise sammuga avamisest kuni viimase saabumiseni
            var open = (int)restaurant.OpensAt.ToTimeSpan().TotalMinutes;
            var lastStart = (int)BookingRules.LastArrival(restaurant.OpensAt, restaurant.ClosesAt).ToTimeSpan().TotalMinutes;
            // Laud on hõivatud kogu õhtu
            var eveningEnd = BookingRules.EveningEnd(date, restaurant.OpensAt, restaurant.ClosesAt);
            for (int minutes = open; minutes <= lastStart; minutes += 30)
            {
                var time = TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(minutes));
                var start = date.ToDateTime(time);
                var end = eveningEnd;
                // Broneeritud laud on suletud kogu päevaks (ka enne broneeringu kellaaega)
                var dayStart = BookingRules.DayStart(start);
                int free = start <= _clock.Now ? 0 : tables.Count(t => !ClosedByEvent(dayEvents, t) && !occupying.Any(r =>
                    r.TableId == t.Id && BookingRules.Overlaps(dayStart, end, r.StartTime, r.EndTime)));
                slots.Add(new SlotDto(time.ToString("HH:mm"), free));
            }
            var ev = dayEvents.FirstOrDefault();
            result.Add(new DaySlotsDto(date, slots, ev is null ? null : new DayEventDto(ev.Id, ev.Code, ev.Title, ev.Zone?.ToString(), EventTexts.Parse(ev.TranslationsJson))));
        }
        return result;
    }

    // ---------- Broneerimine ----------

    public Task<ReservationDto> CreateAsync(CreateReservationDto dto)
    {
        // Personal ei broneeri enda nimele – ainult kliendi nimel (telefonibroneering)
        if (_current.IsStaff)
            throw new ForbiddenException("Personal saab broneerida ainult kliendi nimel (Uus broneering).");
        if (_current.Role == UserRole.Service)
            throw new ForbiddenException("Teenusekonto ei saa broneerida.");
        return CreateCoreAsync(RequireUser(), dto, ReservationStatus.Active);
    }

    /// <summary>Personal broneerib kliendi nimel. Personal ise ei maksa: klient saab makselingi.</summary>
    public Task<ReservationDto> CreatePendingForUserAsync(int userId, CreateReservationDto dto)
    {
        if (!_current.IsStaff) throw new ForbiddenException("Kliendi nimel saab broneerida ainult personal.");
        return CreateCoreAsync(userId, dto, ReservationStatus.PendingPayment);
    }

    public async Task<ReservationDto> GetByPaymentTokenAsync(string token) =>
        ToDto(await _reservations.GetByPaymentTokenAsync(token) ?? throw new NotFoundException("Makselinki ei leitud või see on aegunud."));

    /// <summary>Makse lingi kaudu: broneering kinnitatakse (olek Active).</summary>
    public async Task<ReservationDto> PayByTokenAsync(string token, FakePaymentDto dto)
    {
        var r = await _reservations.GetByPaymentTokenAsync(token) ?? throw new NotFoundException("Makselinki ei leitud või see on aegunud.");
        if (r.Status != ReservationStatus.PendingPayment)
            throw new BusinessRuleException("See broneering on juba kinnitatud või tühistatud.");
        if (r.StartTime <= _clock.Now)
            throw new BusinessRuleException("Külastuse aeg on möödas – makselink ei kehti enam.");
        if (TotalOf(r) > 0 && new string(dto.CardNumber.Where(char.IsDigit).ToArray()).Length < 12)
            throw new BusinessRuleException("Kaardi number on vigane.");
        r.IsPaid = TotalOf(r) > 0;
        r.Status = ReservationStatus.Active;
        await _reservations.SaveChangesAsync();
        _logger.LogInformation("Broneering {Id} kinnitati makselingi kaudu", r.Id);
        var result = ToDto(r);
        await _realtime.TableStatusChangedAsync(result);
        return result;
    }

    private async Task<ReservationDto> CreateCoreAsync(int userId, CreateReservationDto dto, ReservationStatus status)
    {
        var restaurant = await GetRestaurant(dto.RestaurantId);
        var (start, end) = ValidateTime(restaurant, dto.Date, dto.Time, dto.Guests);

        // Üks külaline võib ühel õhtul broneerida kuni 3 lauda
        var mine = await _reservations.GetByUserAsync(userId) ?? new List<Reservation>();
        if (mine.Count(r => BookingRules.Occupies(r.Status) && DateOnly.FromDateTime(r.StartTime) == dto.Date) >= BookingRules.MaxTablesPerEvening)
            throw new BusinessRuleException($"Ühel õhtul saab broneerida kuni {BookingRules.MaxTablesPerEvening} lauda.");
        var table = await ChooseTable(dto.RestaurantId, dto.TableId, dto.Guests, start, end, null);

        var reservation = new Reservation
        {
            TableId = table.Id,
            UserId = userId,
            StartTime = start,
            EndTime = end,
            Guests = dto.Guests,
            SpecialRequest = Clean(dto.SpecialRequest),
            Status = status,
            PaymentToken = status == ReservationStatus.PendingPayment ? Guid.NewGuid().ToString("N") : null,
            CreatedAt = _clock.Now
        };
        // Eeltellimus võib tulla kohe broneeringuga koos (veebis III samm "Gastronoomia")
        if (dto.Items is { Count: > 0 })
            await ApplyPreOrderAsync(reservation, dto.RestaurantId, dto.Items);
        ApplyEvening(reservation, dto);
        // Personali broneering, kus pole midagi maksta (metrdotel valib laua), kinnitub kohe
        // Klient broneerib ise: makse samas sammus (maksmata broneeringut ei jää laua kinni hoidma)
        if (reservation.Status == ReservationStatus.Active && TotalOf(reservation) > 0)
        {
            var digits = new string((dto.Card?.CardNumber ?? "").Where(char.IsDigit).ToArray());
            if (digits.Length < 12)
                throw new BusinessRuleException("Kaardi number on vigane.");
            reservation.IsPaid = true;
        }
        if (reservation.Status == ReservationStatus.PendingPayment && TotalOf(reservation) <= 0)
        {
            reservation.Status = ReservationStatus.Active;
            reservation.PaymentToken = null;
        }
        await _reservations.AddAsync(reservation);
        await _reservations.SaveChangesAsync();
        _logger.LogInformation("Uus broneering {Id}: restoran {Restaurant}, laud {Table}, {Start}, {Guests} inimest",
            reservation.Id, restaurant.Name, table.Number, start, dto.Guests);

        // Reaalajas teade alles pärast salvestamist
        var result = await GetByIdAsync(reservation.Id);
        await _realtime.ReservationCreatedAsync(result);
        await _realtime.TableStatusChangedAsync(result);
        return result;
    }

    public async Task<ReservationDto> UpdateAsync(int id, UpdateReservationDto dto)
    {
        var r = await GetOwned(id);
        if (r.Status is not (ReservationStatus.Active or ReservationStatus.PendingPayment))
            throw new BusinessRuleException("Muuta saab ainult aktiivset broneeringut.");
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

        var result = await GetByIdAsync(id);
        await _realtime.TableStatusChangedAsync(result);
        return result;
    }

    /// <summary>
    /// Tühistamine. Rohkem kui 24 h enne tagastatakse makstud summa.
    /// Vähem kui 24 h enne võib klient samuti tühistada, kuid raha ei tagastata ja ta peab sellega nõustuma.
    /// </summary>
    public async Task<ReservationDto> CancelAsync(int id, CancelReservationDto? dto = null)
    {
        var r = await GetOwned(id);
        if (r.Status is not (ReservationStatus.Active or ReservationStatus.PendingPayment))
            throw new BusinessRuleException("Tühistada saab ainult aktiivset broneeringut.");

        var paid = PaidOf(r); // kohapeal makstavat osa ei tagastata – seda pole makstud
        bool inTime = BookingRules.CanCustomerModify(r.StartTime, _clock.Now);
        if (!inTime && !_current.IsStaff && paid > 0 && dto?.AcceptNoRefund != true)
            throw new BusinessRuleException("Külastuseni on vähem kui 24 tundi: makstud summat ei tagastata. Tühistamiseks kinnitage, et olete sellega nõus.");

        r.Status = ReservationStatus.Cancelled;
        r.CancelledAt = _clock.Now;
        r.RefundAmount = (r.RefundAmount ?? 0m) + (inTime || _current.IsStaff ? paid : 0m);
        await _reservations.SaveChangesAsync();
        _logger.LogInformation("Broneering {Id} tühistati", r.Id);
        var result = ToDto(r);
        await _realtime.TableStatusChangedAsync(result);
        return result;
    }

    // ---------- Eeltellimus ja makse ----------

    public async Task<ReservationDto> SetPreOrderAsync(int id, PreOrderDto dto)
    {
        var r = await GetOwned(id);
        if (r.Status != ReservationStatus.Active)
            throw new BusinessRuleException("Eeltellimust saab lisada ainult aktiivsele broneeringule.");
        EnsureCustomerCanModify(r);

        // Eeltellimust võib muuta ka pärast makset: kallim tellimus -> makstakse vahe, odavam -> vahe tagastatakse
        var before = TotalOf(r);
        await ApplyPreOrderAsync(r, r.Table!.RestaurantId, dto.Items);
        var diff = TotalOf(r) - before;
        if (_current.IsStaff)
        {
            // Personal lisab külalise palvel: juurde tulnud osa makstakse kohapeal, vähenemisel vähendatakse
            // kõigepealt kohapeal makstavat osa ja alles siis tagastatakse makstud raha
            var onSite = r.OnSitePreOrder + diff;
            if (onSite < 0 && r.IsPaid) r.RefundAmount = (r.RefundAmount ?? 0) - onSite;
            r.OnSitePreOrder = Math.Max(0, onSite);
            if (dto.SpecialRequest is not null) r.SpecialRequest = Clean(dto.SpecialRequest);
            await _reservations.SaveChangesAsync();
            _logger.LogInformation("Broneering {Id}: personal muutis eeltellimust ({Diff} €, kohapeal {OnSite} €)", r.Id, diff, r.OnSitePreOrder);
            return await GetByIdAsync(id);
        }
        var due = r.IsPaid ? diff : TotalOf(r) - r.Extras.Where(e => !e.Locked).Sum(e => e.Price) - r.OnSitePreOrder;
        if (due > 0)
        {
            var digits = new string((dto.Card?.CardNumber ?? "").Where(char.IsDigit).ToArray());
            if (digits.Length < 12)
                throw new BusinessRuleException($"Tasuda tuleb {due:0.##} € – sisestage kaardi andmed.");
            r.IsPaid = true;
            _logger.LogInformation("Broneering {Id}: eeltellimuse eest makstud {Due} € (võltsmakse)", r.Id, due);
        }
        else if (r.IsPaid && diff < 0)
        {
            r.RefundAmount = (r.RefundAmount ?? 0) - diff;
            _logger.LogInformation("Broneering {Id}: eeltellimuse vahe {Diff} € tagastatakse", r.Id, -diff);
        }
        if (TotalOf(r) <= 0) r.IsPaid = false;
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
        if (TotalOf(r) <= 0)
            throw new BusinessRuleException("Pole midagi maksta: eeltellimus ja lisateenused puuduvad.");

        // Võltsmakse: kontrollime ainult vormingut, kaardiandmeid ei salvestata.
        var digits = new string(dto.CardNumber.Where(char.IsDigit).ToArray());
        if (digits.Length < 12)
            throw new BusinessRuleException("Kaardi number on vigane.");

        r.IsPaid = true;
        // Kõik selle makse sees olevad lisateenused on nüüd makstud ja lukus
        foreach (var x in r.Extras) x.Locked = true;
        await _reservations.SaveChangesAsync();
        _logger.LogInformation("Broneering {Id} makstud (võltsmakse), summa {Total}", r.Id, TotalOf(r));
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
        // "Saabus" / "Ei tulnud" saab vahetada (eksimuse parandamiseks), kuid mitte tühistatud ega makset ootaval broneeringul
        if (status is ReservationStatus.Arrived or ReservationStatus.NoShow)
        {
            if (r.Status is not (ReservationStatus.Active or ReservationStatus.Arrived or ReservationStatus.NoShow))
                throw new BusinessRuleException("Saabumist saab märkida ainult aktiivsel broneeringul.");
            if (DateOnly.FromDateTime(BookingRules.DayStart(r.StartTime)) > DateOnly.FromDateTime(BookingRules.DayStart(_clock.Now)))
                throw new BusinessRuleException("Saabumist saab märkida alles külastuse päeval.");
        }
        r.Status = status;
        await _reservations.SaveChangesAsync();
        _logger.LogInformation("Broneeringu {Id} uus olek: {Status}", r.Id, status);
        var result = ToDto(r);
        await _realtime.TableStatusChangedAsync(result);
        return result;
    }

    public async Task<TablePlanDto> GetPlanAsync(int restaurantId, DateOnly date)
    {
        restaurantId = await DefaultRestaurant(restaurantId);
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

    /// <summary>Kalender personalile: broneeringute arv päevade kaupa.</summary>
    public async Task<List<CalendarDayDto>> GetCalendarAsync(int? restaurantId, DateOnly from, int days)
    {
        if (days < 1 || days > 62) throw new BusinessRuleException("Päevade arv peab olema 1 kuni 62.");
        var list = (await _reservations.GetForStatsAsync(restaurantId, from, from.AddDays(days - 1)))
            .Where(r => BookingRules.Occupies(r.Status)).ToList();
        return Enumerable.Range(0, days).Select(i =>
        {
            var d = from.AddDays(i);
            var day = list.Where(r => DateOnly.FromDateTime(r.StartTime) == d).ToList();
            return new CalendarDayDto(d, day.Count, day.Sum(r => r.Guests), day.Count(r => r.Occasion != Occasion.None));
        }).ToList();
    }

    // ---------- Sõnumid ja lisateenused ----------

    public async Task<List<MessageDto>> GetMessagesAsync(int id)
    {
        var r = await GetOwned(id);
        return r.Messages.OrderBy(m => m.CreatedAt).Select(ToMessageDto).ToList();
    }

    /// <summary>Klient kirjutab restoranile või personal vastab.</summary>
    public async Task<MessageDto> AddMessageAsync(int id, NewMessageDto dto)
    {
        var r = await GetOwned(id);
        var text = dto.Text.Trim();
        if (text.Length == 0)
            throw new BusinessRuleException("Sõnum on tühi.");

        var message = new ReservationMessage
        {
            FromStaff = _current.IsStaff,
            AuthorName = _current.IsStaff ? "Argent" : r.User?.Name ?? "",
            Text = text,
            CreatedAt = _clock.Now
        };
        r.Messages.Add(message);
        await _reservations.SaveChangesAsync();
        _logger.LogInformation("Uus sõnum broneeringule {Id} ({Who})", r.Id, message.FromStaff ? "personal" : "klient");
        return ToMessageDto(message);
    }

    /// <summary>Lisateenuseid saab muuta ka vähem kui 24 h enne – need valmistatakse kohapeal.</summary>
    public async Task<ReservationDto> SetExtrasAsync(int id, SetExtrasDto dto)
    {
        var r = await GetOwned(id);
        if (r.Status != ReservationStatus.Active)
            throw new BusinessRuleException("Lisateenuseid saab muuta ainult aktiivsel broneeringul.");

        var wanted = dto.Extras.Distinct().ToList();
        // Broneerimisel lisatud (makstud) teenust klient eemaldada ei saa; hiljem lisatut saab. Personal saab alati.
        if (!_current.IsStaff && r.Extras.Any(x => x.Locked && !wanted.Contains(x.Code)))
            throw new BusinessRuleException("Broneerimisel makstud teenust ei saa eemaldada. Kirjutage vajadusel restoranile broneeringu sõnumites.");
        r.Extras.RemoveAll(x => !wanted.Contains(x.Code));
        AddExtras(r, wanted, locked: false);
        await _reservations.SaveChangesAsync();
        return ToDto(r);
    }

    /// <summary>Personal: külalise soovid (allergiad, märkus köögile, soovid, tähtpäev). Klient seda kasutada ei saa.</summary>
    public async Task<ReservationDto> SetStaffDetailsAsync(int id, StaffDetailsDto dto)
    {
        if (!_current.IsStaff) throw new ForbiddenException("Seda saab muuta ainult personal.");
        var r = await GetOwned(id);
        if (r.Status is not (ReservationStatus.Active or ReservationStatus.PendingPayment or ReservationStatus.Arrived))
            throw new BusinessRuleException("Muuta saab ainult aktiivset broneeringut.");
        r.Allergies = dto.Allergies is { Count: > 0 } ? string.Join(',', dto.Allergies.Distinct()) : null;
        r.AllergyNote = Clean(dto.AllergyNote);
        r.SpecialRequest = Clean(dto.SpecialRequest);
        r.Occasion = dto.Occasion;
        if (r.Occasion == Occasion.None) r.Gift = ChefGift.None;
        await _reservations.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    private static MessageDto ToMessageDto(ReservationMessage m) =>
        new(m.Id, m.FromStaff, m.AuthorName, m.Text, m.CreatedAt);

    // ---------- Taustateenus (Worker kutsub API kaudu) ----------

    /// <summary>Homsed aktiivsed broneeringud, millele pole veel meeldetuletust saadetud.</summary>
    public async Task<List<RemindDueDto>> GetRemindDueAsync()
    {
        var tomorrow = _clock.Now.Date.AddDays(1);
        var list = await _reservations.GetRemindDueAsync(tomorrow, tomorrow.AddDays(1));
        return list.Select(r => new RemindDueDto(
            r.Id, r.UserId, r.User?.Name ?? "", r.Table?.Restaurant?.Name ?? "",
            r.Table?.Number ?? 0, r.Guests, r.StartTime, r.User?.Language ?? "et")).ToList();
    }

    public async Task MarkRemindedAsync(int id)
    {
        var r = await _reservations.GetByIdAsync(id) ?? throw NotFound(id);
        r.Reminded = true;
        await _reservations.SaveChangesAsync();
    }

    /// <summary>Kui klient pole 30 minuti jooksul saabunud, märgitakse NoShow ja laud vabaneb.</summary>
    public async Task<NoShowResultDto> MarkNoShowsAsync()
    {
        var cutoff = _clock.Now - BookingRules.NoShowAfter;
        var list = await _reservations.GetNoShowCandidatesAsync(cutoff);
        foreach (var r in list) r.Status = ReservationStatus.NoShow;

        // Personali broneering, mida klient 24 tunni jooksul ei maksnud, tühistatakse ja laud vabaneb
        var unpaid = new List<UnpaidCancelledDto>();
        foreach (var r in await _reservations.GetExpiredPendingAsync(_clock.Now.AddHours(-24), _clock.Now) ?? new List<Reservation>())
        {
            r.Status = ReservationStatus.Cancelled;
            r.CancelledAt = _clock.Now;
            list.Add(r);
            unpaid.Add(new UnpaidCancelledDto(r.UserId, r.Id, $"{r.Table?.Restaurant?.Name} {r.StartTime:dd.MM HH:mm}"));
        }
        await _reservations.SaveChangesAsync();

        foreach (var r in list)
        {
            _logger.LogInformation("Broneering {Id} märgiti NoShow, laud {Table} vabanes", r.Id, r.Table?.Number);
            await _realtime.TableStatusChangedAsync(ToDto(r));
        }
        return new NoShowResultDto(list.Count, list.Select(r => r.Id).ToList(), unpaid);
    }

    // ---------- Abimeetodid ----------

    /// <summary>Õhtu detailid: tähtpäev, kingitus, peakülaline, keel, lisateenused ja laua valimise tasu.</summary>
    private static void ApplyEvening(Reservation r, CreateReservationDto dto)
    {
        r.Occasion = dto.Occasion;
        // Kingitus ainult tähtpäeva puhul
        r.Gift = dto.Occasion == Occasion.None ? ChefGift.None : dto.Gift;
        r.HostType = dto.HostType;
        r.HostName = dto.HostType == HostType.Other ? Clean(dto.HostName) : null;
        r.AddressForm = dto.AddressForm;
        r.ServiceLanguage = dto.ServiceLanguage is "et" or "ru" or "en" ? dto.ServiceLanguage : "et";
        r.Allergies = dto.Allergies is { Count: > 0 } ? string.Join(',', dto.Allergies.Distinct()) : null;
        r.AllergyNote = Clean(dto.AllergyNote);
        r.SeatFee = BookingRules.SeatFee(dto.TableId is not null && dto.ChosenByGuest, TimeOnly.FromDateTime(r.StartTime));
        AddExtras(r, dto.Extras, locked: true);
    }

    /// <summary>Lisab lisateenused kataloogist (hind tellimise hetkel).</summary>
    private static void AddExtras(Reservation r, IEnumerable<string>? codes, bool locked)
    {
        foreach (var code in (codes ?? Enumerable.Empty<string>()).Distinct())
        {
            if (r.Extras.Any(x => x.Code == code)) continue;
            var info = ExtrasCatalog.Find(code) ?? throw new BusinessRuleException($"Tundmatu lisateenus \"{code}\".");
            r.Extras.Add(new ReservationExtra { Code = info.Code, Name = info.Name, Price = info.Price, Locked = locked });
        }
    }

    /// <summary>Kaardiga makstud summa: kõik peale pärast makset lisatud lisateenuste (need makstakse kohapeal).</summary>
    private static decimal PaidOf(Reservation r) =>
        r.IsPaid ? TotalOf(r) - r.Extras.Where(e => !e.Locked).Sum(e => e.Price) - r.OnSitePreOrder : 0m;

    /// <summary>Kohapeal makstav osa: pärast broneerimist lisatud lisateenused.
    /// Makset ootava (makselingiga) broneeringu summa makstakse lingi kaudu, mitte kohapeal.</summary>
    private static decimal OnSiteOf(Reservation r) =>
        r.Status == ReservationStatus.PendingPayment ? 0m : r.Extras.Where(e => !e.Locked).Sum(e => e.Price) + r.OnSitePreOrder;

    private static decimal TotalOf(Reservation r) =>
        r.PreOrderItems.Sum(p => p.UnitPrice * p.Quantity) + r.Extras.Sum(e => e.Price) + r.SeatFee;

    /// <summary>Kontrollib menüüpunktid ja asendab broneeringu eeltellimuse.</summary>
    private async Task ApplyPreOrderAsync(Reservation r, int restaurantId, List<PreOrderLineDto> items)
    {
        await ResolveNamesAsync(restaurantId, items);
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

    /// <summary>Read, kus roog on antud nimega (mitte id-ga): leiab selle restorani menüüst roa id.</summary>
    private async Task ResolveNamesAsync(int restaurantId, List<PreOrderLineDto> items)
    {
        foreach (var line in items.Where(i => i.MenuItemId <= 0))
        {
            var name = line.Name?.Trim();
            if (string.IsNullOrEmpty(name))
                throw new BusinessRuleException("Igal eeltellimuse real peab olema menuItemId või roa nimi (name).");
            var found = (await _menu.GetPagedAsync(new MenuQuery { RestaurantId = restaurantId, Search = name, PageSize = 100 })).Items;
            var exact = found.Where(m => string.Equals(m.Name, name, StringComparison.OrdinalIgnoreCase)).ToList();
            var pick = exact.Count == 1 ? exact[0] : found.Count == 1 ? found[0] : null;
            if (pick is null)
                throw found.Count == 0
                    ? new NotFoundException($"Rooga \"{name}\" selle restorani menüüs ei ole.")
                    : new BusinessRuleException($"\"{name}\" sobib mitmele roale: {string.Join(", ", found.Select(m => m.Name))}. Kirjutage täpsem nimi.");
            line.MenuItemId = pick.Id;
        }
    }

    private (DateTime Start, DateTime End) ValidateTime(Restaurant restaurant, DateOnly date, TimeOnly time, int guests)
    {
        if (guests < 1 || guests > 20)
            throw new BusinessRuleException("Inimeste arv peab olema 1 kuni 20.");

        var start = date.ToDateTime(time);
        // Laud jääb külalisele kogu õhtuks (kuni sulgemiseni), isegi kui ta on seal 2 tundi
        var end = BookingRules.EveningEnd(date, restaurant.OpensAt, restaurant.ClosesAt);
        var last = BookingRules.LastArrival(restaurant.OpensAt, restaurant.ClosesAt);

        if (start <= _clock.Now)
            throw new BusinessRuleException("Broneeringu aeg ei saa olla minevikus.");
        if (date > DateOnly.FromDateTime(_clock.Now).AddDays(BookingRules.MaxDaysAhead))
            throw new BusinessRuleException("Broneerida saab kuni 6 kuud ette.");
        if (time < restaurant.OpensAt || time > last)
            throw new BusinessRuleException(
                $"Saabuda saab {restaurant.OpensAt:HH\\:mm} kuni {last:HH\\:mm}. Restoran on avatud {restaurant.OpensAt:HH\\:mm}-{restaurant.ClosesAt:HH\\:mm}.");

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
        // Laud on hõivatud kogu päeva: kontrollime päeva algusest õhtu lõpuni
        start = BookingRules.DayStart(start);
        var occupying = await _reservations.GetOccupyingAsync(tables.Select(t => t.Id), start, end, excludeId);
        var busy = occupying
            .Where(r => BookingRules.Occupies(r.Status) && BookingRules.Overlaps(start, end, r.StartTime, r.EndTime))
            .Select(r => r.TableId)
            .ToHashSet();
        // Gastronoomilise õhtu päeval on õhtu saal (või kogu restoran) tavaliseks broneerimiseks suletud
        if (tables.Count > 0)
        {
            var events = await _reservations.GetEventsAsync(tables[0].RestaurantId, start, start.AddDays(1)) ?? new List<GastroEvent>();
            busy.UnionWith(tables.Where(t => ClosedByEvent(events, t)).Select(t => t.Id));
        }
        return busy;
    }

    /// <summary>Kas laud on õhtu tõttu suletud (õhtu kogu restoranis või selle laua saalis).</summary>
    private static bool ClosedByEvent(IEnumerable<GastroEvent> events, DiningTable t) =>
        events.Any(e => e.Zone is null || e.Zone == t.Location);

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

    /// <summary>Kui restorani ei antud (nt GET /api/tables/available?date=&amp;time=&amp;guests=), kasutatakse esimest restorani.</summary>
    private async Task<int> DefaultRestaurant(int id)
    {
        if (id > 0) return id;
        var first = (await _restaurants.GetAllAsync(null)).OrderBy(r => r.Id).FirstOrDefault()
                    ?? throw new NotFoundException("Restorane pole.");
        return first.Id;
    }

    private async Task<Restaurant> GetRestaurant(int id) =>
        await _restaurants.GetByIdAsync(id) ?? throw new NotFoundException($"Restorani {id} ei leitud.");

    private int RequireUser() => _current.UserId ?? throw new UnauthorizedException("Pole sisse logitud.");

    private static NotFoundException NotFound(int id) => new($"Broneeringut {id} ei leitud.");

    private static string? Clean(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    private ReservationDto ToDto(Reservation r)
    {
        var items = r.PreOrderItems.Select(p => new PreOrderItemDto(
            p.MenuItemId, L.Word(p.MenuItem?.Name ?? "", _current.Language), p.Quantity, p.UnitPrice, p.UnitPrice * p.Quantity)).ToList();

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
            r.CreatedAt,
            r.Occasion,
            r.Gift,
            r.HostType,
            r.HostName,
            r.AddressForm,
            r.ServiceLanguage,
            r.SeatFee,
            r.Extras.Select(e => new ExtraDto(e.Code, e.Name, e.Price, e.Locked)).ToList(),
            r.Extras.Sum(e => e.Price),
            TotalOf(r),
            r.Messages.Count,
            r.Messages.OrderBy(m => m.CreatedAt).LastOrDefault()?.FromStaff ?? false,
            r.CancelledAt,
            r.RefundAmount,
            (r.Allergies ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
            r.AllergyNote,
            r.User?.Email,
            r.PaymentToken,
            PaidOf(r),
            OnSiteOf(r),
            r.User?.Language ?? "et");
    }
}
