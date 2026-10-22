using Microsoft.EntityFrameworkCore;
using Subapp1.Models;

namespace Subapp1.DAL;

public class QuizRepository : IQuizRepository
{
    private readonly GameDbContext _context;
    private readonly ILogger<QuizRepository> _logger;

    public QuizRepository(GameDbContext context, ILogger<QuizRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Retrieve all quizzes from the database.
    public async Task<IEnumerable<Quiz>> GetAllAsync()
    {
        return await _context.Quizzes.ToListAsync();
    }

    // Retrieve a quiz by its ID.
    public async Task<Quiz?> GetByIdAsync(int id)
    {
        return await _context.Quizzes.FindAsync(id);
    }

    // Retrieve a quiz by its ID along with its associated questions.
    public async Task<Quiz?> GetByIdWithQuestionsAsync(int id)
    {
        return await _context.Quizzes
            .Include(q => q.QuizQuestions)
            .FirstOrDefaultAsync(q => q.QuizId == id);
    }

    // Add a new quiz to the database.
    public async Task<bool> AddAsync(Quiz quiz, List<int> selectedQuestionIds)
    {
        // Verify that every selected question exists.
        var validQuestionIds = await _context.Questions
            .Where(q => selectedQuestionIds.Contains(q.QuestionId))
            .Select(q => q.QuestionId)
            .ToListAsync();
        
        if (validQuestionIds.Count != selectedQuestionIds.Distinct().Count())
        {
            _logger.LogWarning("Quiz creation rejected because one or more selected question IDs are invalid.");
            return false; // Some selected questions do not exist.
        }

        // Create the many-to-many relationships between the quiz and its questions.
        foreach (int questionId in validQuestionIds)
        {
            quiz.QuizQuestions.Add(new QuizQuestion
            {
                QuestionId = questionId
            });
        }
        try
        {
           _context.Quizzes.Add(quiz);
           await _context.SaveChangesAsync();; 

           _logger.LogInformation("Quiz '{QuizId}' created successfully", quiz.QuizId);
              return true;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error occurred while creating the quiz.");
            throw; // Rethrow the exception to be handled by the calling code.
        }     
    }

    // Update a quiz and its associated questions.
    public async Task<bool> UpdateAsync(Quiz quiz, List<int> selectedQuestionIds)
    {
        var existingQuiz = await _context.Quizzes.Include(q => q.QuizQuestions).FirstOrDefaultAsync(q => q.QuizId == quiz.QuizId);

        if (existingQuiz == null)
        {
            _logger.LogWarning("Quiz update failed: Quiz with ID '{QuizId}' not found.", quiz.QuizId);
            return false;
        }

        // Verify that every selected question exists.
        var validQuestionIds = await _context.Questions
            .Where(q => selectedQuestionIds.Contains(q.QuestionId))
            .Select(q => q.QuestionId)
            .ToListAsync();

        if (validQuestionIds.Count != selectedQuestionIds.Distinct().Count())
        {
            _logger.LogWarning("Quiz with Id '{QuizId}' update rejected because one or more selected question IDs are invalid.", quiz.QuizId);
            return false; // Some selected questions do not exist.
        }

        // Update the quiz details.
        existingQuiz.Title = quiz.Title;
        existingQuiz.Description = quiz.Description;

        // Remove relationships for questions that were deselected.
        var removedQuestions = existingQuiz.QuizQuestions
            .Where(qq => !selectedQuestionIds.Contains(qq.QuestionId))
            .ToList();
        
        foreach (var quizQuestion in removedQuestions)
        {
            _context.QuizQuestions.Remove(quizQuestion);
        }

        // Add relationships for questions that are newly selected.
        var existingQuestionIds = existingQuiz.QuizQuestions.Select(qq => qq.QuestionId).ToHashSet();

        foreach (int questionId in validQuestionIds)
        {
            if (!existingQuestionIds.Contains(questionId))
            {
                existingQuiz.QuizQuestions.Add(new QuizQuestion
                {
                    QuizId = existingQuiz.QuizId,
                    QuestionId = questionId
                });
            }
        }

        try
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Quiz '{QuizId}' updated successfully.", existingQuiz.QuizId);

            return true;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error occurred while updating the quiz '{QuizId}'.", quiz.QuizId);
            throw; // Rethrow the exception to be handled by the calling code.
        }
    }

    // Delete a quiz by its ID, only if it has no recorded game results.
    public async Task<bool> DeleteAsync(int id)
    {
        var quiz = await _context.Quizzes.FindAsync(id);

        if (quiz == null)
        {
            _logger.LogWarning("Quiz deletion failed: Quiz with ID '{QuizId}' not found.", id);
            return false;
        }
        // Preserve quizzes that have recorded game results to maintain historical data integrity.
        bool hasGameResults = await _context.GameResults.AnyAsync(result => result.QuizId == id);
        if (hasGameResults)
        {
            _logger.LogWarning("Quiz '{QuizId}' cannot be deleted because it has recordedgame results.", id);
            return false; 
        }

        try
        {
            _context.Quizzes.Remove(quiz);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Quiz '{QuizId}' deleted successfully.", id);
            return true;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error occurred while deleting the quiz '{QuizId}'.", id);
            throw; // Rethrow the exception to be handled by the calling code.
        }
    }
}