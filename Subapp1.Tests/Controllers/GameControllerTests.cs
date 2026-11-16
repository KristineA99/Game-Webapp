using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Subapp1.Controllers;
using Subapp1.DAL;
using Subapp1.Models;
using Subapp1.ViewModels;
using System.Text.Json;
using System.Security.Claims;

namespace Subapp1.Tests.Controllers;

public class GameControllerTests
{
    private readonly IQuestionRepository _questionRepository;
    private readonly IQuizRepository _quizRepository;
    private readonly IGameResultRepository _gameResultRepository;
    private readonly GameController _controller;
    private readonly ISession _session;

    public GameControllerTests()
    {
        _questionRepository = Substitute.For<IQuestionRepository>();
        _quizRepository = Substitute.For<IQuizRepository>();
        _gameResultRepository = Substitute.For<IGameResultRepository>();

        _session = new TestSession();

        _controller = new GameController(
            _questionRepository,
            _quizRepository,
            _gameResultRepository);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                Session = _session
            }
        };
    }

    // Verify that a correct answer adds one point and marks the question completed.
    [Fact]
    public async Task Answer_WhenCorrect_IncreasesScoreAndCompletesQuestion()
    {
        // Arrange: simulate a game waiting for an answer to question 10.
        var question = new Question
        {
            QuestionId = 10,
            CorrectOption = 2
        };

        _questionRepository.GetByIdAsync(10).Returns(question);

        _session.SetInt32("CurrentQuestionId", 10);
        _session.SetString("AwaitingAnswer", "true");
        _session.SetInt32("Score", 2);
        _session.SetString("CompletedQuestionIds", "[]");

        // Act
        var result = await _controller.Answer(
            questionId: 10,
            selectedOption: 2);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Play", viewResult.ViewName);

        var model = Assert.IsType<GameViewModel>(viewResult.Model);
        Assert.Same(question, model.Question);
        Assert.Equal(true, model.IsCorrect);
        Assert.Equal(3, model.Score);

        Assert.Equal(3, _session.GetInt32("Score"));
        Assert.Equal("false", _session.GetString("AwaitingAnswer"));

        var completedIds = JsonSerializer.Deserialize<List<int>>(
            _session.GetString("CompletedQuestionIds")!);

        Assert.NotNull(completedIds);
        Assert.Equal(10, Assert.Single(completedIds));
    }
    // Verify that an incorrect answer keeps the score unchanged
    // and allows the player to continue to another question.
    [Fact]
    public async Task Answer_WhenIncorrect_KeepsScoreAndAllowsProgression()
    {
        // Arrange
        var question = new Question
        {
            QuestionId = 10,
            CorrectOption = 2
        };

        var nextQuestion = new Question
        {
            QuestionId = 20,
            CorrectOption = 1
        };

        _questionRepository.GetByIdAsync(10).Returns(question);
        _questionRepository.GetRandomAsync(
            Arg.Is<List<int>>(ids => ids.Count == 1 && ids[0] == 10),
            7).Returns(nextQuestion);

        _session.SetInt32("SelectedQuizId", 7);
        _session.SetInt32("CurrentQuestionId", 10);
        _session.SetString("AwaitingAnswer", "true");
        _session.SetInt32("Score", 2);
        _session.SetString("CompletedQuestionIds", "[]");

        // Act: submit an incorrect option.
        var answerResult = await _controller.Answer(
            questionId: 10,
            selectedOption: 1);

        // Assert: the answer is completed without awarding a point.
        var answerView = Assert.IsType<ViewResult>(answerResult);
        var answerModel = Assert.IsType<GameViewModel>(answerView.Model);

        Assert.Equal(false, answerModel.IsCorrect);
        Assert.Equal(2, answerModel.Score);
        Assert.Equal(2, _session.GetInt32("Score"));
        Assert.Equal("false", _session.GetString("AwaitingAnswer"));

        var completedIds = JsonSerializer.Deserialize<List<int>>(
            _session.GetString("CompletedQuestionIds")!);

        Assert.NotNull(completedIds);
        Assert.Equal(10, Assert.Single(completedIds));

        // Act: continue using the same session.
        var continueResult = await _controller.Continue();

        // Assert: the next question is ready, with the score preserved.
        var nextView = Assert.IsType<ViewResult>(continueResult);
        Assert.Equal("Play", nextView.ViewName);

        var nextModel = Assert.IsType<GameViewModel>(nextView.Model);
        Assert.Same(nextQuestion, nextModel.Question);
        Assert.Equal(2, nextModel.Score);
        Assert.Equal(20, _session.GetInt32("CurrentQuestionId"));
        Assert.Equal("true", _session.GetString("AwaitingAnswer"));
    }

    // Verify that submitting the same answer twice does not award another point.
    [Fact]
    public async Task Answer_WhenSubmittedTwice_DoesNotScoreAgain()
    {
        // Arrange
        var question = new Question
        {
            QuestionId = 10,
            CorrectOption = 2
        };

        _questionRepository.GetByIdAsync(10).Returns(question);

        _session.SetInt32("CurrentQuestionId", 10);
        _session.SetString("AwaitingAnswer", "true");
        _session.SetInt32("Score", 2);
        _session.SetString("CompletedQuestionIds", "[]");

        // Act: accept the first answer.
        var firstResult = await _controller.Answer(
            questionId: 10,
            selectedOption: 2);

        // Assert: the first submission earns one point.
        Assert.IsType<ViewResult>(firstResult);
        Assert.Equal(3, _session.GetInt32("Score"));

        // Act: repeat the submission without resetting the session.
        var repeatedResult = await _controller.Answer(
            questionId: 10,
            selectedOption: 2);

        // Assert: the repeated answer is rejected before retrieving the question.
        var redirect = Assert.IsType<RedirectToActionResult>(repeatedResult);
        Assert.Equal(nameof(GameController.Play), redirect.ActionName);

        Assert.Equal(3, _session.GetInt32("Score"));
        Assert.Equal("false", _session.GetString("AwaitingAnswer"));

        var completedIds = JsonSerializer.Deserialize<List<int>>(
            _session.GetString("CompletedQuestionIds")!);

        Assert.NotNull(completedIds);
        Assert.Equal(10, Assert.Single(completedIds));

        await _questionRepository.Received(1).GetByIdAsync(10);
    }

    // Verify that completing an authenticated game saves the correct result
    // and repeating Continue does not save it again.
    [Fact]
    public async Task Continue_WhenGameCompleted_SavesResultOnlyOnce()
    {
        // Arrange: identify the logged-in player.
        _controller.HttpContext.User = new ClaimsPrincipal(
            new ClaimsIdentity(
                new[]
                {
                new Claim(ClaimTypes.NameIdentifier, "user-123"),
                new Claim(ClaimTypes.Name, "Test Player")
                },
                authenticationType: "Test"));

        _session.SetInt32("SelectedQuizId", 7);
        _session.SetString("QuizName", "Security quiz");
        _session.SetInt32("Score", 3);
        _session.SetInt32("TotalQuestions", 5);
        _session.SetString("CompletedQuestionIds", "[10,20,30,40,50]");
        _session.SetString("AwaitingAnswer", "false");
        _session.SetString(
            "GameStartedAt",
            DateTime.UtcNow.AddMinutes(-1).ToString("O"));

        // No unanswered questions remain in this quiz.
        _questionRepository.GetRandomAsync(
            Arg.Any<List<int>>(),
            7).Returns((Question?)null);

        _gameResultRepository
            .AddAsync(Arg.Any<GameResult>())
            .Returns(true);

        // Act: finish the game.
        var result = await _controller.Continue();

        // Assert: saving succeeds before the result page is shown.
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(GameController.Result), redirect.ActionName);
        Assert.Equal("true", _session.GetString("ResultSaved"));
        Assert.Equal("true", _session.GetString("HasWon"));

        await _gameResultRepository.Received(1).AddAsync(
            Arg.Is<GameResult>(saved =>
                saved.UserId == "user-123" &&
                saved.PlayerName == "Test Player" &&
                saved.QuizId == 7 &&
                saved.QuizName == "Security quiz" &&
                saved.Score == 3 &&
                saved.TotalQuestions == 5 &&
                saved.DurationSeconds >= 0));

        // Act: repeat completion using the same session.
        var repeatedResult = await _controller.Continue();

        // Assert: Continue has already been consumed, so no second save occurs.
        var repeatedRedirect =
            Assert.IsType<RedirectToActionResult>(repeatedResult);

        Assert.Equal(
            nameof(GameController.Play),
            repeatedRedirect.ActionName);

        await _gameResultRepository.Received(1)
            .AddAsync(Arg.Any<GameResult>());
    }

    // Keep session writes available to later reads and repeated controller calls.
    private sealed class TestSession : ISession
    {
        private readonly Dictionary<string, byte[]> _values = new();

        public string Id { get; } = Guid.NewGuid().ToString();
        public bool IsAvailable => true;
        public IEnumerable<string> Keys => _values.Keys;

        public void Clear() => _values.Clear();

        public void Remove(string key) => _values.Remove(key);

        public void Set(string key, byte[] value) => _values[key] = value;

        public bool TryGetValue(string key, out byte[] value)
        {
            return _values.TryGetValue(key, out value!);
        }

        public Task LoadAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task CommitAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}