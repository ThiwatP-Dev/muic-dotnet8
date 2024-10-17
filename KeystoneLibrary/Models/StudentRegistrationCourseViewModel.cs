using KeystoneLibrary.Models.DataModels;

namespace KeystoneLibrary.Models
{
    public class StudentRegistrationCourseViewModel
    {
        public string Term { get; set; }
        public int AcademicYear { get; set; }
        public int AcademicTerm { get; set; }
        public bool IsSummer { get; set; }
        public List<RegistrationCourse> RegistrationCourses { get; set; } = new List<RegistrationCourse>();
        public List<RegistrationLog> RegistrationLogs { get; set; } = new List<RegistrationLog>();
        public long TermId { get; set; }
        public decimal TotalCredit { get; set; }
        public decimal TotalGPTS { get; set; }
        public string TotalGPTSText => TotalGPTS.ToString(StringFormat.TwoDecimal);
        public decimal CumulativeCreditComp { get; set; }
        public decimal CumulativeCreditRegis { get; set; }
        public string CumulativeCreditCompText => CumulativeCreditComp.ToString(StringFormat.NumberString);
        public string CumulativeCreditRegisText => CumulativeCreditRegis.ToString(StringFormat.NumberString);
        public decimal TotalRegistrationCredit { get; set; }
        public decimal TotalRegistrationCreditFromCredit { get; set; }
        public string TotalCreditText => TotalCredit.ToString(StringFormat.NumberString);
        public string TotalRegistrationCreditText => TotalRegistrationCredit.ToString(StringFormat.NumberString);
        public string TotalRegistrationCreditFromCreditText => TotalRegistrationCreditFromCredit.ToString(StringFormat.NumberString);
        public decimal CumulativeGTPS { get; set; }
        public string CumulativeGTPSText => CumulativeGTPS.ToString(StringFormat.TwoDecimal);
        public decimal CumulativeGPA { get; set; }
        public string CumulativeGPAText => CumulativeGPA.ToString(StringFormat.TwoDecimal);
        public decimal TermGPA { get; set; }
        public string TermGPAText => TermGPA.ToString(StringFormat.TwoDecimal);
    }
}