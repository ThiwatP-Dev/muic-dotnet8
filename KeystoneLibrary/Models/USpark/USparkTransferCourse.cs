namespace KeystoneLibrary.Models.USpark
{
    public class USparkTransferCourse
    {
        [JsonProperty("studentCode")]
        public string StudentCode { get; set; }

        [JsonProperty("oldCourses")] 
        public IList<TransferCourseDetail> OldCourses { get; set; }

        [JsonProperty("newCourses")]
        public IList<TransferCourseDetail> NewCourses { get; set; }

        public class TransferCourseDetail
        {
            [JsonProperty("ksCourseId")]
            public long KsCourseId { get; set; }

            [JsonProperty("ksSectionId")]
            public long? KsSectionId { get; set; }

            [JsonProperty("ksTermId")]
            public long KsTermId { get; set; }

            [JsonProperty("grade")]
            public string Grade { get; set; }
        }
    }
}
