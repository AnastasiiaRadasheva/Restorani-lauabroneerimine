using Moq;
using Restoran.Application.Entities;
using Restoran.Application.Interfaces;
using Restoran.Application.Services;

namespace Restoran.Tests;

public class StatsServiceTests
{
    [Fact]
    public async Task Stats_GroupsByWeekdayAndHour_AndIgnoresCancelled()
    {
        var friday = new DateTime(2026, 11, 6, 19, 0, 0);   // reede
        var saturday = new DateTime(2026, 11, 7, 19, 0, 0); // laupäev
        var repo = new Mock<IReservationRepository>();
        repo.Setup(r => r.GetForStatsAsync(null, It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
            .ReturnsAsync(new List<Reservation>
            {
                new() { StartTime = friday, Guests = 2, Status = ReservationStatus.Arrived },
                new() { StartTime = saturday, Guests = 4, Status = ReservationStatus.Arrived },
                new() { StartTime = saturday, Guests = 3, Status = ReservationStatus.NoShow },
                new() { StartTime = saturday.AddHours(-5), Guests = 2, Status = ReservationStatus.Cancelled },
            });
        var clock = new Mock<IClock>();
        clock.Setup(c => c.Now).Returns(new DateTime(2026, 11, 9));

        var stats = await new StatsService(repo.Object, clock.Object).GetAsync(null, null, null);

        Assert.Equal(3, stats.TotalReservations);
        Assert.Equal(2, stats.ByWeekday.Single(d => d.Label == "Laupäev").Reservations);
        Assert.Equal(3, stats.ByHour.Single(h => h.Label == "19:00").Reservations);
        Assert.Equal("2026-11-07", stats.BusiestDates.First().Label);
        Assert.Equal(33.3, stats.NoShowPercent);
    }
}
