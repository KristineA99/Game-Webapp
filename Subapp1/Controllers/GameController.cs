using Microsoft.AspNetCore.Mvc;
using Subapp1.DAL;
using Subapp1.Models;

namespace Subapp1.Controllers;

public class GameController : Controller  // Gamecontroller inherits from ASP.NET's controller class
{
    // Stores the question repository for use in this controller.
    // private = only this class can access the field
    // readonly = the field cannot be reassigned after the constructor has initialized it.
    private readonly IQuestionRepository _questionRepository;  
    // ASP.NET provides an implementation of IQuestionRepository through dependency injection when it creates Gamecontroller
    public GameController(IQuestionRepository questionRepository)
    {
        _questionRepository = questionRepository;
    }


    //Displays the initial game page.
    public IActionResult Play()   // returns the view under Game/play when requested
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
        // Retrieve a random question from the database
        var question = await _questionRepository.GetRandomAsync();    

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

        // Get the current score stored in teh player's session.
        int score = HttpContext.Session.GetInt32("Score") ?? 0;

        // Checks whether the selected option matches the correct option stored for this question in the database
        bool isCorrect = selectedOption == question.CorrectOption;

        //Increase the score only when the answer is correct.
        if (isCorrect)
        {
            score++;
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
        // Retrieves a random question from the database
        var question = await _questionRepository.GetRandomAsync();

        // Get the current score from the player's session.
        int score = HttpContext.Session.GetInt32("Score") ?? 0;

        // Keep the current score when loading the next question. 
        var viewModel = new GameViewModel
        {
            Question = question,
            Score = score
        };

        return View("Play", viewModel);
    }
}