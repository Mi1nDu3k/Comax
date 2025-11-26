using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Comax.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddViewCountToComic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: "Roles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: "Ratings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: "Comments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: "Comics",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: "Chapters",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: "Categories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: "Authors",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "Ratings");

            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "Comics");

            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "Chapters");

            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "Authors");
        }
    }
}
