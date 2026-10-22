using System.ComponentModel.DataAnnotations;

namespace Subapp1.Models;

public class Category
{
    // Primary key
    public int CategoryId { get; set; }

    // Name of the category, e.g. "Security" or "Networking"
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    // One category can contain many questions.
    public ICollection<Question> Questions { get; set; } = new List<Question>();
}