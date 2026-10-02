namespace Subapp1.Models;

public class GameViewModel
{
    public Question? Question { get; set; }

    public int Score { get; set; }

    public bool? IsCorrect { get; set; }

    // True when the player has correctly answered all questions.
    public bool HasWon { get; set; }
}