using Microsoft.EntityFrameworkCore;
using Subapp1.Models;
using Subapp1.ViewModels;

namespace Subapp1.DAL;

public class GameResultRepository : IGameResultRepository
{
    private readonly GameDbContext _context;
    private readonly ILogger<GameResultRepository> _logger;

    public GameResultRepository(GameDbContext context, ILogger<GameResultRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Calculate the totals shown at the top of the progress page.
    public async Task<ProgressSummary> GetSummaryForUserAsync(string userId)
    {
        var results = _context.GameResults.Where(r => r.UserId == userId);

        return new ProgressSummary
        {
            TotalPoints = await results.SumAsync(r => r.Score),
            QuizzesPlayed = await results.CountAsync(),
            PerfectScores = await results.CountAsync(r => r.TotalQuestions > 0 && r.Score == r.TotalQuestions)
        };
    }

    // Get the player's attempts that match every filter value.
    public async Task<IEnumerable<GameResult>> GetFilteredForUserAsync(string userId, ProgressFilterViewModel filter)
    {
        // Start with only this player's results. This is always applied first,
        // so a player can never see someone else's attempts, whatever the filter says.
        var query = _context.GameResults
            .Include(r => r.Quiz)
            .Where(r => r.UserId == userId)
            .AsNoTracking();

        // Add search, filters and sorting.
        query = GameResultFilter.Apply(query, filter);

        return await query.ToListAsync();
    }

    // Get the distinct quizzes the player has played, sorted by title.
    public async Task<IEnumerable<Quiz>> GetPlayedQuizzesForUserAsync(string userId)
    {
        return await _context.Quizzes
            .Where(q => q.GameResults.Any(r => r.UserId == userId))
            .OrderBy(q => q.Title)
            .AsNoTracking()
            .ToListAsync();
    }

    // Save a completed game. Called by the game when a player finishes a quiz.
    public async Task<bool> AddAsync(GameResult result)
    {
        try
        {
            _context.GameResults.Add(result);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Game result '{GameResultId}' saved for quiz '{QuizId}' with score {Score}/{TotalQuestions}.",
                result.GameResultId, result.QuizId, result.Score, result.TotalQuestions);
            return true;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error occurred while saving a game result for quiz '{QuizId}'.", result.QuizId);
            return false;
        }
    }
}
