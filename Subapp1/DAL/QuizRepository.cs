using Microsoft.EntityFrameworkCore;
using Subapp1.Models;

namespace Subapp1.DAL;

public class QuizRepository : IQuizRepository
{
    private readonly GameDbContext _context;

    public QuizRepository(GameDbContext context)
    {
        _context = context;
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

        // Save the quiz and its question relationships together.
        _context.Quizzes.Add(quiz);
        return await _context.SaveChangesAsync() > 0;
    }

    // Update a quiz and its associated questions.
    public async Task<bool> UpdateAsync(Quiz quiz, List<int> selectedQuestionIds)
    {
        var existingQuiz = await _context.Quizzes.Include(q => q.QuizQuestions).FirstOrDefaultAsync(q => q.QuizId == quiz.QuizId);

        if (existingQuiz == null)
        {
            return false;
        }

        // Verify that every selected question exists.
        var validQuestionIds = await _context.Questions
            .Where(q => selectedQuestionIds.Contains(q.QuestionId))
            .Select(q => q.QuestionId)
            .ToListAsync();

        if (validQuestionIds.Count != selectedQuestionIds.Distinct().Count())
        {
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

        // Find the questions that are newly selected.
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

        await _context.SaveChangesAsync();
        return true;
    }

    // Delete a quiz by its ID, only if it has no recorded game results.
    public async Task<bool> DeleteAsync(int id)
    {
        var quiz = await _context.Quizzes.FindAsync(id);

        if (quiz == null)
        {
            return false;
        }
        // Preserve quizzes that have recorded game results to maintain historical data integrity.
        bool hasGameResults = await _context.GameResults.AnyAsync(result => result.QuizId == id);
        if (hasGameResults)
        {
            return false; // Cannot delete the quiz as it has associated game results.
        }

        _context.Quizzes.Remove(quiz);
        return await _context.SaveChangesAsync() > 0;
    }
}