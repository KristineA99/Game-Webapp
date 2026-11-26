using Subapp2.Models;

namespace Subapp2.DAL;

// Distinguishes invalid question selections from unexpected save failures.
public enum QuizSaveResult
{
    Saved,
    InvalidQuestions,
    Failed
}

// Distinguishes a missing quiz from an unexpected deletion failure.
public enum QuizDeleteResult
{
    Deleted,
    NotFound,
    Failed
}

public interface IQuizRepository
{
    // Retrieve all quizzes. Returns null if the database call fails.
    Task<IEnumerable<Quiz>?> GetAllAsync();

    // Retrieve a quiz by its ID along with its associated questions.
    Task<Quiz?> GetByIdWithQuestionsAsync(int id);

    // Create a new quiz.
Task<QuizSaveResult> AddAsync(Quiz quiz, List<int> selectedQuestionIds);

    // Update an existing quiz.
Task<QuizSaveResult> UpdateAsync(Quiz quiz, List<int> selectedQuestionIds);

    // Delete a quiz by its ID.
    Task<QuizDeleteResult> DeleteAsync(int id);
}
