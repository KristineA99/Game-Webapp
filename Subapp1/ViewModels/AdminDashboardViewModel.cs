namespace Subapp1.ViewModels
{
    // Contains the statistics display and shortcuts for the admin dashboard
    public class AdminDashboardViewModel
    {
       public int CategoryCount { get; set; }
       public int QuestionCount { get; set; }
       public int QuizCount { get; set; }

       // Quizzes need at least five questions to be playable.
       public int PlayableQuizCount { get; set; }
       public int UnplayableQuizCount { get; set; }
    }
}