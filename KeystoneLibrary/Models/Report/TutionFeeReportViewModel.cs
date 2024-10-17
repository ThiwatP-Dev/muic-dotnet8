namespace KeystoneLibrary.Models.Report
{
    public class TuitionFeeReportViewModel
    {
        public Criteria Criteria { get; set; }
        public List<TuitionFeeReportResultViewModel> Results { get; set; }
    }

    public class TuitionFeeReportResultViewModel
    {
        public long CourseId { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public string CreditText { get; set; }
        public int Batch { get; set; }
        public string FormularName { get; set; }
        public decimal FirstAmount { get; set; }
        public decimal SecondAmount { get; set; }
    }
}