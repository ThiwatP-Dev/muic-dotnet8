namespace KeystoneLibrary.Models
{
    public class SchoolRevenueSummaryViewModel
    {
        public Criteria Criteria { get; set; }
        public List<SchoolRevenueSummaryResult> Results { get; set; }
    }

    public class SchoolRevenueSummaryResult
    {
        public string FacultyName { get; set; }
        public string FeeName { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public string CodeAndName => $"{ CourseCode } { CourseName }";
        public string StudentCode { get; set; }
        public string StudentName { get; set; }
        public string StudentCodeAndName => $"{ StudentCode } { StudentName }";
        public decimal Amount { get; set; }
    }
}