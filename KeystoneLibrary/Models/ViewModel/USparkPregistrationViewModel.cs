namespace KeystoneLibrary.Models.ViewModel
{
    public class USparkPreregistrationViewModel
	{
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("ksSectionId")]
        public long KSSectionId { get; set; }

        [JsonProperty("channel")]
        public string Channel { get; set; }

        [JsonProperty("isPaid")]
        public bool IsPaid { get; set; }

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; } // A = ADDED, D = DELETED
    }
}

