namespace KeystoneLibrary.Models
{
    public class DismissStudentViewModel
    {
        public long Id { get; set; }
        public Guid StudentId { get; set; }
        public long AcademicLevelId { get; set; }
        public long TermId { get; set; }
        public long ProbationId { get; set; }
        public string Code { get; set; }
        public string FullName { get; set; }
        public string AcademicLevel { get; set; }
        public string Faculty { get; set; }
        public string Department { get; set; }
        public string Curriculum { get; set; }
        public string CurriculumVersion { get; set; }
        public string Term { get; set; }
        public string Probation { get; set; }
        public decimal GPA { get; set; }
        public int CreditEarned { get; set; }
        public string Advisor { get; set; }
        public string Remark { get; set; }
        public List<DismissAcademicYearAndTerm> Terms { get; set; }
        public List<StudentDismissTermGPA> TermGPAs { get; set; }
    }

    public class StudentDismissTermGPA
    {
        public int AcademicYear { get; set; }
        public int AcademicTerm { get; set; }
        public decimal? GPA { get; set; }
    }

    public class DismissAcademicYearAndTerm 
    {
        public int AcademicYear { get; set; }
        public List<DismissAcademicYearAndTermDetail> AcademicYearAndTermDetails { get; set; }
    }

    public class DismissAcademicYearAndTermDetail
    {
        public long TermId { get; set; }
        public int AcademicTerm { get; set; }
    }
}