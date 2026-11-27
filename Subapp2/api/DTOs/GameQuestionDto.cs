namespace Subapp2.DTOs;

// Gameplay responses omit the correct answer; scoring happens on the server.
public class GameQuestionDto
{
    public int QuestionId { get; set; }

    public string Text { get; set; } = string.Empty;

    public string OptionA { get; set; } = string.Empty;

    public string OptionB { get; set; } = string.Empty;

    public string OptionC { get; set; } = string.Empty;

    public string OptionD { get; set; } = string.Empty;
}