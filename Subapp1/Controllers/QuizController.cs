using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Subapp1.DAL;
using Microsoft.AspNetCore.Mvc.Rendering;
using Subapp1.Models;
using Subapp1.ViewModels;

namespace Subapp1.Controllers;

[Authorize (Roles = "Admin")] // Only users in the Admin role can access these pages
public class QuizController : Controller
{
    private readonly IQuizRepository _quizRepository;
    private readonly IQuestionRepository _questionRepository;

    // Inject the quiz repository through the constructor.
    public QuizController(IQuizRepository quizRepository, IQuestionRepository questionRepository)
    {
        _quizRepository = quizRepository;
        _questionRepository = questionRepository;
    }

    // GET: /Quiz/
    // Display all quizzes.
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var quizzes = await _quizRepository.GetAllAsync();
        return View(quizzes);
    }

    // GET: /Quiz/Create
    // Display the form with available questions for selection.
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var viewModel = new QuizFormViewModel();
        await PopulateAvailableQuestionsAsync(viewModel); 
        return View(viewModel);
    }

    // POST: /Quiz/Create
    // Validate and save the new quiz with its selected questions.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create( QuizFormViewModel viewModel)
    {
        // A quiz must have at least one question selected.
        if (viewModel.SelectedQuestionIds == null || viewModel.SelectedQuestionIds.Count == 0)
        {
            ModelState.AddModelError(
                nameof(viewModel.SelectedQuestionIds), 
                "At least one question must be selected."
                );
        }

        // Check all validation rules before saving.
        if (!ModelState.IsValid)
        {
            await PopulateAvailableQuestionsAsync(viewModel);
            return View(viewModel);
        }

        // Convert the ViewModel to a Quiz entity.
        var quiz = new Quiz
        {
            Title = viewModel.Title,
            Description = viewModel.Description
        };

        bool success = await _quizRepository.AddAsync(
            quiz, 
            viewModel.SelectedQuestionIds ?? new List<int>()
            );

        if (!success)
        {
            ModelState.AddModelError(
                "", 
                "Unable to create the quiz. Please check the selected questions."
                );

            await PopulateAvailableQuestionsAsync(viewModel);
            return View(viewModel);
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: /Quiz/Edit/{id}
    // Display the form for editing an existing quiz and its selected questions.
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var quiz = await _quizRepository.GetByIdWithQuestionsAsync(id);
        if (quiz == null)
        {
            return NotFound();
        }

        var viewModel = new QuizFormViewModel
        {
            QuizId = quiz.QuizId,
            Title = quiz.Title,
            Description = quiz.Description,
            SelectedQuestionIds = quiz.QuizQuestions.Select(qq => qq.QuestionId).ToList()
        };

        await PopulateAvailableQuestionsAsync(viewModel);
        return View(viewModel);
    }

    // POST: /Quiz/Edit/{id}
    // Validate and save the updated quiz and its selected questions.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, QuizFormViewModel viewModel)
    {
        if (id != viewModel.QuizId)
        {
            return BadRequest();
        }

        // A quiz must have at least one question selected.
        if (viewModel.SelectedQuestionIds == null || viewModel.SelectedQuestionIds.Count == 0)
        {
            ModelState.AddModelError(
                nameof(viewModel.SelectedQuestionIds), 
                "At least one question must be selected."
                );
        }

        // Check all validation rules before saving.
        if (!ModelState.IsValid)
        {
            await PopulateAvailableQuestionsAsync(viewModel);
            return View(viewModel);
        }

        var quiz = new Quiz
        {
            QuizId = viewModel.QuizId,
            Title = viewModel.Title,
            Description = viewModel.Description
        };

        bool success = await _quizRepository.UpdateAsync(
            quiz, 
            viewModel.SelectedQuestionIds ?? new List<int>()
            );

        if (!success)
        {
            ModelState.AddModelError(
                "", 
                "Unable to update the quiz. It may no longer exist, or some selected questions are invalid."
                );

            await PopulateAvailableQuestionsAsync(viewModel);
            return View(viewModel);
        }

        return RedirectToAction(nameof(Index));
    }

    // Get: /Quiz/Delete/{id}
    // Display a confirmation page before deleting a quiz.
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var quiz = await _quizRepository.GetByIdAsync(id);
        if (quiz == null)
        {
            return NotFound();
        }
        return View(quiz);
    }

    // POST: /Quiz/Delete/{id}
    // Delete the quiz if it has no recorded game results.
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var quiz = await _quizRepository.GetByIdAsync(id);
        if (quiz == null)
        {
            return NotFound();
        }

        bool success = await _quizRepository.DeleteAsync(id);
        if (!success)
        {
            ModelState.AddModelError(
                "", 
                "Unable to delete this quiz. It may have recorded game results."
                );
            return View("Delete", quiz);
        }

        return RedirectToAction(nameof(Index));
    }

    // Helper method to populate the AvailableQuestions list in the ViewModel.
    private async Task PopulateAvailableQuestionsAsync(QuizFormViewModel viewModel)
    {
        var questions = await _questionRepository.GetAllAsync();
        viewModel.AvailableQuestions = questions?.Select(q => new SelectListItem
        {
            Value = q.QuestionId.ToString(),
            Text = q.Text,
            Selected = viewModel.SelectedQuestionIds?.Contains(q.QuestionId) == true
        }).ToList() ?? new List<SelectListItem>();
    }
}