namespace KeystoneLibrary.Models
{
    public class SectionReportViewModel
    {
        public long Id { get; set; }
        public string CourseCode { get; set; }
        public string CourseNameEn { get; set; }
        public int CourseCredit { get; set; }
        public decimal CourseLab { get; set; }
        public decimal CourseLecture { get; set; }
        public decimal CourseOther { get; set; }
        public string FacultyNameEn { get; set; }
        public string Number { get; set; }
        public string MidtermDateText { get; set; }
        public DateTime? MidtermDate { get; set; }
        public TimeSpan? MidtermStart { get; set; }
        public TimeSpan? MidtermEnd { get; set; }
        public DateTime? FinalDate { get; set; }
        public TimeSpan? FinalStart { get; set; }
        public TimeSpan? FinalEnd { get; set; }
        public string FinalDateText { get; set; }
        public int SeatLimit { get; set; }
        public int SeatUsed { get; set; }
        public int TotalSeatUsed { get; set; }
        public int PlanningSeat { get; set; }
        public string Status { get; set; }
        public bool IsClosed { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string Remark { get; set; }
        public long? ParentSection { get; set; }
        public DateTime? ClosedSectionAt { get; set; }
        public DateTime? OpenedSectionAt { get; set; }
        public int SeatAvailable { get; set; }
        public bool IsOutbound { get; set; }
        public bool IsSpecialCase { get; set; }
        public long? ParentSectionId { get; set; }
        public string ParentSectionCourseCode { get; set; }
        public string ParentSectionNumber { get; set; }
        public int ParentSeatUsed { get; set; }
        public string FirstNameEn { get; set; }
        public string LastNameEn { get; set; }
        public string Title { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedByText { get; set; }

        public string MainInstructorFullNameEn => $"{ Title } { FirstNameEn } { LastNameEn }";
        public List<SectionDetailReportViewModel> SectionDetails { get; set; }
        public List<JointSectionReportViewModel> JointSections { get; set; }
                public string MidtermText => $"{ MidtermStart?.ToString(StringFormat.TimeSpan) } - { MidtermEnd?.ToString(StringFormat.TimeSpan) }";
        public string FinalText => $"{ FinalStart?.ToString(StringFormat.TimeSpan) } - { FinalEnd?.ToString(StringFormat.TimeSpan) }";
        public string MidtermString => (MidtermDate == new DateTime() || MidtermDate == null) ?  "" : $"{ MidtermDate?.ToString(StringFormat.ShortDate) }";
        public string FinalString => (FinalDate == new DateTime() || FinalDate == null) ?  "" : $"{ FinalDate?.ToString(StringFormat.ShortDate) }";
        public string MidtermDateTime => $"{ MidtermString } { MidtermText }";
        public string FinalDateTime => $"{ FinalString } { FinalText }";
        public string CreditText => $"{ CourseCredit.ToString(StringFormat.GeneralDecimal) } ({ CourseLecture.ToString(StringFormat.GeneralDecimal) }-{ CourseLab.ToString(StringFormat.GeneralDecimal) }-{ CourseOther.ToString(StringFormat.GeneralDecimal) })";
        public string CourseNameEnAndCredit => $"{ CourseNameEn } { CreditText }";
        public string OpenedSectionAtText => OpenedSectionAt?.ToString(StringFormat.ShortDate);
        public string ClosedSectionAtText => ClosedSectionAt?.ToString(StringFormat.ShortDate);
        public string ApprovedAtText => ApprovedAt?.ToString(StringFormat.ShortDate);
        public long? CourseRateId { get; set; }
        public string SpecialChar => CourseRateId == 2 ? "**" : "";
        public string CodeAndSpecialChar => $"{ CourseCode }{SpecialChar}";
        public long? ParentCourseRateId { get; set; }
        public string ParentSpecialChar => ParentCourseRateId == 2 ? "**" : "";
        public string ParentCodeAndSpecialChar => $"{ ParentSectionCourseCode }{ParentSpecialChar}";  
        public int NumberValue
        {
            get
            {
                int number;
                bool success = Int32.TryParse(Number, out number);
                return success ? number : 0;
            }
        }
        public string SectionTypes 
        { 
            get 
            {
                string result = "( ";
                if(ParentSectionId == null || ParentSectionId == 0)
                {
                    result += "Master";
                }
                else
                {
                    result += "Joint";
                }

                if(IsSpecialCase)
                {
                    result += ", Ghost";
                }

                if(IsOutbound)
                {
                    result += ", Outbound ";
                }

                return result + " )";
            }
         }
    }

    public class JointSectionReportViewModel
    {
        public long Id { get; set; }
        public long? ParentSectionId { get; set; }
        public string Number { get; set; }
        public string CourseCode { get; set; }
        public int SeatUsed { get; set; }
        public long? CourseRateId { get; set; }
        public string SpecialChar => CourseRateId == 2 ? "**" : "";
        public string CodeAndSpecialChar => $"{ CourseCode }{SpecialChar}";  
    }

    public class SectionDetailReportViewModel
    {
        public long SectionDetailId { get; set; }
        public long SectionId { get; set; }
        public int Day { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public List<string> InstructorSections { get; set; }
        public string RoomNameEn { get; set; }
        public string StartTimeText => $"{ StartTime.ToString(StringFormat.TimeSpan) }";
        public string EndTimeText => $"{ EndTime.ToString(StringFormat.TimeSpan) }";
        public string Dayofweek
        {
            get
            {
                if (Day != -1)
                {
                    var day = Enum.GetName(typeof(DayOfWeek),Day)?.Substring(0,3).ToUpper();
                    return day;
                }
                return "";
            }
        }
    }
}