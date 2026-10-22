using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Subapp1.Models;

namespace Subapp1.ViewModels;

// Represents the fields used when creating or editing a question.
// Keeps form input separate from the database entity.
public class QuestionFormViewModel
{
    public int QuestionId { get; set; }

    // The category selected for this question.
    [Range(1, int.MaxValue, ErrorMessage = "Please select a category.")]
    [Display(Name = "Category")]
    public int CategoryId { get; set; }

    // Categories available in the dropdown.
    public List<SelectListItem> AvailableCategories { get; set; } = new();

    [Required(ErrorMessage = "The question text is required.")]
    [StringLength(300, MinimumLength = 5,
        ErrorMessage = "The question must be between 5 and 300 characters.")]
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

    [Range(1, 4, ErrorMessage = "Choose which option is correct.")]
    [Display(Name = "Correct answer")]
    public int CorrectOption { get; set; }

    // Create a form model from an existing Question.
    public static QuestionFormViewModel FromQuestion(Question question)
    {
        return new QuestionFormViewModel
        {
            QuestionId = question.QuestionId,
            CategoryId = question.CategoryId,
            Text = question.Text,
            OptionA = question.OptionA,
            OptionB = question.OptionB,
            OptionC = question.OptionC,
            OptionD = question.OptionD,
            CorrectOption = question.CorrectOption
        };
    }

    // Convert the form model back to a Question entity.
    public Question ToQuestion()
    {
        return new Question
        {
            QuestionId = this.QuestionId,
            CategoryId = this.CategoryId,
            Text = this.Text,
            OptionA = this.OptionA,
            OptionB = this.OptionB,
            OptionC = this.OptionC,
            OptionD = this.OptionD,
            CorrectOption = this.CorrectOption
        };
    }
}