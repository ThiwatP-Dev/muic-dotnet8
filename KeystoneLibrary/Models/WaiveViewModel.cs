namespace KeystoneLibrary.Models
{
    public class WaiveViewModel
    {   
        public long Id { get; set; }
        public List<WaiveItemViewModel> InvoiceItems { get; set; }
        public decimal TotalDiscount { get; set; }
        public string DiscountRemark { get; set; }
    }

    public class WaiveItemViewModel
    {
        public long Id { get; set; }
        public decimal DiscountAmount { get; set; }
        public string DiscountRemark { get; set; }
    }
}