namespace KeystoneLibrary.Models.Report
{
    public class GradingReportByCurriculumViewModel
    {
        public Criteria Criteria { get; set; }
        public List<StudentRegistrationCourseViewModel> RegistrationCourses { get; set; } = new List<StudentRegistrationCourseViewModel>();
        public StudentRegistrationCoursesViewModel StudentRegistrationCoursesViewModels { get; set; } = new StudentRegistrationCoursesViewModel();
        public GraduatingRequestViewModel GraduatingRequest { get; set; } = new GraduatingRequestViewModel();
    }
}