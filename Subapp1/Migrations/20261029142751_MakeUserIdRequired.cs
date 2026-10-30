using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Subapp1.Migrations
{
    /// <inheritdoc />
    public partial class MakeUserIdRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameResults_AspNetUsers_UserId",
                table: "GameResults");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "GameResults",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_GameResults_AspNetUsers_UserId",
                table: "GameResults",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameResults_AspNetUsers_UserId",
                table: "GameResults");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "GameResults",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddForeignKey(
                name: "FK_GameResults_AspNetUsers_UserId",
                table: "GameResults",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
