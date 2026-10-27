using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace Subapp1.ViewModels;

public class QuizFormViewModel
{
    public int QuizId { get; set; }

    // Name of the quiz
    [Required(ErrorMessage = "The quiz title is required.")]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    // Optional description of the quiz
    [StringLength(500)]
    public string? Description { get; set; }

    // IDs of questions selected for this quiz.
    public List<int> SelectedQuestionIds { get; set; } = new ();

    // Questions available for selection in the form.
    public List<SelectListItem> AvailableQuestions { get; set; } = new ();

    // Categories selected in the quiz form.
    // Used to filter which questions are displayed.
    public List<int> SelectedCategoryIds { get; set; } = new();

    // Categories available for filtering.
    public List<SelectListItem> AvailableCategories { get; set; } = new();

    // Maps each question ID to its category ID.
    // This allows JavaScript to filter questions without another server request.
    public Dictionary<int, int> QuestionCategoryIds { get; set; } = new();

}