using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Restoran.Application.Interfaces;
using Restoran.Application.Services;
using Restoran.Infrastructure.Data;
using Restoran.Infrastructure.Repositories;
using Restoran.Infrastructure.Security;

namespace Restoran.Infrastructure;

public static class DependencyInjection
{
    /// <summary>Registreerib andmebaasi, repositooriumid ja teenused DI konteinerisse.</summary>
    public static IServiceCollection AddRestoran(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(o => o.UseSqlite(config.GetConnectionString("Default")));
        services.AddSingleton<IPasswordService, PasswordService>();
        services.AddSingleton<IClock, SystemClock>();

        // Repositooriumid
        services.AddScoped<IRestaurantRepository, RestaurantRepository>();
        services.AddScoped<ITableRepository, TableRepository>();
        services.AddScoped<IMenuRepository, MenuRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IReservationRepository, ReservationRepository>();
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<ISupportRepository, SupportRepository>();
        services.AddScoped<IDayNoteRepository, DayNoteRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();

        // Teenused (äriloogika)
        services.AddScoped<IRestaurantService, RestaurantService>();
        services.AddScoped<ITableService, TableService>();
        services.AddScoped<IMenuService, MenuService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IReservationService, ReservationService>();
        services.AddScoped<IStatsService, StatsService>();
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<ISupportService, SupportService>();
        services.AddScoped<IDayNoteService, DayNoteService>();
        services.AddScoped<ICustomerService, CustomerService>();

        // SMS ja e-kiri (demo, kui võtmeid pole seadistatud)
        services.AddSingleton<ISmsSender, Messaging.SmsSender>();
        services.AddSingleton<IEmailSender, Messaging.EmailSender>();
        services.AddScoped<IBookingMessenger, Messaging.BookingMessenger>();
        services.AddScoped<INotificationService, NotificationService>();

        return services;
    }
}
