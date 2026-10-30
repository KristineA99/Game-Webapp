using Microsoft.AspNetCore.Mvc.Rendering;
using Subapp1.Models;

namespace Subapp1.ViewModels;

// Totals shown above the list of attempts. Always calculated from ALL of the player's attempts,
// so the numbers do not change when the player searches or filters the list.
public class ProgressSummary
{
    // Sum of points from every attempt.
    public int TotalPoints { get; set; }

    // Number of completed attempts.
    public int QuizzesPlayed { get; set; }

    // Number of attempts where every question was answered correctly.
    public int PerfectScores { get; set; }
}

// Everything the Progress/Index view needs.
public class PlayerProgressViewModel
{
    public ProgressSummary Summary { get; set; } = new();

    // The current search and filter values, so the form keeps what the player typed.
    public ProgressFilterViewModel Filter { get; set; } = new();

    // Attempts that match the filter, already sorted.
    public IEnumerable<GameResult> Attempts { get; set; } = new List<GameResult>();

    // Quizzes the player has played, for the quiz dropdown.
    public List<SelectListItem> AvailableQuizzes { get; set; } = new();
}
