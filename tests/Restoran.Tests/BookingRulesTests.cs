using Restoran.Application.Entities;
using Restoran.Application.Services;

namespace Restoran.Tests;

/// <summary>Broneerimise reeglite testid (ilma andmebaasita).</summary>
public class BookingRulesTests
{
    private static readonly DateTime Day = new(2026, 12, 1);

    private static DiningTable Table(int id, int seats, bool active = true) =>
        new() { Id = id, Number = id, Seats = seats, IsActive = active };

    [Fact]
    public void Overlaps_ReturnsTrue_WhenTimesIntersect()
    {
        // 18:00-20:00 ja 19:00-21:00
        var result = BookingRules.Overlaps(Day.AddHours(18), Day.AddHours(20), Day.AddHours(19), Day.AddHours(21));
        Assert.True(result);
    }

    [Fact]
    public void Overlaps_ReturnsFalse_WhenOneEndsWhenOtherStarts()
    {
        // 18:00-20:00 ja 20:00-22:00 ei kattu
        var result = BookingRules.Overlaps(Day.AddHours(18), Day.AddHours(20), Day.AddHours(20), Day.AddHours(22));
        Assert.False(result);
    }

    [Theory]
    [InlineData(ReservationStatus.Active, true)]
    [InlineData(ReservationStatus.Arrived, true)]
    [InlineData(ReservationStatus.Cancelled, false)]
    [InlineData(ReservationStatus.NoShow, false)]
    public void Occupies_OnlyActiveAndArrivedBlockTable(ReservationStatus status, bool expected)
    {
        Assert.Equal(expected, BookingRules.Occupies(status));
    }

    [Fact]
    public void PickBestTable_ChoosesSmallestTableThatFits()
    {
        var tables = new[] { Table(1, 8), Table(2, 4), Table(3, 2), Table(4, 6) };

        var best = BookingRules.PickBestTable(tables, new HashSet<int>(), guests: 3);

        Assert.NotNull(best);
        Assert.Equal(4, best!.Seats); // mitte 6 ega 8
    }

    [Fact]
    public void PickBestTable_SkipsBusyAndInactiveTables()
    {
        var tables = new[] { Table(1, 2), Table(2, 2, active: false), Table(3, 4) };

        var best = BookingRules.PickBestTable(tables, new HashSet<int> { 1 }, guests: 2);

        Assert.Equal(3, best!.Id);
    }

    [Fact]
    public void PickBestTable_ReturnsNull_WhenGroupIsTooBig()
    {
        var tables = new[] { Table(1, 2), Table(2, 4) };

        var best = BookingRules.PickBestTable(tables, new HashSet<int>(), guests: 10);

        Assert.Null(best);
    }

    [Theory]
    [InlineData(48, true)]
    [InlineData(25, true)]
    [InlineData(23, false)]
    [InlineData(1, false)]
    public void CanCustomerModify_OnlyMoreThan24HoursBefore(int hoursBefore, bool expected)
    {
        var now = Day.AddHours(12);
        var start = now.AddHours(hoursBefore);

        Assert.Equal(expected, BookingRules.CanCustomerModify(start, now));
    }
}
