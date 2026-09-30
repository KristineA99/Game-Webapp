using Microsoft.AspNetCore.Mvc;
using Subapp1.DAL;

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
        return View();
    }

    // Use the repository stored in _questionsRepository, call its GetAllAsync() method,
    // wait for the database operation to finish and store the result in questions
    [HttpPost]
    public async Task<IActionResult> Attack()
    {
        //Retrieve a random question from the database
        var question = await _questionRepository.GetRandomAsync();    

        // A new game starts with a score of 0.
        ViewBag.Score = 0;

        //Display Play.cshtml and pass the selected question to the view
        return View("Play", question);
    }

    // Handles the answer selected by player
    [HttpPost]
    public async Task<IActionResult> Answer(
        int questionId, 
        int selectedOption,
        int score)
    {
        // Retrieves the question that the player answered.
        var question = await _questionRepository.GetByIdAsync(questionId);

        if (question == null)
        {
            return NotFound();
        }

        // Checks whether the selected option matches the correct option stored for this question in the database
        bool isCorrect = selectedOption == question.CorrectOption;

        //Increase the score only when the answer is correct.
        if (isCorrect)
        {
            score++;
        }

        // Makes the result available to Play.cshtml.
        ViewBag.IsCorrect = isCorrect;
        ViewBag.Score = score;

        return View("Play", question);
    }

    // Handles the player continuing after answering a question
    [HttpPost]
    public async Task<IActionResult> Continue(int score)
    {
        // Retrieves a random question from the database
        var question = await _questionRepository.GetRandomAsync();

        //Keep the current score when loading the next question.
        ViewBag.Score = score;

        return View("Play", question);
    }
}