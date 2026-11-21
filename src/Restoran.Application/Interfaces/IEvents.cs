using Restoran.Application.DTOs;
using Restoran.Application.Entities;

namespace Restoran.Application.Interfaces;

public interface IEventRepository : IRepository<GastroEvent>
{
    /// <summary>Tulevased õhtud koos registreerimiste, eripakkumiste ja restoraniga.</summary>
    Task<List<GastroEvent>> GetUpcomingAsync(DateTime from);
    Task<GastroEvent?> GetWithRegistrationsAsync(int id);
    Task<EventRegistration?> GetRegistrationAsync(int id);
    Task<EventRegistration?> GetRegistrationByTokenAsync(string token);
    Task<List<EventRegistration>> GetRegistrationsOfUserAsync(int userId);
    /// <summary>Makset ootavad registreerimised, mille õhtu algab enne antud aega.</summary>
    Task<List<EventRegistration>> GetPendingBeforeAsync(DateTime eventStartBefore);
}

public interface IEventService
{
    Task<List<GastroEventDto>> GetUpcomingAsync();
    /// <summary>Personalile: ka hiljuti toimunud õhtud.</summary>
    Task<List<GastroEventDto>> GetAllForStaffAsync();
    Task<List<EventTableDto>> GetTablesAsync(int id);
    Task<EventRegistrationDto> JoinAsync(int id, JoinEventDto dto);
    Task<EventRegistrationDto> JoinForCustomerAsync(int id, int customerId, JoinEventDto dto);
    Task<List<EventRegistrationDto>> GetMyRegistrationsAsync();
    Task<List<EventRegistrationDto>> GetRegistrationsAsync(int eventId);
    Task<EventRegistrationDto> CancelRegistrationAsync(int registrationId, CancelReservationDto? dto);
    Task<EventRegistrationDto> PayRegistrationAsync(int registrationId, FakePaymentDto card);
    Task<EventRegistrationDto> GetByPaymentTokenAsync(string token);
    Task<EventRegistrationDto> PayByTokenAsync(string token, FakePaymentDto card);
    /// <summary>Maksmata registreerimised tühistatakse 24 h enne õhtut. Tagastab tühistatud registreerimiste arvu.</summary>
    Task<List<UnpaidCancelledDto>> CancelUnpaidAsync();
    Task<GastroEventDto> CreateAsync(SaveEventDto dto);
    Task<GastroEventDto> UpdateAsync(int id, SaveEventDto dto);
    Task DeleteAsync(int id);
}
