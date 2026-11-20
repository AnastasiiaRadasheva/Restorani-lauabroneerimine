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
    [InlineData(true, 17, 30, 10)]  // ise valib väljaspool tipptunde
    [InlineData(true, 18, 0, 30)]   // tipptund algab 18:00
    [InlineData(true, 20, 0, 30)]   // 20:00 on veel tipptund
    [InlineData(true, 20, 30, 10)]
    public void SeatFee_DependsOnChoiceAndPeakHours(bool chosenByGuest, int hour, int minute, int expected)
    {
        Assert.Equal(expected, BookingRules.SeatFee(chosenByGuest, new TimeOnly(hour, minute)));
    }

    private static readonly DateTime Now = new(2026, 11, 20, 12, 0, 0);
    private readonly Mock<IEventRepository> _events = new();
    private readonly Mock<ICurrentUser> _current = new();
    private readonly Mock<IClock> _clock = new();

    public EveningTests()
    {
        _clock.Setup(c => c.Now).Returns(Now);
        _current.Setup(c => c.UserId).Returns(5);
    }

    private EventService Service() => new(_events.Object, _current.Object, _clock.Object, NullLogger<EventService>.Instance);

    private GastroEvent Event(string? rule, int capacity = 10) => new()
    {
        Id = 1, Title = "Must ball", StartTime = Now.AddDays(3), Capacity = capacity, Rule = rule
    };

    [Fact]
    public async Task Join_WithDressCodeRule_RequiresAcceptingRules()
    {
        _events.Setup(e => e.GetWithRegistrationsAsync(1)).ReturnsAsync(Event("black tie"));

        await Assert.ThrowsAsync<BusinessRuleException>(() => Service().JoinAsync(1, new JoinEventDto { Guests = 2, AcceptRules = false }));
        _events.Verify(e => e.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Join_Throws409_WhenNotEnoughSeats()
    {
        var ev = Event(null, capacity: 4);
        ev.Registrations.Add(new EventRegistration { UserId = 9, Guests = 3 });
        _events.Setup(e => e.GetWithRegistrationsAsync(1)).ReturnsAsync(ev);

        await Assert.ThrowsAsync<ConflictException>(() => Service().JoinAsync(1, new JoinEventDto { Guests = 2 }));
    }

    [Fact]
    public async Task Join_SavesRegistration_AndReturnsSeatsLeft()
    {
        _events.Setup(e => e.GetWithRegistrationsAsync(1)).ReturnsAsync(Event("valge riietus"));

        var result = await Service().JoinAsync(1, new JoinEventDto { Guests = 2, AcceptRules = true });

        Assert.Equal(2, result.MyGuests);
        Assert.Equal(8, result.SeatsLeft);
        _events.Verify(e => e.SaveChangesAsync(), Times.Once);
    }
}
