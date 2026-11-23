using System.ComponentModel.DataAnnotations;

namespace Subapp2.DTOs;

// Accept account details only; users cannot assign themselves a role.
public class RegisterDto
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please choose a nickname.")]
    [StringLength(20, MinimumLength = 3,
        ErrorMessage = "The nickname must be between 3 and 20 characters.")]
    [RegularExpression(@"^[a-zA-Z0-9_.-]+$",
        ErrorMessage = "Use only letters, numbers, _ . and -")]
    public string Nickname { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm your password.")]
    [Compare(nameof(Password),
        ErrorMessage = "The passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}