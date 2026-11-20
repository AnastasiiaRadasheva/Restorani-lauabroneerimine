using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Restoran.Application.Common;
using Restoran.Application.DTOs;
using Restoran.Application.Entities;
using Restoran.Application.Interfaces;
using Restoran.Application.Services;

namespace Restoran.Tests;

/// <summary>ReservationService testid. Repositooriumid on asendatud Moq mockidega.</summary>
public class ReservationServiceTests
{
    private static readonly DateTime Now = new(2026, 12, 1, 10, 0, 0);

    private readonly Mock<IReservationRepository> _reservations = new();
    private readonly Mock<ITableRepository> _tables = new();
    private readonly Mock<IRestaurantRepository> _restaurants = new();
    private readonly Mock<IMenuRepository> _menu = new();
    private readonly Mock<ICurrentUser> _current = new();
    private readonly Mock<IClock> _clock = new();
    private readonly Mock<IRealtimeNotifier> _realtime = new();

    private readonly Restaurant _restaurant = new()
    {
        Id = 1, Name = "Aed", OpensAt = new TimeOnly(12, 0), ClosesAt = new TimeOnly(23, 0)
    };

    private readonly List<DiningTable> _tableList;

    public ReservationServiceTests()
    {
        _tableList = new List<DiningTable>
        {
            new() { Id = 1, RestaurantId = 1, Number = 1, Seats = 2, IsActive = true, Restaurant = _restaurant },
            new() { Id = 2, RestaurantId = 1, Number = 2, Seats = 4, IsActive = true, Restaurant = _restaurant },
            new() { Id = 3, RestaurantId = 1, Number = 3, Seats = 8, IsActive = true, Restaurant = _restaurant },
        };

        _clock.Setup(c => c.Now).Returns(Now);
        _current.Setup(c => c.UserId).Returns(10);
        _current.Setup(c => c.IsStaff).Returns(false);
        _restaurants.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(_restaurant);
        _tables.Setup(t => t.GetByRestaurantAsync(1, true)).ReturnsAsync(_tableList);
        _reservations
            .Setup(r => r.GetOccupyingAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int?>()))
            .ReturnsAsync(new List<Reservation>());
    }

    private ReservationService CreateService() => new(
        _reservations.Object, _tables.Object, _restaurants.Object, _menu.Object,
        _current.Object, _clock.Object, NullLogger<ReservationService>.Instance, _realtime.Object);

    private static CreateReservationDto Request(int guests, int? tableId = null) => new()
    {
        RestaurantId = 1,
        Date = new DateOnly(2026, 12, 5),
        Time = new TimeOnly(19, 0),
        Guests = guests,
        TableId = tableId
    };

    [Fact]
    public async Task Create_Throws409_WhenChosenTableIsAlreadyBooked()
    {
        // Laual 2 on juba broneering 18:00-20:00
        _reservations
            .Setup(r => r.GetOccupyingAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int?>()))
            .ReturnsAsync(new List<Reservation>
            {
                new()
                {
                    TableId = 2, Status = ReservationStatus.Active,
                    StartTime = new DateTime(2026, 12, 5, 18, 0, 0), EndTime = new DateTime(2026, 12, 5, 20, 0, 0)
                }
            });

        var service = CreateService();

        await Assert.ThrowsAsync<ConflictException>(() => service.CreateAsync(Request(guests: 4, tableId: 2)));
        _reservations.Verify(r => r.AddAsync(It.IsAny<Reservation>()), Times.Never);
        _realtime.Verify(n => n.ReservationCreatedAsync(It.IsAny<ReservationDto>()), Times.Never);
    }

    [Fact]
    public async Task Create_AutoPicksSmallestFreeTable_AndSaves()
    {
        Reservation? saved = null;
        _reservations.Setup(r => r.AddAsync(It.IsAny<Reservation>()))
            .Callback<Reservation>(r => { r.Id = 99; saved = r; })
            .Returns(Task.CompletedTask);
        _reservations.Setup(r => r.GetWithDetailsAsync(99))
            .ReturnsAsync(() => { saved!.Table = _tableList.First(t => t.Id == saved.TableId); return saved; });

        var result = await CreateService().CreateAsync(Request(guests: 3));

        Assert.Equal(2, result.TableId); // 4-kohaline, mitte 8-kohaline
        Assert.Equal(new DateTime(2026, 12, 5, 21, 0, 0), result.EndTime); // kestus 2 h
        _reservations.Verify(r => r.SaveChangesAsync(), Times.Once);
        // Personalile saadetakse reaalajas teade
        _realtime.Verify(n => n.ReservationCreatedAsync(It.Is<ReservationDto>(d => d.Id == 99)), Times.Once);
    }

    [Fact]
    public async Task Create_Throws400_WhenTimeIsInThePast()
    {
        var dto = Request(guests: 2);
        dto.Date = new DateOnly(2026, 11, 1);

        await Assert.ThrowsAsync<BusinessRuleException>(() => CreateService().CreateAsync(dto));
    }

    [Fact]
    public async Task Create_Throws400_WhenOutsideOpeningHours()
    {
        var dto = Request(guests: 2);
        dto.Time = new TimeOnly(22, 0); // lõpeks 24:00, aga restoran suleb 23:00

        await Assert.ThrowsAsync<BusinessRuleException>(() => CreateService().CreateAsync(dto));
    }

    [Fact]
    public async Task Cancel_Throws_WhenLessThan24HoursBefore()
    {
        var reservation = new Reservation
        {
            Id = 5, UserId = 10, TableId = 1, Table = _tableList[0], Status = ReservationStatus.Active,
            StartTime = Now.AddHours(5), EndTime = Now.AddHours(7)
        };
        _reservations.Setup(r => r.GetWithDetailsAsync(5)).ReturnsAsync(reservation);

        await Assert.ThrowsAsync<BusinessRuleException>(() => CreateService().CancelAsync(5));
        Assert.Equal(ReservationStatus.Active, reservation.Status);
    }

    [Fact]
    public async Task Cancel_StaffCanCancelEvenLessThan24HoursBefore()
    {
        _current.Setup(c => c.UserId).Returns(1);
        _current.Setup(c => c.IsStaff).Returns(true);
        var reservation = new Reservation
        {
            Id = 5, UserId = 10, TableId = 1, Table = _tableList[0], Status = ReservationStatus.Active,
            StartTime = Now.AddHours(5), EndTime = Now.AddHours(7)
        };
        _reservations.Setup(r => r.GetWithDetailsAsync(5)).ReturnsAsync(reservation);

        var result = await CreateService().CancelAsync(5);

        Assert.Equal(ReservationStatus.Cancelled, result.Status);
    }

    [Fact]
    public async Task GetById_ThrowsForbidden_ForOtherCustomersReservation()
    {
        _reservations.Setup(r => r.GetWithDetailsAsync(7)).ReturnsAsync(new Reservation { Id = 7, UserId = 555 });

        await Assert.ThrowsAsync<ForbiddenException>(() => CreateService().GetByIdAsync(7));
    }

    [Fact]
    public async Task PreOrder_Throws_WhenMenuItemNotAvailable()
    {
        var reservation = new Reservation
        {
            Id = 5, UserId = 10, TableId = 1, Table = _tableList[0], Status = ReservationStatus.Active,
            StartTime = Now.AddDays(3), EndTime = Now.AddDays(3).AddHours(2)
        };
        _reservations.Setup(r => r.GetWithDetailsAsync(5)).ReturnsAsync(reservation);
        _menu.Setup(m => m.GetByIdsAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(new List<MenuItem> { new() { Id = 3, RestaurantId = 1, Name = "Steik", IsAvailable = false, Price = 20 } });

        var dto = new PreOrderDto { Items = { new PreOrderLineDto { MenuItemId = 3, Quantity = 1 } } };

        await Assert.ThrowsAsync<BusinessRuleException>(() => CreateService().SetPreOrderAsync(5, dto));
    }

    [Fact]
    public async Task Slots_ShowsZeroFreeTables_WhenAllFittingTablesAreBusy()
    {
        // 4 inimest mahuvad lauda 2 (4 kohta) ja lauda 3 (8 kohta). Mõlemad on hõivatud 19:00-21:00.
        var day = new DateOnly(2026, 12, 5);
        _reservations
            .Setup(r => r.GetOccupyingAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int?>()))
            .ReturnsAsync(new List<Reservation>
            {
                new() { TableId = 2, Status = ReservationStatus.Active, StartTime = day.ToDateTime(new TimeOnly(19, 0)), EndTime = day.ToDateTime(new TimeOnly(21, 0)) },
                new() { TableId = 3, Status = ReservationStatus.Arrived, StartTime = day.ToDateTime(new TimeOnly(19, 0)), EndTime = day.ToDateTime(new TimeOnly(21, 0)) },
            });

        var result = await CreateService().GetSlotsAsync(1, day, 1, guests: 4);

        var slots = result.Single().Slots;
        Assert.Equal("12:00", slots.First().Time);
        Assert.Equal("21:00", slots.Last().Time); // viimane algus 2 h enne sulgemist (23:00)
        Assert.Equal(0, slots.Single(s => s.Time == "19:00").FreeTables);
        Assert.Equal(0, slots.Single(s => s.Time == "18:00").FreeTables); // 18-20 kattub 19-21-ga
        Assert.Equal(2, slots.Single(s => s.Time == "17:00").FreeTables);
    }

    [Fact]
    public async Task Create_WithItems_SavesPreOrderAtPriceOfTheMoment()
    {
        Reservation? saved = null;
        _menu.Setup(m => m.GetByIdsAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(new List<MenuItem> { new() { Id = 5, RestaurantId = 1, Name = "Austrid", Price = 32m, IsAvailable = true } });
        _reservations.Setup(r => r.AddAsync(It.IsAny<Reservation>()))
            .Callback<Reservation>(r => { r.Id = 7; saved = r; })
            .Returns(Task.CompletedTask);
        _reservations.Setup(r => r.GetWithDetailsAsync(7)).ReturnsAsync(() => saved);

        var request = Request(guests: 2);
        request.Items = new List<PreOrderLineDto> { new() { MenuItemId = 5, Quantity = 2 } };
        var result = await CreateService().CreateAsync(request);

        Assert.Single(saved!.PreOrderItems);
        Assert.Equal(32m, saved.PreOrderItems[0].UnitPrice);
        Assert.Equal(64m, result.PreOrderTotal);
    }

    private Reservation OwnedReservation(int userId) => new()
    {
        Id = 50, UserId = userId, TableId = 1, Status = ReservationStatus.Active,
        StartTime = Now.AddHours(5), EndTime = Now.AddHours(7), // alla 24 h
        Table = _tableList[0], User = new User { Id = userId, Name = "Kati Klient" }
    };

    [Fact]
    public async Task AddMessage_FromCustomer_IsAllowedEvenWithin24Hours()
    {
        var r = OwnedReservation(10);
        _reservations.Setup(x => x.GetWithDetailsAsync(50)).ReturnsAsync(r);

        var msg = await CreateService().AddMessageAsync(50, new NewMessageDto { Text = "  Kas tort saab olla pähkliteta?  " });

        Assert.False(msg.FromStaff);
        Assert.Equal("Kati Klient", msg.AuthorName);
        Assert.Equal("Kas tort saab olla pähkliteta?", msg.Text);
        Assert.Single(r.Messages);
    }

    [Fact]
    public async Task AddMessage_ToSomeoneElsesReservation_IsForbidden()
    {
        _reservations.Setup(x => x.GetWithDetailsAsync(50)).ReturnsAsync(OwnedReservation(99));

        await Assert.ThrowsAsync<ForbiddenException>(() => CreateService().AddMessageAsync(50, new NewMessageDto { Text = "Tere" }));
    }

    [Fact]
    public async Task SetExtras_ReplacesExtras_AndUpdatesTotal()
    {
        var r = OwnedReservation(10);
        r.Extras.Add(new ReservationExtra { Code = "violin", Name = "Viiuldaja", Price = 120m });
        _reservations.Setup(x => x.GetWithDetailsAsync(50)).ReturnsAsync(r);

        var result = await CreateService().SetExtrasAsync(50, new SetExtrasDto { Extras = new() { "flowers", "chair" } });

        Assert.Equal(new[] { "flowers", "chair" }, result.Extras.Select(e => e.Code));
        Assert.Equal(45m, result.Total);
    }

    [Fact]
    public async Task MarkNoShows_MarksLateActiveReservations_AndNotifies()
    {
        var late = new Reservation { Id = 1, TableId = 1, Table = _tableList[0], Status = ReservationStatus.Active, StartTime = Now.AddMinutes(-45), EndTime = Now.AddMinutes(75) };
        _reservations.Setup(r => r.GetNoShowCandidatesAsync(Now.AddMinutes(-30))).ReturnsAsync(new List<Reservation> { late });

        var result = await CreateService().MarkNoShowsAsync();

        Assert.Equal(1, result.Count);
        Assert.Equal(ReservationStatus.NoShow, late.Status);
        _realtime.Verify(n => n.TableStatusChangedAsync(It.IsAny<ReservationDto>()), Times.Once);
    }
}
