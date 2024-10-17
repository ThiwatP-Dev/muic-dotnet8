namespace KeystoneLibrary.Models
{
    public class ExaminationPeriodReportViewModel
    {
        public Criteria Criteria { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Term { get; set; }
        public string CampusName { get; set; }
        public List<ExaminationPeriodByCourseReport> ExaminationPeriodByCourseReports { get; set; }
        public List<SummaryExaminationPeriodReportViewModel> SummaryExaminationPeriodReportViewModels { get; set; }
        public List<string> ExamTimes { get; set; }
    }

    public class ExaminationPeriodByCourseReport
    {
        public DateTime ExaminationAt { get; set; }
        public string ExaminationAtDayOfWeek => ExaminationAt.ToString(StringFormat.DayOfWeekMonthYear);
        public string ExaminationAtDay => ExaminationAt.ToString(StringFormat.DayMonthYear);
        public List<ExaminationPeriodByCourseDetail> ExaminationPeriodByCourseDetails { get; set; }
    }

    public class ExaminationPeriodByCourseDetail
    {
        public long SectionId { get; set; }
        public TimeSpan? ExamStartTime { get; set; }
        public TimeSpan? ExamEndTime { get; set; }
        public string Period
        {
            get
            {
                var examStartHour = ExamStartTime?.Hours ?? 0;
                return examStartHour < 12 ? "Morning" : "Afternoon";
            }
        }

        public string BeginTime => ExamStartTime == null ? "N/A" : ExamStartTime.Value.ToString(StringFormat.TimeSpan);
        public string EndTime => ExamEndTime == null ? "N/A" : ExamEndTime.Value.ToString(StringFormat.TimeSpan);
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public int TotalStudent { get; set; }
    }

    public class SummaryExaminationPeriodReportViewModel
    {
        public DateTime Date { get; set; }
        public string DateString => Date.ToString(StringFormat.DayOfWeekMonthYear);
        public List<SummaryExaminationPeriodDetail> SummaryExaminationPeriodDetails { get; set; }
        public int Total => SummaryExaminationPeriodDetails.Sum(x => x.Count);
        public String TotalString => Total == 0 ? "-" : Total.ToString();
    }

    public class SummaryExaminationPeriodDetail
    {
        public string Time { get; set; }
        public int Count { get; set; }
        public string CountString => Count == 0 ? "-" : Count.ToString();
    }
}