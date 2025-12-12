using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Comax.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSubStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SubStatus",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubStatus",
                table: "Users");
        }
    }
}
