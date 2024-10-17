using KeystoneLibrary.Models.DataModels.Questionnaire;

namespace KeystoneLibrary.Models.Report
{
    public class QuestionnaireReportViewModel
    {
        public Criteria Criteria { get; set; }
        public string Summary { get; set; }
        public string CurrentTimeText => DateTime.UtcNow.ToString(StringFormat.ShortDateTime);
        public string TermText { get; set; }
        public string LecturerNames { get; set; }
        public string CourseCodeAndName { get; set; }
        public string SectionNumber { get; set; }
        public decimal TotalScore { get; set; }
        public string TotalScoreText => TotalScore.ToString(StringFormat.TwoDecimal);
        public int EvaluatedCount { get; set; }
        public int TakenSeat { get; set; }
        public int TotalEnrolled { get; set; }
        public List<QuestionGroup> Groups { get; set; }
    }
}