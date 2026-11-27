using Subapp2.Models;

namespace Subapp2.DAL;

// Lets the API distinguish a missing question from a database failure.
public enum QuestionDeleteResult
{
    Deleted,
    NotFound,
    Failed
}

public interface IQuestionRepository
{
    // READ: get all questions for the Admin page
    Task<IEnumerable<Question>?> GetAllAsync();
     
       // READ: get questions that match a search text and/or a category (both optional)
    Task<IEnumerable<Question>?> GetFilteredAsync(string? searchText, int? categoryId);

    // READ: get one specific question by its id.
    Task<Question?> GetByIdAsync(int id);

    // READ: get a random question for the game
    Task<Question?> GetRandomAsync(List<int> excludeIds);

    // CREATE: add a new question.
    Task<bool> AddAsync(Question question);

    // UPDATE: save changes to an existing question.
    Task<bool> UpdateAsync(Question question);

    // DELETE: remove a question by its id.
    Task<QuestionDeleteResult> DeleteAsync(int id);
}