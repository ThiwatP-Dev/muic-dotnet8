namespace KeystoneLibrary.Models.Api
{
    public class SectionUpdateSlotInstructorViewModel
    {
        [JsonProperty("sectionId")]
        public long SectionId { get; set; }

        [JsonProperty("main_lecturer_username")]
        public string MainLecturerUsername { get; set; }

        [JsonProperty("sectionSlotId")]
        public long SectionSlotId { get; set; }

        [JsonProperty("new_lecturer_username")]
        public string NewLecturerUsername { get; set; }

        [JsonProperty("updatedBy")]
        public string UpdatedBy { get; set; }
    }
}
