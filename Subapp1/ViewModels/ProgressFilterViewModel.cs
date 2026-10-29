using System.ComponentModel.DataAnnotations;

namespace Subapp1.ViewModels;

// How the list of previous attempts is sorted.
public enum ProgressSortOrder
{
    [Display(Name = "Newest first")] Newest,
    [Display(Name = "Oldest first")] Oldest,
    [Display(Name = "Highest score")] HighestScore,
    [Display(Name = "Lowest score")] LowestScore
}

// Filterd by quiz name. Allways sorted by date, newest first. 
public class ProgressFilterViewModel
{
    // Show only attempts at one specific quiz.
    [Display(Name = "Quiz")]
    public int? QuizId { get; set; }

    [Display(Name = "Sort by")]
    public ProgressSortOrder Sort { get; set; } = ProgressSortOrder.Newest;

    // True when a quiz is selected.
    public bool HasActiveFilters => QuizId.HasValue;
}
