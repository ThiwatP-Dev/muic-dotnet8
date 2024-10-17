namespace KeystoneLibrary.Models.ViewModel
{
    public class USparkCalculateTuitionRequestViewModel
	{
		[JsonProperty("studentCode")]
		public string StudentCode { get; set; }

		[JsonProperty("ksTermId")]
		public long KSTermId { get; set; }

		[JsonProperty("preregistrations")]
		public IEnumerable<USparkPreregistrationViewModel> Preregistrations { get; set; }

		[JsonProperty("registrationLogs")]
		public IEnumerable<USparkRegistrationLogViewModel> RegistrationLogs { get; set; }
	}
}

