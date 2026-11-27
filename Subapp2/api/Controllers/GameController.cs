using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Subapp2.DAL;
using Subapp2.DTOs;

namespace Subapp2.Controllers;

[ApiController]
[Route("api/game")]
// Guests, registered players and administrators can all play.
[AllowAnonymous]
// State-changing requests still require an antiforgery token.
[AutoValidateAntiforgeryToken]
public class GameController : ControllerBase
{
    private readonly IQuizRepository _quizRepository;
    private readonly ILogger<GameController> _logger;

    public GameController(
        IQuizRepository quizRepository,
        ILogger<GameController> logger)
    {
        _quizRepository = quizRepository;
        _logger = logger;
    }

    [HttpGet("quizzes/{quizId:int}")]
    public async Task<IActionResult> GetQuiz(int quizId)
    {
        Subapp2.Models.Quiz? quiz;

        try
        {
            quiz = await _quizRepository.GetByIdWithQuestionsAsync(quizId);
        }
        catch (Exception)
        {
            // The repository logs the exception; return a safe message to the player.
            return StatusCode(500, "Unable to load the quiz. Please try again.");
        }

        if (quiz == null)
        {
            _logger.LogWarning(
                "Gameplay quiz {QuizId} was not found.", quizId);

            return NotFound("Quiz not found.");
        }

        if (quiz.QuizQuestions.Count == 0)
        {
            _logger.LogWarning(
                "Gameplay quiz {QuizId} has no questions.", quizId);

            return BadRequest("This quiz has no questions. Please choose another quiz.");
        }

        // Explicit mapping prevents correct answers from reaching the player.
        var response = new GameQuizDto
        {
            QuizId = quiz.QuizId,
            Title = quiz.Title,
            Description = quiz.Description,
            Questions = quiz.QuizQuestions
                .OrderBy(link => link.QuestionId)
                .Select(link => new GameQuestionDto
                {
                    QuestionId = link.QuestionId,
                    Text = link.Question.Text,
                    OptionA = link.Question.OptionA,
                    OptionB = link.Question.OptionB,
                    OptionC = link.Question.OptionC,
                    OptionD = link.Question.OptionD
                })
                .ToList()
        };

        return Ok(response);
    }
}