using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Comax.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateChapterNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<float>(
                name: "ChapterNumber",
                table: "Chapters",
                type: "float",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ChapterNumber",
                table: "Chapters",
                type: "int",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");
        }
    }
}
