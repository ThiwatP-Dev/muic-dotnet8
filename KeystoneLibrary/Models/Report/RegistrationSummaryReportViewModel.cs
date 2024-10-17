namespace KeystoneLibrary.Models.Report
{
    public class RegistrationSummaryReportViewModel
    {
        public Criteria Criteria { get; set; }
        public List<RegistrationSummaryReport> Results { get; set; }
    }

    public class RegistrationSummaryReport
    {
        public string StudentCode { get; set; }
        public string StuentFullNameEn { get; set; }
        public string Course { get; set; }
        public int Section { get; set; }
        public int Master { get; set; }
        public int Joint { get; set; }
        public int TotalRegistration { get; set; }
        public int RegistrationCredit { get; set; }
        public int AcademicCredit { get; set; }
    }
}