using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Subapp1.Migrations
{
    /// <inheritdoc />
    public partial class RequireUserOnGameResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DurationSeconds",
                table: "GameResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DurationSeconds",
                table: "GameResults");
        }
    }
}
