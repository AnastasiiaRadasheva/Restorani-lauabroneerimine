using Microsoft.EntityFrameworkCore;
using Restoran.Application.Entities;
using Restoran.Application.Interfaces;
using Restoran.Application.Services;

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
                new User { Name = "Mihkel Saar", Email = "mihkel@example.com", Phone = "+372 5666 1001", Role = UserRole.Customer },
                new User { Name = "Sofia Lepp", Email = "sofia@example.com", Phone = "+372 5666 1002", Role = UserRole.Customer },
                new User { Name = "Kristjan Kuusk", Email = "kristjan@example.com", Phone = "+372 5666 1003", Role = UserRole.Customer },
                new User { Name = "Elena Petrova", Email = "elena@example.com", Phone = "+372 5666 1004", Role = UserRole.Customer },
                new User { Name = "Marko Rebane", Email = "marko@example.com", Phone = "+372 5666 1005", Role = UserRole.Customer },
                new User { Name = "Laura Tamm", Email = "laura@example.com", Phone = "+372 5666 1006", Role = UserRole.Customer },
            };
            var plain = users.Select(u => u.Role switch
            {
                UserRole.Admin => "Admin123!",
                UserRole.Waiter => "Waiter123!",
                UserRole.Service => "Service123!",
                _ => "Klient123!"
            }).ToArray();
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
                    RestaurantId = 1, Code = "steel", Title = "Terase õhtu", Kind = "Degustatsiooniõhtusöök",
                    Description = "Seitsmekäiguline degustatsioonimenüü, valmistatud lahtisel tulel.",
                    Program = "Lahtine köök: peakokk valmistab käike saali ees, taustaks akustiline kitarr.",
                    StartTime = sunday.AddHours(20), PricePerGuest = 140m, DressCode = "smart elegant",
                    ImageUrl = "/img/metal.jpg", Capacity = 30
                },
                new GastroEvent
                {
                    RestaurantId = 1, Code = "white", Title = "Valge õhtusöök", Kind = "Peakoka autorisett",
                    Description = "Portselanist serveering, koorekastmed ja Burgundia valged veinid. Riietus – valge.",
                    Program = "Klaver ja keelpillikvartett, iga käigu juurde oma Burgundia vein.",
                    StartTime = sunday.AddDays(7).AddHours(19.5), PricePerGuest = 160m, DressCode = "täielikult valge",
                    Rule = "Õhtu riietus on täielikult valge. Teist värvi riietes külalistel võidakse viisakalt paluda lahkuda; osalustasu sel juhul ei tagastata.",
                    ImageUrl = "/img/sculpture.jpg", Capacity = 24
                },
                new GastroEvent
                {
                    RestaurantId = 1, Code = "black", Title = "Must ball", NoChildren = true, Kind = "Pidulik õhtu",
                    Description = "Riietus black tie, džässitrio ja õhtusöök keskööni.",
                    Program = "Džässitrio, tantsud ja õhtusöök keskööni.",
                    StartTime = sunday.AddDays(14).AddHours(21), PricePerGuest = 180m, DressCode = "black tie",
                    Zone = TableLocation.Upstairs, // teisel korrusel, kus on lava
                    Rule = "Õhtu riietus on black tie: smoking või põrandani õhtukleit. Kui riietus ei vasta nõuetele, võib administratsioon osalemisest keelduda; osalustasu ei tagastata.",
                    ImageUrl = "/img/couture.jpg", Capacity = 22
                },
                // Hooaja järgmised õhtud (umbes poole aasta peale, laupäeviti)
                new GastroEvent
                {
                    RestaurantId = 2, Code = "truffle", Title = "Trühvliõhtu", Kind = "Hooaja degustatsioon",
                    Description = "Valge ja must trühvel viies käigus, Piemonte veinid.",
                    Program = "Klaveriõhtu, peakokk tutvustab iga käiku.",
                    StartTime = sunday.AddDays(41).AddHours(19.5), PricePerGuest = 150m, DressCode = "smart elegant",
                    ImageUrl = "/img/events/truffle.jpg", Capacity = 30
                },
                new GastroEvent
                {
                    RestaurantId = 3, Code = "oyster", Title = "Austrid ja džäss", Kind = "Mereannide õhtu", NoChildren = true,
                    Description = "Austrid, Kamtšatka krabi ja šampanja.",
                    Program = "Džässibänd elavas esituses ja šampanjabaar.",
                    StartTime = sunday.AddDays(76).AddHours(20), PricePerGuest = 130m, DressCode = "kokteil",
                    ImageUrl = "/img/events/oyster.jpg", Capacity = 20
                },
                new GastroEvent
                {
                    RestaurantId = 1, Code = "vinyl", Title = "Vein ja vinüül", Kind = "Veiniõhtu",
                    Description = "Väikeveinimajade veinid; sommeljee valib iga plaadi juurde veini.",
                    Program = "DJ mängib vinüülplaate, väikesed suupisted.",
                    StartTime = sunday.AddDays(111).AddHours(19), PricePerGuest = 90m, DressCode = "",
                    Zone = TableLocation.Hall, ImageUrl = "/img/events/vinyl.jpg", Capacity = 30
                },
                new GastroEvent
                {
                    RestaurantId = 2, Code = "spring", Title = "Kevadine aiaõhtu", Kind = "Peakoka autorisett",
                    Description = "Esimesed kevadised köögiviljad ja ürdid, kerged veinid.",
                    Program = "Keelpillitrio terrassil päikeseloojangul.",
                    StartTime = sunday.AddDays(146).AddHours(19), PricePerGuest = 120m, DressCode = "smart casual",
                    Zone = TableLocation.Terrace, ImageUrl = "/img/events/spring.jpg", Capacity = 30
                },
                new GastroEvent
                {
                    RestaurantId = 3, Code = "midnight", Title = "Kesköö õhtusöök", Kind = "Pidulik õhtu", NoChildren = true,
                    Description = "Pikk lauaõhtu valge öö valguses kuni kella kaheni.",
                    Program = "Keelpillikvartett, pärast keskööd DJ.",
                    StartTime = sunday.AddDays(174).AddHours(21), PricePerGuest = 170m, DressCode = "black tie",
                    Rule = "Õhtu riietus on black tie: smoking või põrandani õhtukleit. Kui riietus ei vasta nõuetele, võib administratsioon osalemisest keelduda; osalustasu ei tagastata.",
                    ImageUrl = "/img/events/midnight.jpg", Capacity = 20
                });
            await db.SaveChangesAsync();

            // Iga õhtu eripakkumised ("ainult sel õhtul")
            foreach (var ev in await db.Events.ToListAsync())
                foreach (var d in EventCatalog.DrinksFor(ev.Code))
                    db.EventMenuItems.Add(new EventMenuItem { EventId = ev.Id, Code = d.Code, Name = d.Name, Description = d.Description, Price = d.Price });
            await db.SaveChangesAsync();

            // Osa laudu on juba teiste külaliste poolt võetud (makstud)
            var guests = await db.Users.Where(u => u.Role == UserRole.Customer && u.Email != "klient@restoran.ee").ToListAsync();
            var events = await db.Events.ToListAsync();
            // (õhtu, laud, külaline) – üks külaline registreerub igale õhtule ainult korra
            var taken = new[] { (0, 2, 0), (0, 5, 1), (1, 1, 0), (1, 4, 1), (1, 9, 2), (2, 13, 0), (2, 14, 1), (2, 15, 2) };
            foreach (var (e, tableNr, g) in taken)
            {
                db.EventRegistrations.Add(new EventRegistration
                {
                    EventId = events[e].Id, UserId = guests[g % guests.Count].Id, TableId = tableNr,
                    Guests = 2, AcceptedRules = true, Total = events[e].PricePerGuest * 2,
                    Status = ReservationStatus.Active, IsPaid = true
                });
            }
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
            var slots = new[] { 17.0, 18.0, 19.0, 19.5, 20.0, 21.0 };
            // Restoranid täituvad erinevalt: Vanalinn kõige rohkem, Kadriorg kõige vähem
            var busy = new Dictionary<int, double> { [1] = 0.55, [2] = 0.4, [3] = 0.25 };
            var today = DateTime.Today;
            var list = new List<Reservation>();
            var restaurants = await db.Restaurants.ToDictionaryAsync(r => r.Id);
            // Gastronoomilise õhtu päeval on õhtu saal tavaliseks broneerimiseks suletud
            var seededEvents = await db.Events.ToListAsync();

            for (int d = -60; d <= 6; d++)
            {
                var day = today.AddDays(d);
                double dayJitter = 0.6 + rnd.NextDouble() * 0.8;
                foreach (var table in tables)
                {
                    if (seededEvents.Any(e => e.RestaurantId == table.RestaurantId && e.StartTime.Date == day.Date && (e.Zone == null || e.Zone == table.Location)))
                        continue;
                    // Laud on hõivatud kogu õhtu, seega kuni üks broneering laua kohta päevas
                    // Nädalapäeviti erinev täituvus + iga päev veidi juhuslik
                    double weekday = day.DayOfWeek switch
                    {
                        DayOfWeek.Monday => 0.5, DayOfWeek.Tuesday => 0.6, DayOfWeek.Wednesday => 0.8,
                        DayOfWeek.Thursday => 1.0, DayOfWeek.Friday => 1.4, DayOfWeek.Saturday => 1.5, _ => 0.9
                    };
                    double chance = busy[table.RestaurantId] * weekday * dayJitter;
                    if (d >= 0) chance /= 2;
                    if (rnd.NextDouble() > chance) continue;

                    var r0 = restaurants[table.RestaurantId];
                    var start = day.AddHours(slots[rnd.Next(slots.Length)]);
                    var end = r0.ClosesAt > r0.OpensAt ? day.Add(r0.ClosesAt.ToTimeSpan()) : day.AddDays(1).Add(r0.ClosesAt.ToTimeSpan());
                    var status = start < DateTime.Now
                        ? (rnd.NextDouble() < 0.1 ? ReservationStatus.NoShow : ReservationStatus.Arrived)
                        : ReservationStatus.Active;
                    list.Add(new Reservation
                    {
                        TableId = table.Id,
                        UserId = customers[rnd.Next(customers.Count)].Id,
                        StartTime = start,
                        EndTime = end,
                        Guests = Math.Max(1, table.Seats - rnd.Next(0, 2)),
                        Status = status,
                        Reminded = start < DateTime.Now,
                        Occasion = rnd.NextDouble() < 0.12 ? (Occasion)rnd.Next(1, 5) : Occasion.None,
                        CreatedAt = start.AddDays(-rnd.Next(1, 10))
                    });
                }
            }
            db.Reservations.AddRange(list);
            await db.SaveChangesAsync();
        }
    }
}
