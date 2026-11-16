using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Subapp1.Controllers;
using Subapp1.DAL;
using Subapp1.Models;
using Subapp1.ViewModels;

namespace Subapp1.Tests.Controllers;

public class QuizControllerTests
{
    private readonly IQuizRepository _quizRepository;
    private readonly IQuestionRepository _questionRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly QuizController _controller;

    public QuizControllerTests()
    {
        _quizRepository = Substitute.For<IQuizRepository>();
        _questionRepository = Substitute.For<IQuestionRepository>();
        _categoryRepository = Substitute.For<ICategoryRepository>();

        _categoryRepository.GetAllAsync().Returns(new List<Category>
        {
            new() { CategoryId = 1, Name = "Security" },
            new() { CategoryId = 2, Name = "Web development"}
        });

        _questionRepository.GetAllAsync().Returns(new List<Question>
        {
            new()
            {
                QuestionId = 10,
                CategoryId = 1,
                Text = "Which principle limits access to necessary permissions?"
            }
        });

        _controller = new QuizController(
            _quizRepository,
            _questionRepository,
            _categoryRepository);
    }

    // Verify that creating a quiz with no selected questions returns the form without saving.
    [Fact]
    public async Task CreatePost_WhenNoQuestionsSelected_ReturnsFormWithoutSaving()
    {
        // Arrange: keep the other inputs valid to isolate this rule.
        var model = new QuizFormViewModel
        {
            Title = "Security quiz",
            SelectedCategoryIds = new List<int> { 1 },
            SelectedQuestionIds = new List<int>()
        };

        // Act
        var result = await _controller.Create(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(model, viewResult.Model);

        var selectionState = _controller.ModelState[
            nameof(QuizFormViewModel.SelectedQuestionIds)];

        Assert.NotNull(selectionState);
        Assert.NotEmpty(selectionState.Errors);

        // Redisplaying the form must restore its available choices.
        Assert.Equal(2, model.AvailableCategories.Count);
        Assert.Single(model.AvailableQuestions);

        await _quizRepository.DidNotReceive().AddAsync(
            Arg.Any<Quiz>(),
            Arg.Any<List<int>>());
    }

    // Verify that an duplicate question IDs are rejected.
    [Fact]
    public async Task CreatePost_WhenQuestionIdsAreDuplicated_ReturnsFormWithoutSaving()
    {
        // Arrange: both IDs exist, but the same question cannot be selected twice.
        var model = new QuizFormViewModel
        {
            Title = "Security quiz",
            SelectedCategoryIds = new List<int> { 1 },
            SelectedQuestionIds = new List<int> { 10, 10 }
        };

        // Act
        var result = await _controller.Create(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(model, viewResult.Model);

        var selectionState = _controller.ModelState[
            nameof(QuizFormViewModel.SelectedQuestionIds)];

        Assert.NotNull(selectionState);
        Assert.NotEmpty(selectionState.Errors);

        Assert.Equal(2, model.AvailableCategories.Count);
        Assert.Single(model.AvailableQuestions);

        await _quizRepository.DidNotReceive().AddAsync(
            Arg.Any<Quiz>(),
            Arg.Any<List<int>>());
    }

    // Verify that an unknown question ID is rejected before saving.
    [Fact]
    public async Task CreatePost_WhenQuestionDoesNotExist_ReturnsFormWithoutSaving()
    {
        // Arrange: only question 10 exists in the test repository.
        var model = new QuizFormViewModel
        {
            Title = "Security quiz",
            SelectedCategoryIds = new List<int> { 1 },
            SelectedQuestionIds = new List<int> { 999 }
        };

        // Act
        var result = await _controller.Create(model);

        //Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(model, viewResult.Model);

        var selectionState = _controller.ModelState[
            nameof(QuizFormViewModel.SelectedQuestionIds)];

        Assert.NotNull(selectionState);
        Assert.Contains(
        selectionState.Errors,
        error => error.ErrorMessage ==
            "One or more selected questions are invalid.");

        Assert.Equal(2, model.AvailableCategories.Count);
        Assert.Single(model.AvailableQuestions);

        await _quizRepository.DidNotReceive().AddAsync(
            Arg.Any<Quiz>(),
            Arg.Any<List<int>>());
    }

    // Verify that a question belonging to an unselected category is rejected without saving.
    [Fact]
    public async Task CreatePost_WhenQuestionBelongsToUnselectedCategory_ReturnsFormWithoutSaving()
    {
        // Arrange: question 10 belongs to category 1, but only category 2 is selected.
        var model = new QuizFormViewModel
        {
            Title = "Security quiz",
            SelectedCategoryIds = new List<int> { 2 },
            SelectedQuestionIds = new List<int> { 10 }
        };

        // Act
        var result = await _controller.Create(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(model, viewResult.Model);

        var selectionState = _controller.ModelState[
            nameof(QuizFormViewModel.SelectedQuestionIds)];

        Assert.NotNull(selectionState);
        Assert.Contains(
            selectionState.Errors,
            error => error.ErrorMessage ==
                "Selected questions must belong to the selected categories.");

        Assert.Equal(2, model.AvailableCategories.Count);
        Assert.Single(model.AvailableQuestions);

        await _quizRepository.DidNotReceive().AddAsync(
            Arg.Any<Quiz>(),
            Arg.Any<List<int>>());
    }
    // Verify that valid quiz input saves the correct details and question IDs,
    // then redirects to the quiz index.
    [Fact]
    public async Task CreatePost_WhenInputIsValid_SavesQuizAndRedirectsToIndex()
    {
        // Arrange
        var model = new QuizFormViewModel
        {
            Title = "Security quiz",
            Description = "Practice security concepts.",
            SelectedCategoryIds = new List<int> { 1 },
            SelectedQuestionIds = new List<int> { 10 }
        };

        _quizRepository
            .AddAsync(Arg.Any<Quiz>(), Arg.Any<List<int>>())
            .Returns(true);

        // Act
        var result = await _controller.Create(model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(QuizController.Index), redirectResult.ActionName);
        Assert.True(_controller.ModelState.IsValid);

        await _quizRepository.Received(1).AddAsync(
            Arg.Is<Quiz>(quiz =>
                quiz.Title == model.Title &&
                quiz.Description == model.Description),
            Arg.Is<List<int>>(ids =>
                ids.Count == 1 && ids[0] == 10));
    }
}