using System.ComponentModel.DataAnnotations;

namespace Subapp2.Models;

public class Quiz
{
    // Primary key
    public int QuizId { get; set; }

    // Name of the quiz.
    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    // Optional description of the quiz.
    [StringLength(500)]
    public string? Description { get; set; }

    // Questions included in this quiz.
    public ICollection<QuizQuestion> QuizQuestions { get; set; } = new List<QuizQuestion>();
}