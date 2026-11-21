using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Restoran.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ParandusedJaKorrused : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAt",
                table: "Reservations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RefundAmount",
                table: "Reservations",
                type: "TEXT",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "MenuItems",
                type: "TEXT",
                maxLength: 300,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 1,
                column: "ImageUrl",
                value: "/img/menu/austrid.jpg");

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 2,
                column: "ImageUrl",
                value: "/img/menu/tartar.jpg");

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 3,
                column: "ImageUrl",
                value: "/img/menu/hiidlest.jpg");

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 4,
                column: "ImageUrl",
                value: "/img/menu/part.jpg");

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 5,
                column: "ImageUrl",
                value: "/img/menu/sokolaad.jpg");

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 6,
                column: "ImageUrl",
                value: "/img/menu/carabinero.jpg");

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 7,
                column: "ImageUrl",
                value: "/img/menu/kammkarbid.jpg");

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 8,
                column: "ImageUrl",
                value: "/img/menu/krabi.jpg");

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 9,
                column: "ImageUrl",
                value: "/img/menu/kalamari.jpg");

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 10,
                column: "ImageUrl",
                value: "/img/menu/set-argent.jpg");

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 11,
                column: "ImageUrl",
                value: "/img/menu/austrid.jpg");

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 12,
                column: "ImageUrl",
                value: "/img/menu/tartar.jpg");

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 13,
                column: "ImageUrl",
                value: "/img/menu/hiidlest.jpg");

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 14,
                column: "ImageUrl",
                value: "/img/menu/part.jpg");

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 15,
                column: "ImageUrl",
                value: "/img/menu/sokolaad.jpg");

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 16,
                column: "ImageUrl",
                value: "/img/menu/carabinero.jpg");

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 17,
                column: "ImageUrl",
                value: "/img/menu/kammkarbid.jpg");

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 18,
                column: "ImageUrl",
                value: "/img/menu/krabi.jpg");

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 19,
                column: "ImageUrl",
                value: "/img/menu/kalamari.jpg");

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 20,
                column: "ImageUrl",
                value: "/img/menu/set-argent.jpg");

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 21,
                column: "ImageUrl",
                value: "/img/menu/austrid.jpg");

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 22,
                column: "ImageUrl",
                value: "/img/menu/tartar.jpg");

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 23,
                column: "ImageUrl",
                value: "/img/menu/hiidlest.jpg");

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 24,
                column: "ImageUrl",
                value: "/img/menu/part.jpg");

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 25,
                column: "ImageUrl",
                value: "/img/menu/sokolaad.jpg");

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 26,
                column: "ImageUrl",
                value: "/img/menu/carabinero.jpg");

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 27,
                column: "ImageUrl",
                value: "/img/menu/kammkarbid.jpg");

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 28,
                column: "ImageUrl",
                value: "/img/menu/krabi.jpg");

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 29,
                column: "ImageUrl",
                value: "/img/menu/kalamari.jpg");

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 30,
                column: "ImageUrl",
                value: "/img/menu/set-argent.jpg");

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 101,
                column: "ImageUrl",
                value: "/img/menu/set-meri.jpg");

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 102,
                column: "ImageUrl",
                value: "/img/menu/set-teras.jpg");

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 103,
                column: "ImageUrl",
                value: "/img/menu/veinisaade.jpg");

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 104,
                column: "ImageUrl",
                value: "/img/menu/sisefilee.jpg");

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 105,
                column: "ImageUrl",
                value: "/img/menu/suflee.jpg");

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 106,
                column: "ImageUrl",
                value: "/img/menu/sampanja.jpg");

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 111,
                column: "ImageUrl",
                value: "/img/menu/set-meri.jpg");

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 112,
                column: "ImageUrl",
                value: "/img/menu/set-teras.jpg");

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 113,
                column: "ImageUrl",
                value: "/img/menu/veinisaade.jpg");

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 114,
                column: "ImageUrl",
                value: "/img/menu/sisefilee.jpg");

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 115,
                column: "ImageUrl",
                value: "/img/menu/suflee.jpg");

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 116,
                column: "ImageUrl",
                value: "/img/menu/sampanja.jpg");

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 121,
                column: "ImageUrl",
                value: "/img/menu/set-meri.jpg");

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 122,
                column: "ImageUrl",
                value: "/img/menu/set-teras.jpg");

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 123,
                column: "ImageUrl",
                value: "/img/menu/veinisaade.jpg");

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 124,
                column: "ImageUrl",
                value: "/img/menu/sisefilee.jpg");

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 125,
                column: "ImageUrl",
                value: "/img/menu/suflee.jpg");

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 126,
                column: "ImageUrl",
                value: "/img/menu/sampanja.jpg");

            migrationBuilder.UpdateData(
                table: "Restaurants",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "BarHeight", "BarWidth", "BarX", "BarY" },
                values: new object[] { 36, 30, 34, 58 });

            migrationBuilder.UpdateData(
                table: "Restaurants",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "BarHeight", "BarX", "BarY" },
                values: new object[] { 40, 36, 56 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Height", "Location", "Spot" },
                values: new object[] { 30, "Upstairs", "akna all" });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Height", "IsRound", "Location", "Spot", "Width", "X" },
                values: new object[] { 30, false, "Upstairs", "kamina juures", 22, 36 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Height", "Location", "Spot", "Width", "X", "Y" },
                values: new object[] { 40, "Upstairs", "privaatne nurk", 26, 68, 6 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "Height", "Location", "Width", "X", "Y" },
                values: new object[] { 32, "Upstairs", 54, 10, 56 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "Height", "Seats", "Width", "X" },
                values: new object[] { 26, 4, 20, 6 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "Height", "Seats", "Width", "X", "Y" },
                values: new object[] { 26, 4, 20, 6, 38 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "Height", "Spot", "Width", "X", "Y" },
                values: new object[] { 22, "sissepääsu juures", 20, 6, 72 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "Height", "Seats", "Spot", "Width", "X", "Y" },
                values: new object[] { 40, 6, "vaade merele", 24, 70, 6 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "Height", "Seats", "Width", "X", "Y" },
                values: new object[] { 24, 2, 20, 72, 60 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "Height", "IsRound", "Location", "Seats", "Spot", "Width", "X", "Y" },
                values: new object[] { 30, true, "Terrace", 2, "mere ääres", 16, 6, 8 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "Height", "Spot", "Width", "X" },
                values: new object[] { 30, "mere ääres", 16, 28 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "Spot", "X" },
                values: new object[] { "päikese käes", 50 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "Height", "Spot", "Width", "X", "Y" },
                values: new object[] { 32, "varikatuse all", 34, 8, 56 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "Height", "Width", "X", "Y" },
                values: new object[] { 36, 36, 58, 52 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 21,
                columns: new[] { "Height", "Seats", "Width", "X", "Y" },
                values: new object[] { 24, 2, 20, 6, 8 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 22,
                columns: new[] { "Height", "Seats", "X", "Y" },
                values: new object[] { 28, 4, 6, 42 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 23,
                columns: new[] { "Height", "Seats", "X", "Y" },
                values: new object[] { 28, 4, 72, 8 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 24,
                columns: new[] { "Height", "Seats", "Width", "X", "Y" },
                values: new object[] { 40, 6, 24, 70, 52 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 25,
                columns: new[] { "Height", "IsRound", "Location", "Seats", "Spot", "Width", "X", "Y" },
                values: new object[] { 34, true, "Terrace", 2, "vaade pargile", 18, 10, 14 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "Height", "IsRound", "Location", "Spot", "Width", "X", "Y" },
                values: new object[] { 34, true, "Terrace", "vaade pargile", 18, 40, 14 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "Height", "Seats", "Spot", "X", "Y" },
                values: new object[] { 34, 4, "purskkaevu juures", 70, 14 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "Height", "IsRound", "Location", "Seats", "Spot", "Width", "X", "Y" },
                values: new object[] { 26, false, "Upstairs", 2, "akna all", 20, 10, 12 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "Height", "Location", "Width", "X", "Y" },
                values: new object[] { 40, "Upstairs", 26, 40, 12 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "Height", "Location", "Spot", "Width", "X", "Y" },
                values: new object[] { 30, "Upstairs", "pidulik laud", 60, 12, 60 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancelledAt",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "RefundAmount",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "MenuItems");

            migrationBuilder.UpdateData(
                table: "Restaurants",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "BarHeight", "BarWidth", "BarX", "BarY" },
                values: new object[] { 54, 26, 38, 46 });

            migrationBuilder.UpdateData(
                table: "Restaurants",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "BarHeight", "BarX", "BarY" },
                values: new object[] { 54, 38, 46 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Height", "Location", "Spot" },
                values: new object[] { 32, "Terrace", "varikatuse all" });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Height", "IsRound", "Location", "Spot", "Width", "X" },
                values: new object[] { 32, true, "Terrace", "vaade tänavale", 18, 38 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Height", "Location", "Spot", "Width", "X", "Y" },
                values: new object[] { 34, "Terrace", "kamina juures", 32, 10, 54 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "Height", "Location", "Width", "X", "Y" },
                values: new object[] { 58, "Terrace", 18, 74, 30 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "Height", "Seats", "Width", "X" },
                values: new object[] { 36, 6, 22, 5 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "Height", "Seats", "Width", "X", "Y" },
                values: new object[] { 36, 6, 22, 5, 52 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "Height", "Spot", "Width", "X", "Y" },
                values: new object[] { 21, "vaikne nurk", 22, 74, 3 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "Height", "Seats", "Spot", "Width", "X", "Y" },
                values: new object[] { 21, 2, "baari juures", 22, 74, 27 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "Height", "Seats", "Width", "X", "Y" },
                values: new object[] { 21, 4, 22, 74, 51 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "Height", "IsRound", "Location", "Seats", "Spot", "Width", "X", "Y" },
                values: new object[] { 21, false, "Hall", 4, "sissepääsu juures", 22, 74, 75 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "Height", "Spot", "Width", "X" },
                values: new object[] { 32, "varikatuse all", 18, 8 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "Spot", "X" },
                values: new object[] { "vaade tänavale", 38 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "Height", "Spot", "Width", "X", "Y" },
                values: new object[] { 34, "kamina juures", 32, 10, 54 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "Height", "Width", "X", "Y" },
                values: new object[] { 58, 18, 74, 30 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 21,
                columns: new[] { "Height", "Seats", "Width", "X", "Y" },
                values: new object[] { 36, 6, 22, 5, 6 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 22,
                columns: new[] { "Height", "Seats", "X", "Y" },
                values: new object[] { 36, 6, 5, 52 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 23,
                columns: new[] { "Height", "Seats", "X", "Y" },
                values: new object[] { 21, 2, 74, 3 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 24,
                columns: new[] { "Height", "Seats", "Width", "X", "Y" },
                values: new object[] { 21, 2, 22, 74, 27 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 25,
                columns: new[] { "Height", "IsRound", "Location", "Seats", "Spot", "Width", "X", "Y" },
                values: new object[] { 21, false, "Hall", 4, "baari juures", 22, 74, 51 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "Height", "IsRound", "Location", "Spot", "Width", "X", "Y" },
                values: new object[] { 21, false, "Hall", "sissepääsu juures", 22, 74, 75 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "Height", "Seats", "Spot", "X", "Y" },
                values: new object[] { 32, 2, "varikatuse all", 8, 8 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "Height", "IsRound", "Location", "Seats", "Spot", "Width", "X", "Y" },
                values: new object[] { 32, true, "Terrace", 4, "vaade tänavale", 18, 38, 8 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "Height", "Location", "Width", "X", "Y" },
                values: new object[] { 34, "Terrace", 32, 10, 54 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "Height", "Location", "Spot", "Width", "X", "Y" },
                values: new object[] { 58, "Terrace", "pikk laud", 18, 74, 30 });
        }
    }
}
