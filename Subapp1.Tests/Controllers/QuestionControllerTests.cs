using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Subapp1.Controllers;
using Subapp1.DAL;
using Subapp1.Models;
using Subapp1.ViewModels;

namespace Subapp1.Tests.Controllers;

// Unit tests for QuestionController.
// The repositories and logger are replaced by substitutes (mocks),
// so the controller is tested in isolation without a real database.
public class QuestionControllerTests
{
    private readonly IQuestionRepository _mockQuestionRepository;
    private readonly ICategoryRepository _mockCategoryRepository;
    private readonly ILogger<QuestionController> _mockLogger;
    private readonly QuestionController _questionController;

    // xUnit creates a new instance of this class for every test,
    // so each test starts with fresh mocks and a fresh controller.
    public QuestionControllerTests()
    {
        _mockQuestionRepository = Substitute.For<IQuestionRepository>();
        _mockCategoryRepository = Substitute.For<ICategoryRepository>();
        _mockLogger = Substitute.For<ILogger<QuestionController>>();

        // Categories used to fill the dropdown in the question form
        _mockCategoryRepository.GetAllAsync().Returns(new List<Category>
        {
            new Category { CategoryId = 1, Name = "Security" },
            new Category { CategoryId = 2, Name = "Web development" }
        });
        // Category 1 exists, so ValidateCategoryAsync accepts it
        _mockCategoryRepository.GetByIdAsync(1).Returns(new Category { CategoryId = 1, Name = "Security" });

        _questionController = new QuestionController(
            _mockQuestionRepository, _mockLogger, _mockCategoryRepository);

        // The controller writes to TempData after successful actions.
        // TempData is normally created by ASP.NET, so we create it manually here.
        _questionController.TempData = new TempDataDictionary(
            new DefaultHttpContext(),
            Substitute.For<ITempDataProvider>());
    }

    // Helper: a question entity as it is stored in the database
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

    // Helper: a valid form model as submitted from the Create/Update form
    private static QuestionFormViewModel CreateValidFormModel()
    {
        return QuestionFormViewModel.FromQuestion(CreateTestQuestion());
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
    public async Task CreatePost_WhenModelIsValid_SavesQuestionAndRedirectsToIndex()
    {
        // Arrange
        var model = CreateValidFormModel();

        // Act
        var result = await _questionController.Create(model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(QuestionController.Index), redirectResult.ActionName);
        // ToQuestion() creates a new object, so we match on its content
        await _mockQuestionRepository.Received(1).AddAsync(
            Arg.Is<Question>(q => q.Text == model.Text && q.CategoryId == model.CategoryId));
    }

    // PT-03
    [Fact]
    public async Task Update_WhenQuestionExists_ReturnsViewWithFormModelAndCategories()
    {
        // Arrange
        var testQuestion = CreateTestQuestion();
        _mockQuestionRepository.GetByIdAsync(1).Returns(testQuestion);

        // Act
        var result = await _questionController.Update(1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<QuestionFormViewModel>(viewResult.ViewData.Model);
        Assert.Equal(testQuestion.QuestionId, model.QuestionId);
        Assert.Equal(testQuestion.Text, model.Text);
        Assert.Equal(2, model.AvailableCategories.Count);
    }

    // PT-04
    [Fact]
    public async Task UpdatePost_WhenUpdateSucceeds_RedirectsToIndex()
    {
        // Arrange
        var model = CreateValidFormModel();
        _mockQuestionRepository.UpdateAsync(Arg.Any<Question>()).Returns(true);

        // Act
        var result = await _questionController.Update(model);

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
    public async Task CreatePost_WhenModelIsInvalid_ReturnsFormAgainAndDoesNotSave()
    {
        // Arrange
        var model = CreateValidFormModel();
        model.Text = string.Empty;
        // Validation does not run automatically in unit tests, so we simulate the error
        _questionController.ModelState.AddModelError(nameof(model.Text), "The question text is required.");

        // Act
        var result = await _questionController.Create(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsAssignableFrom<QuestionFormViewModel>(viewResult.ViewData.Model);
        Assert.Same(model, viewModel);                      // the user's input is kept
        Assert.Equal(2, viewModel.AvailableCategories.Count); // the dropdown is filled again
        await _mockQuestionRepository.DidNotReceive().AddAsync(Arg.Any<Question>());
    }

    // NT-02
    [Fact]
    public async Task CreatePost_WhenCategoryDoesNotExist_ReturnsFormWithCategoryError()
    {
        // Arrange
        var model = CreateValidFormModel();
        model.CategoryId = 99;
        _mockCategoryRepository.GetByIdAsync(99).Returns((Category?)null);

        // Act
        var result = await _questionController.Create(model);

        // Assert
        Assert.IsType<ViewResult>(result);
        Assert.False(_questionController.ModelState.IsValid);
        Assert.True(_questionController.ModelState.ContainsKey(nameof(QuestionFormViewModel.CategoryId)));
        await _mockQuestionRepository.DidNotReceive().AddAsync(Arg.Any<Question>());
    }

    // NT-03
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

    // NT-04
    [Fact]
    public async Task UpdatePost_WhenQuestionDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var model = CreateValidFormModel();
        _mockQuestionRepository.UpdateAsync(Arg.Any<Question>()).Returns(false);

        // Act
        var result = await _questionController.Update(model);

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