using Subapp2.Models;

namespace Subapp2.DAL;

// Explains why deletion succeeded or failed so the API can respond appropriately.
    public enum CategoryDeleteResult
{
    Deleted,
    NotFound,
    HasQuestions,
    Failed
}


public interface ICategoryRepository
{
    // Retrieve all categories. Returns null if the database call fails.
    Task<IEnumerable<Category>?> GetAllAsync();

    // Retrieve a category by its ID.
    Task<Category?> GetByIdAsync(int id);

    // Create a new category.
    Task<bool> AddAsync(Category category);

    // Update an existing category.
    Task<bool> UpdateAsync(Category category);

    // Delete a category by its ID.
    Task<CategoryDeleteResult> DeleteAsync(int id);
}
