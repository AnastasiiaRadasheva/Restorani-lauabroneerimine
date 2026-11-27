using Restoran.Application.Common;
using Restoran.Application.DTOs;
using Restoran.Application.Entities;

namespace Restoran.Application.Interfaces;

public interface IReservationRepository : IRepository<Reservation>
{
    /// <summary>Broneering koos laua, restorani, kasutaja ja eeltellimusega.</summary>
    Task<Reservation?> GetWithDetailsAsync(int id);

    /// <summary>Broneeringud, mis hõivavad antud laudu antud ajavahemikus (Active või Arrived).</summary>
    Task<List<Reservation>> GetOccupyingAsync(IEnumerable<int> tableIds, DateTime start, DateTime end, int? excludeId = null);

    Task<List<Reservation>> GetByUserAsync(int userId);
    Task<Reservation?> GetByPaymentTokenAsync(string token);
    /// <summary>Maksmata personali broneeringud, mis on loodud enne antud aega.</summary>
    Task<List<Reservation>> GetExpiredPendingAsync(DateTime createdBefore, DateTime now);

    /// <summary>Restorani gastronoomilised õhtud ajavahemikus (õhtu päeval on tavaline broneerimine selles saalis suletud).</summary>
    Task<List<GastroEvent>> GetEventsAsync(int restaurantId, DateTime from, DateTime to);

    /// <summary>Tulevased aktiivsed broneeringud, mille eeltellimuses on see roog.</summary>
    Task<List<Reservation>> GetActiveWithMenuItemAsync(int menuItemId, DateTime from);
    Task<PagedResult<Reservation>> GetPagedAsync(ReservationQuery query);
    Task<List<Reservation>> GetForDayAsync(int restaurantId, DateOnly date);

    /// <summary>Broneeringud statistika jaoks (ilma seotud andmeteta).</summary>
    Task<List<Reservation>> GetForStatsAsync(int? restaurantId, DateOnly from, DateOnly to);

    /// <summary>Aktiivsed broneeringud ajavahemikus, millele pole meeldetuletust saadetud.</summary>
    Task<List<Reservation>> GetRemindDueAsync(DateTime from, DateTime to);

    /// <summary>Aktiivsed broneeringud, mis algasid enne antud aega (klient pole saabunud).</summary>
    Task<List<Reservation>> GetNoShowCandidatesAsync(DateTime startedBefore);
}

public interface IReservationService
{
    Task<AvailabilityDto> GetAvailabilityAsync(int restaurantId, DateOnly date, TimeOnly time, int guests);
    Task<List<DaySlotsDto>> GetSlotsAsync(int restaurantId, DateOnly from, int days, int guests);
    Task<ReservationDto> CreateAsync(CreateReservationDto dto);
    /// <summary>Personal broneerib kliendi nimel: olek "ootab makset" ja makselingi kood.</summary>
    Task<ReservationDto> CreatePendingForUserAsync(int userId, CreateReservationDto dto);
    Task<ReservationDto> GetByPaymentTokenAsync(string token);
    Task<ReservationDto> PayByTokenAsync(string token, FakePaymentDto dto);
    Task<ReservationDto> GetByIdAsync(int id);
    Task<List<ReservationDto>> GetMyAsync();
    Task<PagedResult<ReservationDto>> GetPagedAsync(ReservationQuery query);
    Task<ReservationDto> UpdateAsync(int id, UpdateReservationDto dto);
    Task<ReservationDto> CancelAsync(int id, CancelReservationDto? dto = null);
    Task<ReservationDto> SetPreOrderAsync(int id, PreOrderDto dto);
    Task<ReservationDto> PayAsync(int id, FakePaymentDto dto);
    Task<ReservationDto> ChangeStatusAsync(int id, ReservationStatus status);

    // Sõnumid ja lisateenused (lubatud ka vähem kui 24 h enne)
    Task<List<MessageDto>> GetMessagesAsync(int id);
    Task<MessageDto> AddMessageAsync(int id, NewMessageDto dto);
    Task<ReservationDto> SetExtrasAsync(int id, SetExtrasDto dto);
    Task<ReservationDto> SetStaffDetailsAsync(int id, StaffDetailsDto dto);
    Task<TablePlanDto> GetPlanAsync(int restaurantId, DateOnly date);
    Task<List<CalendarDayDto>> GetCalendarAsync(int? restaurantId, DateOnly from, int days);

    // Taustateenuse jaoks
    Task<List<RemindDueDto>> GetRemindDueAsync();
    Task MarkRemindedAsync(int id);
    Task<NoShowResultDto> MarkNoShowsAsync();
    Task<NoShowResultDto> MarkNoShowAsync(int id);
}
