using Microsoft.EntityFrameworkCore;
using Restoran.Application.Entities;
using Restoran.Application.Interfaces;
using Restoran.Infrastructure.Data;

namespace Restoran.Infrastructure.Repositories;

public class EventRepository : Repository<GastroEvent>, IEventRepository
{
    public EventRepository(AppDbContext db) : base(db) { }

    private IQueryable<GastroEvent> Full() =>
        Db.Events
            .Include(e => e.Restaurant)
            .Include(e => e.Items)
            .Include(e => e.Registrations).ThenInclude(r => r.Table)
            .Include(e => e.Registrations).ThenInclude(r => r.User);

    private IQueryable<EventRegistration> Registrations() =>
        Db.EventRegistrations
            .Include(r => r.Event).ThenInclude(e => e!.Restaurant)
            .Include(r => r.Event).ThenInclude(e => e!.Items)
            .Include(r => r.Event).ThenInclude(e => e!.Registrations)
            .Include(r => r.Table)
            .Include(r => r.User);

    public Task<List<GastroEvent>> GetUpcomingAsync(DateTime from) =>
        Full().Where(e => e.StartTime >= from).OrderBy(e => e.StartTime).ToListAsync();

    public Task<GastroEvent?> GetWithRegistrationsAsync(int id) =>
        Full().FirstOrDefaultAsync(e => e.Id == id);

    public Task<EventRegistration?> GetRegistrationAsync(int id) =>
        Registrations().FirstOrDefaultAsync(r => r.Id == id);

    public Task<EventRegistration?> GetRegistrationByTokenAsync(string token) =>
        Registrations().FirstOrDefaultAsync(r => r.PaymentToken == token);

    public Task<List<EventRegistration>> GetRegistrationsOfUserAsync(int userId) =>
        Registrations().Where(r => r.UserId == userId).OrderByDescending(r => r.Event!.StartTime).ToListAsync();

    public Task<List<EventRegistration>> GetPendingBeforeAsync(DateTime eventStartBefore) =>
        Registrations().Where(r => r.Status == ReservationStatus.PendingPayment && r.Event!.StartTime < eventStartBefore).ToListAsync();
}
