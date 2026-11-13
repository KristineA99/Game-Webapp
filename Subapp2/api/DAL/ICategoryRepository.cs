using Subapp2.Models;

namespace Subapp2.DAL;

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
    Task<bool> DeleteAsync(int id);
}
