using KeystoneLibrary.Models.DataModels;

namespace KeystoneLibrary.Models.Report
{
    public class QuestionnaireCourseGroupReportViewModel
    {
        public Criteria Criteria { get; set; }
        public List<QuestionnaireCourseGroupDetail> Details { get; set; }
        public decimal TotalEvaluated => Details.Sum(x => x.EvaluatedCount);
        public decimal TotalRelatedSectionScore => Details.Sum(x => x.RelatedSectionScore);        
        public string TotalScore { get; set; }
    }

    public class QuestionnaireCourseGroupDetail
    {
        public long SectionId { get; set; }
        public string Course { get; set; }
        public string Section { get; set; }
        public string Instructor { get; set; }
        public decimal IndividualTotalScore { get; set; }
        public decimal RelatedSectionScore { get; set; }
        public int EvaluatedCount { get; set; }
        public int TakenSeat { get; set; }
        public Section ParentSection { get; set; }
        public List<Section> JointSections { get; set; }
        public string IndividualTotalScoreText => IndividualTotalScore.ToString(StringFormat.TwoDecimal);
        public string RelatedSectionScoreText => RelatedSectionScore.ToString(StringFormat.TwoDecimal);
    }
}