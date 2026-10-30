using Subapp1.Models;
using Subapp1.ViewModels;

namespace Subapp1.DAL;

public interface IGameResultRepository
{
    // READ - totals for one player: total points, quizzes played and perfect scores.
    Task<ProgressSummary> GetSummaryForUserAsync(string userId);

    // READ - one player's attempts that match the search and filter values, sorted.
    Task<IEnumerable<GameResult>> GetFilteredForUserAsync(string userId, ProgressFilterViewModel filter);

    // READ - the quizzes a player has played, for the quiz dropdown.
    Task<IEnumerable<Quiz>> GetPlayedQuizzesForUserAsync(string userId);

    // CREATE - save a completed game.
    Task<bool> AddAsync(GameResult result);
}
