namespace KeystoneLibrary.Models.Report
{
    public class RegistrationStatisticReportViewModel
    {
        public Criteria Criteria { get; set; }
        public List<RegistrationStatisticReport> RegistrationStatisticReportDetails { get; set; }
    }

    public class RegistrationStatisticReport
    {
        public string StudentCode { get; set; }
        public string StudentFullNameEn { get; set; }
        public string Course { get; set; }
        public int Section => Master + Joint + Other;
        public int Master { get; set; }
        public int Joint { get; set; }
        public int Other { get; set; }
        public int TotalRegistration { get; set; }
        public int RegistrationCredit { get; set; }
        public int AcademicCredit { get; set; }
    }
}