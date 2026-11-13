using System.ComponentModel.DataAnnotations;

namespace Subapp2.DTOs;

// Data Transfer Object: the category data the API sends to and receives from the React frontend.
public class CategoryDto
{
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "The category name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "The name must be between 2 and 100 characters.")]
    public string Name { get; set; } = string.Empty;
}
