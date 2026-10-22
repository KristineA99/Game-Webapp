using System.ComponentModel.DataAnnotations;

namespace Subapp1.DTOs;

// Data Transfer Object: defines exactly which question data the API
// sends to and receives from the React frontend.
public class QuestionDto
{
    public int QuestionId { get; set; }

    [Required(ErrorMessage = "The question text is required.")]
    [StringLength(300, MinimumLength = 5, ErrorMessage = "The question must be between 5 and 300 characters.")]
    public string Text { get; set; } = string.Empty;

    [Required(ErrorMessage = "Option A is required.")]
    [StringLength(100, ErrorMessage = "Options can be at most 100 characters.")]
    public string OptionA { get; set; } = string.Empty;

    [Required(ErrorMessage = "Option B is required.")]
    [StringLength(100, ErrorMessage = "Options can be at most 100 characters.")]
    public string OptionB { get; set; } = string.Empty;

    [Required(ErrorMessage = "Option C is required.")]
    [StringLength(100, ErrorMessage = "Options can be at most 100 characters.")]
    public string OptionC { get; set; } = string.Empty;

    [Required(ErrorMessage = "Option D is required.")]
    [StringLength(100, ErrorMessage = "Options can be at most 100 characters.")]
    public string OptionD { get; set; } = string.Empty;

    [Range(1, 4, ErrorMessage = "Choose which option is correct.")]
    public int CorrectOption { get; set; }
}