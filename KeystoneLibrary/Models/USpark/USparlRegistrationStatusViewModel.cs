namespace KeystoneLibrary.Models.USpark
{
    public class USparkUpdateRegistrationStatusStudentViewModel
    {
        [JsonProperty("IsRegistrationAllowed")]
        public bool IsRegistrationAllowed { get; set; }

        [JsonProperty("IsPaymentAllowed")]
        public bool IsPaymentAllowed { get; set; }

        [JsonProperty("IsSignInAllowed")]
        public bool IsSignInAllowed { get; set; }
    }
}