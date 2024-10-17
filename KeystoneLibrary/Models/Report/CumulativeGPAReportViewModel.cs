namespace KeystoneLibrary.Models.Report
{
    public class CumulativeGPAReportViewModel
    {
        public Guid StudentId { get; set; }
        public string StudentCode { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MidName { get; set; }
        public string Major { get; set; }
        public decimal CGPA { get; set; }
        public int CreditEarned { get; set; }
        public int CompleteCredit { get; set; }
        public string StudentType { get; set; }
        public string ResidentType { get; set; }
        public string AdmissionType { get; set; }
        public string StudentStatus { get; set; }
        public string RegistrationTerm { get; set; }
        public string FullName => string.IsNullOrEmpty(MidName) ? $"{ Title } { FirstName } { LastName }"
                                                                : $"{ Title } { FirstName } { MidName } { LastName }";
    }
}