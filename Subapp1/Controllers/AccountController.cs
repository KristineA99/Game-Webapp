using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Subapp1.ViewModels;

namespace Subapp1.Controllers;

// Handles user registration, login and logout using ASP.NET Core Identity.
// UserManager stores users and hashes passwords; SignInManager handles the login cookie.
public class AccountController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    // GET: /Account/Register
    [HttpGet]
    public IActionResult Register() => View();

    // POST: /Account/Register
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new IdentityUser { UserName = model.Nickname, Email = model.Email };
            // CreateAsync hashes the password and saves the user to the database
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("[AccountController] New user registered: {Email}", model.Email);
                // Log the user in right after registration
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            // Show Identity's errors (e.g. password too weak, email already taken)
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            _logger.LogWarning("[AccountController] Registration failed for {Email}", model.Email);
        }

        return View(model);
    }

    // GET: /Account/Login
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        var model = new LoginViewModel { ReturnUrl = returnUrl };
        return View(model);
    }

    // GET: /Account/AccessDenied
    // Shown when a logged-in user opens a page that requires the Admin role
    [HttpGet]
    public IActionResult AccessDenied()
    {
        _logger.LogWarning("[AccountController] Access denied for user {User}", User.Identity?.Name);
        return View();
    }

    // POST: /Account/Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        // Go back to the page the user came from, but only if it is inside our app
        string targetUrl = (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            ? model.ReturnUrl
            : Url.Action("Index", "Home") ?? "~/";

        if (ModelState.IsValid)
        {
            // Checks the password hash and sets the login cookie
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user != null)
            {
                // Checks the password hash and sets the login cookie
                var result = await _signInManager.PasswordSignInAsync(
                    user, model.Password, model.RememberMe, lockoutOnFailure: false);
            
                if (result.Succeeded)
                {
                    _logger.LogInformation("[AccountController] User logged in: {Email}", model.Email);
                    return Redirect(targetUrl);
                }
            }

            _logger.LogWarning("[AccountController] Failed login attempt for {Email}", model.Email);
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        }

        return View(model);
    }

    // POST: /Account/Logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("[AccountController] User logged out.");
        return RedirectToAction("Index", "Home");
    }
}