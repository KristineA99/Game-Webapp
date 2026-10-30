using Subapp1.Models;
using Subapp1.ViewModels;

namespace Subapp1.DAL;

// Applies the search, filter and sort values from the progress page to a query of game results. 
public static class GameResultFilter
{
    public static IQueryable<GameResult> Apply(IQueryable<GameResult> query, ProgressFilterViewModel filter)
    {
        // One attempts at on specific quiz.
        if (filter.QuizId.HasValue)
        {
            query = query.Where(r => r.QuizId == filter.QuizId.Value);
        }

        // Sorting by date and score.
        query = filter.Sort switch
        {
            ProgressSortOrder.Oldest => query.OrderBy(r => r.CompletedAt).ThenBy(r => r.GameResultId),
            ProgressSortOrder.HighestScore => query.OrderByDescending(r => r.Score).ThenBy(r => r.DurationSeconds).ThenBy(r => r.GameResultId),
            ProgressSortOrder.LowestScore => query.OrderBy(r => r.Score).ThenBy(r => r.DurationSeconds).ThenBy(r => r.CompletedAt),
            _ => query.OrderByDescending(r => r.CompletedAt).ThenByDescending(r => r.GameResultId)
        };

        return query;
    }
}
