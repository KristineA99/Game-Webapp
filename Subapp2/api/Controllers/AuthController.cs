using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Subapp2.DTOs;
using Microsoft.AspNetCore.Antiforgery;

namespace Subapp2.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }
    [AllowAnonymous]
    [HttpGet("csrf")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public IActionResult GetCsrfToken([FromServices] IAntiforgery antiforgery)
    {
        // Store the antiforgery cookie and return its matching request token.
        var tokens = antiforgery.GetAndStoreTokens(HttpContext);

        return Ok(new
        {
            token = tokens.RequestToken
        });
    }

    [AllowAnonymous]
    [HttpPost("register")] 
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterDto model)
    {
        try
        {
            var user = new IdentityUser
            {
                UserName = model.Nickname,
                Email = model.Email
            };

            // Identity validates and hashes the password before saving the account.
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                _logger.LogWarning(
                    "Registration was rejected with error codes {ErrorCodes}.",
                    string.Join(", ", result.Errors.Select(e => e.Code)));

                return ValidationProblem(ModelState);
            }

            // Registration creates an ordinary player; Admin access is seeded separately.
            _logger.LogInformation(
                "User {UserId} registered successfully.", user.Id);

            return StatusCode(StatusCodes.Status201Created, new
            {
                message = "Account created. You can now log in."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "An unexpected error occurred during registration.");

            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Unable to create your account. Please try again later.");
        }
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginDto model)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user != null)
            {
                // Identity checks the password and issues the login cookie.
                // Repeated failed passwords count toward temporary account lockout.
                var result = await _signInManager.PasswordSignInAsync(
                    user,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    _logger.LogInformation(
                        "User {UserId} logged in successfully.", user.Id);

                    return NoContent();
                }
            }

            // Use the same response for unknown accounts and incorrect passwords.
            _logger.LogWarning("Login attempt failed.");

            return Unauthorized(new
            {
                message = "Invalid email or password."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during login.");

            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Unable to log in. Please try again later.");
        }
    }

    [Authorize]
    [HttpGet("me")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            // Resolve the account from the authenticated cookie, not client input.
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized();
            }

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new
            {
                nickname = user.UserName,
                roles
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "An unexpected error occurred while retrieving the current user.");

            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Unable to load your account. Please try again later.");
        }
    }

    [Authorize]
    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        try
        {
            // Remove the authentication cookie from this browser session.
            await _signInManager.SignOutAsync();

            _logger.LogInformation("User logged out successfully.");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "An unexpected error occurred during logout.");

            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Unable to log out. Please try again.");
        }
    }
}