using Microsoft.AspNetCore.Mvc;
using Subapp2.DAL;
using Subapp2.DTOs;
using Subapp2.Models;
using Microsoft.AspNetCore.Authorization;

namespace Subapp2.Controllers;

// API controller for quizzes and the questions they contain.
[ApiController]
[Route("api/[controller]")]   // Base route: api/quizapi
// Restrict quiz management to administrators.
[Authorize(Roles = IdentitySeeder.AdminRole)]
// Require antiforgery tokens for state-changing requests, but not GET requests.
[AutoValidateAntiforgeryToken]
public class QuizAPIController : ControllerBase
{
    private readonly IQuizRepository _quizRepository;
    private readonly ILogger<QuizAPIController> _logger;

    public QuizAPIController(IQuizRepository quizRepository, ILogger<QuizAPIController> logger)
    {
        _quizRepository = quizRepository;
        _logger = logger;
    }

    // GET api/quizapi/quizlist
    [HttpGet("quizlist")]
    public async Task<IActionResult> QuizList()
    {
        var quizzes = await _quizRepository.GetAllAsync();
        if (quizzes == null)
        {
            _logger.LogError(
         "[QuizAPIController] Unable to load quizzes because the repository call failed.");

            return StatusCode(500, "Unable to load quizzes. Please try again.");
        }

        // The question links are sent as a list of question ids
        var quizDtos = quizzes.Select(quiz => new QuizDto
        {
            QuizId = quiz.QuizId,
            Title = quiz.Title,
            Description = quiz.Description,
            QuestionIds = quiz.QuizQuestions.Select(qq => qq.QuestionId).ToList()
        });

        return Ok(quizDtos);
    }

    // GET api/quizapi/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetQuiz(int id)
    {
        Quiz? quiz;

        try
        {
            quiz = await _quizRepository.GetByIdWithQuestionsAsync(id);
        }
        catch (Exception)
        {
            // The repository logs the exception; keep its details out of the response.
            return StatusCode(500, "Unable to load the quiz. Please try again.");
        }
        if (quiz == null)
        {
            _logger.LogError("[QuizAPIController] Quiz not found for the QuizId {QuizId:0000}", id);
            return NotFound("Quiz not found for the QuizId");
        }

        var quizDto = new QuizDto
        {
            QuizId = quiz.QuizId,
            Title = quiz.Title,
            Description = quiz.Description,
            QuestionIds = quiz.QuizQuestions.Select(qq => qq.QuestionId).ToList()
        };

        return Ok(quizDto);
    }

    // [ApiController] validates the DTO before this action runs.
    // The repository then checks that all selected questions exist.
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] QuizDto quizDto)
    {
        if (quizDto == null)
        {
            // Reject a request that does not contain required quiz data.
            return BadRequest("Quiz cannot be null");
        }

        var newQuiz = new Quiz
        {
            Title = quizDto.Title,
            Description = quizDto.Description
        };

        var result = await _quizRepository.AddAsync(newQuiz, quizDto.QuestionIds);

        if (result == QuizSaveResult.Saved)
        {
            quizDto.QuizId = newQuiz.QuizId;
            return CreatedAtAction(nameof(GetQuiz), new { id = newQuiz.QuizId }, quizDto);
        }

        // Invalid selections are input errors; exception details stay in repository logs.
        if (result == QuizSaveResult.InvalidQuestions)
        {
            return BadRequest(
                "One or more selected questions no longer exist. Reload the form and try again.");
        }

        return StatusCode(500, "Unable to create the quiz. Please try again.");
    }

    // PUT api/quizapi/update/5
    [HttpPut("update/{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] QuizDto quizDto)
    {
        if (quizDto == null)
        {
            return BadRequest("Quiz data cannot be null");
        }

        // Find the quiz (with its question links) in the database
        Quiz? existingQuiz;

        try
        {
            existingQuiz = await _quizRepository.GetByIdWithQuestionsAsync(id);
        }
        catch (Exception)
        {
            // Loading failed, so do not attempt to update the quiz.
            return StatusCode(500, "Unable to load the quiz for editing. Please try again.");
        }
        if (existingQuiz == null)
        {
            return NotFound("Quiz not found");
        }

        // Update the quiz properties
        existingQuiz.Title = quizDto.Title;
        existingQuiz.Description = quizDto.Description;

        var result = await _quizRepository.UpdateAsync(
        existingQuiz, quizDto.QuestionIds);

        if (result == QuizSaveResult.Saved)
        {
            quizDto.QuizId = existingQuiz.QuizId;
            return Ok(quizDto);
        }

        if (result == QuizSaveResult.InvalidQuestions)
        {
            return BadRequest(
                "One or more selected questions no longer exist. Reload the form and try again.");
        }

        return StatusCode(500, "Unable to update the quiz. Please try again.");
    }

    // Delete a quiz and distinguish a missing record from a server failure.
    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var result = await _quizRepository.DeleteAsync(id);

        // The repository logs each outcome and any exception details.
        return result switch
        {
            QuizDeleteResult.Deleted => NoContent(),
            QuizDeleteResult.NotFound => NotFound("Quiz not found."),
            _ => StatusCode(500, "Unable to delete the quiz. Please try again.")
        };
    }
}
