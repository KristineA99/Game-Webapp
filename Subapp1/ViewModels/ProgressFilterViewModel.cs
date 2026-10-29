using System.ComponentModel.DataAnnotations;

namespace Subapp1.ViewModels;

// Which results to show, based on whether the score was perfect.
// The Display names are the texts shown in the dropdowns.
public enum ResultFilter
{
    [Display(Name = "All results")] All,
    [Display(Name = "Perfect only")] Perfect,
    [Display(Name = "Not perfect")] NotPerfect
}

// How the list of previous attempts is sorted.
public enum ProgressSortOrder
{
    [Display(Name = "Newest first")] Newest,
    [Display(Name = "Oldest first")] Oldest,
    [Display(Name = "Highest score")] HighestScore,
    [Display(Name = "Lowest score")] LowestScore
}

// The search and filter values from the form on the progress page.
// Every value is optional: an empty value means "do not filter on this".
// The values are sent as query string parameters (GET), so a filtered list can be bookmarked or reloaded.
public class ProgressFilterViewModel : IValidatableObject
{
    // Free text search in the quiz title.
    [StringLength(100, ErrorMessage = "Search can be at most 100 characters.")]
    [Display(Name = "Search quiz title")]
    public string? Search { get; set; }

    // Show only attempts at one specific quiz.
    [Display(Name = "Quiz")]
    public int? QuizId { get; set; }

    // Show only attempts completed on or after this date.
    [DataType(DataType.Date)]
    [Display(Name = "From")]
    public DateTime? FromDate { get; set; }

    // Show only attempts completed on or before this date (the whole day is included).
    [DataType(DataType.Date)]
    [Display(Name = "To")]
    public DateTime? ToDate { get; set; }

    // Show only attempts with at least this many points.
    [Range(0, 1000, ErrorMessage = "Minimum score must be between 0 and 1000.")]
    [Display(Name = "Min. score")]
    public int? MinScore { get; set; }

    // All, only perfect, or only non-perfect attempts.
    [Display(Name = "Result")]
    public ResultFilter Result { get; set; } = ResultFilter.All;

    [Display(Name = "Sort by")]
    public ProgressSortOrder Sort { get; set; } = ProgressSortOrder.Newest;

    // True when at least one filter is in use. Used by the view to show a "Clear filters" link.
    public bool HasActiveFilters =>
        !string.IsNullOrWhiteSpace(Search)
        || QuizId.HasValue
        || FromDate.HasValue
        || ToDate.HasValue
        || MinScore.HasValue
        || Result != ResultFilter.All;

    // Rules that involve more than one field, checked by ModelState together with the attributes above.
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (FromDate.HasValue && ToDate.HasValue && FromDate.Value.Date > ToDate.Value.Date)
        {
            yield return new ValidationResult(
                "The 'From' date must be before or equal to the 'To' date.",
                new[] { nameof(FromDate), nameof(ToDate) });
        }
    }
}
