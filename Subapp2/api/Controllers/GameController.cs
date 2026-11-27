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

    [HttpGet("quizzes")]
    public async Task<IActionResult> GetQuizzes()
    {
        var quizzes = await _quizRepository.GetAllAsync();

        if (quizzes == null)
        {
            // The repository logs the database exception.
            return StatusCode(500, "Unable to load quizzes. Please try again.");
        }

        // Only offer quizzes that contain questions.
        var response = quizzes
            .Where(quiz => quiz.QuizQuestions.Count > 0)
            .Select(quiz => new GameQuizSummaryDto
            {
                QuizId = quiz.QuizId,
                Title = quiz.Title,
                Description = quiz.Description,
                QuestionCount = quiz.QuizQuestions.Count
            })
            .ToList();

        return Ok(response);
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

    [HttpPost("quizzes/{quizId:int}/submit")]
    public async Task<IActionResult> Submit(
    int quizId,
    [FromBody] GameSubmissionDto submission)
    {
        Subapp2.Models.Quiz? quiz;

        try
        {
            quiz = await _quizRepository.GetByIdWithQuestionsAsync(quizId);
        }
        catch (Exception)
        {
            // The repository logs the database exception.
            return StatusCode(500, "Unable to submit the quiz. Please try again.");
        }

        if (quiz == null)
        {
            _logger.LogWarning(
                "Submission rejected because quiz {QuizId} was not found.", quizId);

            return NotFound("Quiz not found.");
        }

        if (quiz.QuizQuestions.Count == 0)
        {
            _logger.LogWarning(
                "Submission rejected because quiz {QuizId} has no questions.", quizId);

            return BadRequest("This quiz has no questions. Please choose another quiz.");
        }

        // Defend against null list entries in manually constructed requests.
        if (submission.Answers.Any(answer => answer == null))
        {
            _logger.LogWarning(
                "Submission for quiz {QuizId} contains a null answer.", quizId);

            return BadRequest("Each answer must contain a question ID and selected option.");
        }

        var answers = submission.Answers;
        var questionIds = quiz.QuizQuestions
            .Select(link => link.QuestionId)
            .ToHashSet();

        if (answers.Select(answer => answer.QuestionId).Distinct().Count()
            != answers.Count)
        {
            _logger.LogWarning(
                "Submission for quiz {QuizId} contains duplicate answers.", quizId);

            return BadRequest("Each question must be answered exactly once.");
        }

        if (answers.Any(answer => !questionIds.Contains(answer.QuestionId)))
        {
            _logger.LogWarning(
                "Submission for quiz {QuizId} contains unrelated questions.", quizId);

            return BadRequest(
                "One or more questions do not belong to this quiz. " +
                "The quiz may have changed while you were playing. " +
                "Refresh the page to restart, or select Leave Quiz and choose the quiz again.");
        }

        // With duplicates and unrelated IDs excluded, equal counts prove completeness.
        if (answers.Count != questionIds.Count)
        {
            _logger.LogWarning(
                "Submission for quiz {QuizId} is incomplete.", quizId);

            return BadRequest(
                "Your answers do not match the quiz's current questions. " +
                "The quiz may have changed while you were playing. " +
                "Select Leave Quiz, choose the quiz again, and answer all questions.");
        }

        // GameAnswerDto validates option numbers from 1 to 4 before this action runs.
        var correctOptions = quiz.QuizQuestions.ToDictionary(
            link => link.QuestionId,
            link => link.Question.CorrectOption);

        int score = answers.Count(
            answer => answer.SelectedOption == correctOptions[answer.QuestionId]);

        _logger.LogInformation(
            "Quiz {QuizId} scored successfully: {Score}/{TotalQuestions}.",
            quizId, score, questionIds.Count);

        return Ok(new GameResultDto
        {
            QuizId = quiz.QuizId,
            QuizTitle = quiz.Title,
            Score = score,
            TotalQuestions = questionIds.Count
        });
    }
}