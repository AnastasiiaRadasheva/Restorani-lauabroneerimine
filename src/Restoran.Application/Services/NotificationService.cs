using Restoran.Application.Common;
using Restoran.Application.DTOs;
using Restoran.Application.Entities;
using Restoran.Application.Interfaces;

namespace Restoran.Application.Services;

public interface INotificationRepository : IRepository<Notification>
{
    Task<List<Notification>> GetByUserAsync(int userId, int take);
}

public interface INotificationService
{
    Task<NotificationDto> CreateAsync(CreateNotificationDto dto);
    Task<List<NotificationDto>> GetMyAsync();
    Task MarkReadAsync(int id);
}

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notifications;
    private readonly IUserRepository _users;
    private readonly ICurrentUser _current;
    private readonly IClock _clock;

    public NotificationService(INotificationRepository notifications, IUserRepository users, ICurrentUser current, IClock clock)
    {
        _notifications = notifications;
        _users = users;
        _current = current;
        _clock = clock;
    }

    public async Task<NotificationDto> CreateAsync(CreateNotificationDto dto)
    {
        _ = await _users.GetByIdAsync(dto.UserId) ?? throw new NotFoundException($"Kasutajat {dto.UserId} ei leitud.");
        var n = new Notification
        {
            UserId = dto.UserId,
            ReservationId = dto.ReservationId,
            Message = dto.Message.Trim(),
            CreatedAt = _clock.Now
        };
        await _notifications.AddAsync(n);
        await _notifications.SaveChangesAsync();
        return ToDto(n);
    }

    public async Task<List<NotificationDto>> GetMyAsync()
    {
        var userId = _current.UserId ?? throw new UnauthorizedException("Pole sisse logitud.");
        return (await _notifications.GetByUserAsync(userId, 20)).Select(ToDto).ToList();
    }

    public async Task MarkReadAsync(int id)
    {
        var userId = _current.UserId ?? throw new UnauthorizedException("Pole sisse logitud.");
        var n = await _notifications.GetByIdAsync(id) ?? throw new NotFoundException($"Teadet {id} ei leitud.");
        if (n.UserId != userId) throw new ForbiddenException("See ei ole sinu teade.");
        n.IsRead = true;
        await _notifications.SaveChangesAsync();
    }

    private static NotificationDto ToDto(Notification n) =>
        new(n.Id, n.UserId, n.ReservationId, n.Message, n.IsRead, n.CreatedAt);
}
