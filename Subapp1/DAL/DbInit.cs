using Microsoft.EntityFrameworkCore;
using Subapp1.Models;

namespace Subapp1.DAL;
public static class DbInit
{
    // Adds initial question data if the database contains no questions.
    public static void Initialize(GameDbContext context)
    {
        // Seed sample questions only if the Questions table is empty.
        if (!context.Questions.Any())
        {
            // CorrectOption uses numbers where: 1 = A, 2 = B, 3 = C, and 4 = D.
            var questions = new List<Question>
            {
                new Question
                {
                    Text = "What is the capital of France?",
                    OptionA = "Paris",
                    OptionB = "Rome",
                    OptionC = "Madrid",
                    OptionD = "Berlin",
                    CorrectOption = 1
                },
                new Question
                {
                    Text = "What is 2 + 2?",
                    OptionA = "3",
                    OptionB = "4",
                    OptionC = "5",
                    OptionD = "6",
                    CorrectOption = 2
                },
                new Question
                {
                    Text = "What is the largest ocean on Earth?",
                    OptionA = "Atlantic Ocean",
                    OptionB = "Indian Ocean",
                    OptionC = "Pacific Ocean",
                    OptionD = "Arctic Ocean",
                    CorrectOption = 3
                }
            };

            // Add all sample questions and persist them to the database.
            context.Questions.AddRange(questions);
            context.SaveChanges();
        }
    }
}