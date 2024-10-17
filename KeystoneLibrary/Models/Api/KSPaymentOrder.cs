namespace KeystoneLibrary.Models.Api
{
    public class KSPaymentOrder
    {
        public string OrderId { get; set; }
        public string Reference1 { get; set; }
        public string Reference2 { get; set; }
        public long KSTermID { get; set; }
        public string StudentCode { get; set; }
        public decimal TotalAmount { get; set; }
        public string Number { get; set; }
        public DateTime PaymentStartedAt { get; set; }
        public DateTime PaymentEndedAt { get; set; }
        public string ReferenceNumber { get; set; }
        public string KSInvoiceId { get; set; }
        public List<KSPaymentOrderDetail> KSPaymentOrderDetails { get; set; }
    }
}