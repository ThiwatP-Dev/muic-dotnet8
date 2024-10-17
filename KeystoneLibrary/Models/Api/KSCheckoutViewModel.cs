namespace KeystoneLibrary.Models.Api
{
    public class KSCheckoutViewModel
    {
        [JsonProperty("studentCode")]
        public string StudentCode { get; set; }

        [JsonProperty("academicYear")]
        public int AcademicYear { get; set; }

        [JsonProperty("term")]
        public int Term { get; set; }
    }
}
