using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AlbumTopTypeColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ViewsByTopDay",
                table: "Album",
                type: "decimal(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ViewsByTopMonth",
                table: "Album",
                type: "decimal(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ViewsByTopWeek",
                table: "Album",
                type: "decimal(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ViewsByTopYear",
                table: "Album",
                type: "decimal(20,0)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ViewsByTopDay",
                table: "Album");

            migrationBuilder.DropColumn(
                name: "ViewsByTopMonth",
                table: "Album");

            migrationBuilder.DropColumn(
                name: "ViewsByTopWeek",
                table: "Album");

            migrationBuilder.DropColumn(
                name: "ViewsByTopYear",
                table: "Album");
        }
    }
}
