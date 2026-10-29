using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

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

    // Number of questions the quiz had when it was played.
    // Stored as a snapshot, because an admin can add or remove questions from the quiz later.
    // A result is a perfect score when Score == TotalQuestions.
    [Range(0, int.MaxValue)]
    public int TotalQuestions { get; set; }

    // Date and time the game was completed, stored in UTC.
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

    // Foreign key referencing the Quiz that was played.
    public int QuizId { get; set; }

    // Navigation property to the associated Quiz.
    public Quiz Quiz { get; set; } = null!;

    // Foreign key to the logged-in user who played the game.
    // Nullable, so guest players can still have results on the leaderboard.
    public string? UserId { get; set; }

    // Navigation property to the Identity user.
    public IdentityUser? User { get; set; }

    // Helper for views: true when every question in the quiz was answered correctly.
    // Not stored in the database. Database queries use the same rule directly (see GameResultFilter).
    public bool IsPerfect => TotalQuestions > 0 && Score == TotalQuestions;
}