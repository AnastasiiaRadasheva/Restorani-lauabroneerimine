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
        Assert.Equal(new DateTime(2026, 12, 5, 23, 0, 0), result.EndTime); // laud on hõivatud kuni sulgemiseni
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
    public async Task Cancel_LessThan24Hours_WithoutConsent_Throws()
    {
        var reservation = new Reservation
        {
            Id = 5, UserId = 10, TableId = 1, Table = _tableList[0], Status = ReservationStatus.Active,
            StartTime = Now.AddHours(5), EndTime = Now.AddHours(7), IsPaid = true, SeatFee = 30m
        };
        _reservations.Setup(r => r.GetWithDetailsAsync(5)).ReturnsAsync(reservation);

        await Assert.ThrowsAsync<BusinessRuleException>(() => CreateService().CancelAsync(5));
        Assert.Equal(ReservationStatus.Active, reservation.Status);
    }

    [Fact]
    public async Task Cancel_LessThan24Hours_NothingPaid_NeedsNoConsent()
    {
        var reservation = new Reservation
        {
            Id = 5, UserId = 10, TableId = 1, Table = _tableList[0], Status = ReservationStatus.Active,
            StartTime = Now.AddHours(5), EndTime = Now.AddHours(7)
        };
        _reservations.Setup(r => r.GetWithDetailsAsync(5)).ReturnsAsync(reservation);

        await CreateService().CancelAsync(5);
        Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
    }

    [Fact]
    public async Task Cancel_LessThan24Hours_WithConsent_CancelsWithoutRefund()
    {
        var reservation = new Reservation
        {
            Id = 5, UserId = 10, TableId = 1, Table = _tableList[0], Status = ReservationStatus.Active,
            StartTime = Now.AddHours(5), EndTime = Now.AddHours(7), IsPaid = true, SeatFee = 30m
        };
        _reservations.Setup(r => r.GetWithDetailsAsync(5)).ReturnsAsync(reservation);

        var result = await CreateService().CancelAsync(5, new CancelReservationDto { AcceptNoRefund = true });

        Assert.Equal(ReservationStatus.Cancelled, result.Status);
        Assert.Equal(0m, result.RefundAmount);
    }

    [Fact]
    public async Task Cancel_MoreThan24Hours_RefundsPaidAmount()
    {
        var reservation = new Reservation
        {
            Id = 5, UserId = 10, TableId = 1, Table = _tableList[0], Status = ReservationStatus.Active,
            StartTime = Now.AddDays(3), EndTime = Now.AddDays(3).AddHours(2), IsPaid = true, SeatFee = 30m
        };
        _reservations.Setup(r => r.GetWithDetailsAsync(5)).ReturnsAsync(reservation);

        var result = await CreateService().CancelAsync(5);

        Assert.Equal(30m, result.RefundAmount);
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
    public async Task Slots_TableIsBusyTheWholeEvening()
    {
        // 4 inimest mahuvad lauda 2 (4 kohta) ja lauda 3 (8 kohta). Mõlemad on hõivatud 19:00-21:00.
        var day = new DateOnly(2026, 12, 5);
        _reservations
            .Setup(r => r.GetOccupyingAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int?>()))
            .ReturnsAsync(new List<Reservation>
            {
                new() { TableId = 2, Status = ReservationStatus.Active, StartTime = day.ToDateTime(new TimeOnly(19, 0)), EndTime = day.ToDateTime(new TimeOnly(23, 0)) },
                new() { TableId = 3, Status = ReservationStatus.Arrived, StartTime = day.ToDateTime(new TimeOnly(19, 0)), EndTime = day.ToDateTime(new TimeOnly(23, 0)) },
            });

        var result = await CreateService().GetSlotsAsync(1, day, 1, guests: 4);

        var slots = result.Single().Slots;
        Assert.Equal("12:00", slots.First().Time);
        Assert.Equal("21:00", slots.Last().Time); // viimane saabumine 2 h enne sulgemist (23:00)
        // Laud on hõivatud kogu õhtu: kui laudu 2 ja 3 on õhtul broneeritud, pole ka 17:00 vaba
        Assert.All(slots, s => Assert.Equal(0, s.FreeTables));
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
        // Ilma kaardita maksmisega broneeringut ei tehta
        await Assert.ThrowsAsync<BusinessRuleException>(() => CreateService().CreateAsync(request));
        request.Card = new FakePaymentDto { CardHolder = "Kati", CardNumber = "4242 4242 4242 4242", Expiry = "12/28", Cvc = "123" };
        var result = await CreateService().CreateAsync(request);
        Assert.True(saved!.IsPaid);

        Assert.Single(saved!.PreOrderItems);
        Assert.Equal(32m, saved.PreOrderItems[0].UnitPrice);
        Assert.Equal(64m, result.PreOrderTotal);
    }

    [Fact]
    public async Task Slots_TableBookedLateIsClosedForTheWholeDay()
    {
        // Laud 2 on broneeritud alles 21:00 – ometi pole see vaba ka 17:00 (laud on suletud kogu päevaks)
        var day = new DateOnly(2026, 12, 5);
        _reservations
            .Setup(r => r.GetOccupyingAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int?>()))
            .ReturnsAsync(new List<Reservation>
            {
                new() { TableId = 2, Status = ReservationStatus.Active, StartTime = day.ToDateTime(new TimeOnly(21, 0)), EndTime = day.ToDateTime(new TimeOnly(23, 0)) },
            });

        var result = await CreateService().GetSlotsAsync(1, day, 1, guests: 4);

        var at17 = result.Single().Slots.Single(s => s.Time == "17:00");
        Assert.Equal(1, at17.FreeTables); // ainult laud 3
    }

    [Fact]
    public void DayStart_NightBookingBelongsToPreviousDay()
    {
        Assert.Equal(new DateTime(2026, 12, 5, 6, 0, 0), BookingRules.DayStart(new DateTime(2026, 12, 5, 19, 0, 0)));
        Assert.Equal(new DateTime(2026, 12, 4, 6, 0, 0), BookingRules.DayStart(new DateTime(2026, 12, 5, 1, 30, 0)));
    }

    [Fact]
    public async Task PreOrder_ChangeAfterPayment_MoreExpensive_RequiresCard()
    {
        var reservation = new Reservation
        {
            Id = 5, UserId = 10, TableId = 1, Table = _tableList[0], Status = ReservationStatus.Active, IsPaid = true,
            StartTime = Now.AddDays(3), EndTime = Now.AddDays(3).AddHours(2)
        };
        _reservations.Setup(r => r.GetWithDetailsAsync(5)).ReturnsAsync(reservation);
        _menu.Setup(m => m.GetByIdsAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(new List<MenuItem> { new() { Id = 3, RestaurantId = 1, Name = "Steik", IsAvailable = true, Price = 20 } });
        var dto = new PreOrderDto { Items = { new PreOrderLineDto { MenuItemId = 3, Quantity = 1 } } };

        await Assert.ThrowsAsync<BusinessRuleException>(() => CreateService().SetPreOrderAsync(5, dto));

        dto.Card = new FakePaymentDto { CardHolder = "Kati", CardNumber = "4242 4242 4242 4242", Expiry = "12/28", Cvc = "123" };
        await CreateService().SetPreOrderAsync(5, dto);
        Assert.True(reservation.IsPaid);
        Assert.Single(reservation.PreOrderItems);
    }

    [Fact]
    public async Task StaffBooking_WithNothingToPay_IsConfirmedImmediately()
    {
        Reservation? saved = null;
        _current.Setup(c => c.IsStaff).Returns(true);
        _reservations.Setup(r => r.AddAsync(It.IsAny<Reservation>()))
            .Callback<Reservation>(r => { r.Id = 9; saved = r; })
            .Returns(Task.CompletedTask);
        _reservations.Setup(r => r.GetWithDetailsAsync(9)).ReturnsAsync(() => saved);

        await CreateService().CreatePendingForUserAsync(20, Request(guests: 2)); // laud valimata -> 0 €

        Assert.Equal(ReservationStatus.Active, saved!.Status);
        Assert.Null(saved.PaymentToken);
    }

    [Fact]
    public async Task Create_ByStaffForThemselves_IsForbidden()
    {
        _current.Setup(c => c.IsStaff).Returns(true);
        await Assert.ThrowsAsync<ForbiddenException>(() => CreateService().CreateAsync(Request(guests: 2)));
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
    public async Task SetExtras_AddsExtras_AndUpdatesTotal()
    {
        var r = OwnedReservation(10);
        r.Extras.Add(new ReservationExtra { Code = "violin", Name = "Viiuldaja", Price = 120m });
        _reservations.Setup(x => x.GetWithDetailsAsync(50)).ReturnsAsync(r);

        var result = await CreateService().SetExtrasAsync(50, new SetExtrasDto { Extras = new() { "violin", "flowers", "chair" } });

        Assert.Equal(new[] { "violin", "flowers", "chair" }, result.Extras.Select(e => e.Code));
        Assert.Equal(165m, result.Total);
    }

    [Fact]
    public async Task SetExtras_PaidAtBookingLocked_LaterAddedRemovable_StaffCanRemove()
    {
        var r = OwnedReservation(10);
        r.StartTime = Now.AddDays(3); r.EndTime = Now.AddDays(3).AddHours(2);
        // Šampanja lisati broneerimisel ja maksti – see on lukus
        r.Extras.Add(new ReservationExtra { Code = "bottle", Name = "Šampanja", Price = 95m, Locked = true });
        _reservations.Setup(x => x.GetWithDetailsAsync(50)).ReturnsAsync(r);

        await Assert.ThrowsAsync<BusinessRuleException>(() => CreateService().SetExtrasAsync(50, new SetExtrasDto { Extras = new() { "flowers" } }));
        await CreateService().SetExtrasAsync(50, new SetExtrasDto { Extras = new() { "bottle", "flowers" } });
        Assert.Equal(2, r.Extras.Count);
        Assert.False(r.Extras.Single(x => x.Code == "flowers").Locked);
        // Hiljem lisatud lilled saab klient ise eemaldada
        await CreateService().SetExtrasAsync(50, new SetExtrasDto { Extras = new() { "bottle" } });
        Assert.Single(r.Extras);
        await CreateService().SetExtrasAsync(50, new SetExtrasDto { Extras = new() { "bottle", "flowers" } });

        _current.Setup(c => c.IsStaff).Returns(true);
        _current.Setup(c => c.UserId).Returns(1);
        await CreateService().SetExtrasAsync(50, new SetExtrasDto { Extras = new() { "flowers" } });
        Assert.Single(r.Extras);
    }

    [Fact]
    public async Task Cancel_RefundsOnlyCardPayment_NotLaterAddedExtras()
    {
        var r = OwnedReservation(10);
        r.StartTime = Now.AddDays(3); r.EndTime = Now.AddDays(3).AddHours(2);
        r.IsPaid = true; r.SeatFee = 30m;
        r.Extras.Add(new ReservationExtra { Code = "flowers", Name = "Roosid", Price = 45m, Locked = true });   // makstud broneerimisel
        r.Extras.Add(new ReservationExtra { Code = "bottle", Name = "Konjak", Price = 95m, Locked = false });   // lisatud hiljem, kohapeal
        _reservations.Setup(x => x.GetWithDetailsAsync(50)).ReturnsAsync(r);

        var result = await CreateService().CancelAsync(50);

        Assert.Equal(75m, result.RefundAmount);
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

    [Fact]
    public async Task Create_Throws_WhenGuestAlreadyHasThreeTablesThatEvening()
    {
        var day = new DateOnly(2026, 12, 5);
        _reservations.Setup(r => r.GetByUserAsync(10)).ReturnsAsync(Enumerable.Range(1, 3).Select(i => new Reservation
        {
            Id = i, UserId = 10, Status = ReservationStatus.Active, StartTime = day.ToDateTime(new TimeOnly(18, 0))
        }).ToList());

        await Assert.ThrowsAsync<BusinessRuleException>(() => CreateService().CreateAsync(Request(guests: 2)));
    }
}
