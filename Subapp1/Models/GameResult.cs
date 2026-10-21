using System.ComponentModel.DataAnnotations;

namespace Subapp1.Models;

public class GameResult
{
    // Primary key
    public int GameResultId { get; set; }

    // Display name for the leaderboard (including guest players)
    [Required]
    [StringLength(30)]
    public string PlayerName { get; set; } = string.Empty;

    // Number of correctly answered questions.
    [Range(0, int.MaxValue)]
    public int Score{ get; set; }

    // Date and time the game was completed, stored in UTC.
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

    // Foreign key referencing the Quiz that was played.
    public int QuizId { get; set; }

    // Navigation property to the associated Quiz.
}