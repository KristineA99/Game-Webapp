namespace Subapp2.DTOs;

// Returned after the server validates the submission and calculates the score.
public class GameResultDto
{
    public int QuizId { get; set; }

    public string QuizTitle { get; set; } = string.Empty;

    public int Score { get; set; }

    public int TotalQuestions { get; set; }
}