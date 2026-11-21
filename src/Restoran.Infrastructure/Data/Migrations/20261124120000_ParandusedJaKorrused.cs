using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Restoran.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ParandusedJaKorrused : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "Users",
                type: "TEXT",
                maxLength: 5,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "PasswordSet",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PasswordToken",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordTokenExpires",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SmsOptIn",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "StageHeight",
                table: "Restaurants",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StageWidth",
                table: "Restaurants",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StageX",
                table: "Restaurants",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StageY",
                table: "Restaurants",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "StageZone",
                table: "Restaurants",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Allergies",
                table: "Reservations",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AllergyNote",
                table: "Reservations",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAt",
                table: "Reservations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentToken",
                table: "Reservations",
                type: "TEXT",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RefundAmount",
                table: "Reservations",
                type: "TEXT",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Locked",
                table: "ReservationExtras",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Link",
                table: "Notifications",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "MenuItems",
                type: "TEXT",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "MenuItems",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Events",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "NoChildren",
                table: "Events",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Zone",
                table: "Events",
                type: "TEXT",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAt",
                table: "EventRegistrations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Drinks",
                table: "EventRegistrations",
                type: "TEXT",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Extras",
                table: "EventRegistrations",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPaid",
                table: "EventRegistrations",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PaymentToken",
                table: "EventRegistrations",
                type: "TEXT",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RefundAmount",
                table: "EventRegistrations",
                type: "TEXT",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "EventRegistrations",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TableId",
                table: "EventRegistrations",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Total",
                table: "EventRegistrations",
                type: "TEXT",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "EventMenuItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EventId = table.Column<int>(type: "INTEGER", nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    Price = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    ImageUrl = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    MenuItemId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventMenuItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventMenuItems_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupportMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    FromStaff = table.Column<bool>(type: "INTEGER", nullable: false),
                    AuthorName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Text = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    ReadByStaff = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupportMessages_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ImageUrl", "Tags" },
                values: new object[] { "/img/menu/austrid.jpg", "shellfish" });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ImageUrl", "Tags" },
                values: new object[] { "/img/menu/tartar.jpg", "egg" });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ImageUrl", "Tags" },
                values: new object[] { "/img/menu/hiidlest.jpg", "fish,milk" });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ImageUrl", "Tags" },
                values: new object[] { "/img/menu/part.jpg", null });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "ImageUrl", "Tags" },
                values: new object[] { "/img/menu/sokolaad.jpg", "milk,vegetarian" });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "ImageUrl", "Tags" },
                values: new object[] { "/img/menu/carabinero.jpg", "shellfish,milk" });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "ImageUrl", "Tags" },
                values: new object[] { "/img/menu/kammkarbid.jpg", "shellfish,nuts,milk" });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "ImageUrl", "Tags" },
                values: new object[] { "/img/menu/krabi.jpg", "shellfish" });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "ImageUrl", "Tags" },
                values: new object[] { "/img/menu/kalamari.jpg", "fish,gluten,milk" });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "ImageUrl", "Tags" },
                values: new object[] { "/img/menu/set-argent.jpg", "gluten,milk,fish" });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "ImageUrl", "Tags" },
                values: new object[] { "/img/menu/austrid.jpg", "shellfish" });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "ImageUrl", "Tags" },
                values: new object[] { "/img/menu/tartar.jpg", "egg" });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "ImageUrl", "Tags" },
                values: new object[] { "/img/menu/hiidlest.jpg", "fish,milk" });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "ImageUrl", "Tags" },
                values: new object[] { "/img/menu/part.jpg", null });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "ImageUrl", "Tags" },
                values: new object[] { "/img/menu/sokolaad.jpg", "milk,vegetarian" });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "ImageUrl", "Tags" },
                values: new object[] { "/img/menu/carabinero.jpg", "shellfish,milk" });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "ImageUrl", "Tags" },
                values: new object[] { "/img/menu/kammkarbid.jpg", "shellfish,nuts,milk" });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "ImageUrl", "Tags" },
                values: new object[] { "/img/menu/krabi.jpg", "shellfish" });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "ImageUrl", "Tags" },
                values: new object[] { "/img/menu/kalamari.jpg", "fish,gluten,milk" });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "ImageUrl", "Tags" },
                values: new object[] { "/img/menu/set-argent.jpg", "gluten,milk,fish" });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 21,
                columns: new[] { "ImageUrl", "Tags" },
                values: new object[] { "/img/menu/austrid.jpg", "shellfish" });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 22,
                columns: new[] { "ImageUrl", "Tags" },
                values: new object[] { "/img/menu/tartar.jpg", "egg" });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 23,
                columns: new[] { "ImageUrl", "Tags" },
                values: new object[] { "/img/menu/hiidlest.jpg", "fish,milk" });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 24,
                columns: new[] { "ImageUrl", "Tags" },
                values: new object[] { "/img/menu/part.jpg", null });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 25,
                columns: new[] { "ImageUrl", "Tags" },
                values: new object[] { "/img/menu/sokolaad.jpg", "milk,vegetarian" });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "ImageUrl", "Tags" },
                values: new object[] { "/img/menu/carabinero.jpg", "shellfish,milk" });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "ImageUrl", "Tags" },
                values: new object[] { "/img/menu/kammkarbid.jpg", "shellfish,nuts,milk" });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "ImageUrl", "Tags" },
                values: new object[] { "/img/menu/krabi.jpg", "shellfish" });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "ImageUrl", "Tags" },
                values: new object[] { "/img/menu/kalamari.jpg", "fish,gluten,milk" });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "ImageUrl", "Tags" },
                values: new object[] { "/img/menu/set-argent.jpg", "gluten,milk,fish" });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 101,
                columns: new[] { "ImageUrl", "Tags" },
                values: new object[] { "/img/menu/set-meri.jpg", "shellfish,fish,milk" });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 102,
                columns: new[] { "ImageUrl", "Tags" },
                values: new object[] { "/img/menu/set-teras.jpg", "gluten" });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 103,
                columns: new[] { "ImageUrl", "Tags" },
                values: new object[] { "/img/menu/veinisaade.jpg", "vegan" });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 104,
                columns: new[] { "ImageUrl", "Tags" },
                values: new object[] { "/img/menu/sisefilee.jpg", "milk" });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 105,
                columns: new[] { "ImageUrl", "Tags" },
                values: new object[] { "/img/menu/suflee.jpg", "gluten,milk,egg,vegetarian" });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 106,
                columns: new[] { "ImageUrl", "Tags" },
                values: new object[] { "/img/menu/sampanja.jpg", "vegan" });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 111,
                columns: new[] { "ImageUrl", "Tags" },
                values: new object[] { "/img/menu/set-meri.jpg", "shellfish,fish,milk" });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 112,
                columns: new[] { "ImageUrl", "Tags" },
                values: new object[] { "/img/menu/set-teras.jpg", "gluten" });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 113,
                columns: new[] { "ImageUrl", "Tags" },
                values: new object[] { "/img/menu/veinisaade.jpg", "vegan" });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 114,
                columns: new[] { "ImageUrl", "Tags" },
                values: new object[] { "/img/menu/sisefilee.jpg", "milk" });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 115,
                columns: new[] { "ImageUrl", "Tags" },
                values: new object[] { "/img/menu/suflee.jpg", "gluten,milk,egg,vegetarian" });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 116,
                columns: new[] { "ImageUrl", "Tags" },
                values: new object[] { "/img/menu/sampanja.jpg", "vegan" });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 121,
                columns: new[] { "ImageUrl", "Tags" },
                values: new object[] { "/img/menu/set-meri.jpg", "shellfish,fish,milk" });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 122,
                columns: new[] { "ImageUrl", "Tags" },
                values: new object[] { "/img/menu/set-teras.jpg", "gluten" });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 123,
                columns: new[] { "ImageUrl", "Tags" },
                values: new object[] { "/img/menu/veinisaade.jpg", "vegan" });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 124,
                columns: new[] { "ImageUrl", "Tags" },
                values: new object[] { "/img/menu/sisefilee.jpg", "milk" });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 125,
                columns: new[] { "ImageUrl", "Tags" },
                values: new object[] { "/img/menu/suflee.jpg", "gluten,milk,egg,vegetarian" });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 126,
                columns: new[] { "ImageUrl", "Tags" },
                values: new object[] { "/img/menu/sampanja.jpg", "vegan" });

            migrationBuilder.UpdateData(
                table: "Restaurants",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "BarHeight", "BarWidth", "BarX", "BarY", "StageHeight", "StageWidth", "StageX", "StageY", "StageZone" },
                values: new object[] { 34, 20, 40, 40, 38, 18, 78, 50, "Upstairs" });

            migrationBuilder.UpdateData(
                table: "Restaurants",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "BarHeight", "BarWidth", "BarX", "BarY", "ClosesAt", "StageHeight", "StageWidth", "StageX", "StageY", "StageZone" },
                values: new object[] { 36, 30, 34, 58, new TimeOnly(3, 0, 0), 12, 46, 14, 34, "Terrace" });

            migrationBuilder.UpdateData(
                table: "Restaurants",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "BarHeight", "BarWidth", "BarY", "StageHeight", "StageWidth", "StageX", "StageY", "StageZone" },
                values: new object[] { 36, 22, 60, 20, 26, 36, 12, "Hall" });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Height", "IsRound", "Seats", "Width", "X", "Y" },
                values: new object[] { 16, true, 2, 10, 4, 4 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Height", "IsRound", "Seats", "Width", "X", "Y" },
                values: new object[] { 16, true, 2, 10, 16, 4 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Height", "Seats", "Spot", "Width", "X", "Y" },
                values: new object[] { 16, 4, "akna all", 14, 28, 4 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Height", "Seats", "Spot", "Width", "X", "Y" },
                values: new object[] { 30, 6, "diivan", 12, 4, 28 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Height", "Spot", "Width", "X", "Y" },
                values: new object[] { 16, "saali keskel", 14, 20, 32 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Height", "IsRound", "Seats", "Spot", "Width", "X", "Y" },
                values: new object[] { 16, true, 2, "saali keskel", 10, 28, 52 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Height", "IsRound", "Location", "Seats", "Spot", "Width", "X", "Y" },
                values: new object[] { 24, false, "Hall", 8, "pikk laud", 22, 4, 68 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Height", "IsRound", "Location", "Spot", "Width", "X", "Y" },
                values: new object[] { 16, false, "Hall", "vaikne nurk", 14, 64, 4 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Height", "IsRound", "Location", "Seats", "Spot", "Width", "X", "Y" },
                values: new object[] { 16, true, "Hall", 2, "vaikne nurk", 12, 84, 4 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "Height", "Location", "Seats", "Spot", "Width", "X", "Y" },
                values: new object[] { 16, "Hall", 4, "baari juures", 14, 64, 28 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "Height", "Number", "RestaurantId", "Spot", "Width", "X", "Y" },
                values: new object[] { 32, 11, 1, "diivan", 12, 84, 26 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "Height", "Number", "RestaurantId", "Seats", "Spot", "Width", "X", "Y" },
                values: new object[] { 16, 12, 1, 4, "sissepääsu juures", 14, 64, 62 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "Height", "IsRound", "Location", "Number", "RestaurantId", "Spot", "Width", "X", "Y" },
                values: new object[] { 24, true, "Upstairs", 13, 1, "akna all", 14, 8, 8 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "Height", "Location", "Number", "RestaurantId", "Seats", "Spot", "Width", "X", "Y" },
                values: new object[] { 26, "Upstairs", 14, 1, 4, "kamina juures", 18, 30, 8 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "Height", "Location", "Number", "RestaurantId", "Seats", "Spot", "Width", "X", "Y" },
                values: new object[] { 36, "Upstairs", 15, 1, 6, "privaatne ruum", 30, 60, 6 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "Height", "Location", "Number", "RestaurantId", "Seats", "Spot", "Width", "X", "Y" },
                values: new object[] { 30, "Upstairs", 16, 1, 10, "pidulik laud", 64, 10, 56 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 21,
                columns: new[] { "Height", "RestaurantId", "Seats", "Width", "X" },
                values: new object[] { 26, 2, 4, 20, 6 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 22,
                columns: new[] { "Height", "RestaurantId", "Seats", "Width", "X", "Y" },
                values: new object[] { 26, 2, 4, 20, 6, 40 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 23,
                columns: new[] { "Height", "RestaurantId", "Spot", "Width", "X", "Y" },
                values: new object[] { 20, 2, "sissepääsu juures", 18, 6, 74 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 24,
                columns: new[] { "Height", "RestaurantId", "Seats", "Spot", "Width", "X", "Y" },
                values: new object[] { 40, 2, 6, "vaade merele", 24, 70, 6 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 25,
                columns: new[] { "Height", "RestaurantId", "Seats", "Width", "X", "Y" },
                values: new object[] { 24, 2, 2, 20, 72, 60 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "Height", "IsRound", "Location", "RestaurantId", "Seats", "Spot", "Width", "X", "Y" },
                values: new object[] { 22, true, "Terrace", 2, 2, "mere ääres", 12, 4, 6 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "Height", "RestaurantId", "Spot", "Width", "X", "Y" },
                values: new object[] { 22, 2, "mere ääres", 12, 20, 6 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "Height", "RestaurantId", "Seats", "Spot", "Width", "X", "Y" },
                values: new object[] { 22, 2, 2, "mere ääres", 12, 36, 6 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "Height", "IsRound", "RestaurantId", "Seats", "Spot", "Width", "X", "Y" },
                values: new object[] { 24, true, 2, 4, "päikese käes", 14, 52, 6 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "Height", "IsRound", "RestaurantId", "Seats", "Spot", "Width", "X", "Y" },
                values: new object[] { 24, true, 2, 4, "päikese käes", 14, 70, 6 });

            migrationBuilder.InsertData(
                table: "Tables",
                columns: new[] { "Id", "Height", "IsActive", "IsRound", "Location", "Number", "RestaurantId", "Seats", "Spot", "Width", "X", "Y" },
                values: new object[,]
                {
                    { 31, 32, true, false, "Terrace", 11, 2, 6, "varikatuse all", 26, 6, 50 },
                    { 32, 32, true, false, "Terrace", 12, 2, 6, "kamina juures", 26, 38, 50 },
                    { 33, 48, true, false, "Terrace", 13, 2, 10, "pikk laud", 26, 70, 42 },
                    { 41, 26, true, false, "Hall", 1, 3, 2, "akna all", 20, 8, 10 },
                    { 42, 30, true, false, "Hall", 2, 3, 4, "akna all", 22, 8, 52 },
                    { 43, 28, true, false, "Hall", 3, 3, 4, "vaikne nurk", 22, 70, 10 },
                    { 44, 36, true, false, "Hall", 4, 3, 6, "baari juures", 26, 68, 54 },
                    { 45, 34, true, true, "Terrace", 5, 3, 2, "vaade pargile", 18, 10, 20 },
                    { 46, 34, true, true, "Terrace", 6, 3, 4, "vaade pargile", 18, 40, 20 },
                    { 47, 34, true, true, "Terrace", 7, 3, 4, "purskkaevu juures", 18, 70, 20 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_PaymentToken",
                table: "Reservations",
                column: "PaymentToken");

            migrationBuilder.CreateIndex(
                name: "IX_EventRegistrations_TableId",
                table: "EventRegistrations",
                column: "TableId");

            migrationBuilder.CreateIndex(
                name: "IX_EventMenuItems_EventId",
                table: "EventMenuItems",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportMessages_UserId",
                table: "SupportMessages",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_EventRegistrations_Tables_TableId",
                table: "EventRegistrations",
                column: "TableId",
                principalTable: "Tables",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventRegistrations_Tables_TableId",
                table: "EventRegistrations");

            migrationBuilder.DropTable(
                name: "EventMenuItems");

            migrationBuilder.DropTable(
                name: "SupportMessages");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_PaymentToken",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_EventRegistrations_TableId",
                table: "EventRegistrations");

            migrationBuilder.DeleteData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 42);

            migrationBuilder.DeleteData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 43);

            migrationBuilder.DeleteData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 44);

            migrationBuilder.DeleteData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 45);

            migrationBuilder.DeleteData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 46);

            migrationBuilder.DeleteData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 47);

            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Language",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PasswordSet",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PasswordToken",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PasswordTokenExpires",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SmsOptIn",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "StageHeight",
                table: "Restaurants");

            migrationBuilder.DropColumn(
                name: "StageWidth",
                table: "Restaurants");

            migrationBuilder.DropColumn(
                name: "StageX",
                table: "Restaurants");

            migrationBuilder.DropColumn(
                name: "StageY",
                table: "Restaurants");

            migrationBuilder.DropColumn(
                name: "StageZone",
                table: "Restaurants");

            migrationBuilder.DropColumn(
                name: "Allergies",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "AllergyNote",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "CancelledAt",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "PaymentToken",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "RefundAmount",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "Locked",
                table: "ReservationExtras");

            migrationBuilder.DropColumn(
                name: "Link",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "NoChildren",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "Zone",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "CancelledAt",
                table: "EventRegistrations");

            migrationBuilder.DropColumn(
                name: "Drinks",
                table: "EventRegistrations");

            migrationBuilder.DropColumn(
                name: "Extras",
                table: "EventRegistrations");

            migrationBuilder.DropColumn(
                name: "IsPaid",
                table: "EventRegistrations");

            migrationBuilder.DropColumn(
                name: "PaymentToken",
                table: "EventRegistrations");

            migrationBuilder.DropColumn(
                name: "RefundAmount",
                table: "EventRegistrations");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "EventRegistrations");

            migrationBuilder.DropColumn(
                name: "TableId",
                table: "EventRegistrations");

            migrationBuilder.DropColumn(
                name: "Total",
                table: "EventRegistrations");

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
                columns: new[] { "BarHeight", "BarWidth", "BarX", "BarY", "ClosesAt" },
                values: new object[] { 54, 26, 38, 46, new TimeOnly(23, 30, 0) });

            migrationBuilder.UpdateData(
                table: "Restaurants",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "BarHeight", "BarWidth", "BarY" },
                values: new object[] { 54, 26, 46 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Height", "IsRound", "Seats", "Width", "X", "Y" },
                values: new object[] { 36, false, 6, 22, 5, 6 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Height", "IsRound", "Seats", "Width", "X", "Y" },
                values: new object[] { 36, false, 6, 22, 5, 52 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Height", "Seats", "Spot", "Width", "X", "Y" },
                values: new object[] { 21, 2, "vaikne nurk", 22, 74, 3 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Height", "Seats", "Spot", "Width", "X", "Y" },
                values: new object[] { 21, 2, "baari juures", 22, 74, 27 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Height", "Spot", "Width", "X", "Y" },
                values: new object[] { 21, "baari juures", 22, 74, 51 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Height", "IsRound", "Seats", "Spot", "Width", "X", "Y" },
                values: new object[] { 21, false, 4, "sissepääsu juures", 22, 74, 75 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Height", "IsRound", "Location", "Seats", "Spot", "Width", "X", "Y" },
                values: new object[] { 32, true, "Terrace", 2, "varikatuse all", 18, 8, 8 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Height", "IsRound", "Location", "Spot", "Width", "X", "Y" },
                values: new object[] { 32, true, "Terrace", "vaade tänavale", 18, 38, 8 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Height", "IsRound", "Location", "Seats", "Spot", "Width", "X", "Y" },
                values: new object[] { 34, false, "Terrace", 6, "kamina juures", 32, 10, 54 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "Height", "Location", "Seats", "Spot", "Width", "X", "Y" },
                values: new object[] { 58, "Terrace", 8, "pikk laud", 18, 74, 30 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "Height", "Number", "RestaurantId", "Spot", "Width", "X", "Y" },
                values: new object[] { 36, 1, 2, "akna all", 22, 5, 6 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "Height", "Number", "RestaurantId", "Seats", "Spot", "Width", "X", "Y" },
                values: new object[] { 36, 2, 2, 6, "akna all", 22, 5, 52 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "Height", "IsRound", "Location", "Number", "RestaurantId", "Spot", "Width", "X", "Y" },
                values: new object[] { 21, false, "Hall", 3, 2, "vaikne nurk", 22, 74, 3 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "Height", "Location", "Number", "RestaurantId", "Seats", "Spot", "Width", "X", "Y" },
                values: new object[] { 21, "Hall", 4, 2, 2, "baari juures", 22, 74, 27 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "Height", "Location", "Number", "RestaurantId", "Seats", "Spot", "Width", "X", "Y" },
                values: new object[] { 21, "Hall", 5, 2, 4, "baari juures", 22, 74, 51 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "Height", "Location", "Number", "RestaurantId", "Seats", "Spot", "Width", "X", "Y" },
                values: new object[] { 21, "Hall", 6, 2, 4, "sissepääsu juures", 22, 74, 75 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 21,
                columns: new[] { "Height", "RestaurantId", "Seats", "Width", "X" },
                values: new object[] { 36, 3, 6, 22, 5 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 22,
                columns: new[] { "Height", "RestaurantId", "Seats", "Width", "X", "Y" },
                values: new object[] { 36, 3, 6, 22, 5, 52 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 23,
                columns: new[] { "Height", "RestaurantId", "Spot", "Width", "X", "Y" },
                values: new object[] { 21, 3, "vaikne nurk", 22, 74, 3 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 24,
                columns: new[] { "Height", "RestaurantId", "Seats", "Spot", "Width", "X", "Y" },
                values: new object[] { 21, 3, 2, "baari juures", 22, 74, 27 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 25,
                columns: new[] { "Height", "RestaurantId", "Seats", "Width", "X", "Y" },
                values: new object[] { 21, 3, 4, 22, 74, 51 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "Height", "IsRound", "Location", "RestaurantId", "Seats", "Spot", "Width", "X", "Y" },
                values: new object[] { 21, false, "Hall", 3, 4, "sissepääsu juures", 22, 74, 75 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "Height", "RestaurantId", "Spot", "Width", "X", "Y" },
                values: new object[] { 32, 3, "varikatuse all", 18, 8, 8 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "Height", "RestaurantId", "Seats", "Spot", "Width", "X", "Y" },
                values: new object[] { 32, 3, 4, "vaade tänavale", 18, 38, 8 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "Height", "IsRound", "RestaurantId", "Seats", "Spot", "Width", "X", "Y" },
                values: new object[] { 34, false, 3, 6, "kamina juures", 32, 10, 54 });

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "Height", "IsRound", "RestaurantId", "Seats", "Spot", "Width", "X", "Y" },
                values: new object[] { 58, false, 3, 8, "pikk laud", 18, 74, 30 });

            migrationBuilder.InsertData(
                table: "Tables",
                columns: new[] { "Id", "Height", "IsActive", "IsRound", "Location", "Number", "RestaurantId", "Seats", "Spot", "Width", "X", "Y" },
                values: new object[,]
                {
                    { 17, 32, true, true, "Terrace", 7, 2, 2, "varikatuse all", 18, 8, 8 },
                    { 18, 32, true, true, "Terrace", 8, 2, 4, "vaade tänavale", 18, 38, 8 },
                    { 19, 34, true, false, "Terrace", 9, 2, 6, "kamina juures", 32, 10, 54 },
                    { 20, 58, true, false, "Terrace", 10, 2, 8, "pikk laud", 18, 74, 30 }
                });
        }
    }
}
