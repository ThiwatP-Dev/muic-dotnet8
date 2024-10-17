using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.Profile;

namespace KeystoneLibrary.Models
{
    public class GroupRegistrationViewModel
    {
        public long AcademicLevelId { get; set; }
        public long TermId { get; set; }
        public string BatchCodeStart { get; set; }
        public string BatchCodeEnd { get; set; }
        public long FacultyId { get; set; }
        public long DepartmentId { get; set; }
        public DateTime? AdmissionStartedAt { get; set; }
        public DateTime? AdmissionEndedAt { get; set; }
        public DateTime? GradePublishStartedAt { get; set; }
        public DateTime? GradePublishEndedAt { get; set; }
        public List<CourseGrade> SelectedCourseGrades { get; set; }
        public SelectList Faculties { get; set; }
        public SelectList Departments { get; set; }
        public List<SelectableGroupRegistrationPlan> SelectablePlans { get; set; }
        public long SelectedPlanId { get; set; }
        public List<SelectablePlannedSchedule> SelectablePlannedSchedules { get; set; }
        public List<SelectableGroupRegistrationStudent> SelectableStudents { get; set; }
        public List<Guid> SelectedStudentIds { get; set; }
        public string SelectedStudentIdJson
        {
            get
            {
                return JsonConvert.SerializeObject(SelectedStudentIds);
            }
        }
        public int RegistrableStudentAmount { get; set; }
        public List<Guid> RegistrableScheduleStudentIds { get; set; }
        public List<Student> RegistrableScheduleStudents { get; set; }
    }
    
    public class CourseGrade
    {
        public long CourseId { get; set; }
        public string Grade { get; set; }
    }

    public class SelectableGroupRegistrationStudent
    {
        public Student Student { get; set; }
        public bool IsSelected { get; set; }
    }

    public class SelectableGroupRegistrationPlan
    {
        public Plan Plan { get; set; }
        public bool IsSelected { get; set; }
        public List<Course> Courses { get; set; }
        public string PlannedCourse 
        { 
            get
            {
                return string.Join(", ", Courses.Select(x => x.Code));
            }
        }
        public int RegistrableStudentAmount { get; set; }
    }

    public class SelectablePlannedSchedule
    {
        public PlanSchedule PlanSchedule { get; set; }
        public List<ScheduleViewModel> ScheduleSections { get; set; }
        public List<Guid> RegistrableStudentIds { get; set; }
        public string SelectedStudentIdJson
        {
            get
            {
                return JsonConvert.SerializeObject(RegistrableStudentIds);
            }
        }
        public int TotalRegistrableStudentAmount { get; set; }
    }
}