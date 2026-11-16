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
            new() { CategoryId = 1, Name = "Security" }
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
        Assert.Single(model.AvailableCategories);
        Assert.Single(model.AvailableQuestions);

        await _quizRepository.DidNotReceive().AddAsync(
            Arg.Any<Quiz>(),
            Arg.Any<List<int>>());
    }

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

        Assert.Single(model.AvailableCategories);
        Assert.Single(model.AvailableQuestions);

        await _quizRepository.DidNotReceive().AddAsync(
            Arg.Any<Quiz>(),
            Arg.Any<List<int>>());
    }
}