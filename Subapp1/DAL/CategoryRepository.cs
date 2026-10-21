using Microsoft.EntityFrameworkCore;
using Subapp1.Models;

namespace Subapp1.DAL;

public class CategoryRepository : ICategoryRepository
{
    private readonly GameDbContext _context;

    public CategoryRepository(GameDbContext context)
    {
        _context = context;
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
        _context.Categories.Add(category);
        return await _context.SaveChangesAsync() > 0;
    }

    // Update an existing category.
    public async Task<bool> UpdateAsync(Category category)
    {
        var existingCategory = await _context.Categories.FindAsync(category.CategoryId);
        if (existingCategory == null)
        {
            return false;
        }

        existingCategory.Name = category.Name;

        await _context.SaveChangesAsync();
        return true;
    }

    // Delete a category by its ID, but only if it has no associated questions.
    public async Task<bool> DeleteAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category == null)
        {
            return false;
        }

        // Check if the category has any associated questions.
        bool hasQuestions = await _context.Questions.AnyAsync(q => q.CategoryId == id);
        
        if (hasQuestions)
        {
            // Cannot delete the category because it has associated questions.
            return false;
        }

        _context.Categories.Remove(category);
        return await _context.SaveChangesAsync() > 0;
        
    }
}