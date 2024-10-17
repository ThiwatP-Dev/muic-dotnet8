namespace KeystoneLibrary.Models.Report
{
    public class VerificationLetterViewModel
    {
        public string RunningNumber { get; set; }
        public DateTime SentAt { get; set; }
        public string SchoolType { get; set; }
        public string SchoolName { get; set; }
        public string Recipient { get; set; }
        public string Signatory { get; set; }
        public string SignatoryPosition { get; set; }
        public string OfficerName { get; set; }
        public string OfficerPosition { get; set; }
        public string OfficerDivision { get; set; }
        public string OfficerPhone { get; set; }
        public string OfficerEmail { get; set; }
        public List<VerificationStudentViewModel> Students { get; set; }
    }

    public class VerificationStudentViewModel
    {
        public string Code { get; set; }
        public string FullName { get; set; }
        public string PreviousSchool { get; set; }
    }
}