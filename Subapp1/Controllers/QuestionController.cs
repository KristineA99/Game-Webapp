using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Subapp1.DAL;
using Subapp1.Models;
using Subapp1.ViewModels;

namespace Subapp1.Controllers;

// Admin pages for managing quiz questions (Create, Read, Update, Delete).

// Based on the ItemController pattern from the course demos
// Demo-ShopDatabase-6-Repository and Demo-ShopErrorHandlingLogging.

public class QuestionController : Controller
{
    // The repository used for all database operations on questions
    private readonly IQuestionRepository _questionRepository;
    // The logger writes errors and warnings to the console or a log file
    private readonly ILogger<QuestionController> _logger;
    private readonly ICategoryRepository _categoryRepository;

    public QuestionController(
        IQuestionRepository questionRepository, 
        ILogger<QuestionController> logger, 
        ICategoryRepository categoryRepository)
    {
        _questionRepository = questionRepository;
        _logger = logger;
        _categoryRepository = categoryRepository;
    }

    // READ - show a list of all questions
    public async Task<IActionResult> Index()
    {
        var questions = await _questionRepository.GetAllAsync();
        // Send the list to Views/Question/Index.cshtml
        return View(questions);
    }
    
    // CREATE - Show empty form with available categories for selection
    [HttpGet] 
    public async Task<IActionResult> Create()
    {
        var model = new QuestionFormViewModel();
        await PopulateCategoriesAsync(model); // Fill the model with available categories
        return View(model);
    }

    // CREATE - Receive and validate the submitted form
    [HttpPost] 
    [ValidateAntiForgeryToken] // protects against fake form submissions from
    // other websites (the form includes a hidden security token automatically).
    public async Task<IActionResult> Create(QuestionFormViewModel model)
    {
        await ValidateCategoryAsync(model);

        // Server-side validation
        if (!ModelState.IsValid)
        {
            await PopulateCategoriesAsync(model); 
            return View(model);
        }

        await _questionRepository.AddAsync(model.ToQuestion());

        TempData["Message"] = "Question created successfully.";
        return RedirectToAction(nameof(Index));
    }

    // UPDATE - show form populated with existing question values.
    [HttpGet]
    public async Task<IActionResult> Update(int id)
    {
        var question = await _questionRepository.GetByIdAsync(id);
        if (question == null)
        {
            _logger.LogWarning("Question '{QuestionId}' was not found when opening the update form", id);
            return NotFound("Question not found");
        }

        var model = QuestionFormViewModel.FromQuestion(question); // Convert the Question entity to a ViewModel
        await PopulateCategoriesAsync(model); // Fill the model with available categories
        return View(model);
    }

    // UPDATE - receive and validate the edited form.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(QuestionFormViewModel model)
    {
        await ValidateCategoryAsync(model);

        // Server-side validation
        if (!ModelState.IsValid)
        {
            await PopulateCategoriesAsync(model);
            return View(model);
        }

        bool updated = await _questionRepository.UpdateAsync(model.ToQuestion());
        if (!updated)
        {
            return NotFound("Question not found.");
        }

        TempData["Message"] = "Question updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    // DELETE - show confirmation page before deleting
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var question = await _questionRepository.GetByIdAsync(id);
        if (question == null)
        {
            _logger.LogWarning("Question '{QuestionId}' was not found when opening the delete confirmation page.", id);
            return NotFound("Question not found");
        }
        return View(question);
    }

    // DELETE - remove the question after confirmation.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        bool deleted = await _questionRepository.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound("Question not found.");
        }

        TempData["Message"] = "Question deleted.";
        return RedirectToAction(nameof(Index));
    }

    // Populate the ViewModel with available categories for selection in a dropdown.
    private async Task PopulateCategoriesAsync(QuestionFormViewModel model)
    {
        var categories = await _categoryRepository.GetAllAsync();
        model.AvailableCategories = categories
        .OrderBy(c => c.Name)
        .Select(c => new SelectListItem
        {
            Value = c.CategoryId.ToString(),
            Text = c.Name
        }).ToList();
    }

    // Verify that the selected category exists in the database. 
    private async Task ValidateCategoryAsync(QuestionFormViewModel model)
    {
        if (model.CategoryId > 0 &&
            await _categoryRepository.GetByIdAsync(model.CategoryId) == null)
        {
            ModelState.AddModelError(
                nameof(model.CategoryId),
                "Selected category does not exist.");
        }
    }   
}