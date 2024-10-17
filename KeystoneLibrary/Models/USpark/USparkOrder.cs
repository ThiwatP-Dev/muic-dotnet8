using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.USpark
{
    public class USparkOrder
    {
        [JsonProperty("ksInvoiceId")]
        public long KSInvoiceID { get; set; }

        [JsonProperty("studentCode")]
        public string StudentCode { get; set; }

        [JsonProperty("reference1")]
        public string Reference1 { get; set; }

        [JsonProperty("reference2")]
        public string Reference2 { get; set; }

        [JsonProperty("isPaid")]
        public bool IsPaid { get; set; }

        [JsonProperty("ksTermId")]
        public long KSTermID { get; set; }

        [JsonProperty("totalAmount")]
        public decimal TotalAmount { get; set; }

        [JsonProperty("number")]
        public string Number { get; set; }

        [JsonProperty("referenceNumber")]
        public int ReferenceNumber { get; set; }

        [NotMapped]
        [JsonProperty("orderDetails")]
        public List<USparkOrderDetail> OrderDetails { get; set; }

        [JsonProperty("paymentEndAt")]
        public DateTime InvoiceExpiryDate { get; set; }

        [JsonProperty("createdAt")]
        public DateTime? CreatedAt { get; set; }

        [JsonProperty("latePaymentDate")]
        public DateTime? LatePaymentDate { get; set; }

        [JsonProperty("lastPaymentDate")]
        public DateTime LastPaymentDate { get; set; }

        [NotMapped]
        [JsonProperty("paymentSlot")]
        public USparkPaymentSlot PaymentSlot { get; set; }
    }

    public class USparkPaymentSlot
    {
        [JsonProperty("startAt")]
        public DateTime PaymentAvailableFrom { get; set; }

        [JsonProperty("endAt")]
        public DateTime PaymentAvailableUntil { get; set; }
    }
}