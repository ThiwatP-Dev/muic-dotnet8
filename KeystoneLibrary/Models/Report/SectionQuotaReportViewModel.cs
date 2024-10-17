namespace KeystoneLibrary.Models.Report
{
    public class SectionQuotaReportViewModel
    {
        public Criteria Criteria { get; set; }
        public List<SectionQuotaReport> SectionQuotaReports { get; set; }
        public List<IGrouping<string, SectionQuotaReport>> GrouppedSectionQuotaReports { get; set; }
        public List<SectionQuotaCount> SectionQuotaCounts { get; set; }
        public List<SectionQuotaCount> FacultySectionQuotaCounts { get; set; }
    }

    public class SectionQuotaReport
    {
        public string DayofWeek { get; set; }
        public string Time { get; set; }
        public List<string> FacultyNames { get; set; }
    }

    public class SectionQuotaCount
    {
        public string Key { get; set; }
        public int Count { get; set; }
    }
} 