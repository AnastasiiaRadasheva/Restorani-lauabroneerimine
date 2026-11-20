using Microsoft.Extensions.Logging;
using Restoran.Application.Common;
using Restoran.Application.DTOs;
using Restoran.Application.Entities;
using Restoran.Application.Interfaces;

namespace Restoran.Application.Services;

/// <summary>Gastronoomilised õhtud ja neile registreerimine.</summary>
public class EventService : IEventService
{
    private readonly IEventRepository _events;
    private readonly ICurrentUser _current;
    private readonly IClock _clock;
    private readonly ILogger<EventService> _logger;

    public EventService(IEventRepository events, ICurrentUser current, IClock clock, ILogger<EventService> logger)
    {
        _events = events;
        _current = current;
        _clock = clock;
        _logger = logger;
    }

    public async Task<List<GastroEventDto>> GetUpcomingAsync()
    {
        var list = await _events.GetUpcomingAsync(_clock.Now);
        return list.Select(ToDto).ToList();
    }

    public async Task<GastroEventDto> JoinAsync(int id, JoinEventDto dto)
    {
        var userId = _current.UserId ?? throw new UnauthorizedException("Pole sisse logitud.");
        var e = await Find(id);

        if (e.StartTime <= _clock.Now)
            throw new BusinessRuleException("See õhtu on juba toimunud.");
        if (e.Rule is not null && !dto.AcceptRules)
            throw new BusinessRuleException("Registreerimiseks tuleb nõustuda riietuse reegliga.");
        if (e.Registrations.Any(r => r.UserId == userId))
            throw new ConflictException("Olete sellele õhtule juba registreerunud.");
        if (SeatsLeft(e) < dto.Guests)
            throw new ConflictException($"Vabu kohti on alles {SeatsLeft(e)}.");

        e.Registrations.Add(new EventRegistration
        {
            UserId = userId,
            Guests = dto.Guests,
            AcceptedRules = dto.AcceptRules,
            CreatedAt = _clock.Now
        });
        await _events.SaveChangesAsync();
        _logger.LogInformation("Kasutaja {User} registreerus õhtule {Event} ({Guests} külalist)", userId, e.Title, dto.Guests);
        return ToDto(e);
    }

    public async Task<GastroEventDto> LeaveAsync(int id)
    {
        var userId = _current.UserId ?? throw new UnauthorizedException("Pole sisse logitud.");
        var e = await Find(id);
        var mine = e.Registrations.FirstOrDefault(r => r.UserId == userId)
                   ?? throw new NotFoundException("Te pole sellele õhtule registreerunud.");
        e.Registrations.Remove(mine);
        await _events.SaveChangesAsync();
        return ToDto(e);
    }

    private async Task<GastroEvent> Find(int id) =>
        await _events.GetWithRegistrationsAsync(id) ?? throw new NotFoundException($"Õhtut {id} ei leitud.");

    private static int SeatsLeft(GastroEvent e) => Math.Max(0, e.Capacity - e.Registrations.Sum(r => r.Guests));

    private GastroEventDto ToDto(GastroEvent e) => new(
        e.Id, e.RestaurantId, e.Restaurant?.Name ?? "", e.Title, e.Kind, e.Description, e.StartTime,
        e.PricePerGuest, e.DressCode, e.Rule, e.ImageUrl, e.Capacity, SeatsLeft(e),
        e.Registrations.FirstOrDefault(r => r.UserId == _current.UserId)?.Guests ?? 0);
}
