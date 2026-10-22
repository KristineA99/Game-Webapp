using Microsoft.EntityFrameworkCore;
using Subapp1.Models;

namespace Subapp1.DAL;

// Handles database access for questions using asynchronous EF Core operations.
// Missing records return null or false, while database errors are logged
// and rethrown for centralized exception handling.

// Based on the repository and error handling patterns from the course demos
// Demo-ShopDatabase-6-Repository and Demo-ShopErrorHandlingLogging.

public class QuestionRepository : IQuestionRepository
{
    private readonly GameDbContext _context;
    // The logger writes messages (e.g. errors) to the console or a log file.
    private readonly ILogger<QuestionRepository> _logger;

    // Constructor: ASP.NET automatically gives us the database context and
    // the logger when the repository is created (dependency injection).

    public QuestionRepository(GameDbContext context, ILogger<QuestionRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    // READ - retrieve all questions
    // Used by the admin page that lists all questions.
    // "async" and "await" let the app keep working while it waits for the database.
    public async Task<IEnumerable<Question>> GetAllAsync()
    {
        try
        {
            return await _context.Questions
                .OrderBy(q => q.QuestionId)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            // The tag [QuestionRepository] shows where in the code the error came from
            _logger.LogError(
                ex,
                "Database error occurred while retrieving questions.");
            throw; // Rethrow the exception to be handled by the calling code.
        }
    }

    // READ - get one question by its id.
    // Used by the Details, Update and Delete pages.
    public async Task<Question?> GetByIdAsync(int id)
    {
        try
        {
            return await _context.Questions.FindAsync(id); // FindAsync looks up the row by its primary key (QuestionId).
        }
        // Returns null if the question doesn not exist.
        // Database errors are logged and rethrown.
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Database error occurred while retrieving question with ID {QuestionId}.",
                id);
            throw;
        }
    }

    // READ - get random question for the game
    // The result is a random question each time.
    public async Task<Question?> GetRandomAsync(List<int> excludeIds)
    {
        try
        {
            return await _context.Questions
                .Where(q => !excludeIds.Contains(q.QuestionId))
                .OrderBy(q => EF.Functions.Random()) // EF.Functions.Random() gives every row a random number, the questions are
                .FirstOrDefaultAsync();              //sorted by that number, and FirstOrDefaultAsync picks the first one
        }
        // Returns null if no eligible questions remain.
        // Database errors are logged and rethrown.
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Database error occurred while retrieving a random question.");
            throw;
        }
    }

    // CREATE - add a new question to the database.
    public async Task AddAsync(Question question)
    {
        try
        {
            _context.Questions.Add(question); // Add() only marks the question as "new" in memory.
            await _context.SaveChangesAsync(); // SaveChangesAsync() is what actually writes it to the database.

            _logger.LogInformation(
                "Question '{QuestionId}' was added successfully.",
                question.QuestionId);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(
                ex,
                "Database error occurred while adding a new question."); 
            throw;
        }
    }

    // UPDATE - edit an existing question.
    public async Task<bool> UpdateAsync(Question question)
    {
        try
        {
            var existingQuestion = await _context.Questions.FindAsync(question.QuestionId);
            if (existingQuestion == null)
            {
               _logger.LogWarning(
                  "Question update failed: Question with ID '{QuestionId}' not found.",
                  question.QuestionId);
               return false; // The question does not exist in the database.
            }

            // Update only the editable properties.
            existingQuestion.Text = question.Text;
            existingQuestion.OptionA = question.OptionA;
            existingQuestion.OptionB = question.OptionB;
            existingQuestion.OptionC = question.OptionC;
            existingQuestion.OptionD = question.OptionD;
            existingQuestion.CorrectOption = question.CorrectOption;
            existingQuestion.CategoryId = question.CategoryId;

            await _context.SaveChangesAsync(); 

            _logger.LogInformation(
                "Question with id '{QuestionId}' was updated successfully.",
                existingQuestion.QuestionId);
            return true; 
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Database error occurred while updating question '{QuestionId}'.",
                question.QuestionId);
            throw; 
        }
    }

    // DELETE - remove a question from the database.
    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var question = await _context.Questions.FindAsync(id);

            if (question == null)
            {
                _logger.LogWarning(
                    "Question deletion failed: Question with ID '{QuestionId}' was not found.",
                    id);
                return false; // The question does not exist in the database.
            }
            _context.Questions.Remove(question); // Remove() marks the question for deletion.
            await _context.SaveChangesAsync(); // SaveChangesAsync() actually deletes it from the database

            _logger.LogInformation(
                "Question with ID '{QuestionId}' was deleted successfully.",
                id);
            
            return true; 
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Database error occurred while deleting question '{QuestionId}'.",
                id);
            throw;
        }
    }
}
