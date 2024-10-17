using KeystoneLibrary.Models.DataModels.Questionnaire;

namespace KeystoneLibrary.Models
{
    public class QuestionnaireStudentViewModel
    {
        
        public Criteria Criteria { get; set; }
        public List<QuestionnaireStudentCourse> Courses { get; set; }
        
    }

    public class QuestionnaireStudentCourse
    {
        public string Term { get; set; }
        public int AcademicYear { get; set; }
        public int AcademicTerm { get; set; }
        public bool InPeriod { get; set; }
        public List<QuestionnaireStudentCourseDetail> Details { get; set; }
    }

    public class QuestionnaireStudentCourseDetail
    {
        public long RegistrationCourseId { get; set; }
        public Guid StudentId { get; set; }
        public long TermId { get; set; }
        public long CourseId { get; set; }
        public long? SectionId { get; set; }
        public long? InstructorId { get; set; }
        public string StudentCode { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public string SectionNumber { get; set; }
        public string InstructorName { get; set; }
        public Questionnaire Questionnaire { get; set; }
        public List<QuestionnaireStudentInstructorDetail> Instructors { get; set; }
    }

    public class QuestionnaireStudentInstructorDetail
    {
        public bool Response { get; set; }
        public long? InstructorId { get; set; }
        public string InstructorName { get; set; }
    }
}