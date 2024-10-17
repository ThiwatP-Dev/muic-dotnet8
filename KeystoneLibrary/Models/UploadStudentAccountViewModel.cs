namespace KeystoneLibrary.Models
{
    public class UploadStudentAccountViewModel
    {
        public string StudentCode { get; set; }
        public string StudentFullName { get; set; }
        public string BankBranchCode { get; set; }
        public string BankAbbreviation { get; set; }
        public string BankAccount { get; set; }
        public bool IsUploadSuccess { get; set; }
    }
}