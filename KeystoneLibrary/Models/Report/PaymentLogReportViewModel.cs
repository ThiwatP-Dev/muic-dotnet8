namespace KeystoneLibrary.Models.Report
{
    public class PaymentLogReportViewModel
    {
        public long Id { get; set; }
        public string InvoiceNumber { get; set; }
        public string Number { get; set; }
        public bool? InvoiceIsCancel { get; set; }
        public bool? InvoiceIsPaid { get; set; }
        public string Reference1 { get; set; }
        public string Reference2 { get; set; }
        public decimal? TotalAmount { get; set; }
        public string TotalAmountText => TotalAmount?.ToString(StringFormat.Money);
        public decimal PaidAmount { get; set; }
        public string PaidAmountText => PaidAmount.ToString(StringFormat.Money);
        public bool IsPaymentSuccess { get; set; }
        public DateTime TransactionAt { get; set; }
        public string CreatedAt => TransactionAt.ToString(StringFormat.ShortDateTime);
        public string Response { get; set; }
        public object ResponseObject { get; set; }
    }
}