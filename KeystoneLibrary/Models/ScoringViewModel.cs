using KeystoneLibrary.Models.DataModels;

namespace KeystoneLibrary.Models
{
    public class ScoringViewModel
    {
        public bool IsMarkAllocation { get; set; }
        public long SectionId { get; set; }
        public long CourseId { get; set; }
        public long TermId { get; set; }
        public string CourseAndCredit { get; set; }
        public string CourseAndCreditJoint { get; set; }
        public string SectionNumber { get; set; }
        public bool IsBarcode { get; set; }
        public int ScoreStudent { get; set; }
        public int GradeAuOrIStudent { get; set; }
        public int Skip { get; set; }
        public int PublishedStudent { get; set; }
        public int WithdrawnStudent { get; set; }
        public int TotalStudent { get; set; }
        public DateTime? LastGradeDate { get; set; }
        public DateTime? SubmitDate { get; set; }
    }

    public class EditScoringViewModel
    {
        public long CourseId { get; set; }
        public long TermId { get; set; }
        public bool IsNext { get; set; }
        public string CourseAndCredit { get; set; }
        public string MainInstructorFullNameEn { get; set; }
        public string TermText { get; set; }
        public decimal TotalAllocationScore => Allocations.Sum(x => x.Score);
        public List<MarkAllocation> Allocations { get; set; } = new List<MarkAllocation>();
        public List<StudentRawScoreViewModel> StudentRawScores { get; set; }
        public List<SectionSearchViewModel> Sections { get; set; }
        public IFormFile UploadFile { get; set; }
    }

    public class ImportScoringViewModel
    {
        public long CourseId { get; set; }
        public long TermId { get; set; }
        public bool IsNext { get; set; }
        public string CourseAndCredit { get; set; }
        public string MainInstructorFullNameEn { get; set; }
        public string TermText { get; set; }
        public decimal TotalAllocationScore => Allocations.Sum(x => x.Score);
        public List<MarkAllocation> Allocations { get; set; } = new List<MarkAllocation>();
        public List<StudentRawScoreViewModel> ImportSuccess { get; set; }
        public List<ImportScoreFailViewModel> ImportWarning { get; set; }
        public List<ImportScoreFailViewModel> ImportFail { get; set; }
        public IFormFile UploadFile { get; set; }
    }

    public class ImportScoreFailViewModel
    {
        public string StudentCode { get; set; }
        public string SectionNumber { get; set; }
        public string CourseCode { get; set; }
        public string Message { get; set; } 
    }

    public class SectionSearchViewModel
    {
        public long SectionId { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public string SectionNumber { get; set; }
        public int TotalStudent { get; set; }
        public int TotalWithdrawn { get; set; }
        public bool IsSelected { get; set; }
    }

    public class StudentRawScoreViewModel
    {
        public long Id { get; set; }
        public Guid StudentId { get; set; }
        public string StudentCode  { get; set; }
        public string StudentTitle { get; set; }
        public string StudentFirstNameEn { get; set; }
        public string StudentLastNameEn { get; set; }
        public string StudentMidNameEn { get; set; }
        public long CourseId { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public string CourseCredit { get; set; }
        public long SectionId { get; set; }
        public string SectionNumber { get; set; }
        public string SectionType { get; set; }
        public bool IsPaid { get; set; }
        public bool IsWithdrawal { get; set; }
        public bool IsGradePublish { get; set; }
        public string GradeName { get; set; }
        public long? GradeId { get; set; }
        public decimal GradeWeight { get; set; }
        public bool IsCalcGrade { get; set; }
        public int Credit { get; set; }  
        public long RegistrationCourseId { get; set; }
        public decimal? TotalScore { get; set; }
        public decimal? Percentage { get; set; }
        public bool IsSkipGrading { get; set; }
        public string DepartmentCode { get; set; }
        public long? GradeTemplateId { get; set; }
        public long? BarcodeId { get; set; }
        public List<StudentRawScoreDetail> StudentRawScoreDetails { get; set; } = new List<StudentRawScoreDetail>();

        public string StudentName => string.IsNullOrEmpty(StudentMidNameEn) ? $"{ StudentTitle } { StudentFirstNameEn } { StudentLastNameEn }"
                                                                            : $"{ StudentTitle } { StudentFirstNameEn } { StudentMidNameEn } { StudentLastNameEn }";
    }
    
}