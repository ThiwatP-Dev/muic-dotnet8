namespace KeystoneLibrary.Models.ViewModel
{
    public class USparkRegistrationLogViewModel
	{
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("round")]
        public int Round { get; set; }

        [JsonProperty("channel")]
        public string Channel { get; set; }

        [JsonProperty("registrationCourses")]
        public string RegistrationCourses { get; set; }

        [JsonProperty("moditifications")]
        public string Modifications { get; set; }

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("createdBy")]
        public string CreatedBy { get; set; }

        [JsonProperty("updatedAt")]
        public DateTime UpdatedAt { get; set; }
    }
}

