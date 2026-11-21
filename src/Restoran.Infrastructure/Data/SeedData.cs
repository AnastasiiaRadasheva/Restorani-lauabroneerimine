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
                BarX = 40, BarY = 40, BarWidth = 20, BarHeight = 34,
                // Lava teisel korrusel (džäss, pidulikud õhtud)
                StageZone = TableLocation.Upstairs, StageX = 78, StageY = 50, StageWidth = 18, StageHeight = 38
            },
            new Restaurant
            {
                Id = 2, Name = "Argent · Kalamaja", Address = "Kalasadama 8, Tallinn",
                Description = "Mereäärne saal ja suur terrass. Mereannid ja šampanja.",
                Latitude = 59.4466, Longitude = 24.7395,
                OpensAt = new TimeOnly(17, 0), ClosesAt = new TimeOnly(3, 0), // mere ääres kuni kella 3-ni öösel
                BarX = 34, BarY = 58, BarWidth = 30, BarHeight = 36,
                // Lava terrassil laudade ridade vahel
                StageZone = TableLocation.Terrace, StageX = 14, StageY = 34, StageWidth = 46, StageHeight = 12
            },
            new Restaurant
            {
                Id = 3, Name = "Argent · Kadriorg", Address = "Weizenbergi 18, Tallinn",
                Description = "Vaikne saal Kadrioru pargi kõrval. Hooajalised setid ja veinid.",
                Latitude = 59.4385, Longitude = 24.7920,
                OpensAt = new TimeOnly(17, 0), ClosesAt = new TimeOnly(23, 30),
                BarX = 38, BarY = 60, BarWidth = 22, BarHeight = 36,
                // Väike lava saali keskel
                StageZone = TableLocation.Hall, StageX = 36, StageY = 12, StageWidth = 26, StageHeight = 20
            });

        // Igal restoranil on oma paigutus. Iga tsoon (saal, terrass, teine korrus) on eraldi plaan, koordinaadid 0-100 %.
        // (number, kohad, asukoht, x, y, laius, kõrgus, kirjeldus, ümmargune)
        var H = TableLocation.Hall;
        var T = TableLocation.Terrace;
        var U = TableLocation.Upstairs;
        var layouts = new Dictionary<int, (int Nr, int Seats, TableLocation Loc, int X, int Y, int W, int H, string Spot, bool Round)[]>
        {
            // Vanalinn: suur saal (12 lauda, diivanid, pikk laud) ja teine korrus eraruumiga
            [1] = new[]
            {
                (1, 2, H, 4, 4, 10, 16, "akna all", true),
                (2, 2, H, 16, 4, 10, 16, "akna all", true),
                (3, 4, H, 28, 4, 14, 16, "akna all", false),
                (4, 6, H, 4, 28, 12, 30, "diivan", false),
                (5, 4, H, 20, 32, 14, 16, "saali keskel", false),
                (6, 2, H, 28, 52, 10, 16, "saali keskel", true),
                (7, 8, H, 4, 68, 22, 24, "pikk laud", false),
                (8, 4, H, 64, 4, 14, 16, "vaikne nurk", false),
                (9, 2, H, 84, 4, 12, 16, "vaikne nurk", true),
                (10, 4, H, 64, 28, 14, 16, "baari juures", false),
                (11, 6, H, 84, 26, 12, 32, "diivan", false),
                (12, 4, H, 64, 62, 14, 16, "sissepääsu juures", false),
                (13, 2, U, 8, 8, 14, 24, "akna all", true),
                (14, 4, U, 30, 8, 18, 26, "kamina juures", false),
                (15, 6, U, 60, 6, 30, 36, "privaatne ruum", false),
                (16, 10, U, 10, 56, 64, 30, "pidulik laud", false),
            },
            // Kalamaja: väike saal ja suur terrass mere ääres
            [2] = new[]
            {
                (1, 4, H, 6, 6, 20, 26, "akna all", false),
                (2, 4, H, 6, 40, 20, 26, "akna all", false),
                (3, 2, H, 6, 74, 18, 20, "sissepääsu juures", false),
                (4, 6, H, 70, 6, 24, 40, "vaade merele", false),
                (5, 2, H, 72, 60, 20, 24, "baari juures", false),
                (6, 2, T, 4, 6, 12, 22, "mere ääres", true),
                (7, 2, T, 20, 6, 12, 22, "mere ääres", true),
                (8, 2, T, 36, 6, 12, 22, "mere ääres", true),
                (9, 4, T, 52, 6, 14, 24, "päikese käes", true),
                (10, 4, T, 70, 6, 14, 24, "päikese käes", true),
                (11, 6, T, 6, 50, 26, 32, "varikatuse all", false),
                (12, 6, T, 38, 50, 26, 32, "kamina juures", false),
                (13, 10, T, 70, 42, 26, 48, "pikk laud", false),
            },
            // Kadriorg: väike hubane saal ja terrass pargi poole
            [3] = new[]
            {
                (1, 2, H, 8, 10, 20, 26, "akna all", false),
                (2, 4, H, 8, 52, 22, 30, "akna all", false),
                (3, 4, H, 70, 10, 22, 28, "vaikne nurk", false),
                (4, 6, H, 68, 54, 26, 36, "baari juures", false),
                (5, 2, T, 10, 20, 18, 34, "vaade pargile", true),
                (6, 4, T, 40, 20, 18, 34, "vaade pargile", true),
                (7, 4, T, 70, 20, 18, 34, "purskkaevu juures", true),
            },
        };

        var tables = new List<DiningTable>();
        foreach (var (r, layout) in layouts)
        {
            foreach (var t in layout)
            {
                tables.Add(new DiningTable
                {
                    Id = (r - 1) * 20 + t.Nr, // kuni 20 lauda restoranis
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

        // Allergeenide ja toitumise sildid samas järjekorras
        var tags = new[] { "shellfish", "egg", "fish,milk", "", "milk,vegetarian", "shellfish,milk", "shellfish,nuts,milk", "shellfish", "fish,gluten,milk", "gluten,milk,fish", "shellfish,fish,milk", "gluten", "vegan", "milk", "gluten,milk,egg,vegetarian", "vegan" };

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
                    Tags = tags[i] == "" ? null : tags[i],
                    IsAvailable = true
                });
            }
        }
        b.Entity<MenuItem>().HasData(items);
    }
}
