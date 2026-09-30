using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Subapp1.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        /// This determines how to move the database forward to this version of the db, is created automatically by the EF core tool and  
        /// translates each property of the question model into a database column, including validation annotations
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Questions",
                columns: table => new
                {
                    QuestionId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true), //ID's will automatically be created by SQLite
                    Text = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    OptionA = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    OptionB = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    OptionC = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    OptionD = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CorrectOption = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", x => x.QuestionId); //QuestionID is the primary key
                });
        }

        /// <inheritdoc />
        /// Determines the process for undoing this version of the db. Created automatically by the EF core tool
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Questions");
        }
    }
}
