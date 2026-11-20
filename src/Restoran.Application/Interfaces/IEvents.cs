using Restoran.Application.DTOs;
using Restoran.Application.Entities;

namespace Restoran.Application.Interfaces;

public interface IEventRepository : IRepository<GastroEvent>
{
    /// <summary>Tulevased õhtud koos registreerimiste ja restoraniga.</summary>
    Task<List<GastroEvent>> GetUpcomingAsync(DateTime from);
    Task<GastroEvent?> GetWithRegistrationsAsync(int id);
}

public interface IEventService
{
    Task<List<GastroEventDto>> GetUpcomingAsync();
    Task<GastroEventDto> JoinAsync(int id, JoinEventDto dto);
    Task<GastroEventDto> LeaveAsync(int id);
}
