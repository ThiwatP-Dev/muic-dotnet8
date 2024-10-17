namespace KeystoneLibrary.Models
{
    public class WaiveGroupViewModel
    {
        public string StudentCode { get; set; }
        public string StudentFullNameEn { get; set; }
        public string InvoiceNumber { get; set; }
        public string ReceiptNumber { get; set; }
        public string FeeItemNameEn { get; set; }
        public string CourseNameEn { get; set; }
        public string SectionNumber { get; set; }
        public decimal Amount { get; set; }
        public decimal DiscountAmount { get; set; }
        public string AmountText => Amount.ToString(StringFormat.Money);
        public string DiscountAmountText => DiscountAmount.ToString(StringFormat.Money);
    }
}