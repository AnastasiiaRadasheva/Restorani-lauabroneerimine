using Restoran.Application.Entities;
using Restoran.Application.Interfaces;

namespace Restoran.Application.Services;

public record CountItem(string Label, int Reservations, int Guests);

/// <summary>Statistika hõivatumatest päevadest ja kellaaegadest.</summary>
public record StatsDto(
    DateOnly From,
    DateOnly To,
    int TotalReservations,
    int TotalGuests,
    double NoShowPercent,
    int Arrived,
    int NoShow,
    int Upcoming,
    int Cancelled,
    List<CountItem> ByWeekday,
    List<CountItem> ByHour,
    List<CountItem> BusiestDates);

public interface IStatsService
{
    Task<StatsDto> GetAsync(int? restaurantId, DateOnly? from, DateOnly? to);
}

public class StatsService : IStatsService
{
    private static readonly string[] WeekdayNames = ["Esmaspäev", "Teisipäev", "Kolmapäev", "Neljapäev", "Reede", "Laupäev", "Pühapäev"];

    private readonly IReservationRepository _reservations;
    private readonly IClock _clock;

    public StatsService(IReservationRepository reservations, IClock clock)
    {
        _reservations = reservations;
        _clock = clock;
    }

    public async Task<StatsDto> GetAsync(int? restaurantId, DateOnly? from, DateOnly? to)
    {
        var today = DateOnly.FromDateTime(_clock.Now);
        var start = from ?? today.AddDays(-60);
        var end = to ?? today.AddDays(14);

        // Tühistatud broneeringud ei lähe arvesse (neid loetakse eraldi)
        var all = await _reservations.GetForStatsAsync(restaurantId, start, end);
        var list = all.Where(r => r.Status != ReservationStatus.Cancelled).ToList();

        // Nädalapäevad esmaspäevast pühapäevani (DayOfWeek algab pühapäevast)
        var byWeekday = Enumerable.Range(0, 7).Select(i =>
        {
            var day = (DayOfWeek)((i + 1) % 7);
            var items = list.Where(r => r.StartTime.DayOfWeek == day).ToList();
            return new CountItem(WeekdayNames[i], items.Count, items.Sum(r => r.Guests));
        }).ToList();

        var byHour = list
            .GroupBy(r => r.StartTime.Hour)
            .OrderBy(g => g.Key)
            .Select(g => new CountItem($"{g.Key:00}:00", g.Count(), g.Sum(r => r.Guests)))
            .ToList();

        var busiestDates = list
            .GroupBy(r => r.StartTime.Date)
            .OrderByDescending(g => g.Count())
            .ThenBy(g => g.Key)
            .Take(5)
            .Select(g => new CountItem(g.Key.ToString("yyyy-MM-dd"), g.Count(), g.Sum(r => r.Guests)))
            .ToList();

        var finished = list.Where(r => r.Status is ReservationStatus.Arrived or ReservationStatus.NoShow).ToList();
        double noShow = finished.Count == 0 ? 0 : Math.Round(100.0 * finished.Count(r => r.Status == ReservationStatus.NoShow) / finished.Count, 1);

        return new StatsDto(start, end, list.Count, list.Sum(r => r.Guests), noShow,
            Arrived: list.Count(r => r.Status == ReservationStatus.Arrived),
            NoShow: list.Count(r => r.Status == ReservationStatus.NoShow),
            Upcoming: list.Count(r => r.Status is ReservationStatus.Active or ReservationStatus.PendingPayment),
            Cancelled: all.Count(r => r.Status == ReservationStatus.Cancelled),
            ByWeekday: byWeekday, ByHour: byHour, BusiestDates: busiestDates);
    }
}
