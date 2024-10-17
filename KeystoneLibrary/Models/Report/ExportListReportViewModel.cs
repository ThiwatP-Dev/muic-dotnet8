namespace KeystoneLibrary.Models
{
    public class ExportListReportViewModel
    {
        public long Id { get; set; }
        public string TermText { get; set; }
        public long SectionId { get; set; }
        public string Division { get; set; }
        public long CourseId { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public int CourseCredit { get; set; }
        public decimal CourseLecture { get; set; }
        public decimal CourseLab { get; set; }
        public decimal CourseOther { get; set; }
        public string InstructorFullNameEn { get; set; }
        public string SectionNumber { get; set; }
        public DateTime? Date { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public int SeatUsed { get; set; }
        public int ParentSeatUsed { get; set; }
        public int TotalStudent { get; set; }
        public long? ParentSectionId { get; set; }
        public long? HeadParentSectionId { get; set; }
        public string ParentSectionCourseCode { get; set; }
        public string ParentSectionNumber { get; set; }
        public string Status { get; set; }
        public bool IsClosed { get; set; }
        public bool IsSpecialCase { get; set; }
        public bool IsOutbound { get; set; }
        public bool IsDisabledFinal { get; set; }
        public bool IsDisabledMidterm { get; set; }
        public string Room { get; set; }
        public string ExaminationType { get; set; }
        public string StudentRemark { get; set; }
        public int TotalProctor { get; set; }
        public bool UseProctor { get; set; }
        public string ProctorName { get; set; }
        public bool AbsentInstructor { get; set; }
        public bool AllowBooklet { get; set; }
        public bool AllowCalculator { get; set; }
        public bool AllowFomulaSheet { get; set; }
        public bool AllowAppendix { get; set; }
        public bool AllowOpenbook { get; set; }
        public string SenderTypeText { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public long? RoomId { get; set; }
        public string ApprovedAtText { get; set; }
        public string ApprovedBy { get; set; }
        public string ApprovedByFullNameEn { get; set; }
        public string DateText => Status == "ne" ? "" : (Date == null) ? "-" : DateString;
        public string DateString => Status == "ne" ? "" : (Date == null) ? "-" : $"{ Date.Value:dddd, MMMM d, yyyy}";
        public string Time => Status == "ne" ? "" : (StartTime == null || EndTime == null) ? "-" : $"{ StartTime.Value.ToString(StringFormat.TimeSpan) } - { EndTime.Value.ToString(StringFormat.TimeSpan) }";
        public string CourseCodeAndNameAndCredit => $"{ CourseCode } { CourseName } { CourseCredit.ToString(StringFormat.GeneralDecimal) } ({ CourseLecture.ToString(StringFormat.GeneralDecimal) }-{ CourseLab.ToString(StringFormat.GeneralDecimal) }-{ CourseOther.ToString(StringFormat.GeneralDecimal) })";
        public List<JointSectionCourseToBeOfferedViewModel> JointSections { get; set; } = new List<JointSectionCourseToBeOfferedViewModel>();
        public string JointSectionsString { get; set; }
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

        public bool ForceShowStatus { get; set; }

        public string StatusText
        {
            get
            {
                if (ParentSectionId != null && !ForceShowStatus)
                {
                    return "";
                }
                switch (Status)
                {
                    case "w":
                        return "Waiting";

                    case "c":
                        return "Confirm";

                    case "a":
                        return "Approved";

                    case "r":
                        return "Reject";

                    case "ne":
                        return "No Exam";

                    case "nd":
                        return "No Date";

                    case "nr":
                        return "No Request";

                    default:
                        return "N/A";
                }
            }
        }
    }
}
