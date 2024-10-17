namespace KeystoneLibrary.Models
{
    public class RoomCalendarViewModel
    {
        public Criteria Criteria { get; set; }
        public List<RoomCalendarDetail> Details { get; set; }
    }

    public class RoomCalendarDetail
    {
        public long Id { get; set; }
        public long SectionSlotId { get; set; }
        public long ExaminationId { get; set; }
        public long ReservationId { get; set; }
        public string ReservationName { get; set; }
        public string CourseCode { get; set; }
        public int Credit { get; set; }
        public decimal Lecture { get; set; }
        public decimal Lab { get; set; }
        public decimal Other { get; set; }
        public string SectionNumber { get; set; }
        public string Time { get; set; }
        public DateTime Date { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string AcademicLevel { get; set; }
        public string Type { get; set; }
        public string Color { get; set; }
        public string Remark { get; set; }
        public bool IsCancel { get; set; }
        public bool IsWaiting { get; set; }
        public long? CourseRateId { get; set; }
        public string SpecialChar => CourseRateId == 2 ? "**" : "";
        public string CreditText => $"{ Credit.ToString(StringFormat.GeneralDecimal) }({ Lecture.ToString(StringFormat.GeneralDecimal) }-{ Lab.ToString(StringFormat.GeneralDecimal) }-{ Other.ToString(StringFormat.GeneralDecimal) })";
        public string CodeAndCredit => $"{ CourseCode }{SpecialChar} { CreditText }";
        public string SectionTitle => $"{ Time }{ Environment.NewLine }{ CodeAndCredit } ({ SectionNumber })";
        public string ReservationTitle => $"{ Time }{ Environment.NewLine }{ ReservationName }";
        public string Title => (SectionSlotId != 0 || ExaminationId != 0) ? SectionTitle : ReservationTitle;
    }
}