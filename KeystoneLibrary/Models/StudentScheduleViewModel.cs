using KeystoneLibrary.Models.DataModels;

namespace KeystoneLibrary.Models
{
    public class StudentScheduleViewModel
    {
        public Criteria Criteria { get; set; }
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string AcademicLevel { get; set; }
        public string Term { get; set; }
        public string Faculty { get; set; }
        public string Department { get; set; }
        public string Advisor { get; set; }
        public string CurriculumVersion { get; set; }
        public List<RegistrationCourse> RegistrationCourses { get; set; }
        public List<ScheduleViewModel> Schedules { get; set; }
    }
}