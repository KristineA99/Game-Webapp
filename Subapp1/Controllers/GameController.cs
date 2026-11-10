using Microsoft.AspNetCore.Mvc;
using Subapp1.DAL;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Rendering;
using Subapp1.ViewModels;
using System.Security.Claims;
using Subapp1.Models;

namespace Subapp1.Controllers;

public class GameController : Controller 
{
    private const string SelectedQuizIdSessionKey = "SelectedQuizId";
    private const string CurrentQuestionIdSessionKey = "CurrentQuestionId";
    private const string AwaitingAnswerSessionKey = "AwaitingAnswer";

    // Repository used to retrieve questions from the database.
    private readonly IQuestionRepository _questionRepository;
    private readonly IQuizRepository _quizRepository;
    private readonly IGameResultRepository _gameResultRepository;

    public GameController(
        IQuestionRepository questionRepository,
        IQuizRepository quizRepository,
        IGameResultRepository gameResultRepository)
    {
        _questionRepository = questionRepository;
        _quizRepository = quizRepository;
        _gameResultRepository = gameResultRepository;
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

    // Saves a completed game for a logged-in player
    // Guest players can finish quizzes and see their score, but their results are not stored.
    private async Task<bool> SaveGameResultAsync(int selectedQuizId)
    {
        // Guests do not have a user ID, so there is nothing to save.
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return true;
        }

        // Avoid saving the same completed attempt more than once.
        if (HttpContext.Session.GetString("ResultSaved") == "true")
        {
            return true;
        }

        int score = HttpContext.Session.GetInt32("Score") ?? 0;
        int totalQuestions = HttpContext.Session.GetInt32("TotalQuestions") ?? 0;

        //Calculate how long the player spent on the quiz.
        string? startedAtValue = HttpContext.Session.GetString("GameStartedAt");

        if (!DateTime.TryParse(
            startedAtValue,
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.RoundtripKind,
            out DateTime startedAt))
        {
            return false;
        }

        int durationSeconds = (int)Math.Max(
            0,
            (DateTime.UtcNow - startedAt).TotalSeconds);
        
        var result = new GameResult
        {
            UserId = userId,
            PlayerName = User.Identity?.Name ?? "Player",
            QuizId = selectedQuizId,
            QuizName = HttpContext.Session.GetString("QuizName") ?? "Quiz",
            Score = score,
            TotalQuestions = totalQuestions,
            DurationSeconds = durationSeconds,
            CompletedAt = DateTime.UtcNow
        };

        bool saved = await _gameResultRepository.AddAsync(result);

        if (saved)
        {
            HttpContext.Session.SetString("ResultSaved", "true");
        }
        
        return saved;
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

        // Track the question currently presented to the player.
        HttpContext.Session.SetInt32(CurrentQuestionIdSessionKey, questionId);
        HttpContext.Session.SetString(AwaitingAnswerSessionKey, "true");

        // Start a new game with a score of 0.
        HttpContext.Session.SetInt32("Score", 0);

        // Store the number of questions for the final score display.
        HttpContext.Session.SetInt32("TotalQuestions", quiz.QuizQuestions.Count);

        // Store the start time so we can calculate the game duration.
        HttpContext.Session.SetString(
            "GameStartedAt",
            DateTime.UtcNow.ToString("O"));
        
        // Keep a snapshot of the quiz title for the completed result.
        HttpContext.Session.SetString("QuizName", quiz.Title);

        // A new game has not yet been saved.
        HttpContext.Session.Remove("ResultSaved");

        // Clear any victory flag left by the player's previous game.
        HttpContext.Session.Remove("HasWon");

        // Pass the question and starting score to Play.cshtml through the ViewModel.
        var viewModel = new GameViewModel
        {
            Question = question,
            Score = 0,
            TotalQuestions = quiz.QuizQuestions.Count
        };

        return View("Play", viewModel);
    }

    // Handles the answer selected by player
    [HttpPost]
    public async Task<IActionResult> Answer(
        int questionId, 
        int selectedOption)
    {
        // Only accept answers to the question currently shown in this game.
        int? currentQuestionId = HttpContext.Session.GetInt32(CurrentQuestionIdSessionKey);
        bool awaitingAnswer = HttpContext.Session.GetString(AwaitingAnswerSessionKey) == "true";

        if (!awaitingAnswer || currentQuestionId != questionId)
        {
            return RedirectToAction(nameof(Play));
        }
        // Reject invalid answer options.
        if (selectedOption < 1 || selectedOption > 4)
        {
            return BadRequest();
        }
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

        //Prevent the same question from being scored more than once.
        if (completedQuestionIds.Contains(questionId))
        {
            return RedirectToAction(nameof(Play));
        }
        
        // only correct answers increase the score.
        if (isCorrect)
        {
            score++;  
        }
        // Mark the question as completed when answered, regardless of correct/incorrect status
        completedQuestionIds.Add(question.QuestionId);
        SaveCompletedQuestionIds(completedQuestionIds);

        // Store the updated score in the session.
        HttpContext.Session.SetInt32("Score", score);

        // The current question has been answered; wait for Continue().
        HttpContext.Session.SetString(AwaitingAnswerSessionKey, "false");

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
        // Don't allow the player to skip a question without answering.
        if (HttpContext.Session.GetString(AwaitingAnswerSessionKey) != "false")
        {
            return RedirectToAction(nameof(Play));
        }

        // Consume the Continue action so it cannot advance twice.
        HttpContext.Session.Remove(AwaitingAnswerSessionKey);

        int? selectedQuizId = HttpContext.Session.GetInt32(SelectedQuizIdSessionKey);
        if (!selectedQuizId.HasValue)
        {
            return RedirectToAction(nameof(Play));
        }

        // Get the completed question IDs stored in the player's session.
        var completedQuestionIds = GetCompletedQuestionIds();

        // Retrieve an unanswered question from the quiz selected when the game started.
        var question = await _questionRepository.GetRandomAsync(completedQuestionIds, selectedQuizId.Value);

        // Get the current score from the player's session.
        int score = HttpContext.Session.GetInt32("Score") ?? 0;

        if (question == null)
        {
        // Only finish when every question in the quiz has been answered.
        int totalQuestions = HttpContext.Session.GetInt32("TotalQuestions") ?? 0;

        if (totalQuestions < 5 || completedQuestionIds.Distinct().Count() != totalQuestions)
        {
            return RedirectToAction(nameof(Play));
        }

        // Save the result for logged-in players before showing the result page.
        bool saved = await SaveGameResultAsync(selectedQuizId.Value);

        if (!saved)
        {
            // Restore the completed-answer state so the player can retry Continue.
            HttpContext.Session.SetString(AwaitingAnswerSessionKey, "false");
            return StatusCode(500, "Could not save your game result. Please try again");
        }

        HttpContext.Session.SetString("HasWon", "true");
        return RedirectToAction(nameof(Result));
        }
    

        // Register the next question as the only one that can be answered.
        HttpContext.Session.SetInt32(CurrentQuestionIdSessionKey, question.QuestionId);
        HttpContext.Session.SetString(AwaitingAnswerSessionKey, "true");

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
    public async Task <IActionResult> Result()
    {
        // Only completed games should be able to open the victory result page.
        if (HttpContext.Session.GetString("HasWon") != "true")
        {
            return RedirectToAction(nameof(Play));
        }

        int? selectedQuizId = HttpContext.Session.GetInt32(SelectedQuizIdSessionKey);

        if (!selectedQuizId.HasValue)
        {
            return RedirectToAction(nameof(Play));
        }

        var quiz = await _quizRepository.GetByIdAsync(selectedQuizId.Value);

        // Reuse the final score from the session to display the completed game result.
        return View(new GameViewModel
        {
            Score = HttpContext.Session.GetInt32("Score") ?? 0,
            TotalQuestions = HttpContext.Session.GetInt32("TotalQuestions") ?? 0,
            QuizName = quiz?.Title ?? "Quiz",
            HasWon = true
        });
    }
}