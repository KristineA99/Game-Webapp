using Microsoft.AspNetCore.Mvc;
using Subapp2.DAL;
using Subapp2.DTOs;
using Subapp2.Models;

namespace Subapp2.Controllers;

// API controller for quiz questions. 
[ApiController]
[Route("api/[controller]")]   // Base route: api/questionapi
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
    [HttpGet("questionlist")]
    public async Task<IActionResult> QuestionList()
    {
        var questions = await _questionRepository.GetAllAsync();

        // null means the database call failed (logged in the repository)
        if (questions == null)
        {
            _logger.LogError("[QuestionAPIController] Question list not found while executing _questionRepository.GetAllAsync()");
            return NotFound("Question list not found");
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
            CorrectOption = question.CorrectOption
        });

        return Ok(questionDtos);
    }

    // GET api/questionapi/5
    // Retrieves one question, e.g. to fill in the update form.
    [HttpGet("{id}")]
    public async Task<IActionResult> GetQuestion(int id)
    {
        var question = await _questionRepository.GetByIdAsync(id);
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
        // returns 400 Bad Request if the DTO breaks its validation attributes (e.g. empty text or no category chosen)
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
        var existingQuestion = await _questionRepository.GetByIdAsync(id);
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

    // DELETE api/questionapi/delete/5
    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        bool returnOk = await _questionRepository.DeleteAsync(id);
        if (!returnOk)
        {
            _logger.LogError("[QuestionAPIController] Question deletion failed for the QuestionId {QuestionId:0000}", id);
            return BadRequest("Question deletion failed");
        }

        // No Content: the question is deleted and there is nothing more to send back
        return NoContent();
    }
}
