using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Subapp1.DAL;
using Subapp1.ViewModels;

namespace Subapp1.Controllers
{   // Only administrators can access the dashboard
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly IQuizRepository _quizRepository;
        public AdminController(
            ICategoryRepository categoryRepository, 
            IQuestionRepository questionRepository, 
            IQuizRepository quizRepository)
        {
            _categoryRepository = categoryRepository;
            _questionRepository = questionRepository;
            _quizRepository = quizRepository;
        }

        // GET: /Admin
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Retrieve current data from the repositories
            var categories = await _categoryRepository.GetAllAsync();
            var questions = await _questionRepository.GetAllAsync();
            var quizzes = await _quizRepository.GetAllAsync();

            // Create the view model
            var viewModel = new AdminDashboardViewModel
            {
                CategoryCount = categories.Count(),
                QuestionCount = questions.Count(),
                QuizCount = quizzes.Count(),

                PlayableQuizCount = quizzes.Count(q => q.QuizQuestions.Count >= 5),
                UnplayableQuizCount = quizzes.Count(q => q.QuizQuestions.Count < 5)
            };

            return View(viewModel);
        }
    }
}