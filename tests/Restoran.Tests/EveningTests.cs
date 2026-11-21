using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Restoran.Application.Common;
using Restoran.Application.DTOs;
using Restoran.Application.Entities;
using Restoran.Application.Interfaces;
using Restoran.Application.Services;

namespace Restoran.Tests;

/// <summary>Laua valimise tasu ja gastronoomiliste õhtute reeglid.</summary>
public class EveningTests
{
    [Theory]
    [InlineData(false, 19, 0, 0)]   // metrdotel valib – tasuta
    [InlineData(true, 17, 30, 30)]  // ise valib väljaspool tipptunde
    [InlineData(true, 18, 0, 40)]   // tipptund algab 18:00
    [InlineData(true, 20, 0, 40)]   // 20:00 on veel tipptund
    [InlineData(true, 20, 30, 30)]
    public void SeatFee_DependsOnChoiceAndPeakHours(bool chosenByGuest, int hour, int minute, int expected)
    {
        Assert.Equal(expected, BookingRules.SeatFee(chosenByGuest, new TimeOnly(hour, minute)));
    }

    private static readonly DateTime Now = new(2026, 11, 20, 12, 0, 0);
    private readonly Mock<IEventRepository> _events = new();
    private readonly Mock<ITableRepository> _tables = new();
    private readonly Mock<ICurrentUser> _current = new();
    private readonly Mock<IClock> _clock = new();

    public EveningTests()
    {
        _clock.Setup(c => c.Now).Returns(Now);
        _current.Setup(c => c.UserId).Returns(5);
    }

    private readonly Mock<IRestaurantRepository> _restaurants = new();
    private static readonly FakePaymentDto Card = new() { CardHolder = "Kati", CardNumber = "4242 4242 4242 4242", Expiry = "12/28", Cvc = "123" };

    private EventService Service() => new(_events.Object, _tables.Object, new Mock<IReservationRepository>().Object,
        _restaurants.Object, new Mock<IMenuRepository>().Object, _current.Object, _clock.Object, NullLogger<EventService>.Instance);

    private GastroEvent Event(string? rule, int capacity = 10, double daysAhead = 3) => new()
    {
        Id = 1, Code = "black", Title = "Must ball", StartTime = Now.AddDays(daysAhead), Capacity = capacity, Rule = rule, PricePerGuest = 100m,
        Items = { new EventMenuItem { Id = 7, Code = "espresso", Name = "Espresso Martini", Price = 16m } }
    };

    [Fact]
    public async Task Join_WithDressCodeRule_RequiresAcceptingRules()
    {
        _events.Setup(e => e.GetWithRegistrationsAsync(1)).ReturnsAsync(Event("black tie"));

        await Assert.ThrowsAsync<BusinessRuleException>(() => Service().JoinAsync(1, new JoinEventDto { Guests = 2, AcceptRules = false, Card = Card }));
        _events.Verify(e => e.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Join_Throws409_WhenNotEnoughSeats()
    {
        var ev = Event(null, capacity: 4);
        ev.Registrations.Add(new EventRegistration { UserId = 9, Guests = 3 });
        _events.Setup(e => e.GetWithRegistrationsAsync(1)).ReturnsAsync(ev);

        await Assert.ThrowsAsync<ConflictException>(() => Service().JoinAsync(1, new JoinEventDto { Guests = 2, Card = Card }));
    }

    [Fact]
    public async Task Join_Self_PaysImmediately()
    {
        _events.Setup(e => e.GetWithRegistrationsAsync(1)).ReturnsAsync(Event("valge riietus"));

        // Ilma kaardita ei saa ise registreeruda
        await Assert.ThrowsAsync<BusinessRuleException>(() => Service().JoinAsync(1, new JoinEventDto { Guests = 2, AcceptRules = true }));

        _events.Setup(e => e.GetWithRegistrationsAsync(1)).ReturnsAsync(Event("valge riietus"));
        var result = await Service().JoinAsync(1, new JoinEventDto { Guests = 2, AcceptRules = true, Card = Card });
        Assert.Equal(2, result.Guests);
        Assert.True(result.IsPaid);
        Assert.Equal("Active", result.Status);
    }

    [Fact]
    public async Task Join_ClosesTwelveHoursBeforeEvening()
    {
        _events.Setup(e => e.GetWithRegistrationsAsync(1)).ReturnsAsync(Event(null, daysAhead: 0.4)); // ~10 h pärast
        await Assert.ThrowsAsync<BusinessRuleException>(() => Service().JoinAsync(1, new JoinEventDto { Guests = 2, Card = Card }));
    }

    [Fact]
    public async Task JoinForCustomer_ByStaff_WaitsForPayment()
    {
        _current.Setup(c => c.IsStaff).Returns(true);
        _events.Setup(e => e.GetWithRegistrationsAsync(1)).ReturnsAsync(Event(null));

        var result = await Service().JoinForCustomerAsync(1, 20, new JoinEventDto { Guests = 2 });

        Assert.Equal("PendingPayment", result.Status);
        Assert.False(result.IsPaid);
        Assert.NotNull(result.PaymentToken);
    }

    [Fact]
    public async Task CancelUnpaid_CancelsPendingRegistration24HoursBefore()
    {
        var ev = Event(null, daysAhead: 0.8); // õhtu ~19 h pärast
        var reg = new EventRegistration { Id = 3, Event = ev, UserId = 20, Guests = 2, Status = ReservationStatus.PendingPayment, CreatedAt = Now.AddDays(-5) };
        _events.Setup(e => e.GetPendingBeforeAsync(It.IsAny<DateTime>())).ReturnsAsync(new List<EventRegistration> { reg });

        var cancelled = await Service().CancelUnpaidAsync();

        Assert.Single(cancelled);
        Assert.Equal(20, cancelled[0].UserId);
        Assert.Equal(ReservationStatus.Cancelled, reg.Status);
    }

    [Fact]
    public async Task Create_RequiresDateAtLeastTwoWeeksAhead()
    {
        _current.Setup(c => c.IsStaff).Returns(true);
        _restaurants.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Restaurant { Id = 1, OpensAt = new TimeOnly(17, 0), ClosesAt = new TimeOnly(23, 30) });
        _tables.Setup(t => t.GetByRestaurantAsync(1, true)).ReturnsAsync(new List<DiningTable> { new() { Id = 1, Seats = 10, Location = TableLocation.Hall } });
        var dto = new SaveEventDto { RestaurantId = 1, Title = "Trühvliõhtu", StartTime = Now.Date.AddDays(7).AddHours(19), Capacity = 10, PricePerGuest = 90 };

        await Assert.ThrowsAsync<BusinessRuleException>(() => Service().CreateAsync(dto));

        dto.Capacity = 11; // rohkem kui saalis kohti
        dto.StartTime = Now.Date.AddDays(15).AddHours(19);
        await Assert.ThrowsAsync<BusinessRuleException>(() => Service().CreateAsync(dto));
    }

    [Fact]
    public void EveningEnd_AfterMidnight_IsNextDay()
    {
        var end = BookingRules.EveningEnd(new DateOnly(2026, 11, 20), new TimeOnly(17, 0), new TimeOnly(3, 0));
        Assert.Equal(new DateTime(2026, 11, 21, 3, 0, 0), end);
        Assert.Equal(new TimeOnly(23, 30), BookingRules.LastArrival(new TimeOnly(17, 0), new TimeOnly(3, 0)));
        Assert.Equal(new TimeOnly(21, 30), BookingRules.LastArrival(new TimeOnly(17, 0), new TimeOnly(23, 30)));
    }

    [Fact]
    public async Task Join_WithEventDrinks_AddsThemToTotal_AndRejectsViolinist()
    {
        _events.Setup(e => e.GetWithRegistrationsAsync(1)).ReturnsAsync(Event(null));
        var dto = new JoinEventDto { Guests = 2, Drinks = new() { new() { ItemId = 7, Quantity = 2 } }, Extras = new() { "flowers" }, Card = Card };

        var result = await Service().JoinAsync(1, dto);
        Assert.Equal(2 * 100m + 2 * 16m + 45m, result.Total);

        _events.Setup(e => e.GetWithRegistrationsAsync(1)).ReturnsAsync(Event(null));
        await Assert.ThrowsAsync<BusinessRuleException>(() => Service().JoinAsync(1, new JoinEventDto { Guests = 2, Extras = new() { "violin" }, Card = Card }));
    }
}
