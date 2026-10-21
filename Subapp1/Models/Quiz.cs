using System.ComponentModel.DataAnnotations;

namespace Subapp1.Models;

public class Quiz
{
    // Primary key
    public int QuizId { get; set; }

    // Name of the quiz, e.g. "Computer Security Practice Quiz"
    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    // Optional description of the quiz
    [StringLength(500)]
    public string? Description { get; set; }

    // Questions included in this quiz (through the join entity).
    public ICollection<QuizQuestion> QuizQuestions { get; set; } = new List<QuizQuestion>();

    // One quiz can have many completed game results.
    public ICollection<GameResult> GameResults { get; set; } = new List<GameResult>();
}