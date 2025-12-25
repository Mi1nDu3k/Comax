using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Comax.Data.Migrations
{
    /// <inheritdoc />
    public partial class RollBackYML : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ComicId1",
                table: "Ratings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_ComicId1",
                table: "Ratings",
                column: "ComicId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Ratings_Comics_ComicId1",
                table: "Ratings",
                column: "ComicId1",
                principalTable: "Comics",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ratings_Comics_ComicId1",
                table: "Ratings");

            migrationBuilder.DropIndex(
                name: "IX_Ratings_ComicId1",
                table: "Ratings");

            migrationBuilder.DropColumn(
                name: "ComicId1",
                table: "Ratings");
        }
    }
}
