using Microsoft.AspNetCore.Mvc;
using Subapp2.DAL;
using Subapp2.DTOs;
using Subapp2.Models;
using Microsoft.AspNetCore.Authorization;

namespace Subapp2.Controllers;

// API controller for quiz questions. 
[ApiController]
[Route("api/[controller]")]   // Base route: api/questionapi
// Management responses include correct answers, so they are restricted to administrators.
[Authorize(Roles = IdentitySeeder.AdminRole)]
// Require antiforgery tokens for state-changing requests, but not GET requests.
[AutoValidateAntiforgeryToken]
public class QuestionAPIController : ControllerBase
{
    private readonly IQuestionRepository _questionRepository;
    private readonly ILogger<QuestionAPIController> _logger;

    public QuestionAPIController(IQuestionRepository questionRepository, ILogger<QuestionAPIController> logger)
    {
        _questionRepository = questionRepository;
        _logger = logger;
    }

    // GET api/questionapi/questionlist
    // Both query parameters are optional. Without them, all questions are returned.
    [HttpGet("questionlist")]
    public async Task<IActionResult> QuestionList([FromQuery] string? search, [FromQuery] int? categoryId)
    {
        var questions = await _questionRepository.GetFilteredAsync(search, categoryId);

        // Database failures return null and produce 500; no matching questions returns an empty list. 
        // With no questions, the frontend displays "No questions match your search."
        if (questions == null)
        {

            _logger.LogError("[QuestionAPIController] Unable to load questions because the repository call failed.)");

            return StatusCode(500, "Unable to load questions. Please try again later.");
        }

        // Map database entities to DTOs before sending them to the client
        var questionDtos = questions.Select(question => new QuestionDto
        {
            QuestionId = question.QuestionId,
            Text = question.Text,
            OptionA = question.OptionA,
            OptionB = question.OptionB,
            OptionC = question.OptionC,
            OptionD = question.OptionD,
            CorrectOption = question.CorrectOption,
            CategoryId = question.CategoryId,
            CategoryName = question.Category?.Name
        });

        return Ok(questionDtos);
    }

    // GET api/questionapi/5
    // Retrieves one question, e.g. to fill in the update form.
    [HttpGet("{id}")]
    public async Task<IActionResult> GetQuestion(int id)
    {
        Question? question;

        try
        {
            question = await _questionRepository.GetByIdAsync(id);
        }
        catch (Exception)
        {
            // The repository logs the exception; keep its details out of the response.
            return StatusCode(500, "Unable to load the question. Please try again.");
        }
        if (question == null)
        {
            _logger.LogError("[QuestionAPIController] Question not found for the QuestionId {QuestionId:0000}", id);
            return NotFound("Question not found for the QuestionId");
        }

        var questionDto = new QuestionDto
        {
            QuestionId = question.QuestionId,
            Text = question.Text,
            OptionA = question.OptionA,
            OptionB = question.OptionB,
            OptionC = question.OptionC,
            OptionD = question.OptionD,
            CorrectOption = question.CorrectOption,
            CategoryId = question.CategoryId
        };

        return Ok(questionDto);
    }

    // POST api/questionapi/create
    // [ApiController] automatically returns 400 when the submitted DTO fails validation.
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] QuestionDto questionDto)
    {
        if (questionDto == null)
        {
            return BadRequest("Question cannot be null");
        }

        // Convert the DTO into a Question entity that can be saved in the database
        var newQuestion = new Question
        {
            Text = questionDto.Text,
            OptionA = questionDto.OptionA,
            OptionB = questionDto.OptionB,
            OptionC = questionDto.OptionC,
            OptionD = questionDto.OptionD,
            CorrectOption = questionDto.CorrectOption,
            CategoryId = questionDto.CategoryId
        };

        bool returnOk = await _questionRepository.AddAsync(newQuestion);
        if (returnOk)
        {
            questionDto.QuestionId = newQuestion.QuestionId; // the id created by the database
            return CreatedAtAction(nameof(GetQuestion), new { id = newQuestion.QuestionId }, questionDto);
        }

        _logger.LogWarning("[QuestionAPIController] Question creation failed {@question}", newQuestion);
        // The repository could not save the question, so report a server error.
        return StatusCode(500, "Internal server error");
    }

    // PUT api/questionapi/update/5
    [HttpPut("update/{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] QuestionDto questionDto)
    {
        if (questionDto == null)
        {
            return BadRequest("Question data cannot be null");
        }

        // Find the question in the database
        Question? existingQuestion;

        try
        {
            existingQuestion = await _questionRepository.GetByIdAsync(id);
        }
        catch (Exception)
        {
            // Loading failed, so do not attempt to update the question.
            return StatusCode(500, "Unable to load the question for editing. Please try again.");
        }
        if (existingQuestion == null)
        {
            return NotFound("Question not found");
        }

        // Update the question properties
        existingQuestion.Text = questionDto.Text;
        existingQuestion.OptionA = questionDto.OptionA;
        existingQuestion.OptionB = questionDto.OptionB;
        existingQuestion.OptionC = questionDto.OptionC;
        existingQuestion.OptionD = questionDto.OptionD;
        existingQuestion.CorrectOption = questionDto.CorrectOption;
        existingQuestion.CategoryId = questionDto.CategoryId;

        // Save the changes
        bool updateSuccessful = await _questionRepository.UpdateAsync(existingQuestion);
        if (updateSuccessful)
        {
            questionDto.QuestionId = existingQuestion.QuestionId;
            return Ok(questionDto); // Return the updated question
        }

        _logger.LogWarning("[QuestionAPIController] Question update failed {@question}", existingQuestion);
        return StatusCode(500, "Internal server error");
    }

    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var result = await _questionRepository.DeleteAsync(id);

        // The repository logs the outcome and any database exception.
        return result switch
        {
            QuestionDeleteResult.Deleted => NoContent(),
            QuestionDeleteResult.NotFound => NotFound("Question not found."),
            _ => StatusCode(500, "Unable to delete the question. Please try again.")
        };
    }
}
