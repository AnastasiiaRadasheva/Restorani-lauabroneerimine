using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Restoran.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ArgentBrand : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Restaurants",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ClosesAt", "Description", "Name", "OpensAt" },
                values: new object[] { new TimeOnly(23, 30, 0), "Kõrgköögi restoran vanalinna südames. Degustatsioonimenüü ja lahtine tuli.", "Argent · Vanalinn", new TimeOnly(17, 0, 0) });

            migrationBuilder.UpdateData(
                table: "Restaurants",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ClosesAt", "Description", "Name", "OpensAt" },
                values: new object[] { new TimeOnly(23, 30, 0), "Mereäärne saal ja suur terrass. Mereannid ja šampanja.", "Argent · Kalamaja", new TimeOnly(17, 0, 0) });

            migrationBuilder.UpdateData(
                table: "Restaurants",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ClosesAt", "Description", "Name", "OpensAt" },
                values: new object[] { new TimeOnly(23, 30, 0), "Vaikne saal Kadrioru pargi kõrval. Hooajalised setid ja veinid.", "Argent · Kadriorg", new TimeOnly(17, 0, 0) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Restaurants",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ClosesAt", "Description", "Name", "OpensAt" },
                values: new object[] { new TimeOnly(23, 0, 0), "Hubane restoran vanalinna südames. Hooajaline Eesti köök.", "Aed Vanalinn", new TimeOnly(12, 0, 0) });

            migrationBuilder.UpdateData(
                table: "Restaurants",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ClosesAt", "Description", "Name", "OpensAt" },
                values: new object[] { new TimeOnly(22, 0, 0), "Mereäärne Aed Kalamajas. Värske kala ja suur terrass.", "Aed Kalamaja", new TimeOnly(11, 0, 0) });

            migrationBuilder.UpdateData(
                table: "Restaurants",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ClosesAt", "Description", "Name", "OpensAt" },
                values: new object[] { new TimeOnly(23, 0, 0), "Rahulik Aed Kadrioru pargi kõrval.", "Aed Kadriorg", new TimeOnly(12, 0, 0) });
        }
    }
}
