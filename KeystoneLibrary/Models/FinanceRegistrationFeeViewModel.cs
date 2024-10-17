using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.Scholarship;

namespace KeystoneLibrary.Models
{
    public class FinanceRegistrationFeeViewModel
    {
        public string TotalAmountText { get; set; }
        public string TotalDiscountAmountText { get; set; }
        public string Code { get; set; }
        public long TermId { get; set; }
        public long InvoiceId { get; set; }
        public DateTime? PaidDate { get; set; }
        
        public RegistrationFeeInvoice Invoice { get; set; }
        public List<ReceiptPaymentMethod> PaymentMethods { get; set; }
        public List<FinancialTransaction> FinancialTransactions { get; set; }

        public string UpdatedBy { get; set; }
    }

    public class RegistrationFeeInvoice
    {
        public string Number { get; set; }
        public long TermId { get; set; }
        public long InvoiceId { get; set; }
        public string InvoiceDateText { get; set; }
        public string InvoiceType { get; set; }
        public string StudentFullName { get; set; }
        public decimal TotalDeductAmount { get; set; }
        public string DiscountRemark { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Reference1 { get; set; }
        public string Reference2 { get; set; }
        public string Reference3 { get; set; }

        public List<RegistrationFeeInvoiceItem> InvoiceItems { get; set; }
    }

    public class RegistrationFeeInvoiceItem
    {
        public bool IsChecked { get; set; }
        public long Id { get; set; }
        public string Fee { get; set; }
        public string FeeCode { get; set; }
        public string FeeItem { get; set; }
        public string Course { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public string Section { get; set; }
        public string Type { get; set; }
        public bool IsPaid { get; set; }
        public decimal Amount { get; set; }
        public string AmountText => Amount.ToString(StringFormat.Money);
        public string DiscountRemark { get; set; }
        public decimal ScholarshipAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string ScholarshipAmountText => ScholarshipAmount.ToString(StringFormat.Money);
        public string DiscountAmountText => DiscountAmount.ToString(StringFormat.Money);
        public string TotalAmountText => TotalAmount.ToString(StringFormat.Money);
    }
}