namespace KeystoneLibrary.Models
{
    public class SectionSlotViewModel
    {
        public Criteria Criteria { get; set; }
        public List<SectionSlotDetailViewModel> SectionSlots { get; set; }

        public long SectionId { get; set; }
    }
    public class SectionSlotDetailViewModel
    {
        public string IsChecked { get; set; }
        public long Id { get; set; }
        public long SectionId { get; set; }
        public int TotalWeeks { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string InstructorCode { get; set; }
        public string Room { get; set; }
        public string CourseCode { get; set; }
        public int CourseCredit { get; set; }
        public decimal CourseLecture { get; set; }
        public decimal CourseLab { get; set; }
        public decimal CourseOther { get; set; }
        public string CourseName { get; set; }
        public string Number { get; set; }
        public string Status { get; set; }
        public string EaminationReservationStatus { get; set; }
        public bool IsMakeUpClass { get; set; }
        public string FirstNameEn { get; set; }
        public string LastNameEn { get; set; }
        public string Title { get; set; }
        public string Remark { get; set; }
        public long TeachingTypeId { get; set; }
        public string TeachingType { get; set; }
        public bool IsExam { get; set; }
        public long ExaminationTypeId { get; set; }
        public long? InstructorId { get; set; }
        public string InstructorFullNameEn => $"{ Title } { FirstNameEn } { LastNameEn }";
        public long? CourseRateId { get; set; }
        public string SpecialChar => CourseRateId == 2 ? "**" : "";
        public string CodeAndSpecialChar => $"{ CourseCode }{SpecialChar}";
        public string CourseCodeAndCredit => $"{ CodeAndSpecialChar } { CourseCredit.ToString(StringFormat.GeneralDecimal) } ({ CourseLecture.ToString(StringFormat.GeneralDecimal) }-{ CourseLab.ToString(StringFormat.GeneralDecimal) }-{ CourseOther.ToString(StringFormat.GeneralDecimal) })";
        public string StartTimeText => $"{ StartTime.ToString(StringFormat.TimeSpan) }";
        public string EndTimeText => $"{ EndTime.ToString(StringFormat.TimeSpan) }";
        public bool IsActive { get; set; }
        public string Dayofweek
        {
            get
            {
               var day = Enum.GetName(typeof(DayOfWeek), Date.DayOfWeek).Substring(0, 3).ToUpper();
               return day ;
            }
        }

        public string DateText => $"{ Date.ToString(StringFormat.ShortDate) }";

        public string StatusText
        {
            get
            {
                if (IsExam)
                {
                    switch (Status)
                    {
                        case "a":
                            return "Approved";
                        case "r":
                            return "Reject";
                        case "c":
                            return "Confirm";
                        case "ne":
                            return "No Exam";
                        default:
                            return "Waiting";
                    }
                }
                else
                {
                    switch (Status)
                    {
                        case "w":
                            return "Waiting";
                        case "p":
                            return "Passed";
                        case "c":
                            return "Cancel";
                        default:
                            return "N/A";
                    }
                }
                return "N/A";
            }
        }
    }
}