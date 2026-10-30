using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Subapp1.Models;

public class GameResult
{
    // Primary key
    public int GameResultId { get; set; }

    // The player's nickname when the game was played.
    // Only logged-in players can save results.
    [Required]
    [StringLength(30)]
    public string PlayerName { get; set; } = string.Empty;

    // Number of correctly answered questions.
    [Range(0, int.MaxValue)]
    public int Score{ get; set; }

    // Number of questions the quiz had when it was played.
    // Stored as a snapshot, because an admin can add or remove questions from the quiz later.
    [Range(0, int.MaxValue)]
    public int TotalQuestions { get; set; }

    // How long the attempt took, in whole seconds. Stored as seconds (int).
    [Range(0, int.MaxValue)]
    public int DurationSeconds { get; set; }

    // Date the game was completed.
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

    // Foreign key referencing the Quiz that was played.
    public int QuizId { get; set; }

    // Navigation property to the associated Quiz.
    public Quiz Quiz { get; set; } = null!;

    // Foreign key to the logged-in user who played the game.
    // Required: guest players cannot save results, so every result belongs to a user.
    public string UserId { get; set; } = string.Empty;

    // Navigation property to the Identity user.
    public IdentityUser User { get; set; } = null!;
}