using Microsoft.AspNetCore.Mvc;
using Subapp1.DAL;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Rendering;
using Subapp1.ViewModels;

namespace Subapp1.Controllers;

public class GameController : Controller 
{
    private const string SelectedQuizIdSessionKey = "SelectedQuizId";

    // Repository used to retrieve questions from the database.
    private readonly IQuestionRepository _questionRepository;
    private readonly IQuizRepository _quizRepository;

    public GameController(
        IQuestionRepository questionRepository,
        IQuizRepository quizRepository)
    {
        _questionRepository = questionRepository;
        _quizRepository = quizRepository;
    }

    // Retrieves the completed question Ids stored in the player's session and converts them from JSON into a list of integers.
    private List<int> GetCompletedQuestionIds()
    {
        var json = HttpContext.Session.GetString("CompletedQuestionIds");
        return json != null
            ? JsonSerializer.Deserialize<List<int>>(json) ?? new List<int>()
            : new List<int>();
    }

    // Saves the list of completed question IDs to the player's session.
    private void SaveCompletedQuestionIds(List<int> ids)
    {
        HttpContext.Session.SetString(
            "CompletedQuestionIds",
            JsonSerializer.Serialize(ids));
    }

    // Displays the initial game page.
    public async Task<IActionResult> Play()
    {
        return View(await CreateStartViewModelAsync());
    }

    private async Task<GameViewModel> CreateStartViewModelAsync(int? selectedQuizId = null)
    {
        // Only offer quizzes with enough questions to support a complete game.
        var quizzes = await _quizRepository.GetAllAsync();
        return new GameViewModel
        {
            AvailableQuizzes = quizzes
                .Where(quiz => quiz.QuizQuestions.Count >= 5)
                // Keep the selection list easy to scan and provide the fields the dropdown needs.
                .OrderBy(quiz => quiz.Title)
                .Select(quiz => new SelectListItem
                {
                    Value = quiz.QuizId.ToString(),
                    Text = quiz.Title,
                    Selected = quiz.QuizId == selectedQuizId
                })
                .ToList()
        };
    }
 
    // Starts a game with a random question from the selected quiz.
    [HttpPost]
    public async Task<IActionResult> Attack(int? selectedQuizId)
    {
        if (!selectedQuizId.HasValue)
        {
            ModelState.AddModelError(
                nameof(GameViewModel.SelectedQuizId),
                "Select a quiz to start.");
            return View("Play", await CreateStartViewModelAsync());
        }

        var quiz = await _quizRepository.GetByIdWithQuestionsAsync(selectedQuizId.Value);
        if (quiz == null)
        {
            ModelState.AddModelError(
                nameof(GameViewModel.SelectedQuizId),
                "The selected quiz is no longer available.");
            return View("Play", await CreateStartViewModelAsync(selectedQuizId));
        }

        if (quiz.QuizQuestions.Count < 5)
        {
            ModelState.AddModelError(
                nameof(GameViewModel.SelectedQuizId),
                "The selected quiz must contain at least five questions.");
            return View("Play", await CreateStartViewModelAsync(selectedQuizId));
        }

        // Start with no completed questions for a new game.
        var completedQuestionIds = new List<int>();

        // Choose the opening question only from the selected quiz.
        int questionIndex = Random.Shared.Next(quiz.QuizQuestions.Count);
        int questionId = quiz.QuizQuestions.ElementAt(questionIndex).QuestionId;
        var question = await _questionRepository.GetByIdAsync(questionId);
        if (question == null)
        {
            return NotFound();
        }

        // Store the empty completed-question list in the session.           
        SaveCompletedQuestionIds(completedQuestionIds);
        
        // Keep the selection so subsequent game requests can stay within this quiz.
        HttpContext.Session.SetInt32(SelectedQuizIdSessionKey, selectedQuizId.Value);

        // Start a new game with a score of 0.
        HttpContext.Session.SetInt32("Score", 0);
        // Clear any victory flag left by the player's previous game.
        HttpContext.Session.Remove("HasWon");

        // Pass the question and starting score to Play.cshtml through the ViewModel.
        var viewModel = new GameViewModel
        {
            Question = question,
            Score = 0
        };

        return View("Play", viewModel);
    }

    // Handles the answer selected by player
    [HttpPost]
    public async Task<IActionResult> Answer(
        int questionId, 
        int selectedOption)
    {
        // Retrieves the question that the player answered.
        var question = await _questionRepository.GetByIdAsync(questionId);

        if (question == null)
        {
            return NotFound();
        }

        // Get the current score stored in the player's session.
        int score = HttpContext.Session.GetInt32("Score") ?? 0;

        // Check whether the selected option matches the correct option stored for this question in the database
        bool isCorrect = selectedOption == question.CorrectOption;

        // Get the completed question IDs stored in the player's session.
        var completedQuestionIds = GetCompletedQuestionIds();

        // Increase the score and mark the question as completed
        // only when the answer is correct.
        if (isCorrect)
        {
            score++;

            completedQuestionIds.Add(question.QuestionId);

            SaveCompletedQuestionIds(completedQuestionIds);
        }

        // Store the updated score in the session.
        HttpContext.Session.SetInt32("Score", score);

        // Pass the answered question, updated score and answer result back to Play.cshtml through the ViewModel.
        var viewModel = new GameViewModel
        {
            Question = question,
            Score = score,
            IsCorrect = isCorrect
        };

        return View("Play", viewModel);
    }

    // Loads another random question after the player clicks continue.
    [HttpPost]
    public async Task<IActionResult> Continue()
    {
        // Get the completed question IDs stored in the player's session.
        var completedQuestionIds = GetCompletedQuestionIds();

        // Retrieve a random question that has not already been answered correctly.
        var question = await _questionRepository.GetRandomAsync(completedQuestionIds);

        // Get the current score from the player's session.
        int score = HttpContext.Session.GetInt32("Score") ?? 0;

        // If no unanswered questions remain, the player has completed the game.
        if (question == null)
        {
            // Store the win for the redirected request, then show the dedicated result page.
            HttpContext.Session.SetString("HasWon", "true");
            return RedirectToAction(nameof(Result));
        }

        // Keep the current score when loading the next question. 
        var viewModel = new GameViewModel
        {
            Question = question,
            Score = score
        };

        return View("Play", viewModel);
    }

    // Displays the result after the player has completed the game.
    [HttpGet]
    public IActionResult Result()
    {
        // Only completed games should be able to open the victory result page.
        if (HttpContext.Session.GetString("HasWon") != "true")
        {
            return RedirectToAction(nameof(Play));
        }

        // Reuse the final score from the session to display the completed game result.
        return View(new GameViewModel
        {
            Score = HttpContext.Session.GetInt32("Score") ?? 0,
            HasWon = true
        });
    }
}