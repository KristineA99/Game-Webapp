using Subapp1.Models;
using System.Text.Json;

namespace Subapp1.DAL;
public static class DbInit
{
    // Called from Program.cs during startup.
    // Adds initial question data if the database contains no questions.
    public static void Initialize(
        GameDbContext context, 
        IWebHostEnvironment environment)
    {

        // Seed initial questions only if the Questions table is empty.
        if (!context.Questions.Any())
        {
            // Build the path to the JSON file containing the initial questions.
            var filePath = Path.Combine(
                environment.ContentRootPath,
                "Data",
                "questions.json");
                
            // Read the JSON file.
            var json = File.ReadAllText(filePath);

            //Convert the JSON data into a list of Question objects.
            var questions = JsonSerializer.Deserialize<List<Question>>(json);

            // Add the questions to the database.
            if (questions != null)
            {
                context.Questions.AddRange(questions);
                context.SaveChanges();
            }
        }
    }
}