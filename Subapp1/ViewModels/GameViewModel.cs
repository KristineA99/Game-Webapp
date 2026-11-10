using Microsoft.AspNetCore.Mvc.Rendering;
using Subapp1.Models;

namespace Subapp1.ViewModels;

public class GameViewModel
{
    public Question? Question { get; set; }

    public int Score { get; set; }

    public int TotalQuestions {get; set; }

    public List<SelectListItem> AvailableQuizzes { get; set; } = new();

    public int? SelectedQuizId { get; set; }
    public string QuizName { get; set; } = string.Empty;

    public bool? IsCorrect { get; set; }

    // True when the player has correctly answered all questions.
    public bool HasWon { get; set; }
}