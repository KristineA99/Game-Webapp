using Microsoft.AspNetCore.Mvc;
using Subapp1.DAL;
using Subapp1.Models;

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

    public QuestionController(IQuestionRepository questionRepository, ILogger<QuestionController> logger)
    {
        _questionRepository = questionRepository;
        _logger = logger;
    }

    // READ - show a list of all questions
    public async Task<IActionResult> Index()
    {
        var questions = await _questionRepository.GetAllAsync();
        // Send the list to Views/Question/Index.cshtml
        return View(questions);
    }

    // CREATE - Show empty form
    [HttpGet] // Runs when the user opens the page.
    public IActionResult Create()
    {
        return View();
    }

    // CREATE - Receive the filled-in form
    [HttpPost] // Runs when the user submits the form.
    [ValidateAntiForgeryToken] // protects against fake form submissions from
    // other websites (the form includes a hidden security token automatically).
    public async Task<IActionResult> Create(Question question)
    {
        // Server-side validation
        if (!ModelState.IsValid)
        {
            return View(question);
        }

        await _questionRepository.AddAsync(question);

        TempData["Message"] = "Question created successfully.";
        return RedirectToAction(nameof(Index));
    }

    // UPDATE - show form filled in with existing values
    [HttpGet]
    public async Task<IActionResult> Update(int id)
    {
        var question = await _questionRepository.GetByIdAsync(id);
        if (question == null)
        {
            _logger.LogWarning("Question '{QuestionId}' was not found when opening the update form", id);
            return NotFound("Question not found");
        }
        return View(question);
    }

    // UPDATE - receive the edited form
    // Works the same way as Create, but calls UpdateAsync
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(Question question)
    {
        // Server-side validation
        if (!ModelState.IsValid)
        {
            return View(question);
        }

        bool updated = await _questionRepository.UpdateAsync(question);
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

    // DELETE - actually delete the question after the user confirms.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        bool deleted = await _questionRepository.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound("Question not found or could.");
        }

        TempData["Message"] = "Question deleted.";
        return RedirectToAction(nameof(Index));
    }
}