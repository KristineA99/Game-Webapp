using System.ComponentModel.DataAnnotations;

namespace Subapp1.ViewModels;

// Holds the data from the registration form
public class RegisterViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please choose a nickname.")]
    [StringLength(20, MinimumLength = 3, ErrorMessage = "The nickname must be between 3 and 20 characters.")]
    [RegularExpression(@"^[a-zA-Z0-9_.-]+$", ErrorMessage = "Use only letters, numbers, _ . and -")]
    [Display(Name = "Nickname")]
    public string Nickname { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}