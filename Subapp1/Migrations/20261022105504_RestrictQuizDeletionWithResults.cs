using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Subapp1.Migrations
{
    /// <inheritdoc />
    public partial class RestrictQuizDeletionWithResults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameResults_Quizzes_QuizId",
                table: "GameResults");

            migrationBuilder.AddForeignKey(
                name: "FK_GameResults_Quizzes_QuizId",
                table: "GameResults",
                column: "QuizId",
                principalTable: "Quizzes",
                principalColumn: "QuizId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameResults_Quizzes_QuizId",
                table: "GameResults");

            migrationBuilder.AddForeignKey(
                name: "FK_GameResults_Quizzes_QuizId",
                table: "GameResults",
                column: "QuizId",
                principalTable: "Quizzes",
                principalColumn: "QuizId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
