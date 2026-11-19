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
                Id = 1, Name = "Argent · Vanalinn", Address = "Rataskaevu 8, Tallinn",
                Description = "Kõrgköögi restoran vanalinna südames. Degustatsioonimenüü ja lahtine tuli.",
                Latitude = 59.4370, Longitude = 24.7428,
                OpensAt = new TimeOnly(17, 0), ClosesAt = new TimeOnly(23, 30),
                BarX = 38, BarY = 46, BarWidth = 26, BarHeight = 54
            },
            new Restaurant
            {
                Id = 2, Name = "Argent · Kalamaja", Address = "Kalasadama 8, Tallinn",
                Description = "Mereäärne saal ja suur terrass. Mereannid ja šampanja.",
                Latitude = 59.4466, Longitude = 24.7395,
                OpensAt = new TimeOnly(17, 0), ClosesAt = new TimeOnly(23, 30),
                BarX = 38, BarY = 46, BarWidth = 26, BarHeight = 54
            },
            new Restaurant
            {
                Id = 3, Name = "Argent · Kadriorg", Address = "Weizenbergi 18, Tallinn",
                Description = "Vaikne saal Kadrioru pargi kõrval. Hooajalised setid ja veinid.",
                Latitude = 59.4385, Longitude = 24.7920,
                OpensAt = new TimeOnly(17, 0), ClosesAt = new TimeOnly(23, 30),
                BarX = 38, BarY = 46, BarWidth = 26, BarHeight = 54
            });

        // Ühine paigutus. Saal ja terrass on eraldi plaanid, mõlemas koordinaadid 0-100 %.
        // (number, kohad, asukoht, x, y, laius, kõrgus, kirjeldus, ümmargune)
        var layout = new (int Nr, int Seats, TableLocation Loc, int X, int Y, int W, int H, string Spot, bool Round)[]
        {
            (1, 6, TableLocation.Hall, 5, 6, 22, 36, "akna all", false),
            (2, 6, TableLocation.Hall, 5, 52, 22, 36, "akna all", false),
            (3, 2, TableLocation.Hall, 74, 3, 22, 21, "vaikne nurk", false),
            (4, 2, TableLocation.Hall, 74, 27, 22, 21, "baari juures", false),
            (5, 4, TableLocation.Hall, 74, 51, 22, 21, "baari juures", false),
            (6, 4, TableLocation.Hall, 74, 75, 22, 21, "sissepääsu juures", false),
            (7, 2, TableLocation.Terrace, 8, 8, 18, 32, "varikatuse all", true),
            (8, 4, TableLocation.Terrace, 38, 8, 18, 32, "vaade tänavale", true),
            (9, 6, TableLocation.Terrace, 10, 54, 32, 34, "kamina juures", false),
            (10, 8, TableLocation.Terrace, 74, 30, 18, 58, "pikk laud", false),
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
                    Spot = t.Spot,
                    IsRound = t.Round,
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
