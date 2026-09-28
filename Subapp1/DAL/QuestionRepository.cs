using Microsoft.EntityFrameworkCore;
using Subapp1.Models;

namespace Subapp1.DAL;

// Handles all database access for questions.
// Every method catches exceptions and logs them, so a database error
// never crashes the app. The controller decides what the user sees.
public class QuestionRepository : IQuestionRepository
{
    private readonly GameDbContext _context;
    private readonly ILogger<QuestionRepository> _logger;

    public QuestionRepository(GameDbContext context)
    {
        _context = context;
        _logger = logger;
    }

    // READ - all questions, sorted by subject
    public async Task<IEnumerable<Question>> GetAllAsync()
    {
        try
        {
            return await _context.Questions
                .OrderBy(q => q.Subject)
                .ThenBy(q => q.Text)
                .ToListAsync();
        }
        catch (Exception e)
        {
            _logger.LogError("[QuestionRepository] GetAllAsync() failed, error message: {e}", e.Message);
            return null;
        }
    }

    // READ - one question
    public async Task<Question?> GetByIdAsync(int id)
    {
        try
        {
            return await _context.Questions.FindAsync(id);
        }
        catch (Exception e)
        {
            _logger.LogError("[QuestionRepository] GetByIdAsync() failed for QuestionId {id}, error message: {e}", id, e.Message);
            return null;
        }
    }

    // READ - random question for gameplay
    public async Task<Question?> GetRandomAsync()
    {
        try
        {
            return await _context.Questions
                .OrderBy(q => EF.Functions.Random())
                .FirstOrDefaultAsync();
        }
        catch (Exception e)
        {
            _logger.LogError("[QuestionRepository] GetRandomAsync() failed, error message: {e}", e.Message);
            return null;
        }
    }

    // CREATE
    public async Task<bool> AddAsync(Question question)
    {
        try
        {
            _context.Questions.Add(question);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError("[QuestionRepository] AddAsync() failed for question {@question}, error message: {e}", question, e.Message);
            return false;
        }
    }

    // UPDATE
    public async Task<bool> UpdateAsync(Question question)
    {
        try
        {
            _context.Questions.Update(question);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError("[QuestionRepository] UpdateAsync() failed for question {@question}, error message: {e}", question, e.Message);
            return false;
        }
    }

    // DELETE
    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var question = await _context.Questions.FindAsync(id);
            if (question == null)
            {
                _logger.LogError("[QuestionRepository] Question not found for QuestionId {id}", id);

                return false;
            }

            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();
            return false;
        }
        catch (Exception e)
        {
            _logger.LogError("[QuestionRepository] DeleteAsync() failed for QuestionId {id}, error message: {e}", id, e.Message);
            return false;
        }
    }
}