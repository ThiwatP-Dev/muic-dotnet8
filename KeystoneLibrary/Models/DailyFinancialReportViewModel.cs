namespace KeystoneLibrary.Models
{
    public class DailyFinancialReportViewModel
    {
        public Criteria Criteria { get; set; }
        public List<DailyFinancialReport> DailyFinancialReports { get; set; }
        public List<DailyFinancialSummary> DailyFinancialSummaries { get; set; }
        public DailyFinancialSummary DailyFinancialReportTotal { get; set; }
        public DailyFinancialSummary DailyFinancialSummaryTotal { get; set; }
    }

    public class DailyFinancialReport
    {
        public string ReceiptNumber { get; set; }
        public string StudentCode { get; set; }
        public string StudentName { get; set; }
        public string FeeNames { get; set; }
        public string PaymentMethods { get; set; }
        public int TotalTransaction { get; set; }
        public decimal Amount { get; set; }
        public string AmountText => Amount.ToString(StringFormat.Money);
        public DateTime CreatedAt { get; set; }
        public string CreatedAtText { get; set; }
    }

    public class DailyFinancialSummary
    {
        public string PaymentMethod { get; set; }
        public int TotalTransaction { get; set; }
        public decimal Amount { get; set; }
        public string AmountText => Amount.ToString(StringFormat.Money);
    }
}