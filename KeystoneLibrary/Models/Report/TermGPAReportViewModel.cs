namespace KeystoneLibrary.Models.Report
{
    public class TermGPAReportViewModel
    {
        public string StudentCode { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string MidName { get; set; }
        public string LastName { get; set; }
        public string DepartmentCode { get; set; }
        public decimal GPA { get; set; }
        public string GPAText => GPA.ToString(StringFormat.TwoDecimal);
        public decimal CummulativeGPA { get; set; }
        public string CummulativeGPAText => CummulativeGPA.ToString(StringFormat.TwoDecimal);
        public decimal TotalCreditEarned { get; set; }
        public string TotalCreditEarnedText => TotalCreditEarned.ToString(StringFormat.NumberString);
        public string StudentTypeName { get; set; }
        public string ResidentTypeName { get; set; }
        public string StudentStatus { get; set; }
        public string Detail { get; set; }
    }
}