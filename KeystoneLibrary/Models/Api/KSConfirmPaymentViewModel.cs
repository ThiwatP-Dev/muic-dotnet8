namespace KeystoneLibrary.Models.Api
{
    public class KSConfirmPaymentViewModel
    {
        [JsonProperty("studentCode")]
        public string StudentCode { get; set; }

        [JsonProperty("orderNumber")]
        public string OrderNumber { get; set; }

        [JsonProperty("totalAmount")]
        public decimal TotalAmount { get; set; }
    }
}
