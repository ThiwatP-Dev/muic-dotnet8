namespace KeystoneLibrary.Models.Report
{
    public class ChangeCurriculumReportViewModel
    {
        public long OldCurriculumId { get; set; }
        public string OldCurriculumName { get; set; }
        public long NewCurriculumId { get; set; }
        public string NewCurriculumName { get; set; }
        public string Faculty { get; set; }
        public string StudentCode { get; set; }
        public string StudentFullName { get; set; }
        public string RequestedTerm { get; set; }
        public string ApprovedTerm { get; set; }
        public string Remark { get; set; }
        public string ApprovedDate { get; set; }
    }
}