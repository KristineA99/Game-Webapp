using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Subapp1.Models;

namespace Subapp1.DAL;

// GameDbContext represents the connection between our C# models and the database.
// It inherits from IdentityDbContext, which adds the ASP.NET Core Identity tables
// (users, roles, logins) in addition to our own domain tables.
public class GameDbContext : IdentityDbContext
{
    //The constructor receives the database configuration from ASP.NET
    //through dependency injection (for example that we are useing SQLite).
    //base(options) passes this configuration to EF Core's DbContext.
    public GameDbContext(DbContextOptions<GameDbContext> options)
        : base(options)
    {
        
    }

    //Represents the Questions table in the database.
    //Question is our model (Models/Question.cs), while Questions is the property
    //we use to access Question records through GameDbContext.
    public DbSet<Question> Questions { get; set; }
    // Represents the Categories table in the database.
    public DbSet<Category> Categories { get; set; }

    // Represents the Quizzes table in the database.
    public DbSet<Quiz> Quizzes { get; set; }
    // Represents the join table between Quizzes and Questions.
    public DbSet<QuizQuestion> QuizQuestions { get; set; }

    // Represents completed game results stored in the database.
    public DbSet<GameResult> GameResults { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // A quiz cannot contain the same question more than once, so we define a composite primary key for the join table.
        modelBuilder.Entity<QuizQuestion>() 
            .HasKey(qq => new { qq.QuizId, qq.QuestionId });
        
        // Prevent deletion of quizzes that have associated game results.
        modelBuilder.Entity<GameResult>()
            .HasOne(result => result.Quiz)
            .WithMany(quiz => quiz.GameResults)
            .HasForeignKey(result => result.QuizId)
            .OnDelete(DeleteBehavior.Restrict); 

        // Prevent deleting a category while it contains questions, to maintain data integrity.
        modelBuilder.Entity<Question>()
            .HasOne(q => q.Category)
            .WithMany(c => c.Questions)
            .HasForeignKey(question => question.CategoryId)
            .OnDelete(DeleteBehavior.Restrict); 
    }
}