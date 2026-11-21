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
                BarX = 34, BarY = 58, BarWidth = 30, BarHeight = 36
            },
            new Restaurant
            {
                Id = 3, Name = "Argent · Kadriorg", Address = "Weizenbergi 18, Tallinn",
                Description = "Vaikne saal Kadrioru pargi kõrval. Hooajalised setid ja veinid.",
                Latitude = 59.4385, Longitude = 24.7920,
                OpensAt = new TimeOnly(17, 0), ClosesAt = new TimeOnly(23, 30),
                BarX = 36, BarY = 56, BarWidth = 26, BarHeight = 40
            });

        // Igal restoranil on oma paigutus. Iga tsoon (saal, terrass, teine korrus) on eraldi plaan, koordinaadid 0-100 %.
        // (number, kohad, asukoht, x, y, laius, kõrgus, kirjeldus, ümmargune)
        var H = TableLocation.Hall;
        var T = TableLocation.Terrace;
        var U = TableLocation.Upstairs;
        var layouts = new Dictionary<int, (int Nr, int Seats, TableLocation Loc, int X, int Y, int W, int H, string Spot, bool Round)[]>
        {
            // Vanalinn: saal ja teine korrus (vanalinnas terrassi pole)
            [1] = new[]
            {
                (1, 6, H, 5, 6, 22, 36, "akna all", false),
                (2, 6, H, 5, 52, 22, 36, "akna all", false),
                (3, 2, H, 74, 3, 22, 21, "vaikne nurk", false),
                (4, 2, H, 74, 27, 22, 21, "baari juures", false),
                (5, 4, H, 74, 51, 22, 21, "baari juures", false),
                (6, 4, H, 74, 75, 22, 21, "sissepääsu juures", false),
                (7, 2, U, 8, 8, 18, 30, "akna all", true),
                (8, 4, U, 36, 8, 22, 30, "kamina juures", false),
                (9, 6, U, 68, 6, 26, 40, "privaatne nurk", false),
                (10, 8, U, 10, 56, 54, 32, "pikk laud", false),
            },
            // Kalamaja: saal ja suur terrass mere ääres
            [2] = new[]
            {
                (1, 4, H, 6, 6, 20, 26, "akna all", false),
                (2, 4, H, 6, 38, 20, 26, "akna all", false),
                (3, 2, H, 6, 72, 20, 22, "sissepääsu juures", false),
                (4, 6, H, 70, 6, 24, 40, "vaade merele", false),
                (5, 2, H, 72, 60, 20, 24, "baari juures", false),
                (6, 2, T, 6, 8, 16, 30, "mere ääres", true),
                (7, 2, T, 28, 8, 16, 30, "mere ääres", true),
                (8, 4, T, 50, 8, 18, 32, "päikese käes", true),
                (9, 6, T, 8, 56, 34, 32, "varikatuse all", false),
                (10, 8, T, 58, 52, 36, 36, "pikk laud", false),
            },
            // Kadriorg: saal, terrass pargi poole ja teine korrus
            [3] = new[]
            {
                (1, 2, H, 6, 8, 20, 24, "akna all", false),
                (2, 4, H, 6, 42, 22, 28, "akna all", false),
                (3, 4, H, 72, 8, 22, 28, "vaikne nurk", false),
                (4, 6, H, 70, 52, 24, 40, "baari juures", false),
                (5, 2, T, 10, 14, 18, 34, "vaade pargile", true),
                (6, 4, T, 40, 14, 18, 34, "vaade pargile", true),
                (7, 4, T, 70, 14, 18, 34, "purskkaevu juures", true),
                (8, 2, U, 10, 12, 20, 26, "akna all", false),
                (9, 6, U, 40, 12, 26, 40, "kamina juures", false),
                (10, 8, U, 12, 60, 60, 30, "pidulik laud", false),
            },
        };

        var tables = new List<DiningTable>();
        foreach (var (r, layout) in layouts)
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

        // Argenti menüü (iga restoran sama). Kategooriad on veebis vahelehtedena.
        var menu = new (string Name, string Desc, string Cat, decimal Price)[]
        {
            ("Austrid Fine de Claire", "šalott, veiniäädikas, rukkileib · 6 tk", "Külmad road", 32m),
            ("Marmorveise tartar", "söel küpsetatud munakollane, kapparid, trühvel", "Külmad road", 28m),
            ("Hiidlest, beurre blanc, kalamari", "spargel, sidruniõli", "Pearoad", 46m),
            ("Part, kirss, suitsupeet", "laagerdatud rind, portveinikaste", "Pearoad", 39m),
            ("Šokolaad ja meresool", "tume šokolaad 70%, oliiviõli", "Magustoidud", 18m),
            ("Söel küpsetatud carabinero", "vetikavõi, laim", "Mereannid", 38m),
            ("Kammkarbid ja lillkapsas", "sarapuupähkel, pruunistatud või", "Mereannid", 34m),
            ("Kamtšatka krabi", "roheline õun, nuikapsas, till", "Mereannid", 36m),
            ("Tuurakalamari, 30 g", "tatrablinid, hapukoor", "Mereannid", 85m),
            ("Set „Argent“ – 7 käiku", "maja peamine õhtusöök, muutub hooajaga", "Setid", 120m),
            ("Set „Meri“ – 5 käiku", "austrid, krabi, kammkarbid, hiidlest, magustoit", "Setid", 95m),
            ("Set „Teras“ – 6 käiku", "liha ja ulukid lahtisel tulel", "Setid", 110m),
            ("Veinisaade", "sommeljee klaas igale käigule", "Setid", 65m),
            ("Veise sisefilee ja mürklid", "Périgueux' kaste, kartulimuss", "Pearoad", 52m),
            ("Pirnisuflee", "vaniljekreem, karamell", "Magustoidud", 16m),
            ("Šampanja, klaas", "brut, Prantsusmaa", "Veinid", 18m),
        };

        // Fotod (Unsplash, tasuta litsents) samas järjekorras nagu menüü
        var photos = new[] { "austrid", "tartar", "hiidlest", "part", "sokolaad", "carabinero", "kammkarbid", "krabi", "kalamari", "set-argent", "set-meri", "set-teras", "veinisaade", "sisefilee", "suflee", "sampanja" };

        var items = new List<MenuItem>();
        for (int r = 1; r <= 3; r++)
        {
            for (int i = 0; i < menu.Length; i++)
            {
                var m = menu[i];
                items.Add(new MenuItem
                {
                    // Esimesed 10 on vanad id-d (1-10, 11-20, 21-30), uued alates 101
                    Id = i < 10 ? (r - 1) * 10 + i + 1 : 100 + (r - 1) * 10 + (i - 9),
                    RestaurantId = r,
                    Name = m.Name,
                    Description = m.Desc,
                    Category = m.Cat,
                    Price = m.Price,
                    ImageUrl = $"/img/menu/{photos[i]}.jpg",
                    IsAvailable = true
                });
            }
        }
        b.Entity<MenuItem>().HasData(items);
    }
}
