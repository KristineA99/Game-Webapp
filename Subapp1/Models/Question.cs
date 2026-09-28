using System.ComponentModel.DataAnnotations;

namespace Subapp1.Models;

// A quiz question with four fixed answer options.
// Validation attributes are checked server-side through ModelState in the controller.
public class Question
{
    public int QuestionId { get; set; }

    [Required(ErrorMessage = "The question text is required.")]
    [StringLength(300, MinimumLength = 5, ErrorMessage = "The question must be between 5 and 300 characters.")]
    [Display(Name = "Question")]
    public string Text { get; set; } = string.Empty;

    [Required(ErrorMessage = "Option A is required.")]
    [StringLength(100, ErrorMessage = "Options can be at most 100 characters.")]
    [Display(Name = "Option A")]
    public string OptionA { get; set; } = string.Empty;

    [Required(ErrorMessage = "Option B is required.")]
    [StringLength(100, ErrorMessage = "Options can be at most 100 characters.")]
    [Display(Name = "Option B")]
    public string OptionB { get; set; } = string.Empty;

    [Required(ErrorMessage = "Option C is required.")]
    [StringLength(100, ErrorMessage = "Options can be at most 100 characters.")]
    [Display(Name = "Option C")]
    public string OptionC { get; set; } = string.Empty;

    [Required(ErrorMessage = "Option D is required.")]
    [StringLength(100, ErrorMessage = "Options can be at most 100 characters.")]
    [Display(Name = "Option D")]
    public string OptionD { get; set; } = string.Empty;

    // Which option is correct: 1 = A, 2 = B, 3 = C, 4 = D
    [Range(1, 4, ErrorMessage = "Choose which option is correct.")]
    [Display(Name = "Correct answer")]
    public int CorrectOption { get; set; }

    [Required(ErrorMessage = "Subject is required.")]
    [StringLength(50, ErrorMessage = "Subject can be at most 50 characters.")]
    public string Subject { get; set; } = string.Empty;

    // Helper for views: the text of the correct option (computed, not stored in the database)
    public string CorrectOptionText => CorrectOption switch
    {
        1 => OptionA,
        2 => OptionB,
        3 => OptionC,
        4 => OptionD,
        _ => string.Empty
    };
}