namespace KeystoneLibrary.Models.Report
{
    public class TransactionFinalGradeReportViewModel
    {
        public string Term { get; set; }
        public string StudentCode { get; set; }
        public string Course { get; set; }
        public string Section { get; set; }
        public string PreviousGrade { get; set; }
        public string CurrentGrade { get; set; }
        public string UpdatedAt { get; set; }
        public string Remark { get; set; }
        public string UpdatedMonthAt { get; set; }
    }
}