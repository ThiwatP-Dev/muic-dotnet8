namespace KeystoneLibrary.Models
{
    public class StudentGPAViewModel
    {
        public decimal GPA { get; set; }
        public decimal CumulativeGPTS { get; set; }
        public int CreditComp { get; set; }
    }

    public class StudentRegistrationCoursesViewModel
    {
        public List<StudentRegistrationCourseViewModel> TransferCourse { get; set; } = new List<StudentRegistrationCourseViewModel>();
        public List<StudentRegistrationCourseViewModel> TransferCourseWithGrade { get; set; } = new List<StudentRegistrationCourseViewModel>();
        public List<StudentRegistrationCourseViewModel> TranscriptGrade { get; set; } = new List<StudentRegistrationCourseViewModel>();
    }
}