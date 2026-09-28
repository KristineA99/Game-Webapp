using System.ComponentModel.DataAnnotations;

namespace Subapp1.Models;

// A quiz question with four fixed answer options.
// Validation attributes are checked server-side through ModelState in the controller.
public class Question
{
    // Primary key: a unique id for each question.
    // and lets the database generate the number automatically.
    public int QuestionId { get; set; } 

    // Question
    [Required(ErrorMessage = "The question text is required.")]
    [StringLength(300, MinimumLength = 5, ErrorMessage = "The question must be between 5 and 300 characters.")]
    [Display(Name = "Question")] // the label shown in the form
    public string Text { get; set; } = string.Empty; // gives the property a default value, so it is never null.


    // Answer A
    [Required(ErrorMessage = "Option A is required.")]
    [StringLength(100, ErrorMessage = "Options can be at most 100 characters.")]
    [Display(Name = "Option A")] // the label shown in the form
    public string OptionA { get; set; } = string.Empty; // gives the property a default value, so it is never null.

    // Answer B
    [Required(ErrorMessage = "Option B is required.")]
    [StringLength(100, ErrorMessage = "Options can be at most 100 characters.")]
    [Display(Name = "Option B")] // the label shown in the form
    public string OptionB { get; set; } = string.Empty; // gives the property a default value, so it is never null.

    // Answer C
    [Required(ErrorMessage = "Option C is required.")]
    [StringLength(100, ErrorMessage = "Options can be at most 100 characters.")]
    [Display(Name = "Option C")] // the label shown in the form
    public string OptionC { get; set; } = string.Empty; // gives the property a default value, so it is never null.

    // Answer D
    [Required(ErrorMessage = "Option D is required.")]
    [StringLength(100, ErrorMessage = "Options can be at most 100 characters.")]
    [Display(Name = "Option D")] // the label shown in the form
    public string OptionD { get; set; } = string.Empty; // gives the property a default value, so it is never null.


    // Correct answer
    // Dropdown with answers 1 = A, 2 = B, 3 = C, 4 = D
    [Range(1, 4, ErrorMessage = "Choose which option is correct.")] // Submitting without choosing gives an error message.
    [Display(Name = "Correct answer")] // the label shown in the form
    public int CorrectOption { get; set; }

    // Helper for views: the text of the correct option is returned, not just answer "1"
    public string CorrectOptionText => CorrectOption switch
    {
        1 => OptionA,
        2 => OptionB,
        3 => OptionC,
        4 => OptionD,
        _ => string.Empty
    };
}