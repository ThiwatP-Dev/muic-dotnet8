namespace KeystoneLibrary.Models.Schedules
{
    public class GeneratedScheduleViewModel 
    {
        public List<SemesterCourseViewModel> Courses { get; set; }
        public List<ScheduleSectionViewModel> Schedules { get; set; }
    }

    public class ScheduleSectionViewModel
    {
        public List<long> SectionIds { get; set; }
        public string ExamConflictMessage { get; set; }
        public bool HasExamConflicted
        {
            get
            {
                return !string.IsNullOrEmpty(ExamConflictMessage);
            }
        }
    }

    public class GenerateSchedule
    {
        public List<CourseSection> CourseSections { get; set; }
      
        public long SemesterId { get; set; }
    }

    public class CourseSection
    {
        public long CourseId { get; set; }
        public List<long> SectionIds { get; set; }
    }

    public class GeneratedSchedultResult
    {
        public GeneratedSchedultResult()
        {
            IsSucceed = false;
            GeneratedScheduleViewModel = new GeneratedScheduleViewModel();
        }

        public bool IsSucceed { get; set; }
        public string ErrorMessage { get; set; }
        public string ExamConflictMessage { get; set; }
        public bool HasExamConflict
        {
            get
            {
                return !string.IsNullOrEmpty(ExamConflictMessage);
            }
        }
        public GeneratedScheduleViewModel GeneratedScheduleViewModel { get; set; }
    }

    public class SemesterCourseViewModel
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public string NameEn { get; set; }
        public string NameTh { get; set; }
        public decimal Credit { get; set; }
        public decimal RegistrationCredit { get; set; }
        public List<SectionViewModel> Sections { get; set; }
    }

    public class SectionViewModel
    {
        public SectionViewModel()
        {
            ClassSchedules = new List<ClassScheduleTimeViewModel>();
        }

        public long Id { get; set; }
        public long CourseId { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public string Number { get; set; }
        public string Remark { get; set; }
        public int SeatAvailable { get; set; }
        public int SeatUsed { get; set; }
        public int SeatLimit { get; set; }
        public bool IsClosed { get; set; }
        public IEnumerable<ClassScheduleTimeViewModel> ClassSchedules { get; set; }
        public ExaminationSchedule ExamSchedule { get; set; }

        public Tuple<bool, string> CheckClassTimeConflict(List<ClassScheduleTimeViewModel> classSchedules)
        {
            for (var i = 0; i < this.ClassSchedules.Count(); i++)
            {
                for (var j = 0; j < classSchedules.Count(); j++)
                {
                    var currentClassSchedule = this.ClassSchedules.ToList()[i];
                    var nextClassSchedule = classSchedules.ToList()[j];

                    if (currentClassSchedule.Day == nextClassSchedule.Day)
                    {
                        if (currentClassSchedule.StartTime.Ticks != 0)
                        {
                            if (currentClassSchedule.StartTime.Ticks < nextClassSchedule.StartTime.Ticks)
                            {
                                if (nextClassSchedule.StartTime.Ticks < currentClassSchedule.EndTime.Ticks)
                                {
                                    var message = String.Format("{0}({1}) and {2}({3})",
                                                                currentClassSchedule.CourseCode, currentClassSchedule.SectionNumber,
                                                                nextClassSchedule.CourseCode, nextClassSchedule.SectionNumber);

                                    return new Tuple<bool, string>(true, message);
                                }
                            }
                            else
                            {
                                if (nextClassSchedule.EndTime.Ticks > currentClassSchedule.StartTime.Ticks)
                                {
                                    var message = String.Format("{0}({1}) and {2}({3})",
                                                                currentClassSchedule.CourseCode, currentClassSchedule.SectionNumber,
                                                                nextClassSchedule.CourseCode, nextClassSchedule.SectionNumber);

                                    return new Tuple<bool, string>(true, message);
                                }
                            }
                        }
                    }
                }
            }
            
            return new Tuple<bool, string>(false, "no conflict");
        }
    }

    public class ExaminationSchedule
    {
        public ExamViewModel Midterm { get; set; }
        public ExamViewModel Final { get; set; }
    }

    public class ExamViewModel
    {
        public TimeSpan? Start { get; set; }
        public TimeSpan? End { get; set; }
        public string Room { get; set; }
        public DateTime? Date { get; set; }
    }
}