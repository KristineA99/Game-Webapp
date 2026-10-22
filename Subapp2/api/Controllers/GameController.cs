using Microsoft.AspNetCore.Mvc;
using Subapp1.DAL;
using System.Text.Json;
using Subapp1.ViewModels;

namespace Subapp1.Controllers;

public class GameController : Controller 
{
    // Repository used to retrieve questions from the database.
    private readonly IQuestionRepository _questionRepository;
    public GameController(IQuestionRepository questionRepository)
    {
        _questionRepository = questionRepository;
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
    public IActionResult Play()   
   {
        // The GameViewModel contains the data that Play.cshtml needs.
        // At the start of the game, Question is null and Score defaults to 0.
        var viewModel = new GameViewModel();

        return View(viewModel);
    }
 
    // Starts the game by retrieving the first random question.
    [HttpPost]
    public async Task<IActionResult> Attack()
    {
        // Start with no completed questions for a new game.
        var completedQuestionIds = new List<int>();

        // Retrieve a random question from the full question pool.
        var question = await _questionRepository.GetRandomAsync(completedQuestionIds);    

        // Store the empty completed-question list in the session.           
        SaveCompletedQuestionIds(completedQuestionIds);
        

        // Start a new game with a score of 0.
        HttpContext.Session.SetInt32("Score", 0);

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
            var victoryViewModel = new GameViewModel
            {
                Score = score,
                HasWon = true
            };

            return View("Play", victoryViewModel);
        }

        // Keep the current score when loading the next question. 
        var viewModel = new GameViewModel
        {
            Question = question,
            Score = score
        };

        return View("Play", viewModel);
    }
}