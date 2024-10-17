namespace KeystoneLibrary.Models
{
    public class SectionCourseToBeOfferedViewModel
    {
        public long Id { get; set; }
        public string CourseCode { get; set; }
        public int CourseCredit { get; set; }
        public decimal CourseLecture { get; set; }
        public decimal CourseLab { get; set; }
        public decimal CourseOther { get; set; }
        public string CourseName { get; set; }
        public string Number { get; set; }
        public DateTime? MidtermDate { get; set; }
        public TimeSpan? MidtermStart { get; set; }
        public TimeSpan? MidtermEnd { get; set; }
        public DateTime? FinalDate { get; set; }
        public TimeSpan? FinalStart { get; set; }
        public TimeSpan? FinalEnd { get; set; }
        public int SeatLimit { get; set; }
        public int SeatUsed { get; set; }
        public int TotalSeatUsed { get; set; }
        public int PlanningSeat { get; set; }
        public string Status { get; set; }
        public bool IsClosed { get; set; }
        public string ApprovedBy { get; set; }
        public string ApprovedAtText { get; set; }
        public string Remark { get; set; }
        public long? ParentSectionId { get; set; }
        public string ParentSectionCourseCode { get; set; }
        public string ParentSectionNumber { get; set; }
        public int ParentSectionSeatUsed { get; set; }
        public bool IsSpecialCase { get; set; }
        public bool IsOutbound { get; set; }
        public string FirstNameEn { get; set; }
        public string LastNameEn { get; set; }
        public string Title { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedByText { get; set; }
        public string MainInstructorFullNameEn => $"{ Title } { FirstNameEn } { LastNameEn }";
        public List<SectionDetailCourseToBeOfferedViewModel> SectionDetails { get; set; }
        public List<JointSectionCourseToBeOfferedViewModel> JointSections { get; set; }
        public string MidtermDateTimeText => (MidtermDate == new DateTime() || MidtermDate == null) ?  "-" : MidtermDateTime;
        public string FinalDateTimeText => (FinalDate == new DateTime() || FinalDate == null) ?  "-" : FinalDateTime;
        public string FinalString => $"{ FinalDate?.ToString(StringFormat.ShortDate) }";
        public string MidtermString => $"{ MidtermDate?.ToString(StringFormat.ShortDate) }";
        public string MidtermDateTime => $"{ MidtermString } { MidtermText }";
        public string FinalDateTime => $"{ FinalString } { FinalText }";
        public string MidtermText => $"{ MidtermStart?.ToString(StringFormat.TimeSpan) } - { MidtermEnd?.ToString(StringFormat.TimeSpan) }";
        public string FinalText => $"{ FinalStart?.ToString(StringFormat.TimeSpan) } - { FinalEnd?.ToString(StringFormat.TimeSpan) }";
        public string CourseCodeAndCredit => $"{ CourseCode } { CourseCredit.ToString(StringFormat.GeneralDecimal) } ({ CourseLecture.ToString(StringFormat.GeneralDecimal) }-{ CourseLab.ToString(StringFormat.GeneralDecimal) }-{ CourseOther.ToString(StringFormat.GeneralDecimal) })";
        public long? ParentCourseRateId { get; set; }
        public string ParentSpecialChar => ParentCourseRateId == 2 ? "**" : "";
        public string ParentCodeAndSpecialChar => $"{ ParentSectionCourseCode }{ParentSpecialChar}";  
        public string SectionTypes 
        { 
            get 
            {
                string result = "( ";
                if (ParentSectionId == null || ParentSectionId == 0)
                {
                    result += "Master";
                }
                else
                {
                    result += "Joint";
                }

                if (IsSpecialCase)
                {
                    result += ", Ghost";
                }

                if (IsOutbound)
                {
                    result += ", Outbound ";
                }

                return result + " )";
            }
        }
         
        public int NumberValue
        {
            get
            {
                int number;
                bool success = Int32.TryParse(Number, out number);
                return success ? number : 0;
            }
        }
    }

    public class JointSectionCourseToBeOfferedViewModel
    {
        public long Id { get; set; }
        public long? ParentSectionId { get; set; }
        public string Number { get; set; }
        public string CourseCode { get; set; }
        public int SeatUsed { get; set; }
        public int SeatLimit { get; set; }
        public long? CourseRateId { get; set; }
        public string SpecialChar => CourseRateId == 2 ? "**" : "";
        public string Remark { get; set; }
        public string CodeAndSpecialChar => $"{ CourseCode }{SpecialChar}";  
    }

    public class AddJointSectionViewModel
    {
        public long Id { get; set; }
        public long CourseId { get; set; }
        public string CourseCode { get; set; }
        public int CourseCredit { get; set; }
        public decimal CourseLecture { get; set; }
        public decimal CourseLab { get; set; }
        public decimal CourseOther { get; set; }
        public string CourseName { get; set; }
        public long? CourseRateId { get; set; }
        public string SpecialChar => CourseRateId == 2 ? "**" : "";
        public string CodeAndSpecialChar => $"{ CourseCode }{SpecialChar}";  
        public string CourseCodeAndCredit => $"{ CodeAndSpecialChar } { CourseCredit.ToString(StringFormat.GeneralDecimal) } ({ CourseLecture.ToString(StringFormat.GeneralDecimal) }-{ CourseLab.ToString(StringFormat.GeneralDecimal) }-{ CourseOther.ToString(StringFormat.GeneralDecimal) })";
        public string Number { get; set; }
        public int SeatLimit { get; set; }
        public int SeatUsed { get; set; }
        public int TotalSeatUsed { get; set; }
        public int PlanningSeat { get; set; }
        public long JointSectionCourseId { get; set; }
        public string JointSectionNumber { get; set; }
        public int JointSectionSeatLimit { get; set; }
        public string JointSectionRemark { get; set; }
        public List<JointSectionCourseToBeOfferedViewModel> OldJointSections { get; set; }
    }
    public class SectionDetailCourseToBeOfferedViewModel
    {
        public long SectionId { get; set; }
        public int Day { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Time => $"{ StartTime.ToString(StringFormat.TimeSpan) } - { EndTime.ToString(StringFormat.TimeSpan) }";
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
        public string DayofweekAndTime => $"{ Dayofweek } { Time }";
    }
}