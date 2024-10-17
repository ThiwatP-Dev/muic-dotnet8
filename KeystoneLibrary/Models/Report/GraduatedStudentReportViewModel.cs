namespace KeystoneLibrary.Models.Report
{
    public class GraduatedStudentReportViewModel
    {
        public Criteria Criteria { get; set; }
        public List<GraduatedStudentReportDetail> GraduatedStudentReportDetails { get; set; }
    }

    public class GraduatedStudentReportDetail
    {
        public long Code { get; set; }
        public string Name { get; set; }
        public string GraduatedTerm { get; set; }
        public string GraduatedDate { get; set; }
        public string GraduatedClass { get; set; }
        public string Honor { get; set; }
        public string StudentStatus { get; set; }
    }
}