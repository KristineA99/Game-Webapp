using Microsoft.EntityFrameworkCore;
using Subapp2.Models;

namespace Subapp2.DAL;

// Handles all database access for categories.
public class CategoryRepository : ICategoryRepository
{
    private readonly GameDbContext _context;
    private readonly ILogger<CategoryRepository> _logger;

    public CategoryRepository(GameDbContext context, ILogger<CategoryRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Retrieve all categories, sorted by name.
    public async Task<IEnumerable<Category>?> GetAllAsync()
    {
        try
        {
            return await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[CategoryRepository] GetAllAsync() failed.");
            return null;
        }
    }

    // Retrieve a category by its ID.
    // Returns null if no category has that id, or if the database call fails.
    public async Task<Category?> GetByIdAsync(int id)
    {
        try
        {
            return await _context.Categories.FindAsync(id);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[CategoryRepository] GetByIdAsync() failed for CategoryId {id}.", id);
            return null;
        }
    }

    // Create a new category.
    public async Task<bool> AddAsync(Category category)
    {
        try
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            _logger.LogInformation("[CategoryRepository] Category {id} was added successfully.", category.CategoryId);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[CategoryRepository] AddAsync() failed for category {@category}.", category);
            return false;
        }
    }

    // Update an existing category.
    public async Task<bool> UpdateAsync(Category category)
    {
        try
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();

            _logger.LogInformation("[CategoryRepository] Category {id} was updated successfully.", category.CategoryId);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[CategoryRepository] UpdateAsync() failed for category {@category}.", category);
            return false;
        }
    }

    // Delete a category by its ID, but only if it has no associated questions.
    public async Task<CategoryDeleteResult> DeleteAsync(int id)
    {
        try
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                _logger.LogWarning("Category {CategoryId} was not found for deletion.", id);
                return CategoryDeleteResult.NotFound;
            }

            // Domain rule: a category that still contains questions cannot be deleted.
            bool hasQuestions = await _context.Questions.AnyAsync(q => q.CategoryId == id);
            if (hasQuestions)
            {
                _logger.LogWarning("Category {Categoryid} cannot be deleted because it has associated questions.", id);
                return CategoryDeleteResult.HasQuestions;
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Category {Categoryid} was deleted successfully.", id);
            return CategoryDeleteResult.Deleted;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to delete category {CategoryId} .", id);
            return CategoryDeleteResult.Failed;
        }
    }
}
