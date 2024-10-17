namespace KeystoneLibrary.Models.Api
{
    public class KSPaymentOrderDetail
    {
        public long OrderId { get; set; }
        public decimal Amount  { get; set; }
        public string ItemCode { get; set; }
        public string ItemNameEn { get; set; }
        public string ItemNameTh { get; set; }
    }
}