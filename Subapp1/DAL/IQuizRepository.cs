using Subapp1.Models;

namespace Subapp1.DAL;

public interface IQuizRepository
{
    // Retrieve all quizzes.
    Task<IEnumerable<Quiz>> GetAllAsync();

    // Retrieve a quiz by its ID.
    Task<Quiz?> GetByIdAsync(int id);

    // Create a new quiz.
    Task<bool> AddAsync(Quiz quiz, List<int> selectedQuestionIds);

    // Update an existing quiz.
    Task<bool> UpdateAsync(Quiz quiz, List<int> selectedQuestionIds);

    // Delete a quiz by its ID.
    Task<bool> DeleteAsync(int id);

    //Retrieve a quiz by its ID along with its associated questions.
    Task<Quiz?> GetByIdWithQuestionsAsync(int id);
}