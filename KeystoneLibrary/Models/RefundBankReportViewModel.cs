namespace KeystoneLibrary.Models
{
    public class RefundBankReportViewModel
    {
        public Criteria Criteria { get; set; }
        public List<RefundBankReportDetail> RefundBankReportDetails { get; set; }
    }

    public class RefundBankReportDetail
    {
        public Guid StudentId { get; set; }
        public long TermId { get; set; }
        public string AccountNumber { get; set; }
        public decimal Amount { get; set; }
        public string BankBranch { get; set; }
        public string AccountName { get; set; }
        public string StudentCode { get; set; }
        public string RefundedAt { get; set; }
        public string AmountText => Amount.ToString(StringFormat.TwoDecimal);
        public string RefundIdString { get; set; }
        public List<long> RefundIds { get; set; }
        public string CreatedDate { get; set; }
        public string CreatedBy { get; set; }
    }
}