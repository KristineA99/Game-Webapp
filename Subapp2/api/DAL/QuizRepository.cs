using Microsoft.EntityFrameworkCore;
using Subapp2.Models;

namespace Subapp2.DAL;

// Handles all database access for quizzes.
public class QuizRepository : IQuizRepository
{
    private readonly GameDbContext _context;
    private readonly ILogger<QuizRepository> _logger;

    public QuizRepository(GameDbContext context, ILogger<QuizRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Retrieve all quizzes from the database, with their question links.
    public async Task<IEnumerable<Quiz>?> GetAllAsync()
    {
        try
        {
            return await _context.Quizzes
                .Include(q => q.QuizQuestions)
                .OrderBy(q => q.Title)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[QuizRepository] GetAllAsync() failed.");
            return null;
        }
    }

    // Retrieve a quiz by its ID along with its associated questions.
    public async Task<Quiz?> GetByIdWithQuestionsAsync(int id)
    {
        try
        {
            return await _context.Quizzes
                .Include(q => q.QuizQuestions)
                .FirstOrDefaultAsync(q => q.QuizId == id);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[QuizRepository] GetByIdWithQuestionsAsync() failed for QuizId {id}.", id);
            return null;
        }
    }

    // Add a new quiz to the database.
    public async Task<bool> AddAsync(Quiz quiz, List<int> selectedQuestionIds)
    {
        try
        {
            // Rule: every selected question must exist.
            var validQuestionIds = await _context.Questions
                .Where(q => selectedQuestionIds.Contains(q.QuestionId))
                .Select(q => q.QuestionId)
                .ToListAsync();

            if (validQuestionIds.Count != selectedQuestionIds.Distinct().Count())
            {
                _logger.LogWarning("[QuizRepository] Quiz creation rejected because one or more selected question IDs are invalid.");
                return false;
            }

            // Create the many-to-many relationships between the quiz and its questions.
            foreach (int questionId in validQuestionIds)
            {
                quiz.QuizQuestions.Add(new QuizQuestion
                {
                    QuestionId = questionId
                });
            }

            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();

            _logger.LogInformation("[QuizRepository] Quiz {id} was added successfully.", quiz.QuizId);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[QuizRepository] AddAsync() failed for quiz {title}.", quiz.Title);
            return false;
        }
    }

    // Update a quiz and its associated questions.
    public async Task<bool> UpdateAsync(Quiz quiz, List<int> selectedQuestionIds)
    {
        try
        {
            // Rule: every selected question must exist.
            var validQuestionIds = await _context.Questions
                .Where(q => selectedQuestionIds.Contains(q.QuestionId))
                .Select(q => q.QuestionId)
                .ToListAsync();

            if (validQuestionIds.Count != selectedQuestionIds.Distinct().Count())
            {
                _logger.LogWarning("[QuizRepository] Quiz {id} update rejected because one or more selected question IDs are invalid.", quiz.QuizId);
                return false;
            }

            // Remove relationships for questions that were deselected.
            var removedQuestions = quiz.QuizQuestions
                .Where(qq => !selectedQuestionIds.Contains(qq.QuestionId))
                .ToList();

            foreach (var quizQuestion in removedQuestions)
            {
                quiz.QuizQuestions.Remove(quizQuestion);
            }

            // Add relationships for questions that are newly selected.
            var existingQuestionIds = quiz.QuizQuestions.Select(qq => qq.QuestionId).ToHashSet();

            foreach (int questionId in validQuestionIds)
            {
                if (!existingQuestionIds.Contains(questionId))
                {
                    quiz.QuizQuestions.Add(new QuizQuestion
                    {
                        QuizId = quiz.QuizId,
                        QuestionId = questionId
                    });
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("[QuizRepository] Quiz {id} was updated successfully.", quiz.QuizId);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[QuizRepository] UpdateAsync() failed for QuizId {id}.", quiz.QuizId);
            return false;
        }
    }

    // Delete a quiz by its ID. Its rows in the join table are removed too,
    // but the questions themselves are kept.
    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var quiz = await _context.Quizzes.FindAsync(id);
            if (quiz == null)
            {
                _logger.LogWarning("[QuizRepository] Quiz not found for QuizId {id}.", id);
                return false;
            }

            _context.Quizzes.Remove(quiz);
            await _context.SaveChangesAsync();

            _logger.LogInformation("[QuizRepository] Quiz {id} was deleted successfully.", id);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[QuizRepository] DeleteAsync() failed for QuizId {id}.", id);
            return false;
        }
    }
}
