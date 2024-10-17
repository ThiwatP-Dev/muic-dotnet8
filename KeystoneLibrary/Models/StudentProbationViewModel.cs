using KeystoneLibrary.Models.DataModels;

namespace KeystoneLibrary.Models
{
    public class StudentProbationViewModel
    {
        public Criteria Criteria { get; set; }
        public List<StudentProbationDetail> Students { get; set; }
        public List<Term> Terms { get; set; }
        public long ProbationTermId { get; set; }
        public long ProbationId { get; set; }
        public long RetireId { get; set; }
    }
    public class StudentProbationDetail
    {
        public long StudentProbationId { get; set; }
        public bool IsCheck { get; set; }
        public bool IsSendEmail { get; set; }
        public Guid StudentId { get; set; }
        public string StudentCode { get; set; }
        public string StudentTitle { get; set; }
        public string StudentFirstName { get; set; }
        public string StudentMidName { get; set; }
        public string StudentLastName { get; set; }
        public string FacultyCode { get; set; }
        public string FacultyName { get; set; }
        public string DepartmentCode { get; set; }
        public string DepartmentName { get; set; }
        public int Credit { get; set; }
        public string CurriculumVersionNameEn { get; set; }
        public decimal StudentGPA { get; set; }
        public string StudentEmail { get; set; }
        public int ProbationTime { get; set; }
        public long AdvisorId { get; set; }
        public string AdvisorName { get; set; }
        public string AdvisorTitle { get; set; }
        public string AdvisorFirstName { get; set; }
        public string AdvisorLastName { get; set; }
        public string AdvisorEmail { get; set; }
        public long ProbationTermId { get; set; }
        public long ProbationId { get; set; }
        public long RetireId { get; set; }
        public int AcademicTerm { get; set; }
        public int AcademicYear { get; set; }
        public string ProbationName { get; set; }
        public string RetireName { get; set; }
        public string StudentStatus { get; set; }
        public string TermText => $"{ AcademicTerm }/{ AcademicYear }";
        public string CreditText => Credit.ToString(StringFormat.NumberString);
        public string StudentFullName => string.IsNullOrEmpty(StudentMidName) ? $"{ StudentFirstName } { StudentLastName }"
                                                                              : $"{ StudentFirstName } { StudentMidName } { StudentLastName }";
        public string AdvisorFullName => $"{ AdvisorFirstName } { AdvisorLastName }";
        public List<StudentProbationTermGPA> TermGPAs { get; set; }
    }

    public class StudentProbationTermGPA
    {
        public int AcademicYear { get; set; }
        public int AcademicTerm { get; set; }
        public decimal? GPA { get; set; }
        public string DisplayText { get; set; }
        public bool IsProbation { get; set; }
    }

    // public class AcademicYearAndTerm 
    // {
    //     public int AcademicYear { get; set; }
    //     public List<AcademicYearAndTermDetail> AcademicYearAndTermDetails { get; set; }
    // }

    public class AcademicYearAndTermDetail
    {
        public long TermId { get; set; }
        public int AcademicTerm { get; set; }
    }
}