using KeystoneLibrary.Models.DataModels;

namespace KeystoneLibrary.Models
{
    public class FullScholarshipRegistrationViewModel
    {
        public Criteria Criteria { get; set; }
        public List<FullScholarshipRegistrationResultViewModel> Results { get; set; }
    }

    public class FullScholarshipRegistrationResultViewModel
    {
        public Guid StudentId { get; set; }
        public string StudentCode { get; set; }
        public string StudentFullName { get; set; }
        public long? ScholarshipStudentId { get; set; }
        public string ScholarshipName { get; set; }
        public decimal Balance { get; set; }
        public string BalanceText => Balance.ToString(StringFormat.Money);
        public decimal TotalAmount { get; set; }
        public string TotalAmountText => TotalAmount.ToString(StringFormat.Money);
        public string InvoiceIds { get; set; }
        public List<Invoice> Invoices { get; set; }
    }
}