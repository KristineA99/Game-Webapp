using Subapp1.Models;

namespace Subapp1.DAL;

public interface IQuestionRepository
{
    // READ - get all questions for the Admin page
    Task<IEnumerable<Question>?> GetAllAsync();

    // READ - get one specific question
    Task<Question?> GetByIdAsync(int id);

    // READ - get a random question for the game
    Task<Question?> GetRandomAsync();

    // CREATE
    Task<bool> AddAsync(Question question);

    // UPDATE
    Task<bool> UpdateAsync(Question question);

    // DELETE
    Task<bool> DeleteAsync(int id);
}