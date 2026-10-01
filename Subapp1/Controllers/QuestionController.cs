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
        
        // null means the database call failed (an empty list would mean "no questions yet")
        if (questions == null)
        {
            _logger.LogError("[QuestionController] Question list not found while executing GetAllAsync()");
            return NotFound("Question list not found");
        }
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
        // Server-side validation: show the form again with error messages if invalid
        if (!ModelState.IsValid)
        {
            return View(question);
        }

        bool ok = await _questionRepository.AddAsync(question);
        if (ok)
        {
            // TempData keeps a message for the next page only,
            // so the Index page can show "Question created."
            TempData["Message"] = "Question created.";

            // Redirect to the list, so refreshing the page does not submit the form again
            return RedirectToAction(nameof(Index));
        }

        // Saving failed (the repository has already logged the details).
        _logger.LogWarning("[QuestionController] Question creation failed {@question}", question);
        // Log a warning here too and show a general error message at the top of the form.
        ModelState.AddModelError(string.Empty, "The question could not be saved. Please try again.");
        return View(question);
    }

    // UPDATE - show form filled in with existing values
    [HttpGet]
    public async Task<IActionResult> Update(int id)
    {
        var question = await _questionRepository.GetByIdAsync(id);
        if (question == null)
        {
            _logger.LogError("[QuestionController] Question not found when updating QuestionId {id}", id);
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
        if (!ModelState.IsValid)
        {
            return View(question);
        }

        bool ok = await _questionRepository.UpdateAsync(question);
        if (ok)
        {
            TempData["Message"] = "Question updated.";
            return RedirectToAction(nameof(Index));
        }

        _logger.LogWarning("[QuestionController] Question update failed {@question}", question);
        ModelState.AddModelError(string.Empty, "The question could not be updated. Please try again.");
        return View(question);
    }

    // DELETE - show confirmation page before deleting
    // Nothing is deleted here; the user must confirm first.
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var question = await _questionRepository.GetByIdAsync(id);
        if (question == null)
        {
            _logger.LogError("[QuestionController] Question not found when deleting QuestionId {id}", id);
            return NotFound("Question not found");
        }
        return View(question);
    }

    // DELETE - actually delete the question after the user confirms.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        bool ok = await _questionRepository.DeleteAsync(id);
        if (!ok)
        {
            _logger.LogError("[QuestionController] Question deletion failed for QuestionId {id}", id);
            return BadRequest("Question deletion failed");
        }

        TempData["Message"] = "Question deleted.";
        return RedirectToAction(nameof(Index));
    }
}