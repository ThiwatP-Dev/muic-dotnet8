namespace KeystoneLibrary.Models.Report
{
    public class ReceiptPDFViewModel
    {
        public string StudentCode { get; set; }
        public string StudentFullName { get; set; }
        public string FacultyName { get; set; }
        public string DepartmentName { get; set; }
        public string ReceiptNumber { get; set; }
        public DateTime ReceiptAt { get; set; }
        public string ReceiptAtText => ReceiptAt.ToString(StringFormat.ShortDate);
        public string InvoiceNumber { get; set; }
        
        public string ReceiverName { get; set; }
        public string ReceiverFullName { get; set; }
        public string ReceiverRoleName { get; set; }

        public List<ReceiptPDFCourse> Courses { get; set; }
        public List<ReceiptPDFFee> Fees { get; set; }
        public List<ReceiptPDFPayment> Payments { get; set; }

        public string TotalCreditText => (Courses?.Sum(x => (int?)x.Credit) ?? 0).ToString(StringFormat.NumberString);
        public string TotalFeeText => (Fees?.Sum(x => (decimal?)x.Amount) ?? 0).ToString(StringFormat.Money);
        public string TotalPaymentText => (Payments?.Sum(x => (decimal?)x.Amount) ?? 0).ToString(StringFormat.Money);
        public string TotalAmountText { get; set; }
    }

    public class ReceiptPDFCourse
    {
        public string Code { get; set; }
        public string NameEn { get; set; }
        public int Credit { get; set; }
        public string CreditText => Credit.ToString(StringFormat.NumberString);
        public string SectionNumber { get; set; }
    }

    public class ReceiptPDFFee
    {
        public string NameEn { get; set; }
        public decimal Amount { get; set; }
        public string AmountText => Amount.ToString(StringFormat.Money);
    }

    public class ReceiptPDFPayment 
    {
        public string NameEn { get; set; }
        public decimal Amount { get; set; }
        public string AmountText => Amount.ToString(StringFormat.Money);
    }
}