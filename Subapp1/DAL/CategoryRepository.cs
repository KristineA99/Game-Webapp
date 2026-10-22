using Microsoft.EntityFrameworkCore;
using Subapp1.Models;

namespace Subapp1.DAL;

public class CategoryRepository : ICategoryRepository
{
    private readonly GameDbContext _context;
    private readonly ILogger<CategoryRepository> _logger;

    public CategoryRepository(GameDbContext context, ILogger<CategoryRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Retrieve all categories.
    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        return await _context.Categories.ToListAsync();
    }

    // Retrieve a category by its ID.
    public async Task<Category?> GetByIdAsync(int id)
    {
        return await _context.Categories.FindAsync(id);
    }

    // Create a new category.
    public async Task<bool> AddAsync(Category category)
    {
        try
        {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();   

        _logger.LogInformation("Category '{CategoryName}' created successfully with ID '{CategoryId}'.", category.Name, category.CategoryId);
        return true;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error occurred while creating category '{CategoryName}'.", category.Name);
            throw; // Rethrow the exception to be handled by the calling code.
        }
    }

    // Update an existing category.
    public async Task<bool> UpdateAsync(Category category)
    {
        var existingCategory = await _context.Categories.FindAsync(category.CategoryId);
        if (existingCategory == null)
        {
            _logger.LogWarning("Category update failed: Category with ID '{CategoryId}' not found.", category.CategoryId);
            return false;
        }

        existingCategory.Name = category.Name;

        try
        {
        await _context.SaveChangesAsync();
        _logger.LogInformation("Category '{CategorId}' updated successfully.", existingCategory.CategoryId);
        return true;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error occurred while updating category '{CategoryId}'.", category.CategoryId);
            throw; // Rethrow the exception to be handled by the calling code.
        } 
    }

    // Delete a category by its ID, but only if it has no associated questions.
    public async Task<bool> DeleteAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category == null)
        {
            _logger.LogWarning("Category deletion failed: Category with ID '{CategoryId}' not found.", id);
            return false;
        }

        // Check if the category has any associated questions.
        bool hasQuestions = await _context.Questions.AnyAsync(q => q.CategoryId == id);
        
        if (hasQuestions)
        {
            _logger.LogWarning("Category with ID '{CategoryId}' cannot be deleted because it has associated questions.", id);
            // Cannot delete the category because it has associated questions.
            return false;
        }

        try
        {
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Category '{CategoryId}' deleted successfully.", id);
            return true;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error occurred while deleting the category '{CategoryId}'.", id);
            throw; // Rethrow the exception to be handled by the calling code.
        } 
    }
}