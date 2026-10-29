using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Subapp1.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerProgressToGameResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TotalQuestions",
                table: "GameResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "GameResults",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GameResults_UserId",
                table: "GameResults",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_GameResults_AspNetUsers_UserId",
                table: "GameResults",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameResults_AspNetUsers_UserId",
                table: "GameResults");

            migrationBuilder.DropIndex(
                name: "IX_GameResults_UserId",
                table: "GameResults");

            migrationBuilder.DropColumn(
                name: "TotalQuestions",
                table: "GameResults");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "GameResults");
        }
    }
}
