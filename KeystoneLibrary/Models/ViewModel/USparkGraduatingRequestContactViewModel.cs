namespace KeystoneLibrary.Models.ViewModel
{
    public class USparkGraduatingRequestContactViewModel
	{
		[JsonProperty("studentCode")]
		public string StudentCode { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("phoneNumber")]
        public string PhoneNumber { get; set; }
	}
}

