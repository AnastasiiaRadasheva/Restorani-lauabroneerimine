using Microsoft.EntityFrameworkCore;
using Restoran.Application.Entities;
using Restoran.Application.Interfaces;

namespace Restoran.Infrastructure.Data;

/// <summary>
/// Käivitamisel lisatavad andmed: kasutajad (paroolid räsitakse)
/// ja näidisbroneeringud statistika jaoks.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db, IPasswordService passwords)
    {
        if (!await db.Users.AnyAsync())
        {
            var users = new[]
            {
                new User { Name = "Admin", Email = "admin@restoran.ee", Role = UserRole.Admin },
                new User { Name = "Mari Teenindaja", Email = "waiter@restoran.ee", Role = UserRole.Waiter },
                new User { Name = "Kati Klient", Email = "klient@restoran.ee", Phone = "+372 5555 1234", Role = UserRole.Customer },
                new User { Name = "Worker teenus", Email = "service@restoran.ee", Role = UserRole.Service },
                // Näidiskliendid statistika jaoks
                new User { Name = "Jaan Tamm", Email = "jaan@example.com", Phone = "+372 5111 1111", Role = UserRole.Customer },
                new User { Name = "Liis Kask", Email = "liis@example.com", Phone = "+372 5222 2222", Role = UserRole.Customer },
                new User { Name = "Peeter Saar", Email = "peeter@example.com", Phone = "+372 5333 3333", Role = UserRole.Customer },
                new User { Name = "Anna Mägi", Email = "anna@example.com", Phone = "+372 5444 4444", Role = UserRole.Customer },
            };
            var plain = new[] { "Admin123!", "Waiter123!", "Klient123!", "Service123!", "Klient123!", "Klient123!", "Klient123!", "Klient123!" };
            for (int i = 0; i < users.Length; i++)
                users[i].PasswordHash = passwords.Hash(users[i], plain[i]);

            db.Users.AddRange(users);
            await db.SaveChangesAsync();
        }

        if (!await db.Events.AnyAsync())
        {
            // Gastronoomilised õhtud kolmel järgmisel pühapäeval
            var sunday = DateTime.Today.AddDays(((int)DayOfWeek.Sunday - (int)DateTime.Today.DayOfWeek + 7) % 7);
            if (sunday <= DateTime.Today) sunday = sunday.AddDays(7);
            db.Events.AddRange(
                new GastroEvent
                {
                    RestaurantId = 1, Title = "Terase õhtu", Kind = "Degustatsiooniõhtusöök",
                    Description = "Seitsmekäiguline degustatsioonimenüü, valmistatud lahtisel tulel.",
                    StartTime = sunday.AddHours(20), PricePerGuest = 140m, DressCode = "smart elegant",
                    ImageUrl = "/img/metal.jpg", Capacity = 30
                },
                new GastroEvent
                {
                    RestaurantId = 1, Title = "Valge õhtusöök", Kind = "Peakoka autorisett",
                    Description = "Portselanist serveering, koorekastmed ja Burgundia valged veinid. Riietus – valge.",
                    StartTime = sunday.AddDays(7).AddHours(19.5), PricePerGuest = 160m, DressCode = "täielikult valge",
                    Rule = "Õhtu riietus on täielikult valge. Teist värvi riietes külalistel võidakse viisakalt paluda lahkuda; osalustasu sel juhul ei tagastata.",
                    ImageUrl = "/img/sculpture.jpg", Capacity = 24
                },
                new GastroEvent
                {
                    RestaurantId = 1, Title = "Must ball", Kind = "Pidulik õhtu",
                    Description = "Riietus black tie, džässitrio ja õhtusöök keskööni.",
                    StartTime = sunday.AddDays(14).AddHours(21), PricePerGuest = 180m, DressCode = "black tie",
                    Rule = "Õhtu riietus on black tie: smoking või põrandani õhtukleit. Kui riietus ei vasta nõuetele, võib administratsioon osalemisest keelduda; osalustasu ei tagastata.",
                    ImageUrl = "/img/couture.jpg", Capacity = 40
                });
            await db.SaveChangesAsync();
        }

        if (!await db.Reservations.AnyAsync())
        {
            // Näidisbroneeringud jagatakse näidisklientide vahel (mitte testkliendile klient@restoran.ee)
            var customers = await db.Users
                .Where(u => u.Role == UserRole.Customer && u.Email != "klient@restoran.ee")
                .ToListAsync();
            var tables = await db.Tables.ToListAsync();
            var rnd = new Random(42);
            var slots = new[] { 17.0, 19.0, 21.0 }; // Argent on õhturestoran (17:00–23:30)
            var today = DateTime.Today;
            var list = new List<Reservation>();

            for (int d = -60; d <= 3; d++)
            {
                var day = today.AddDays(d);
                bool weekend = day.DayOfWeek is DayOfWeek.Friday or DayOfWeek.Saturday;
                foreach (var table in tables)
                {
                    foreach (var slot in slots)
                    {
                        // Õhtuti ja nädalavahetusel on rohkem broneeringuid
                        double chance = (slot == 19.0 ? 0.45 : 0.25) + (weekend ? 0.25 : 0);
                        if (d >= 0) chance /= 3;
                        if (rnd.NextDouble() > chance) continue;

                        var start = day.AddHours(slot);
                        var status = start < DateTime.Now
                            ? (rnd.NextDouble() < 0.1 ? ReservationStatus.NoShow : ReservationStatus.Arrived)
                            : ReservationStatus.Active;
                        list.Add(new Reservation
                        {
                            TableId = table.Id,
                            UserId = customers[rnd.Next(customers.Count)].Id,
                            StartTime = start,
                            EndTime = start.AddHours(2),
                            Guests = Math.Max(1, table.Seats - rnd.Next(0, 2)),
                            Status = status,
                            Reminded = start < DateTime.Now,
                            CreatedAt = start.AddDays(-rnd.Next(1, 10))
                        });
                    }
                }
            }
            db.Reservations.AddRange(list);
            await db.SaveChangesAsync();
        }
    }
}
