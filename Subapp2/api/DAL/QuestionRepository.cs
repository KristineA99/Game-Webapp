using Microsoft.EntityFrameworkCore;
using Subapp2.Models;

namespace Subapp2.DAL;

// Handles all database access for questions.
public class QuestionRepository : IQuestionRepository
{
    private readonly GameDbContext _context;
    
    // The logger writes messages to the console or a log file.
    private readonly ILogger<QuestionRepository> _logger;

    // Automatically gives us the database context and the logger when 
    // the repository is created.
    public QuestionRepository(GameDbContext context, ILogger<QuestionRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    // READ: all questions
    // Used by the admin page that lists all questions.
    public async Task<IEnumerable<Question>?> GetAllAsync()
    {
        try
        {
            return await _context.Questions
                .OrderBy(q => q.QuestionId)
                .ToListAsync();
        }
        catch (Exception e)
        {
            // The tag [QuestionRepository] shows where in the code the error came from
            _logger.LogError(
                e,
                "[QuestionRepository] GetAllAsync() failed.");
            return null;
        }
    }

    // READ: get one question by its id.
    // Used by the Details, Update and Delete pages.
    public async Task<Question?> GetByIdAsync(int id)
    {
        try
        {
            return await _context.Questions.FindAsync(id); // FindAsync looks up the row by its primary key (QuestionId).
        }
        // Returns null if no question has that id, or if the database call fails.
        catch (Exception e)
        {
            _logger.LogError(
                e,
                "[QuestionRepository] GetByIdAsync() failed for QuestionId {id}.",
                id);
            return null;
        }
    }

    // READ: get random question for the game
    // The result is a random question each time.
    public async Task<Question?> GetRandomAsync(List<int> excludeIds)
    {
        try
        {
            return await _context.Questions
                .Where(q => !excludeIds.Contains(q.QuestionId))
                // EF.Functions.Random() gives every row a random number, the questions are
                // sorted by that number, and FirstOrDefaultAsync picks the first one
                .OrderBy(q => EF.Functions.Random()) 
                .FirstOrDefaultAsync();              
        }
        // Returns null if the table is empty or the database call fails.
        catch (Exception e)
        {
            _logger.LogError(
                e,
                "[QuestionRepository] GetRandomAsync() failed,");
            return null;
        }
    }

    // CREATE: add a new question to the database.
    public async Task<bool> AddAsync(Question question)
    {
        try
        {
            _context.Questions.Add(question); // Add() only marks the question as "new" in memory.
            await _context.SaveChangesAsync(); // SaveChangesAsync() actually writes it to the database.

            _logger.LogInformation(
                "[QuestionRepository] Question {id} was added successfully.",
                question.QuestionId);
            return true; // Returns true if it was saved, false if something went wrong.
        }
        catch (Exception e)
        {
            // {@question} logs the whole question object, so we can see what was being saved when the error occurred.
            _logger.LogError(
                e,
                "[QuestionRepository] AddAsync() failed for question {@question}",
                question); 
            return false;
        }
    }

    // UPDATE: edit an existing question.
    public async Task<bool> UpdateAsync(Question question)
    {
        try
        {
            _context.Questions.Update(question);// Update() marks the question as changed.
            await _context.SaveChangesAsync(); // SaveChangesAsync() writes the changes to the database.

            _logger.LogInformation(
                "[QuestionRepository] Question {id} was updated successfully.",
                question.QuestionId);
            return true; // Returns true even if nothing was changed.
        }
        // Returns false only if an error occurs.
        catch (Exception e)
        {
            _logger.LogError(
                e,
                "[QuestionRepository] UpdateAsync() failed for question {@question}.",
                question);
            return false; 
        }
    }

    // DELETE: remove a question from the database.
    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var question = await _context.Questions.FindAsync(id); //look up the question. 

            if (question == null) // If the question does not exist, we log it and return false.
            {
                _logger.LogWarning("[QuestionRepository] Question not found for QuestionId {id}", id);

                return false;
            }

             // If the question exist, we remove it and save the change.
            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "[QuestionRepository] Question {id} was deleted successfully.",
                id);
            return true;
        }
        // If the question exist, but an error occures.
        catch (Exception e)
        {
            _logger.LogError(
                e,
                "[QuestionRepository] DeleteAsync() failed for QuestionId {id}",
                id);
            return false;
        }
    }
}
