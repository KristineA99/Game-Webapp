using Microsoft.AspNetCore.Mvc;
using Subapp1.DAL;
using Subapp1.DTOs;

namespace Subapp1.Controllers;

// API controller for quiz questions. Returns JSON instead of HTML views.
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
}