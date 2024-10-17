namespace KeystoneLibrary.Models.Report
{
    public class WaitingDocumentReportViewModel
    {
        public string Code { get; set; }
        public string FullName { get; set; }
        public string Faculty { get; set; }
        public string Department { get; set; }
        public string Gender { get; set; }
        public string Email { get; set; }
        public string PersonalEmail { get; set; }
        public string Phone { get; set; }
        public string PreviousSchool { get; set; }
        public string StudentStatus { get; set; }
        public List<WaitingDocument> WaitingDocuments { get; set; }
    }

    public class WaitingDocument
    {
        public string DocumentName { get; set; }
        public string Remark { get; set; }
    }
}