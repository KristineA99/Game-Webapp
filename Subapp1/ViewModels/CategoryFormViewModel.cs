using System.ComponentModel.DataAnnotations;

namespace Subapp1.ViewModels;

// Keep editable category fields separate from the database entity 
// and its related questions.
public class CategoryFormViewModel
{
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "The category name is required.")]
    [StringLength(100, ErrorMessage = "The category name cannot exceed 100 characters.")]
    public string Name { get; set; } = string.Empty;
}