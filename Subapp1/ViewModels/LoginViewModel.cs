using System.ComponentModel.DataAnnotations;

namespace Subapp1.ViewModels;

// Holds the data from the login form
public class LoginViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember me?")]
    public bool RememberMe { get; set; }

    // The page the user tried to open before being sent to login
    public string? ReturnUrl { get; set; }
}