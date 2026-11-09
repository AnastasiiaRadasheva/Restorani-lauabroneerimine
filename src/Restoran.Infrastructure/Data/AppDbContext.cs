using Microsoft.EntityFrameworkCore;
using Restoran.Application.Entities;

namespace Restoran.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Restaurant> Restaurants => Set<Restaurant>();
    public DbSet<DiningTable> Tables => Set<DiningTable>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<PreOrderItem> PreOrderItems => Set<PreOrderItem>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.Email).HasMaxLength(200).IsRequired();
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.Phone).HasMaxLength(30);
            e.Property(x => x.Role).HasConversion<string>().HasMaxLength(20);
        });

        b.Entity<Restaurant>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.Address).HasMaxLength(200);
            e.Property(x => x.Description).HasMaxLength(1000);
        });

        b.Entity<DiningTable>(e =>
        {
            e.ToTable("Tables");
            e.HasOne(x => x.Restaurant).WithMany(r => r.Tables)
                .HasForeignKey(x => x.RestaurantId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => new { x.RestaurantId, x.Number }).IsUnique();
            e.Property(x => x.Location).HasConversion<string>().HasMaxLength(20);
        });

        b.Entity<Reservation>(e =>
        {
            e.HasOne(x => x.Table).WithMany(t => t.Reservations)
                .HasForeignKey(x => x.TableId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.User).WithMany(u => u.Reservations)
                .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.SpecialRequest).HasMaxLength(500);
            e.HasIndex(x => new { x.TableId, x.StartTime });
        });

        b.Entity<MenuItem>(e =>
        {
            e.HasOne(x => x.Restaurant).WithMany(r => r.MenuItems)
                .HasForeignKey(x => x.RestaurantId).OnDelete(DeleteBehavior.Cascade);
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.Category).HasMaxLength(50);
            e.Property(x => x.Description).HasMaxLength(500);
            e.Property(x => x.Price).HasPrecision(10, 2);
        });

        b.Entity<PreOrderItem>(e =>
        {
            e.HasOne(x => x.Reservation).WithMany(r => r.PreOrderItems)
                .HasForeignKey(x => x.ReservationId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.MenuItem).WithMany()
                .HasForeignKey(x => x.MenuItemId).OnDelete(DeleteBehavior.Restrict);
            e.Property(x => x.UnitPrice).HasPrecision(10, 2);
        });

        b.Entity<Notification>(e =>
        {
            e.HasOne(x => x.User).WithMany(u => u.Notifications)
                .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            e.Property(x => x.Message).HasMaxLength(500);
        });

        SeedData.Apply(b);
    }
}
