namespace Subapp1.Models;

public class QuizQuestion
{
    // Foreign key referencing the Quiz
    public int QuizId { get; set; }

    // Navigation property to the Quiz
    public Quiz Quiz { get; set; } = null!;

    // Foreign key referencing the question 
    public int QuestionId { get; set; }
    
    // Navigation property to the Question
    public Question Question { get; set; } = null!;
}