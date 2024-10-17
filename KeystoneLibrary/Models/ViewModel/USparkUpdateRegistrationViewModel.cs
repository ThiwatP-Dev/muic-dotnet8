namespace KeystoneLibrary.Models.ViewModel
{
    public class USparkUpdateRegistrationViewModel
    {
        [JsonProperty("studentCode")]
        public string StudentCode { get; set; }

        [JsonProperty("KSSectionIds")]
        public List<long> KSSectionIds { get; set; }

        [JsonProperty("channel")]
        public string Channel { get; set; }

        [JsonProperty("createdBy")]
        public string CreatedBy { get; set; }

        [JsonProperty("KSTermId")]
        public long KSTermId { get; set; }

    }
}

