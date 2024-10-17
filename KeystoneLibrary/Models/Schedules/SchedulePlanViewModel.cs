namespace KeystoneLibrary.Models.Schedules
{
    public class SchedulePlanViewModel
    {
        public List<ShowScheduleViewModel> Schedules { get; set; } = new List<ShowScheduleViewModel>();
        public List<SemesterCourseViewModel> Courses { get; set; } = new List<SemesterCourseViewModel>();
    }

    public class ShowScheduleViewModel
    {
        public List<SectionViewModel> Sections { get; set; } = new List<SectionViewModel>();
        public bool HasExamConflicted { get; set; }
    }
}