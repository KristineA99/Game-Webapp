using Subapp1.Models;

namespace Subapp1.DAL;

public interface IQuestionRepository
{
    // READ - get all questions for the Admin page
    Task<IEnumerable<Question>?> GetAllAsync();

    // READ - get one specific question by its id.
    Task<Question?> GetByIdAsync(int id);

    // READ - get a random question for the game
    Task<Question?> GetRandomAsync(List<int> excludeIds);

    // CREATE - add a new question.
    Task<bool> AddAsync(Question question);

    // UPDATE - save changes to an existing question.
    Task<bool> UpdateAsync(Question question);

    // DELETE - remove a question by its id.
    Task<bool> DeleteAsync(int id);
}