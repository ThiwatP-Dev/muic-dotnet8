using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.Scholarship;

namespace KeystoneLibrary.Models
{
    public class ScholarshipProfileViewModel
    {
        public Guid StudentId { get; set; }
        public string StudentCode { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ProfileImageURL { get; set; }
        public decimal GPA { get; set; }
        public int CreditEarned { get; set; }
        public int RegistrationCredit { get; set; }
        public string Program { get; set; }
        public string Faculty { get; set; }
        public string Department { get; set; }
        public string Curriculum { get; set; }
        public string CurriculumVersion { get; set; }
        public string Minor { get; set; }
        public string Concentration { get; set; }
        public bool AllowRegistration { get; set; }
        public bool IsFinishedRegistration { get; set; }
        public bool AllowAdvising { get; set; }
        public bool IsGraduating { get; set; }
        public bool IsMaintainedStatus { get; set; }
        
        [NotMapped]
        public string GPAText => GPA.ToString(StringFormat.TwoDecimal);
        public List<ScholarshipStudent> ScholarshipStudents { get; set; }
        public List<Voucher> Vouchers { get; set; }
        public List<FinancialTransaction> Transactions { get; set; }
        public List<FinancialTransaction> RefundTransactions => Transactions?.Where(x => x.Type == "r").ToList();
        public List<FinancialTransaction> UsageTransactions => Transactions?.ToList();
    }
}