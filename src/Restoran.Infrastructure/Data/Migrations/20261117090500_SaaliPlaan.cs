using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Restoran.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SaaliPlaan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRound",
                table: "Tables",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Spot",
                table: "Tables",
                type: "TEXT",
                maxLength: 60,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Restaurants",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "BarHeight", "BarWidth", "BarX", "BarY" },
                values: new object[] { 54, 26, 38, 46 });

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
                columns: new[] { "BarHeight", "BarWidth", "BarX", "BarY" },
                values: new object[] { 54, 26, 38, 46 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Height", "IsRound", "Seats", "Spot", "Width", "X" },
                values: new object[] { 36, false, 6, "akna all", 22, 5 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Height", "IsRound", "Seats", "Spot", "Width", "X", "Y" },
                values: new object[] { 36, false, 6, "akna all", 22, 5, 52 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Height", "IsRound", "Seats", "Spot", "Width", "X", "Y" },
                values: new object[] { 21, false, 2, "vaikne nurk", 22, 74, 3 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Height", "IsRound", "Spot", "Width", "X", "Y" },
                values: new object[] { 21, false, "baari juures", 22, 74, 27 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Height", "IsRound", "Seats", "Spot", "Width", "X", "Y" },
                values: new object[] { 21, false, 4, "baari juures", 22, 74, 51 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Height", "IsRound", "Location", "Seats", "Spot", "Width", "X", "Y" },
                values: new object[] { 21, false, "Hall", 4, "sissepääsu juures", 22, 74, 75 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Height", "IsRound", "Seats", "Spot", "X", "Y" },
                values: new object[] { 32, true, 2, "varikatuse all", 8, 8 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Height", "IsRound", "Spot", "X", "Y" },
                values: new object[] { 32, true, "vaade tänavale", 38, 8 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Height", "IsRound", "Seats", "Spot", "Width", "X", "Y" },
                values: new object[] { 34, false, 6, "kamina juures", 32, 10, 54 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "Height", "IsRound", "Seats", "Spot", "X", "Y" },
                values: new object[] { 58, false, 8, "pikk laud", 74, 30 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "Height", "IsRound", "Seats", "Spot", "Width", "X" },
                values: new object[] { 36, false, 6, "akna all", 22, 5 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "Height", "IsRound", "Seats", "Spot", "Width", "X", "Y" },
                values: new object[] { 36, false, 6, "akna all", 22, 5, 52 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "Height", "IsRound", "Seats", "Spot", "Width", "X", "Y" },
                values: new object[] { 21, false, 2, "vaikne nurk", 22, 74, 3 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "Height", "IsRound", "Spot", "Width", "X", "Y" },
                values: new object[] { 21, false, "baari juures", 22, 74, 27 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "Height", "IsRound", "Seats", "Spot", "Width", "X", "Y" },
                values: new object[] { 21, false, 4, "baari juures", 22, 74, 51 });

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
                columns: new[] { "Height", "IsRound", "Seats", "Spot", "X", "Y" },
                values: new object[] { 32, true, 2, "varikatuse all", 8, 8 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "Height", "IsRound", "Spot", "X", "Y" },
                values: new object[] { 32, true, "vaade tänavale", 38, 8 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "Height", "IsRound", "Seats", "Spot", "Width", "X", "Y" },
                values: new object[] { 34, false, 6, "kamina juures", 32, 10, 54 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "Height", "IsRound", "Seats", "Spot", "X", "Y" },
                values: new object[] { 58, false, 8, "pikk laud", 74, 30 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 21,
                columns: new[] { "Height", "IsRound", "Seats", "Spot", "Width", "X" },
                values: new object[] { 36, false, 6, "akna all", 22, 5 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 22,
                columns: new[] { "Height", "IsRound", "Seats", "Spot", "Width", "X", "Y" },
                values: new object[] { 36, false, 6, "akna all", 22, 5, 52 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 23,
                columns: new[] { "Height", "IsRound", "Seats", "Spot", "Width", "X", "Y" },
                values: new object[] { 21, false, 2, "vaikne nurk", 22, 74, 3 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 24,
                columns: new[] { "Height", "IsRound", "Spot", "Width", "X", "Y" },
                values: new object[] { 21, false, "baari juures", 22, 74, 27 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 25,
                columns: new[] { "Height", "IsRound", "Seats", "Spot", "Width", "X", "Y" },
                values: new object[] { 21, false, 4, "baari juures", 22, 74, 51 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "Height", "IsRound", "Location", "Seats", "Spot", "Width", "X", "Y" },
                values: new object[] { 21, false, "Hall", 4, "sissepääsu juures", 22, 74, 75 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "Height", "IsRound", "Seats", "Spot", "X", "Y" },
                values: new object[] { 32, true, 2, "varikatuse all", 8, 8 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "Height", "IsRound", "Spot", "X", "Y" },
                values: new object[] { 32, true, "vaade tänavale", 38, 8 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "Height", "IsRound", "Seats", "Spot", "Width", "X", "Y" },
                values: new object[] { 34, false, 6, "kamina juures", 32, 10, 54 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "Height", "IsRound", "Seats", "Spot", "X", "Y" },
                values: new object[] { 58, false, 8, "pikk laud", 74, 30 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRound",
                table: "Tables");

            migrationBuilder.DropColumn(
                name: "Spot",
                table: "Tables");

            migrationBuilder.UpdateData(
                table: "Restaurants",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "BarHeight", "BarWidth", "BarX", "BarY" },
                values: new object[] { 50, 22, 42, 45 });

            migrationBuilder.UpdateData(
                table: "Restaurants",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "BarHeight", "BarWidth", "BarX", "BarY" },
                values: new object[] { 40, 22, 42, 5 });

            migrationBuilder.UpdateData(
                table: "Restaurants",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "BarHeight", "BarWidth", "BarX", "BarY" },
                values: new object[] { 50, 22, 42, 45 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Height", "Seats", "Width", "X" },
                values: new object[] { 22, 4, 16, 4 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Height", "Seats", "Width", "X", "Y" },
                values: new object[] { 14, 2, 12, 24, 6 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Height", "Seats", "Width", "X", "Y" },
                values: new object[] { 38, 6, 18, 4, 55 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Height", "Width", "X", "Y" },
                values: new object[] { 14, 12, 24, 30 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Height", "Seats", "Width", "X", "Y" },
                values: new object[] { 35, 8, 14, 24, 58 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Height", "Location", "Seats", "Width", "X", "Y" },
                values: new object[] { 12, "Terrace", 2, 18, 78, 4 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Height", "Seats", "X", "Y" },
                values: new object[] { 16, 4, 78, 22 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Height", "X", "Y" },
                values: new object[] { 16, 78, 44 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Height", "Seats", "Width", "X", "Y" },
                values: new object[] { 12, 2, 18, 78, 66 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "Height", "Seats", "X", "Y" },
                values: new object[] { 14, 6, 78, 82 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "Height", "Seats", "Width", "X" },
                values: new object[] { 22, 4, 16, 4 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "Height", "Seats", "Width", "X", "Y" },
                values: new object[] { 14, 2, 12, 24, 6 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "Height", "Seats", "Width", "X", "Y" },
                values: new object[] { 38, 6, 18, 4, 55 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "Height", "Width", "X", "Y" },
                values: new object[] { 14, 12, 24, 30 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "Height", "Seats", "Width", "X", "Y" },
                values: new object[] { 35, 8, 14, 24, 58 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "Height", "Location", "Seats", "Width", "X", "Y" },
                values: new object[] { 12, "Terrace", 2, 18, 78, 4 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "Height", "Seats", "X", "Y" },
                values: new object[] { 16, 4, 78, 22 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "Height", "X", "Y" },
                values: new object[] { 16, 78, 44 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "Height", "Seats", "Width", "X", "Y" },
                values: new object[] { 12, 2, 18, 78, 66 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "Height", "Seats", "X", "Y" },
                values: new object[] { 14, 6, 78, 82 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 21,
                columns: new[] { "Height", "Seats", "Width", "X" },
                values: new object[] { 22, 4, 16, 4 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 22,
                columns: new[] { "Height", "Seats", "Width", "X", "Y" },
                values: new object[] { 14, 2, 12, 24, 6 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 23,
                columns: new[] { "Height", "Seats", "Width", "X", "Y" },
                values: new object[] { 38, 6, 18, 4, 55 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 24,
                columns: new[] { "Height", "Width", "X", "Y" },
                values: new object[] { 14, 12, 24, 30 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 25,
                columns: new[] { "Height", "Seats", "Width", "X", "Y" },
                values: new object[] { 35, 8, 14, 24, 58 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "Height", "Location", "Seats", "Width", "X", "Y" },
                values: new object[] { 12, "Terrace", 2, 18, 78, 4 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "Height", "Seats", "X", "Y" },
                values: new object[] { 16, 4, 78, 22 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "Height", "X", "Y" },
                values: new object[] { 16, 78, 44 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "Height", "Seats", "Width", "X", "Y" },
                values: new object[] { 12, 2, 18, 78, 66 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "Height", "Seats", "X", "Y" },
                values: new object[] { 14, 6, 78, 82 });
        }
    }
}
