namespace KeystoneLibrary.Models.USpark
{
    public class USparkWithdrawalViewModel
    {
        public bool IsWithdrawalPeriod { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime EndedAt { get; set; }
        public int? MinCredit { get; set; }
        public int RegistrationCredit { get; set; }
        public IEnumerable<USparkWithdrawalCourse> Courses { get; set; }
    }

    public class USparkWithdrawalCourse
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public string NameEn { get; set; }
        public string NameTh { get; set; }
        public long? SectionId { get; set; }
        public int Credit { get; set; }
        public int RegistrationCredit { get; set; }
        public string Status { get; set; }
        public string Remark { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class WithdrawalStudentViewModel
    {
        public string Code { get; set; }
        public string Status { get; set; }
        public string Reason { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class USparkWithdrawalCriteria
    {
        [JsonProperty("studentCode")]
        public string StudentCode { get; set; }

        [JsonProperty("sectionId")]
        public long SectionId { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set;}
    }
}