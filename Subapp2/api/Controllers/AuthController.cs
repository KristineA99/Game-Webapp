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
}