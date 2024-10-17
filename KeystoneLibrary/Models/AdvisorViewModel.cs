using KeystoneLibrary.Models.DataModels.Advising;

namespace KeystoneLibrary.Models
{
    public class AdvisorViewModel
    {
        public Guid StudentId { get; set; }
        public string StudentCode { get; set; }
        public string StudentName { get; set; }
        public string AcademicLevel { get; set; }
        public string Faculty { get; set; }
        public string Department { get; set; }
        public string Curriculum { get; set; }
        public string CurriculumVersion { get; set; }
        public int CreditComplete { get; set; }
        public int? CreditEarned { get; set; }
        public int? CreditTransfer { get; set; }
        public decimal GPA { get; set; }
        public long RegistrationTermId { get; set; }
        public string RegistrationTerm { get; set; }
        public List<RegistrationCourseDetail> RegistrationCourses { get; set; } = new List<RegistrationCourseDetail>();
        public int RegistrationTermTotalCredit => RegistrationCourses == null ? 0 : RegistrationCourses.Sum(x => x.Credit);
        public long CurrentTermId { get; set; }
        public string CurrentTerm { get; set; }
        public List<RegistrationCourseDetail> CurrentTermCourses { get; set; }
        public int CurrentTermTotalCredit { get; set; }
        public AdvisorDetail AdvisorDetail { get; set; } = new AdvisorDetail();
        public List<AdvisingLog> AdvisingLogs { get; set; }
        public List<AdvisingCourse> AdvisingCourses { get; set; }
        public bool IsAdvise { get; set; }
        public bool IsPayment { get; set; }
        public bool IsRegistration { get; set; }
    }

    public class AdvisorDetail
    {
        public long AdvisorId { get; set; }
        public string AdvisorName { get; set; }
        public DateTime Date { get; set; }
        public string Note { get; set; }
        public Guid StudentId { get; set; }
        public long AdvisingLogId { get; set; }
        public long? AdvisingStatusId { get; set; }
    }

    public class RegistrationCourseDetail
    {
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public string Section { get; set; }
        public int Credit { get; set; }
        public string GradeName { get; set; }
        public decimal? GradeWeight { get; set; }
    }
}