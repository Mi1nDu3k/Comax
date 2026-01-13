using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Comax.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateHistoryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Histories");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Histories");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Histories");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Histories");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Histories");

            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "Histories");

            migrationBuilder.RenameColumn(
                name: "ReadAt",
                table: "Histories",
                newName: "LastReadTime");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastReadTime",
                table: "Histories",
                newName: "ReadAt");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Histories",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Histories",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Histories",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "RowVersion",
                table: "Histories",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Histories",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: "Histories",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
