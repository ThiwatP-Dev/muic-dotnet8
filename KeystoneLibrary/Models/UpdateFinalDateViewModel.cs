namespace KeystoneLibrary.Models
{
    public class UpdateFinalDateViewModel
    {
        public Criteria Criteria { get; set; }
        public TimeSpan? FinalStart { get; set; }
        public TimeSpan? FinalEnd { get; set; }
        public DateTime? FinalDate { get; set; }
        public List<UpdateFinalDate> Results { get; set; }
        public List<UpdateFinalDate> Success { get; set; }
        public List<UpdateFinalDate> Fails { get; set; }
    }

    public class UpdateFinalDate
    {
        public bool IsSelected { get; set; }
        public long SectionId { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public int CourseCredit { get; set; }
        public decimal CourseLecture { get; set; }
        public decimal CourseLab { get; set; }
        public decimal CourseOther { get; set; }
        public string SectionNumber { get; set; }
        public string InstructorName { get; set; }
        public string SeatUsedText { get; set; }
        public string StatusText { get; set; }
        public string Status { get; set; }
        public string SectionTypes { get; set; }
        public string Reason { get; set; }
        public DateTime? FinalDate { get; set; }
        public string FinalDateTime { get; set; }
        public string FinalDateTimeText => (FinalDate == new DateTime() || FinalDate == null) ?  "-" : FinalDateTime;
        public string CourseCreditText => $"{ CourseCredit.ToString(StringFormat.GeneralDecimal) } ({ CourseLecture.ToString(StringFormat.GeneralDecimal) }-{ CourseLab.ToString(StringFormat.GeneralDecimal) }-{ CourseOther.ToString(StringFormat.GeneralDecimal) })";
        public string CourseCodeAndCredit => $"{ CourseCode } { CourseCreditText }";
    }
}