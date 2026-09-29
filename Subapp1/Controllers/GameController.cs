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
        var questions = await _questionRepository.GetAllAsync();    
        return View("Play");
    }
}