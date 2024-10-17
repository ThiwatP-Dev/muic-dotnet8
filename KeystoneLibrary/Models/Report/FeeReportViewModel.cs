namespace KeystoneLibrary.Models.Report
{
    public class FeeReportViewModel
    {
        public List<Tuple<long, string>> FeeGroups { get; set; }
        public List<FeeReportItemViewModel> FeeReportItemViewModels { get; set; }
        public Criteria Criteria { get; set; }

        public FeeReportViewModel()
        {
            FeeGroups = new List<Tuple<long, string>>();
            FeeReportItemViewModels = new List<FeeReportItemViewModel>();
        }
    }

    public class FeeReportItemViewModel
    {
        public string StudentCode { get; set; }
        public string StudentName { get; set; }
        public string Faculty { get; set; }
        public string Department { get; set; }
        public string FeeGroup { get; set; }
        public List<string> FeeItemAmounts { get; set; }
        public string TotalAmount { get; set; }
        public string InvoiceNumber { get; set; }
        public string ReceiptNumber { get; set; }
        public DateTime? TransactionDate { get; set; }
        public string TransactionDateText { get; set; }
        public string Type { get; set; }
        public string Method { get; set; }
        public string Status { get; set; }
    }
}