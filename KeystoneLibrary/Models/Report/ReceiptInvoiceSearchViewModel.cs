namespace KeystoneLibrary.Models.Report
{
    public class ReceiptInvoiceSearchViewModel
    {
        public Criteria Criteria { get; set; }
        public List<ReceiptInvoiceSearchResultViewModel> Results { get; set; }
    }

    public class ReceiptInvoiceSearchResultViewModel
    {
        public long Id { get; set; }
        public string Number { get; set; }
        public string StudentCode { get; set; }
        public string StudentFullName { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal ScholarshipPayAmount { get; set; }
        public decimal Amount { get; set; }
        public string TotalAmountText => TotalAmount.ToString(StringFormat.Money);
        public string ScholarshipPayAmountText => ScholarshipPayAmount.ToString(StringFormat.Money);
        public string AmountText => Amount.ToString(StringFormat.Money);
        public string TypeText { get; set; }
        public bool IsPaid { get; set; }
        public bool IsCancel { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedAtText => CreatedAt.ToString(StringFormat.ShortDate);
    }
}