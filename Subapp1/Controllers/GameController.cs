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

    //Handles the player's attack (what happens when attack button is clicked).
    //For now, it simply returns the Play view again.
    [HttpPost]
    public IActionResult Attack()
    {
        return View("Play");
    }
}