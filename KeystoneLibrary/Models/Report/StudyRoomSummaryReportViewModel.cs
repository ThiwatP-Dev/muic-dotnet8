namespace KeystoneLibrary.Models.Report
{
    public class StudyRoomSummaryReportViewModel
    {
        public Criteria Criteria { get; set; }
        public List<string> MonthYearList { get; set; }
        public List<StudyRoomSummaryReport> StudyRoomSummaryReports { get; set; }
    }

    public class StudyRoomSummaryReport
    {
        public string BuildingName { get; set; }
        public List<StudyRoomSummary> StudyRoomSummaries { get; set; }
    }

    public class StudyRoomSummary
    {
        public string Month { get; set; }
        public double TotalHours { get; set; }
        public string TotalHoursString => TotalHours.ToString(StringFormat.TwoDecimal);
    }
}