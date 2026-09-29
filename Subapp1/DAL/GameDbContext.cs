using Microsoft.EntityFrameworkCore;
using Subapp1.Models;

namespace Subapp1.DAL;

//GameDbContext represents the connextion between our C# models and the database.
//It inherits from EF Core's DbContext, which provides the database functionality.
public class GameDbContext : DbContext
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
}