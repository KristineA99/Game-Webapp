using Microsoft.AspNetCore.Mvc;
using Subapp2.DAL;
using Subapp2.DTOs;
using Subapp2.Models;

namespace Subapp2.Controllers;

// API controller for quizzes and the questions they contain.
[ApiController]
[Route("api/[controller]")]   // Base route: api/quizapi
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
            _logger.LogError("[QuizAPIController] Quiz list not found while executing _quizRepository.GetAllAsync()");
            return NotFound("Quiz list not found");
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
        var quiz = await _quizRepository.GetByIdWithQuestionsAsync(id);
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

    // POST api/quizapi/create
    // The repository checks that every selected question exists.
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] QuizDto quizDto)
    {
        if (quizDto == null)
        {
            // Returns Bad Request if the quiz has no title or no questions.
            return BadRequest("Quiz cannot be null");
        }

        var newQuiz = new Quiz
        {
            Title = quizDto.Title,
            Description = quizDto.Description
        };

        bool returnOk = await _quizRepository.AddAsync(newQuiz, quizDto.QuestionIds);
        if (returnOk)
        {
            quizDto.QuizId = newQuiz.QuizId;
            return CreatedAtAction(nameof(GetQuiz), new { id = newQuiz.QuizId }, quizDto);
        }

        _logger.LogWarning("[QuizAPIController] Quiz creation failed {@quiz}", quizDto);
        return StatusCode(500, "Internal server error");
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
        var existingQuiz = await _quizRepository.GetByIdWithQuestionsAsync(id);
        if (existingQuiz == null)
        {
            return NotFound("Quiz not found");
        }

        // Update the quiz properties
        existingQuiz.Title = quizDto.Title;
        existingQuiz.Description = quizDto.Description;

        // Save the changes, including which questions are in the quiz
        bool updateSuccessful = await _quizRepository.UpdateAsync(existingQuiz, quizDto.QuestionIds);
        if (updateSuccessful)
        {
            quizDto.QuizId = existingQuiz.QuizId;
            return Ok(quizDto);
        }

        _logger.LogWarning("[QuizAPIController] Quiz update failed {@quiz}", quizDto);
        return StatusCode(500, "Internal server error");
    }

    // DELETE api/quizapi/delete/5
    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        bool returnOk = await _quizRepository.DeleteAsync(id);
        if (!returnOk)
        {
            _logger.LogError("[QuizAPIController] Quiz deletion failed for the QuizId {QuizId:0000}", id);
            return BadRequest("Quiz deletion failed");
        }

        return NoContent();
    }
}
