using System.ComponentModel.DataAnnotations;

namespace Subapp2.DTOs;

// The questions in the quiz are sent as a list of question ids.
public class QuizDto
{
    public int QuizId { get; set; }

    [Required(ErrorMessage = "The quiz title is required.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "The title must be between 3 and 100 characters.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "The description can be at most 500 characters.")]
    public string? Description { get; set; }

    [MinLength(1, ErrorMessage = "A quiz must contain at least one question.")]
    public List<int> QuestionIds { get; set; } = new List<int>();
}
