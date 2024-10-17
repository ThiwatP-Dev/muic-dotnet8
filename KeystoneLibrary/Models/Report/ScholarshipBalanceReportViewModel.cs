namespace KeystoneLibrary.Models.Report
{
    public class ScholarshipBalanceReportViewModel
    {
        public Criteria Criteria { get; set; }
        public List<ScholarshipReportDetail> ScholarshipDetails { get; set; }
    }

    public class ScholarshipReportDetail
    {
        public int Year { get; set; }
        public string Name { get; set; }
        public decimal? Budget { get; set; }
        public decimal? Amount => Budget / Price;
        public decimal? Price { get; set; }
        public decimal Total { get; set; }
        public int TotalStudent { get; set; }
        public decimal? Balance { get; set; }
        public string BudgetText => Budget?.ToString(StringFormat.Money);
        public string AmountText => Amount?.ToString(StringFormat.Money);
        public string PriceText => Price?.ToString(StringFormat.Money);
        public string TotalText => Total.ToString(StringFormat.Money);
        public string BalanceText => Balance?.ToString(StringFormat.Money);
        public List<ScholarshipStudentDeatil> ScholarshipStudentDeatils { get; set; }
    }

    public class ScholarshipStudentDeatil
    {
        public string FullName { get; set; }
        public string Code { get; set; }
        public string Department { get; set; }
        public decimal Price { get; set; }
        public string PriceText => Price.ToString(StringFormat.Money);
    }
}