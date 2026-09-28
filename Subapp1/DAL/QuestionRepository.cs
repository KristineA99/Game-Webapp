using Microsoft.EntityFrameworkCore;
using Subapp1.Models;

namespace Subapp1.DAL;

public class QuestionRepository : IQuestionRepository
{
    private readonly GameDbContext _context;

    public QuestionRepository(GameDbContext context)
    {
        _context = context;
    }

    // READ - all questions
    public async Task<IEnumerable<Question>> GetAllAsync()
    {
        return await _context.Questions.ToListAsync();
    }

    // READ - one question
    public async Task<Question?> GetByIdAsync(int id)
    {
        return await _context.Questions.FindAsync(id);
    }

    // READ - random question for gameplay
    public async Task<Question?> GetRandomAsync()
    {
        return await _context.Questions
            .OrderBy(q => Guid.NewGuid())
            .FirstOrDefaultAsync();
    }

    // CREATE
    public async Task<bool> AddAsync(Question question)
    {
        _context.Questions.Add(question);
        return await _context.SaveChangesAsync() > 0;
    }

    // UPDATE
    public async Task<bool> UpdateAsync(Question question)
    {
        _context.Questions.Update(question);
        return await _context.SaveChangesAsync() > 0;
    }

    // DELETE
    public async Task<bool> DeleteAsync(int id)
    {
        var question = await _context.Questions.FindAsync(id);

        if (question == null)
        {
            return false;
        }

        _context.Questions.Remove(question);
        return await _context.SaveChangesAsync() > 0;
    }
}