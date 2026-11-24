using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Restoran.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class OhtuTolked : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TranslationsJson",
                table: "Events",
                type: "TEXT",
                maxLength: 4000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TranslationsJson",
                table: "Events");
        }
    }
}
