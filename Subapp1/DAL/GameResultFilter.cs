using Subapp1.Models;
using Subapp1.ViewModels;

namespace Subapp1.DAL;

// Applies the search, filter and sort values from the progress page to a query of game results.
//
// Each filter adds one more .Where(...) to the query. Because the query is an IQueryable,
// nothing is sent to the database until the repository calls ToListAsync(). EF Core then
// combines all the Where clauses into ONE SQL query with "AND" between them.
//
// The method is kept separate from the repository so it can be unit tested with a plain
// in-memory list (list.AsQueryable()), without a real database.
public static class GameResultFilter
{
    public static IQueryable<GameResult> Apply(IQueryable<GameResult> query, ProgressFilterViewModel filter)
    {
        // 1. Free text search in the quiz title (case-insensitive).
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim().ToLower();
            query = query.Where(r => r.Quiz.Title.ToLower().Contains(search));
        }

        // 2. One specific quiz.
        if (filter.QuizId.HasValue)
        {
            query = query.Where(r => r.QuizId == filter.QuizId.Value);
        }

        // 3. Date range. CompletedAt is stored in UTC, and the dates are compared as UTC dates.
        if (filter.FromDate.HasValue)
        {
            var from = filter.FromDate.Value.Date;
            query = query.Where(r => r.CompletedAt >= from);
        }

        if (filter.ToDate.HasValue)
        {
            // "To 28.10" should include everything on the 28th, so compare with the start of the next day.
            var toExclusive = filter.ToDate.Value.Date.AddDays(1);
            query = query.Where(r => r.CompletedAt < toExclusive);
        }

        // 4. Minimum score.
        if (filter.MinScore.HasValue)
        {
            query = query.Where(r => r.Score >= filter.MinScore.Value);
        }

        // 5. Perfect / not perfect. Written out in full (not r.IsPerfect),
        // because EF Core can only translate stored columns to SQL.
        query = filter.Result switch
        {
            ResultFilter.Perfect => query.Where(r => r.TotalQuestions > 0 && r.Score == r.TotalQuestions),
            ResultFilter.NotPerfect => query.Where(r => r.TotalQuestions == 0 || r.Score != r.TotalQuestions),
            _ => query
        };

        // 6. Sorting. GameResultId is used as a tie-breaker so the order is always stable.
        query = filter.Sort switch
        {
            ProgressSortOrder.Oldest => query.OrderBy(r => r.CompletedAt).ThenBy(r => r.GameResultId),
            ProgressSortOrder.HighestScore => query.OrderByDescending(r => r.Score).ThenByDescending(r => r.CompletedAt),
            ProgressSortOrder.LowestScore => query.OrderBy(r => r.Score).ThenByDescending(r => r.CompletedAt),
            _ => query.OrderByDescending(r => r.CompletedAt).ThenByDescending(r => r.GameResultId)
        };

        return query;
    }
}
