using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Restoran.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Restaurants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Latitude = table.Column<double>(type: "REAL", nullable: false),
                    Longitude = table.Column<double>(type: "REAL", nullable: false),
                    OpensAt = table.Column<TimeOnly>(type: "TEXT", nullable: false),
                    ClosesAt = table.Column<TimeOnly>(type: "TEXT", nullable: false),
                    BarX = table.Column<int>(type: "INTEGER", nullable: false),
                    BarY = table.Column<int>(type: "INTEGER", nullable: false),
                    BarWidth = table.Column<int>(type: "INTEGER", nullable: false),
                    BarHeight = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Restaurants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    Role = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MenuItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RestaurantId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Price = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    IsAvailable = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MenuItems_Restaurants_RestaurantId",
                        column: x => x.RestaurantId,
                        principalTable: "Restaurants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tables",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RestaurantId = table.Column<int>(type: "INTEGER", nullable: false),
                    Number = table.Column<int>(type: "INTEGER", nullable: false),
                    Seats = table.Column<int>(type: "INTEGER", nullable: false),
                    Location = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    X = table.Column<int>(type: "INTEGER", nullable: false),
                    Y = table.Column<int>(type: "INTEGER", nullable: false),
                    Width = table.Column<int>(type: "INTEGER", nullable: false),
                    Height = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tables_Restaurants_RestaurantId",
                        column: x => x.RestaurantId,
                        principalTable: "Restaurants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    ReservationId = table.Column<int>(type: "INTEGER", nullable: true),
                    Message = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    IsRead = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reservations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TableId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    StartTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Guests = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    SpecialRequest = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IsPaid = table.Column<bool>(type: "INTEGER", nullable: false),
                    Reminded = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reservations_Tables_TableId",
                        column: x => x.TableId,
                        principalTable: "Tables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reservations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PreOrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ReservationId = table.Column<int>(type: "INTEGER", nullable: false),
                    MenuItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreOrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PreOrderItems_MenuItems_MenuItemId",
                        column: x => x.MenuItemId,
                        principalTable: "MenuItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PreOrderItems_Reservations_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Restaurants",
                columns: new[] { "Id", "Address", "BarHeight", "BarWidth", "BarX", "BarY", "ClosesAt", "Description", "Latitude", "Longitude", "Name", "OpensAt" },
                values: new object[,]
                {
                    { 1, "Rataskaevu 8, Tallinn", 50, 22, 42, 45, new TimeOnly(23, 0, 0), "Hubane restoran vanalinna südames. Hooajaline Eesti köök.", 59.436999999999998, 24.742799999999999, "Aed Vanalinn", new TimeOnly(12, 0, 0) },
                    { 2, "Kalasadama 8, Tallinn", 40, 22, 42, 5, new TimeOnly(22, 0, 0), "Mereäärne Aed Kalamajas. Värske kala ja suur terrass.", 59.446599999999997, 24.7395, "Aed Kalamaja", new TimeOnly(11, 0, 0) },
                    { 3, "Weizenbergi 18, Tallinn", 50, 22, 42, 45, new TimeOnly(23, 0, 0), "Rahulik Aed Kadrioru pargi kõrval.", 59.438499999999998, 24.792000000000002, "Aed Kadriorg", new TimeOnly(12, 0, 0) }
                });

            migrationBuilder.InsertData(
                table: "MenuItems",
                columns: new[] { "Id", "Category", "Description", "IsAvailable", "Name", "Price", "RestaurantId" },
                values: new object[,]
                {
                    { 1, "Supid", "Metsaseentest koorene supp", true, "Seenesupp", 6.50m, 1 },
                    { 2, "Supid", "Klassikaline Põhjamaade lõhesupp", true, "Lõhesupp", 8.90m, 1 },
                    { 3, "Pearoad", "Veise välisfilee, köögiviljad, punaveinikaste", true, "Veisesteik", 24.00m, 1 },
                    { 4, "Pearoad", "Lõhefilee, kartulipüree, tilli-võikaste", true, "Ahjulõhe", 19.50m, 1 },
                    { 5, "Pearoad", "Kreemine risoto metsaseentega", true, "Seenerisoto", 15.00m, 1 },
                    { 6, "Magustoidud", "Kamavaht marjadega", true, "Kamavaht", 6.00m, 1 },
                    { 7, "Magustoidud", "Soe šokolaadikook jäätisega", true, "Šokolaadikook", 7.50m, 1 },
                    { 8, "Joogid", "Rabarberi-piparmündi limonaad", true, "Kodune limonaad", 4.00m, 1 },
                    { 11, "Supid", "Metsaseentest koorene supp", true, "Seenesupp", 6.50m, 2 },
                    { 12, "Supid", "Klassikaline Põhjamaade lõhesupp", true, "Lõhesupp", 8.90m, 2 },
                    { 13, "Pearoad", "Veise välisfilee, köögiviljad, punaveinikaste", true, "Veisesteik", 24.00m, 2 },
                    { 14, "Pearoad", "Lõhefilee, kartulipüree, tilli-võikaste", true, "Ahjulõhe", 19.50m, 2 },
                    { 15, "Pearoad", "Kreemine risoto metsaseentega", true, "Seenerisoto", 15.00m, 2 },
                    { 16, "Magustoidud", "Kamavaht marjadega", true, "Kamavaht", 6.00m, 2 },
                    { 17, "Magustoidud", "Soe šokolaadikook jäätisega", true, "Šokolaadikook", 7.50m, 2 },
                    { 18, "Joogid", "Rabarberi-piparmündi limonaad", true, "Kodune limonaad", 4.00m, 2 },
                    { 21, "Supid", "Metsaseentest koorene supp", true, "Seenesupp", 6.50m, 3 },
                    { 22, "Supid", "Klassikaline Põhjamaade lõhesupp", true, "Lõhesupp", 8.90m, 3 },
                    { 23, "Pearoad", "Veise välisfilee, köögiviljad, punaveinikaste", true, "Veisesteik", 24.00m, 3 },
                    { 24, "Pearoad", "Lõhefilee, kartulipüree, tilli-võikaste", true, "Ahjulõhe", 19.50m, 3 },
                    { 25, "Pearoad", "Kreemine risoto metsaseentega", true, "Seenerisoto", 15.00m, 3 },
                    { 26, "Magustoidud", "Kamavaht marjadega", true, "Kamavaht", 6.00m, 3 },
                    { 27, "Magustoidud", "Soe šokolaadikook jäätisega", true, "Šokolaadikook", 7.50m, 3 },
                    { 28, "Joogid", "Rabarberi-piparmündi limonaad", true, "Kodune limonaad", 4.00m, 3 }
                });

            migrationBuilder.InsertData(
                table: "Tables",
                columns: new[] { "Id", "Height", "IsActive", "Location", "Number", "RestaurantId", "Seats", "Width", "X", "Y" },
                values: new object[,]
                {
                    { 1, 22, true, "Hall", 1, 1, 4, 16, 4, 6 },
                    { 2, 14, true, "Hall", 2, 1, 2, 12, 24, 6 },
                    { 3, 38, true, "Hall", 3, 1, 6, 18, 4, 55 },
                    { 4, 14, true, "Hall", 4, 1, 2, 12, 24, 30 },
                    { 5, 35, true, "Hall", 5, 1, 8, 14, 24, 58 },
                    { 6, 12, true, "Terrace", 6, 1, 2, 18, 78, 4 },
                    { 7, 16, true, "Terrace", 7, 1, 4, 18, 78, 22 },
                    { 8, 16, true, "Terrace", 8, 1, 4, 18, 78, 44 },
                    { 9, 12, true, "Terrace", 9, 1, 2, 18, 78, 66 },
                    { 10, 14, true, "Terrace", 10, 1, 6, 18, 78, 82 },
                    { 11, 22, true, "Hall", 1, 2, 4, 16, 4, 6 },
                    { 12, 14, true, "Hall", 2, 2, 2, 12, 24, 6 },
                    { 13, 38, true, "Hall", 3, 2, 6, 18, 4, 55 },
                    { 14, 14, true, "Hall", 4, 2, 2, 12, 24, 30 },
                    { 15, 35, true, "Hall", 5, 2, 8, 14, 24, 58 },
                    { 16, 12, true, "Terrace", 6, 2, 2, 18, 78, 4 },
                    { 17, 16, true, "Terrace", 7, 2, 4, 18, 78, 22 },
                    { 18, 16, true, "Terrace", 8, 2, 4, 18, 78, 44 },
                    { 19, 12, true, "Terrace", 9, 2, 2, 18, 78, 66 },
                    { 20, 14, true, "Terrace", 10, 2, 6, 18, 78, 82 },
                    { 21, 22, true, "Hall", 1, 3, 4, 16, 4, 6 },
                    { 22, 14, true, "Hall", 2, 3, 2, 12, 24, 6 },
                    { 23, 38, true, "Hall", 3, 3, 6, 18, 4, 55 },
                    { 24, 14, true, "Hall", 4, 3, 2, 12, 24, 30 },
                    { 25, 35, true, "Hall", 5, 3, 8, 14, 24, 58 },
                    { 26, 12, true, "Terrace", 6, 3, 2, 18, 78, 4 },
                    { 27, 16, true, "Terrace", 7, 3, 4, 18, 78, 22 },
                    { 28, 16, true, "Terrace", 8, 3, 4, 18, 78, 44 },
                    { 29, 12, true, "Terrace", 9, 3, 2, 18, 78, 66 },
                    { 30, 14, true, "Terrace", 10, 3, 6, 18, 78, 82 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_RestaurantId",
                table: "MenuItems",
                column: "RestaurantId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PreOrderItems_MenuItemId",
                table: "PreOrderItems",
                column: "MenuItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PreOrderItems_ReservationId",
                table: "PreOrderItems",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_TableId_StartTime",
                table: "Reservations",
                columns: new[] { "TableId", "StartTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_UserId",
                table: "Reservations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tables_RestaurantId_Number",
                table: "Tables",
                columns: new[] { "RestaurantId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "PreOrderItems");

            migrationBuilder.DropTable(
                name: "MenuItems");

            migrationBuilder.DropTable(
                name: "Reservations");

            migrationBuilder.DropTable(
                name: "Tables");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Restaurants");
        }
    }
}
