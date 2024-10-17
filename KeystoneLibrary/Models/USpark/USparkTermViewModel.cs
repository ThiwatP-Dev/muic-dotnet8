namespace KeystoneLibrary.Models.USpark
{
    public class USparkTermViewModel
    {
        public long KsTermId { get; set; }
        public long AcademicYear { get; set; }
        public long AcademicTerm { get; set; }
        public long TermTypeId { get; set; }

        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public DateTime? FirstRegistrationEndedAt { get; set; }
        public DateTime? FirstRegistrationPaymentEndedAt { get; set; }
        public DateTime? AddDropPaymentEndedAt { get; set; }
        public DateTime? LastPaymentEndedAt { get; set; }
        //public bool IsAdvising { get; set; }
        //public bool IsRegistration { get; set; }
        //public bool IsAdmission { get; set; }
        //public bool IsQuestionnaire { get; set; }

        public string TermText { get; set; }
        public string TermThText { get; set; }
        public string TermPeriodText { get; set; }
        public string TermTypeText { get; set; }
    }
}
