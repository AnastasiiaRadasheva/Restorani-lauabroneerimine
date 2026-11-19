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
    Task<PagedResult<Reservation>> GetPagedAsync(ReservationQuery query);
    Task<List<Reservation>> GetForDayAsync(int restaurantId, DateOnly date);
}

public interface IReservationService
{
    Task<AvailabilityDto> GetAvailabilityAsync(int restaurantId, DateOnly date, TimeOnly time, int guests);
    Task<List<DaySlotsDto>> GetSlotsAsync(int restaurantId, DateOnly from, int days, int guests);
    Task<ReservationDto> CreateAsync(CreateReservationDto dto);
    Task<ReservationDto> GetByIdAsync(int id);
    Task<List<ReservationDto>> GetMyAsync();
    Task<PagedResult<ReservationDto>> GetPagedAsync(ReservationQuery query);
    Task<ReservationDto> UpdateAsync(int id, UpdateReservationDto dto);
    Task<ReservationDto> CancelAsync(int id);
    Task<ReservationDto> SetPreOrderAsync(int id, PreOrderDto dto);
    Task<ReservationDto> PayAsync(int id, FakePaymentDto dto);
    Task<ReservationDto> ChangeStatusAsync(int id, ReservationStatus status);
    Task<TablePlanDto> GetPlanAsync(int restaurantId, DateOnly date);
}
