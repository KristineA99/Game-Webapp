namespace Subapp1.Models;

public class GameViewModel
{
    public Question? Question { get; set; }

    public int Score { get; set; }

    public bool? IsCorrect { get; set; }
}