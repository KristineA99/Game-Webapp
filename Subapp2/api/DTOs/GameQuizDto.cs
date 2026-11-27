namespace Subapp2.DTOs;

// Contains the quiz information and questions needed to play.
public class GameQuizDto
{
    public int QuizId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public List<GameQuestionDto> Questions { get; set; } = new();
}