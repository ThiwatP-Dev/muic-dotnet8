using KeystoneLibrary.Models.DataModels;

namespace KeystoneLibrary.Models
{
    public class GradeMaintenanceViewModel
    {
        public Criteria Criteria { get; set; }
        public string StudentFullName { get; set; }
        public string CurriculumName { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public string SectionNumber { get; set; }
        public long StudentScoreId { get; set; }
        public string Grade { get; set; }
        public List<GradingInformation> GradingInfo { get; set; }
    }

    public class GradingInformation
    {
        public string GradesInTemplate { get; set; }
        public long StudentScoreId { get; set; }
        public long StudentRawScoreId { get; set; }
        public Course Course { get; set; }
        public Section Section { get; set; }
        public string PreviousGrade { get; set; }
        public long? CurrentGradeId { get; set; }
        public string Remark { get; set; }
        public string StudentCode { get; set; }
        public long TermId { get; set; }
        public string SectionNumber { get; set; }
        public Instructor MainInstructor { get; set; }
        public bool IsGradePublished { get; set; }
        public long RegistrationCourseId { get; set; }
        public List<GradingLog> GradingLogs { get; set; }
        public long AcademicLevelId { get; set; }
        public IFormFile UploadFile { get; set; }
    }
}