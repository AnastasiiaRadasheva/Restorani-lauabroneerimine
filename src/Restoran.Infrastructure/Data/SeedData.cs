using Microsoft.EntityFrameworkCore;
using Restoran.Application.Entities;

namespace Restoran.Infrastructure.Data;

/// <summary>Algandmed, mis lähevad migratsiooni (restoranid, lauad, menüü).</summary>
public static class SeedData
{
    public static void Apply(ModelBuilder b)
    {
        b.Entity<Restaurant>().HasData(
            new Restaurant
            {
                Id = 1, Name = "Aed Vanalinn", Address = "Rataskaevu 8, Tallinn",
                Description = "Hubane restoran vanalinna südames. Hooajaline Eesti köök.",
                Latitude = 59.4370, Longitude = 24.7428,
                OpensAt = new TimeOnly(12, 0), ClosesAt = new TimeOnly(23, 0),
                BarX = 42, BarY = 45, BarWidth = 22, BarHeight = 50
            },
            new Restaurant
            {
                Id = 2, Name = "Aed Kalamaja", Address = "Kalasadama 8, Tallinn",
                Description = "Mereäärne Aed Kalamajas. Värske kala ja suur terrass.",
                Latitude = 59.4466, Longitude = 24.7395,
                OpensAt = new TimeOnly(11, 0), ClosesAt = new TimeOnly(22, 0),
                BarX = 42, BarY = 5, BarWidth = 22, BarHeight = 40
            },
            new Restaurant
            {
                Id = 3, Name = "Aed Kadriorg", Address = "Weizenbergi 18, Tallinn",
                Description = "Rahulik Aed Kadrioru pargi kõrval.",
                Latitude = 59.4385, Longitude = 24.7920,
                OpensAt = new TimeOnly(12, 0), ClosesAt = new TimeOnly(23, 0),
                BarX = 42, BarY = 45, BarWidth = 22, BarHeight = 50
            });

        // Ühine saali paigutus: (number, kohad, asukoht, x, y, laius, kõrgus) protsentides
        var layout = new (int Nr, int Seats, TableLocation Loc, int X, int Y, int W, int H)[]
        {
            (1, 4, TableLocation.Hall, 4, 6, 16, 22),
            (2, 2, TableLocation.Hall, 24, 6, 12, 14),
            (3, 6, TableLocation.Hall, 4, 55, 18, 38),
            (4, 2, TableLocation.Hall, 24, 30, 12, 14),
            (5, 8, TableLocation.Hall, 24, 58, 14, 35),
            (6, 2, TableLocation.Terrace, 78, 4, 18, 12),
            (7, 4, TableLocation.Terrace, 78, 22, 18, 16),
            (8, 4, TableLocation.Terrace, 78, 44, 18, 16),
            (9, 2, TableLocation.Terrace, 78, 66, 18, 12),
            (10, 6, TableLocation.Terrace, 78, 82, 18, 14),
        };

        var tables = new List<DiningTable>();
        for (int r = 1; r <= 3; r++)
        {
            foreach (var t in layout)
            {
                tables.Add(new DiningTable
                {
                    Id = (r - 1) * 10 + t.Nr,
                    RestaurantId = r,
                    Number = t.Nr,
                    Seats = t.Seats,
                    Location = t.Loc,
                    X = t.X, Y = t.Y, Width = t.W, Height = t.H,
                    IsActive = true
                });
            }
        }
        b.Entity<DiningTable>().HasData(tables);

        var menu = new (string Name, string Desc, string Cat, decimal Price)[]
        {
            ("Seenesupp", "Metsaseentest koorene supp", "Supid", 6.50m),
            ("Lõhesupp", "Klassikaline Põhjamaade lõhesupp", "Supid", 8.90m),
            ("Veisesteik", "Veise välisfilee, köögiviljad, punaveinikaste", "Pearoad", 24.00m),
            ("Ahjulõhe", "Lõhefilee, kartulipüree, tilli-võikaste", "Pearoad", 19.50m),
            ("Seenerisoto", "Kreemine risoto metsaseentega", "Pearoad", 15.00m),
            ("Kamavaht", "Kamavaht marjadega", "Magustoidud", 6.00m),
            ("Šokolaadikook", "Soe šokolaadikook jäätisega", "Magustoidud", 7.50m),
            ("Kodune limonaad", "Rabarberi-piparmündi limonaad", "Joogid", 4.00m),
        };

        var items = new List<MenuItem>();
        for (int r = 1; r <= 3; r++)
        {
            for (int i = 0; i < menu.Length; i++)
            {
                var m = menu[i];
                items.Add(new MenuItem
                {
                    Id = (r - 1) * 10 + i + 1,
                    RestaurantId = r,
                    Name = m.Name,
                    Description = m.Desc,
                    Category = m.Cat,
                    Price = m.Price,
                    IsAvailable = true
                });
            }
        }
        b.Entity<MenuItem>().HasData(items);
    }
}
