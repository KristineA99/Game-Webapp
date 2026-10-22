using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Subapp1.Controllers;
using Subapp1.DAL;
using Subapp1.Models;

namespace Subapp1.Tests.Controllers;

// Unit tests for QuestionController.
// The repository and logger are replaced by substitutes (mocks),
// so the controller is tested in isolation without a real database.
public class QuestionControllerTests
{
    private readonly IQuestionRepository _mockQuestionRepository;
    private readonly ILogger<QuestionController> _mockLogger;
    private readonly QuestionController _questionController;

    // xUnit creates a new instance of this class for every test,
    // so each test starts with fresh mocks and a fresh controller.
    public QuestionControllerTests()
    {
        _mockQuestionRepository = Substitute.For<IQuestionRepository>();
        _mockLogger = Substitute.For<ILogger<QuestionController>>();
        _questionController = new QuestionController(_mockQuestionRepository, _mockLogger);

        // The controller writes to TempData after successful actions.
        // TempData is normally created by ASP.NET, so we create it manually here.
        _questionController.TempData = new TempDataDictionary(
            new DefaultHttpContext(),
            Substitute.For<ITempDataProvider>());
    }

    // Helper: creates a valid question used in several tests
    private static Question CreateTestQuestion()
    {
        return new Question
        {
            QuestionId = 1,
            CategoryId = 1,
            Text = "Which part of the CIA triad protects information from unauthorized disclosure?",
            OptionA = "Confidentiality",
            OptionB = "Integrity",
            OptionC = "Availability",
            OptionD = "Authentication",
            CorrectOption = 1
        };
    }

    // ===================== POSITIVE TESTS =====================

    // PT-01
    [Fact]
    public async Task Index_WhenQuestionsExist_ReturnsViewWithQuestions()
    {
        // Arrange
        var questionList = new List<Question>
        {
            CreateTestQuestion(),
            new Question
            {
                QuestionId = 2,
                CategoryId = 1,
                Text = "Which security principle gives users only the permissions they need?",
                OptionA = "Defense in depth",
                OptionB = "Least privilege",
                OptionC = "Separation of duties",
                OptionD = "Availability",
                CorrectOption = 2
            }
        };
        _mockQuestionRepository.GetAllAsync().Returns(questionList);

        // Act
        var result = await _questionController.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<Question>>(viewResult.ViewData.Model);
        Assert.Equal(2, model.Count());
        Assert.Equal(questionList, model);
    }

    // PT-02
    [Fact]
    public async Task Create_WhenModelIsValid_SavesQuestionAndRedirectsToIndex()
    {
        // Arrange
        var testQuestion = CreateTestQuestion();

        // Act
        var result = await _questionController.Create(testQuestion);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(QuestionController.Index), redirectResult.ActionName);
        await _mockQuestionRepository.Received(1).AddAsync(testQuestion);
    }

    // PT-03
    [Fact]
    public async Task Update_WhenQuestionExists_ReturnsViewWithQuestion()
    {
        // Arrange
        var testQuestion = CreateTestQuestion();
        _mockQuestionRepository.GetByIdAsync(1).Returns(testQuestion);

        // Act
        var result = await _questionController.Update(1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var viewQuestion = Assert.IsAssignableFrom<Question>(viewResult.ViewData.Model);
        Assert.Equal(testQuestion, viewQuestion);
    }

    // PT-04
    [Fact]
    public async Task UpdatePost_WhenUpdateSucceeds_RedirectsToIndex()
    {
        // Arrange
        var testQuestion = CreateTestQuestion();
        _mockQuestionRepository.UpdateAsync(testQuestion).Returns(true);

        // Act
        var result = await _questionController.Update(testQuestion);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(QuestionController.Index), redirectResult.ActionName);
    }

    // PT-05
    [Fact]
    public async Task Delete_WhenQuestionExists_ReturnsViewWithQuestion()
    {
        // Arrange
        var testQuestion = CreateTestQuestion();
        _mockQuestionRepository.GetByIdAsync(1).Returns(testQuestion);

        // Act
        var result = await _questionController.Delete(1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var viewQuestion = Assert.IsAssignableFrom<Question>(viewResult.ViewData.Model);
        Assert.Equal(testQuestion, viewQuestion);
    }

    // PT-06
    [Fact]
    public async Task DeleteConfirmed_WhenDeleteSucceeds_RedirectsToIndex()
    {
        // Arrange
        _mockQuestionRepository.DeleteAsync(1).Returns(true);

        // Act
        var result = await _questionController.DeleteConfirmed(1);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(QuestionController.Index), redirectResult.ActionName);
    }

    // ===================== NEGATIVE TESTS =====================

    // NT-01
    [Fact]
    public async Task Create_WhenModelIsInvalid_ReturnsViewWithSameQuestionAndDoesNotSave()
    {
        // Arrange
        var testQuestion = CreateTestQuestion();
        testQuestion.Text = string.Empty;
        // Validation does not run automatically in unit tests, so we simulate the error
        _questionController.ModelState.AddModelError("Text", "The question text is required.");

        // Act
        var result = await _questionController.Create(testQuestion);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var viewQuestion = Assert.IsAssignableFrom<Question>(viewResult.ViewData.Model);
        Assert.Equal(testQuestion, viewQuestion);
        await _mockQuestionRepository.DidNotReceive().AddAsync(Arg.Any<Question>());
    }

    // NT-02
    [Fact]
    public async Task Update_WhenQuestionDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        _mockQuestionRepository.GetByIdAsync(99).Returns((Question?)null);

        // Act
        var result = await _questionController.Update(99);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    // NT-03
    [Fact]
    public async Task UpdatePost_WhenModelIsInvalid_ReturnsViewWithSameQuestionAndDoesNotSave()
    {
        // Arrange
        var testQuestion = CreateTestQuestion();
        testQuestion.CorrectOption = 0;
        _questionController.ModelState.AddModelError("CorrectOption", "Choose which option is correct.");

        // Act
        var result = await _questionController.Update(testQuestion);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var viewQuestion = Assert.IsAssignableFrom<Question>(viewResult.ViewData.Model);
        Assert.Equal(testQuestion, viewQuestion);
        await _mockQuestionRepository.DidNotReceive().UpdateAsync(Arg.Any<Question>());
    }

    // NT-04
    [Fact]
    public async Task UpdatePost_WhenQuestionDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var testQuestion = CreateTestQuestion();
        _mockQuestionRepository.UpdateAsync(testQuestion).Returns(false);

        // Act
        var result = await _questionController.Update(testQuestion);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    // NT-05
    [Fact]
    public async Task Delete_WhenQuestionDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        _mockQuestionRepository.GetByIdAsync(99).Returns((Question?)null);

        // Act
        var result = await _questionController.Delete(99);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    // NT-06
    [Fact]
    public async Task DeleteConfirmed_WhenDeleteFails_ReturnsNotFound()
    {
        // Arrange
        _mockQuestionRepository.DeleteAsync(99).Returns(false);

        // Act
        var result = await _questionController.DeleteConfirmed(99);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }
}