using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Subapp1.Migrations
{
    /// <inheritdoc />
    public partial class AddQuizNameSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameResults_Quizzes_QuizId",
                table: "GameResults");

            migrationBuilder.AlterColumn<int>(
                name: "QuizId",
                table: "GameResults",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<string>(
                name: "QuizName",
                table: "GameResults",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(
                """
                UPDATE GameResults
                SET QuizName = (
                    SELECT Title
                    FROM Quizzes
                    WHERE Quizzes.QuizId = GameResults.QuizId
                )
                WHERE QuizId IS NOT NULL
                    AND EXISTS (
                        SELECT 1
                        FROM Quizzes
                        WHERE Quizzes.QuizId = GameResults.QuizId
                    );
                """);

            migrationBuilder.AddForeignKey(
                name: "FK_GameResults_Quizzes_QuizId",
                table: "GameResults",
                column: "QuizId",
                principalTable: "Quizzes",
                principalColumn: "QuizId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameResults_Quizzes_QuizId",
                table: "GameResults");

            migrationBuilder.DropColumn(
                name: "QuizName",
                table: "GameResults");

            migrationBuilder.AlterColumn<int>(
                name: "QuizId",
                table: "GameResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_GameResults_Quizzes_QuizId",
                table: "GameResults",
                column: "QuizId",
                principalTable: "Quizzes",
                principalColumn: "QuizId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
