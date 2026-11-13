using Microsoft.EntityFrameworkCore;
using Subapp2.Models;

namespace Subapp2.DAL;

//GameDbContext represents the connextion between our C# models and the database.
public class GameDbContext : DbContext
{

    public GameDbContext(DbContextOptions<GameDbContext> options)
        : base(options)
    {
        
    }

    //Represents the Questions table in the database.
    public DbSet<Question> Questions { get; set; }

    // Represents the Categories table in the database.
    public DbSet<Category> Categories { get; set; }

    // Represents the Quizzes table in the database.
    public DbSet<Quiz> Quizzes { get; set; }
    
    // Represents the join table between Quizzes and Questions.
    public DbSet<QuizQuestion> QuizQuestions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // A quiz cannot contain the same question more than once, so we define a composite primary key for the join table.
        modelBuilder.Entity<QuizQuestion>()
            .HasKey(qq => new { qq.QuizId, qq.QuestionId });

        // Prevent deleting a category while it contains questions, to maintain data integrity.
        modelBuilder.Entity<Question>()
            .HasOne(q => q.Category)
            .WithMany(c => c.Questions)
            .HasForeignKey(question => question.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}