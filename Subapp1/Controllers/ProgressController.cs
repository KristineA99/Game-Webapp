using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Subapp1.DAL;
using Subapp1.ViewModels;

namespace Subapp1.Controllers;

// Shows the logged-in player's progress: totals and a searchable list of previous quiz attempts.
[Authorize] // Every logged-in user can see their own progress. Guests are sent to the login page.
public class ProgressController : Controller
{
    private readonly IGameResultRepository _gameResultRepository;
    private readonly ILogger<ProgressController> _logger;

    public ProgressController(IGameResultRepository gameResultRepository, ILogger<ProgressController> logger)
    {
        _gameResultRepository = gameResultRepository;
        _logger = logger;
    }

    // GET: /Progress?Search=crypto&Result=Perfect&Sort=HighestScore ...
    // The filter values come from the query string, so the form uses method="get".
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] ProgressFilterViewModel filter)
    {
        // The id of the logged-in user, read from the login cookie.
        // Never taken from the query string, so players cannot look at each other's results.
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Challenge();
        }

        // If the filter is invalid (for example From is after To), show the error
        // and the full list instead, while keeping the player's input in the form.
        var effectiveFilter = filter;
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("[ProgressController] Invalid progress filter for user {UserId}.", userId);
            effectiveFilter = new ProgressFilterViewModel { Sort = filter.Sort };
        }

        var playedQuizzes = await _gameResultRepository.GetPlayedQuizzesForUserAsync(userId);

        var viewModel = new PlayerProgressViewModel
        {
            Summary = await _gameResultRepository.GetSummaryForUserAsync(userId),
            Attempts = await _gameResultRepository.GetFilteredForUserAsync(userId, effectiveFilter),
            Filter = filter,
            AvailableQuizzes = playedQuizzes.Select(q => new SelectListItem
            {
                Value = q.QuizId.ToString(),
                Text = q.Title,
                Selected = q.QuizId == filter.QuizId
            }).ToList()
        };

        return View(viewModel);
    }
}
