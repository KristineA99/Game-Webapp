using Microsoft.AspNetCore.Mvc;

namespace Subapp1.Controllers;

public class GameController : Controller  // Gamecontroller inherits from ASP.NET's controller class
{
    public IActionResult Play()   // returns the view under Game/play when requested
    {
        return View();
    }
}