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

// Multi-parameter search. Allways sorted by date, newest first. 
public class ProgressFilterViewModel : IValidatableObject
{
    // Show only attempts at one specific quiz.
    [Display(Name = "Quiz")]
    public int? QuizId { get; set; }

    // Show only attempts completed on or after this date.
    [Display(Name = "From")]
    [DataType(DataType.Date)]
    public DateTime? FromDate { get; set; }

    // Show only attempts completed on or before this date.
    [Display(Name = "To")]
    [DataType(DataType.Date)]
    public DateTime? ToDate { get; set; }

    [Display(Name = "Sort by")]
    public ProgressSortOrder Sort { get; set; } = ProgressSortOrder.Newest;

    // True when at least one filter is applied.
    public bool HasActiveFilters => QuizId.HasValue || FromDate.HasValue || ToDate.HasValue;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (FromDate.HasValue && ToDate.HasValue && FromDate.Value.Date > ToDate.Value.Date)
        {
            yield return new ValidationResult(
                "The 'From' date must be before the 'To' date.", 
                new[] { nameof(FromDate), nameof(ToDate) });
        }
    }
}


