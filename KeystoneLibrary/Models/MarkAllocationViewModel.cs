namespace KeystoneLibrary.Models
{
    public class MarkAllocationViewModel
    {
        public long CourseId { get; set; }
        public long TermId { get; set; }
        public string Course { get; set; }
        public bool IsMarkAllocation { get; set; }
        public decimal TotalScore => Details != null ? Details.Sum(x => x.Score) : 0;
        public bool IsScoreChanged { get; set; }
        public List<MarkAllocationDetail> Details { get; set; }
        public List<MarkAllocationScoreCurve> LetterCurves { get; set; }
        public List<MarkAllocationScoreCurve> PassFailCurves { get; set; }
    }

    public class MarkAllocationDetail
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Abbreviation { get; set; }
        public decimal Score { get; set; }
        public int Sequence { get; set; }
    }

    public class MarkAllocationScoreCurve
    {
        public long GradingCurveId { get; set; }
        public long? GradeTemplateId { get; set; }
        public long GradeId { get; set; }
        public long TermId { get; set; }
        public string Grade { get; set; }
        public decimal? MinScore { get; set; }
        public decimal? MaxScore { get; set; }
    }
}