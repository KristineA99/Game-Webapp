namespace Subapp2.DTOs;

// Represents each quiz on the player's quiz selection page.
// Shows enough information to choose a quiz without loading its questions.
public class GameQuizSummaryDto
{
    public int QuizId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int QuestionCount { get; set; }
}