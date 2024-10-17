namespace KeystoneLibrary.Models
{
    public class ScholarshipFinancialTransactionReportViewModel
    {
        public string StudentCode { get; set; }
        public long ScholarshipId { get; set; }
        public long SignatoryId1 { get; set; }
        public long SignatoryId2 { get; set; }
        public Criteria Criteria { get; set; }
        public List<ScholarshipFinancialTransactionDetail> Details { get; set; }
    }

    public class ScholarshipFinancialTransactionDetail
    {
        public long Id { get; set; }
        public string StudentCode { get; set; }
        public string StudentName { get; set; }
        public string Division { get; set; }
        public string Major { get; set; }
        public string Scholarship { get; set; }
        public string CurrentTerm { get; set; }
        public string Condition { get; set; }
        public string SignatoryName1 { get; set; }
        public string SignatoryName2 { get; set; }
        public string SignatoryPosition1 { get; set; }
        public string SignatoryPosition2 { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public int TotalYear { get; set; }
        public string TotalAmount => FinancialTransactions.Sum(x => x.Amount).ToString(StringFormat.Money);
        public List<FinancialTransactionDetail> FinancialTransactions { get; set; }
    }

    public class FinancialTransactionDetail
    {
        public string Term { get; set; }
        public string Type { get; set; }
        public decimal Amount { get; set; }
        public string AmountText => Amount.ToString(StringFormat.Money);
    }
}