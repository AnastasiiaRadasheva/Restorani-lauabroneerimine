using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Restoran.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ArgentMenuu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Category", "Description", "Name", "Price" },
                values: new object[] { "Külmad road", "šalott, veiniäädikas, rukkileib · 6 tk", "Austrid Fine de Claire", 32m });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Category", "Description", "Name", "Price" },
                values: new object[] { "Külmad road", "söel küpsetatud munakollane, kapparid, trühvel", "Marmorveise tartar", 28m });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Description", "Name", "Price" },
                values: new object[] { "spargel, sidruniõli", "Hiidlest, beurre blanc, kalamari", 46m });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Description", "Name", "Price" },
                values: new object[] { "laagerdatud rind, portveinikaste", "Part, kirss, suitsupeet", 39m });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Category", "Description", "Name", "Price" },
                values: new object[] { "Magustoidud", "tume šokolaad 70%, oliiviõli", "Šokolaad ja meresool", 18m });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Category", "Description", "Name", "Price" },
                values: new object[] { "Mereannid", "vetikavõi, laim", "Söel küpsetatud carabinero", 38m });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Category", "Description", "Name", "Price" },
                values: new object[] { "Mereannid", "sarapuupähkel, pruunistatud või", "Kammkarbid ja lillkapsas", 34m });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Category", "Description", "Name", "Price" },
                values: new object[] { "Mereannid", "roheline õun, nuikapsas, till", "Kamtšatka krabi", 36m });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "Category", "Description", "Name", "Price" },
                values: new object[] { "Külmad road", "šalott, veiniäädikas, rukkileib · 6 tk", "Austrid Fine de Claire", 32m });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "Category", "Description", "Name", "Price" },
                values: new object[] { "Külmad road", "söel küpsetatud munakollane, kapparid, trühvel", "Marmorveise tartar", 28m });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "Description", "Name", "Price" },
                values: new object[] { "spargel, sidruniõli", "Hiidlest, beurre blanc, kalamari", 46m });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "Description", "Name", "Price" },
                values: new object[] { "laagerdatud rind, portveinikaste", "Part, kirss, suitsupeet", 39m });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "Category", "Description", "Name", "Price" },
                values: new object[] { "Magustoidud", "tume šokolaad 70%, oliiviõli", "Šokolaad ja meresool", 18m });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "Category", "Description", "Name", "Price" },
                values: new object[] { "Mereannid", "vetikavõi, laim", "Söel küpsetatud carabinero", 38m });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "Category", "Description", "Name", "Price" },
                values: new object[] { "Mereannid", "sarapuupähkel, pruunistatud või", "Kammkarbid ja lillkapsas", 34m });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "Category", "Description", "Name", "Price" },
                values: new object[] { "Mereannid", "roheline õun, nuikapsas, till", "Kamtšatka krabi", 36m });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 21,
                columns: new[] { "Category", "Description", "Name", "Price" },
                values: new object[] { "Külmad road", "šalott, veiniäädikas, rukkileib · 6 tk", "Austrid Fine de Claire", 32m });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 22,
                columns: new[] { "Category", "Description", "Name", "Price" },
                values: new object[] { "Külmad road", "söel küpsetatud munakollane, kapparid, trühvel", "Marmorveise tartar", 28m });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 23,
                columns: new[] { "Description", "Name", "Price" },
                values: new object[] { "spargel, sidruniõli", "Hiidlest, beurre blanc, kalamari", 46m });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 24,
                columns: new[] { "Description", "Name", "Price" },
                values: new object[] { "laagerdatud rind, portveinikaste", "Part, kirss, suitsupeet", 39m });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 25,
                columns: new[] { "Category", "Description", "Name", "Price" },
                values: new object[] { "Magustoidud", "tume šokolaad 70%, oliiviõli", "Šokolaad ja meresool", 18m });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "Category", "Description", "Name", "Price" },
                values: new object[] { "Mereannid", "vetikavõi, laim", "Söel küpsetatud carabinero", 38m });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "Category", "Description", "Name", "Price" },
                values: new object[] { "Mereannid", "sarapuupähkel, pruunistatud või", "Kammkarbid ja lillkapsas", 34m });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "Category", "Description", "Name", "Price" },
                values: new object[] { "Mereannid", "roheline õun, nuikapsas, till", "Kamtšatka krabi", 36m });

            migrationBuilder.InsertData(
                table: "MenuItems",
                columns: new[] { "Id", "Category", "Description", "IsAvailable", "Name", "Price", "RestaurantId" },
                values: new object[,]
                {
                    { 9, "Mereannid", "tatrablinid, hapukoor", true, "Tuurakalamari, 30 g", 85m, 1 },
                    { 10, "Setid", "maja peamine õhtusöök, muutub hooajaga", true, "Set „Argent“ – 7 käiku", 120m, 1 },
                    { 19, "Mereannid", "tatrablinid, hapukoor", true, "Tuurakalamari, 30 g", 85m, 2 },
                    { 20, "Setid", "maja peamine õhtusöök, muutub hooajaga", true, "Set „Argent“ – 7 käiku", 120m, 2 },
                    { 29, "Mereannid", "tatrablinid, hapukoor", true, "Tuurakalamari, 30 g", 85m, 3 },
                    { 30, "Setid", "maja peamine õhtusöök, muutub hooajaga", true, "Set „Argent“ – 7 käiku", 120m, 3 },
                    { 101, "Setid", "austrid, krabi, kammkarbid, hiidlest, magustoit", true, "Set „Meri“ – 5 käiku", 95m, 1 },
                    { 102, "Setid", "liha ja ulukid lahtisel tulel", true, "Set „Teras“ – 6 käiku", 110m, 1 },
                    { 103, "Setid", "sommeljee klaas igale käigule", true, "Veinisaade", 65m, 1 },
                    { 104, "Pearoad", "Périgueux' kaste, kartulimuss", true, "Veise sisefilee ja mürklid", 52m, 1 },
                    { 105, "Magustoidud", "vaniljekreem, karamell", true, "Pirnisuflee", 16m, 1 },
                    { 106, "Veinid", "brut, Prantsusmaa", true, "Šampanja, klaas", 18m, 1 },
                    { 111, "Setid", "austrid, krabi, kammkarbid, hiidlest, magustoit", true, "Set „Meri“ – 5 käiku", 95m, 2 },
                    { 112, "Setid", "liha ja ulukid lahtisel tulel", true, "Set „Teras“ – 6 käiku", 110m, 2 },
                    { 113, "Setid", "sommeljee klaas igale käigule", true, "Veinisaade", 65m, 2 },
                    { 114, "Pearoad", "Périgueux' kaste, kartulimuss", true, "Veise sisefilee ja mürklid", 52m, 2 },
                    { 115, "Magustoidud", "vaniljekreem, karamell", true, "Pirnisuflee", 16m, 2 },
                    { 116, "Veinid", "brut, Prantsusmaa", true, "Šampanja, klaas", 18m, 2 },
                    { 121, "Setid", "austrid, krabi, kammkarbid, hiidlest, magustoit", true, "Set „Meri“ – 5 käiku", 95m, 3 },
                    { 122, "Setid", "liha ja ulukid lahtisel tulel", true, "Set „Teras“ – 6 käiku", 110m, 3 },
                    { 123, "Setid", "sommeljee klaas igale käigule", true, "Veinisaade", 65m, 3 },
                    { 124, "Pearoad", "Périgueux' kaste, kartulimuss", true, "Veise sisefilee ja mürklid", 52m, 3 },
                    { 125, "Magustoidud", "vaniljekreem, karamell", true, "Pirnisuflee", 16m, 3 },
                    { 126, "Veinid", "brut, Prantsusmaa", true, "Šampanja, klaas", 18m, 3 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 101);

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 102);

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 103);

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 104);

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 105);

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 106);

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 111);

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 112);

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 113);

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 114);

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 115);

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 116);

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 121);

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 122);

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 123);

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 124);

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 125);

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 126);

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Category", "Description", "Name", "Price" },
                values: new object[] { "Supid", "Metsaseentest koorene supp", "Seenesupp", 6.50m });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Category", "Description", "Name", "Price" },
                values: new object[] { "Supid", "Klassikaline Põhjamaade lõhesupp", "Lõhesupp", 8.90m });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Description", "Name", "Price" },
                values: new object[] { "Veise välisfilee, köögiviljad, punaveinikaste", "Veisesteik", 24.00m });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Description", "Name", "Price" },
                values: new object[] { "Lõhefilee, kartulipüree, tilli-võikaste", "Ahjulõhe", 19.50m });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Category", "Description", "Name", "Price" },
                values: new object[] { "Pearoad", "Kreemine risoto metsaseentega", "Seenerisoto", 15.00m });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Category", "Description", "Name", "Price" },
                values: new object[] { "Magustoidud", "Kamavaht marjadega", "Kamavaht", 6.00m });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Category", "Description", "Name", "Price" },
                values: new object[] { "Magustoidud", "Soe šokolaadikook jäätisega", "Šokolaadikook", 7.50m });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Category", "Description", "Name", "Price" },
                values: new object[] { "Joogid", "Rabarberi-piparmündi limonaad", "Kodune limonaad", 4.00m });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "Category", "Description", "Name", "Price" },
                values: new object[] { "Supid", "Metsaseentest koorene supp", "Seenesupp", 6.50m });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "Category", "Description", "Name", "Price" },
                values: new object[] { "Supid", "Klassikaline Põhjamaade lõhesupp", "Lõhesupp", 8.90m });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "Description", "Name", "Price" },
                values: new object[] { "Veise välisfilee, köögiviljad, punaveinikaste", "Veisesteik", 24.00m });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "Description", "Name", "Price" },
                values: new object[] { "Lõhefilee, kartulipüree, tilli-võikaste", "Ahjulõhe", 19.50m });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "Category", "Description", "Name", "Price" },
                values: new object[] { "Pearoad", "Kreemine risoto metsaseentega", "Seenerisoto", 15.00m });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "Category", "Description", "Name", "Price" },
                values: new object[] { "Magustoidud", "Kamavaht marjadega", "Kamavaht", 6.00m });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "Category", "Description", "Name", "Price" },
                values: new object[] { "Magustoidud", "Soe šokolaadikook jäätisega", "Šokolaadikook", 7.50m });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "Category", "Description", "Name", "Price" },
                values: new object[] { "Joogid", "Rabarberi-piparmündi limonaad", "Kodune limonaad", 4.00m });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 21,
                columns: new[] { "Category", "Description", "Name", "Price" },
                values: new object[] { "Supid", "Metsaseentest koorene supp", "Seenesupp", 6.50m });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 22,
                columns: new[] { "Category", "Description", "Name", "Price" },
                values: new object[] { "Supid", "Klassikaline Põhjamaade lõhesupp", "Lõhesupp", 8.90m });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 23,
                columns: new[] { "Description", "Name", "Price" },
                values: new object[] { "Veise välisfilee, köögiviljad, punaveinikaste", "Veisesteik", 24.00m });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 24,
                columns: new[] { "Description", "Name", "Price" },
                values: new object[] { "Lõhefilee, kartulipüree, tilli-võikaste", "Ahjulõhe", 19.50m });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 25,
                columns: new[] { "Category", "Description", "Name", "Price" },
                values: new object[] { "Pearoad", "Kreemine risoto metsaseentega", "Seenerisoto", 15.00m });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "Category", "Description", "Name", "Price" },
                values: new object[] { "Magustoidud", "Kamavaht marjadega", "Kamavaht", 6.00m });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "Category", "Description", "Name", "Price" },
                values: new object[] { "Magustoidud", "Soe šokolaadikook jäätisega", "Šokolaadikook", 7.50m });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "Category", "Description", "Name", "Price" },
                values: new object[] { "Joogid", "Rabarberi-piparmündi limonaad", "Kodune limonaad", 4.00m });
        }
    }
}
