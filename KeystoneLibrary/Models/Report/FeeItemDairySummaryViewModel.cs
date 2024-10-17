namespace KeystoneLibrary.Models
{
    public class FeeItemDairySummaryViewModel
    {
        public string GrandTotal => FeeItemTerms.Sum(x => x.GrandTotal).ToString(StringFormat.TwoDecimal);
        public Criteria Criteria { get; set; }
        public List<FeeItemDairyTerm> FeeItemTerms { get; set; } = new List<FeeItemDairyTerm>();
    }

    public class FeeItemDairyTerm
    {
        public string AcademicLevelHeader { get; set; }
        public string TotalBySemester { get; set; }
        public int AcademicTerm { get; set; }
        public int AcademicYear { get; set; }
        public decimal GrandTotal => FeeItemDetails.Sum(x => x.Amount);
        public string GrandTotalText => GrandTotal.ToString(StringFormat.TwoDecimal);
        public List<FeeItemDairyDetail> FeeItemDetails { get; set; } = new List<FeeItemDairyDetail>();
    }

    public class FeeItemDairyDetail
    {
        public DateTime Date { get; set; }
        public string DateText { get; set; }
        public string FeeCode { get; set; }
        public string FeeType { get; set; }
        public decimal Amount { get; set; }
        public string AmountText => Amount.ToString(StringFormat.TwoDecimal);
    }
}