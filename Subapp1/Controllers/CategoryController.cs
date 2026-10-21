using Microsoft.AspNetCore.Mvc;
using Subapp1.DAL;
using Subapp1.Models;

namespace Subapp1.Controllers;

public class CategoryController : Controller
{
    private readonly ICategoryRepository _categoryRepository;

    // Inject the category repository through the constructor.
    public CategoryController(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    // GET: /Category/
    // Display all categories. 
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var categories = await _categoryRepository.GetAllAsync();
        return View(categories);
    }

    // GET: /Category/Create
    // Display the form for creating a new category.
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    // POST: /Category/Create
    // Validate and save the new category.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category category)
    {
        if (!ModelState.IsValid)
        {
            return View(category);
        }

    bool success = await _categoryRepository.AddAsync(category);

        if (!success)
        {
            ModelState.AddModelError("", "Unable to create the category.");
            return View(category);
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: /Category/Edit/{id}
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
        {
            return NotFound();
        }
        return View(category);
    }

    // POST: /Category/Edit/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Category category)
    {
        if (id != category.CategoryId)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(category);
        }

        bool success = await _categoryRepository.UpdateAsync(category);

        if (!success)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: /Category/Delete/{id}
    // Display a confirmation page before deleting a category.
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);

        if (category == null)
    {
        return NotFound();
    }

        return View(category);
    }

    // POST: /Category/Delete/{id}
    // Attempt to delete the category.
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);

        if (category == null)
        {
            return NotFound();
        }

        bool success = await _categoryRepository.DeleteAsync(id);

        if (!success)
        {
            ModelState.AddModelError(
                "",
                "Unable to delete this category. Make sure it has no associated questions."
            );

            return View("Delete", category);
        }
        
         return RedirectToAction(nameof(Index));
    }
   

}

