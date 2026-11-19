using Microsoft.AspNetCore.Mvc.Rendering;
using Subapp1.Models;

namespace Subapp1.ViewModels;

public class GameViewModel
{
    public Question? Question { get; set; }

    public int Score { get; set; }

    public int TotalQuestions { get; set; }

    public List<SelectListItem> AvailableQuizzes { get; set; } = new();

    public int? SelectedQuizId { get; set; }
    public string QuizName { get; set; } = string.Empty;

    public bool? IsCorrect { get; set; }

    // True when the player has completed all questions, regardless of score.
    public bool HasWon { get; set; }

    // Display a warning when the completed result could not be saved.
    public bool SaveFailed { get; set; }

    // Only offer retry when the failure may be resolved by another save attempt.
    public bool CanRetrySave { get; set; }
}