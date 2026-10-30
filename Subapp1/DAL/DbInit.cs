using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

        // Seed questions and categories when the Questions table is empty.
        if (!context.Questions.Any())
        {
            // Read the JSON file containing the sample questions.
            var filePath = Path.Combine(
                environment.ContentRootPath,
                "Data",
                "questions.json");
            var json = File.ReadAllText(filePath);


            var seedQuestions =
                JsonSerializer.Deserialize<List<SeedQuestion>>(json)
                ?? new List<SeedQuestion>();

            // Create the 12 computer security course categories, even if a category
            // currently has no questions in the JSON file.
            var categoryNames = new[]
            {
                "Module 2 - Introduction to Principles of Computer Security",
                "Module 3 - Introduction to Cryptology",
                "Module 4 - Cryptographic Algorithms",
                "Module 5 - Security protocols",
                "Module 6 - Authentication (Digital Security)",
                "Module 7 - Authorization",
                "Module 8 - Identity, Authentication and Authorization protocols and standards",
                "Module 9 - Network Security Protocols",
                "Module 10 - Network Security 1 (Firewalls)",
                "Module 11 - Network Security 2 (Intrusion Detection/Prevention Systems)",
                "Module 12 - Software Security 1",
                "Module 13 - Software Security 2"
            };

             // Reuse any existing categories instead of creating duplicates.
            var categories = context.Categories.ToList();

            foreach (var name in categoryNames)
            {
                if (!categories.Any(c => c.Name == name))
                {
                    var category = new Category { Name = name };

                    context.Categories.Add(category);
                    categories.Add(category);
                }
            }

            // Convert each SeedQuestion into a database Question entity.
            foreach (var seed in seedQuestions)
            {
                var category = categories.FirstOrDefault(
                    c => c.Name == seed.Category);

                if (category == null)
                {
                    throw new InvalidOperationException(
                        $"Unknown seed category: {seed.Category}");
                }

                var question = new Question
                {
                    Category = category,
                    Text = seed.Text,
                    OptionA = seed.OptionA,
                    OptionB = seed.OptionB,
                    OptionC = seed.OptionC,
                    OptionD = seed.OptionD,
                    CorrectOption = seed.CorrectOption
                };

                context.Questions.Add(question);
            }

            context.SaveChanges();
        }
          // Seed example quizzes after the questions have been saved.
        SeedQuizzes(context);
    }


    private static void SeedQuizzes(GameDbContext context)
    {
        // Get questions with their categories.
        var questions = context.Questions
            .OrderBy(q => q.QuestionId)
            .ToList();

        var categories = context.Categories.ToDictionary(
            c => c.CategoryId,
            c => c.Name);       
        
        // Quiz 1: Ten questions from Modules 2 and 3.
        var fundamentals = questions
            .Where(q =>
                categories[q.CategoryId].StartsWith("Module 2 -") ||
                categories[q.CategoryId].StartsWith("Module 3 -"))
            .Take(10)
            .ToList();


        AddQuizIfMissing(
            context,
            "Security Fundamentals Practice",
            "Practice security principles and introductory cryptology.",
            fundamentals);

        // Quiz 2: Ten questions from Modules 3 and 4.
        var cryptography = questions
            .Where(q =>
                categories[q.CategoryId].StartsWith("Module 3 -") ||
                categories[q.CategoryId].StartsWith("Module 4 -"))
            .Take(10)
            .ToList();

        AddQuizIfMissing(
            context,
            "Cryptography Practice",
            "Practice cryptology concepts and cryptographic algorithms.",
            cryptography);

        // Quiz 3: Three questions from Module 7.
        // This quiz intentionally has fewer than five questions.
        var authorization = questions
            .Where(q =>
                categories[q.CategoryId].StartsWith("Module 7 -"))
            .Take(3)
            .ToList();

        AddQuizIfMissing(
            context,
            "Authorization Mini Quiz",
            "A short quiz for testing the minimum-question requirement.",
            authorization);

        context.SaveChanges();
    }

    // Seeds example quizzes for the development database.
    // Only creates a quiz if no quiz with the same title exists, preventing
    // duplicates when the application restarts.
    // If a seeded quiz is deleted, it will be recreated on the next startup.
    // This is intended for development and testing, not for restoring
    // deleted quizzes in a production environment.
    private static void AddQuizIfMissing(
        GameDbContext context,
        string title,
        string description,
        List<Question> questions)
    {
        // Avoid adding the same example quiz on every startup.
        if (context.Quizzes.Any(q => q.Title == title))
        {
        return;
        }

        var quiz = new Quiz
        {
            Title = title,
            Description = description,
        };

        // Associate the selected questions with this quiz.
        foreach (var question in questions)
        {
            quiz.QuizQuestions.Add(new QuizQuestion
            {
                QuestionId = question.QuestionId
            });
        }
            
        context.Quizzes.Add(quiz);
        context.SaveChanges();
    }

    // Development only: adds example quiz attempts for one user, so the progress page
    // and its search and filters have data to show. Runs only if the user has no results yet.
    // Note: quizzes with results cannot be deleted, so the seeded quizzes become undeletable in development.
    public static void SeedSampleGameResults(GameDbContext context, IdentityUser user)
    {
        if (context.GameResults.Any(r => r.UserId == user.Id))
        {
            return;
        }

        var quizzes = context.Quizzes
            .Include(q => q.QuizQuestions)
            .Where(q => q.QuizQuestions.Any())
            .OrderBy(q => q.QuizId)
            .ToList();

        if (quizzes.Count == 0)
        {
            return;
        }

        // (which quiz, how many correct answers, how many days ago)
        // A score of -1 means "all questions correct", so the example works whatever the quiz size is.
        var samples = new (int QuizIndex, int Score, int DaysAgo)[]
        {
            (0, -1, 1),
            (1, 6, 2),
            (2, -1, 3),
            (0, 7, 6),
            (1, -1, 9),
            (2, 1, 14),
            (0, 4, 20),
            (1, 9, 30)
        };

        foreach (var (quizIndex, score, daysAgo) in samples)
        {
            var quiz = quizzes[quizIndex % quizzes.Count];
            int total = quiz.QuizQuestions.Count;

            context.GameResults.Add(new GameResult
            {
                QuizId = quiz.QuizId,
                UserId = user.Id,
                PlayerName = user.UserName ?? "Player",
                TotalQuestions = total,
                Score = score < 0 ? total : Math.Min(score, total),
                DurationSeconds = 60 + (daysAgo * 47 + quizIndex * 13) % 300,
                CompletedAt = DateTime.UtcNow.AddDays(-daysAgo).AddHours(-quizIndex)
            });
        }

        context.SaveChanges();
    }
}
