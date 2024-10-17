using KeystoneLibrary.Models.DataModels;

namespace KeystoneLibrary.Models
{
    public class GraduatingStudentReportViewModel
    {
        public Guid StudentId { get; set; }
        public string StudentCode { get; set; }
        public string StudentFullName { get; set; }
        public string Faculty { get; set; }
        public string Department { get; set; }
        public string Curriculum { get; set; }
        public string CurriculumVersion { get; set; }
        public int CreditEarned { get; set; }
        public int ExpectedCredit { get; set; }
        public int TotalCreditCurriculum { get; set; }
        public List<RegistrationCourse> RegistrationCourses { get; set; } = new List<RegistrationCourse>();
        public int TotalRegistrationCredit { get; set; }
        public int TotalCreditEarn => CreditEarned + TotalRegistrationCredit;
        public int TotalCreditEarnWithExpected => CreditEarned + TotalRegistrationCredit + ExpectedCredit;
    }
}