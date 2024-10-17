using KeystoneLibrary.Models.DataModels;

namespace KeystoneLibrary.Models
{
    public class CheatingStatisticReportViewModel
    {
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public List<Term> TermHeader { get; set; }
        public string Faculty { get; set; }
        public string Batch { get; set; }
        public string TermText { get; set; }
        public Criteria Criteria { get; set; }
        public List<CheatingStatisticReportCount> StatisticCheatingFaculties { get; set; }
        public List<CheatingStatisticReportCount> StatisticCheatingBatches { get; set; }
        public List<CheatingStatisticReportCount> StatisticCheatingTerms { get; set; }
    }

    public class CheatingStatisticReportCount
    {
        public string Term { get; set; }
        public int StudentCount { get; set; }
    }
}