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
            // Create the Computer Security category if it doesn't exist.
            var category = context.Categories.FirstOrDefault(c => c.Name == "Computer Security");
            if (category == null)
            {
                category = new Category { Name = "Computer Security" };
                context.Categories.Add(category);
            }

            // Build the path to the JSON file containing the initial questions and read the file.
            var filePath = Path.Combine(
                environment.ContentRootPath,
                "Data",
                "questions.json");
            var json = File.ReadAllText(filePath);

            //Convert the JSON data into a list of Question objects.
            var questions = JsonSerializer.Deserialize<List<Question>>(json);

            // Add the questions to the database.
            if (questions != null)
            {
                // Assign all seeded questions to the "Computer Security" category.
                foreach (var question in questions)
                {
                    question.Category = category;
                }

                context.Questions.AddRange(questions);
                context.SaveChanges();
            }
        }
    }
}