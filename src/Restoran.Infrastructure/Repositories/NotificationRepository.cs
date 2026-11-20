using Microsoft.EntityFrameworkCore;
using Restoran.Application.Entities;
using Restoran.Application.Services;
using Restoran.Infrastructure.Data;

namespace Restoran.Infrastructure.Repositories;

public class NotificationRepository : Repository<Notification>, INotificationRepository
{
    public NotificationRepository(AppDbContext db) : base(db) { }

    public Task<List<Notification>> GetByUserAsync(int userId, int take) =>
        Db.Notifications.Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(take)
            .ToListAsync();
}
