using KeystoneLibrary.Models.DataModels;

namespace KeystoneLibrary.Models
{
    public class ExaminationReservationManagementViewModel
    {
        public long Id { get; set; }
        public string TermText { get; set; }
        public long SectionId { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public int CourseCredit { get; set; }
        public decimal CourseLecture { get; set; }
        public decimal CourseLab { get; set; }
        public decimal CourseOther { get; set; }
        public string InstructorFullNameEn { get; set; }
        public string SectionNumber { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int SeatUsed { get; set; }
        public int ParentSeatUsed { get; set; }
        public int TotalStudent { get; set; }
        public long? ParentSectionId { get; set; }
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
        public bool AbsentInstructor { get; set; }
        public bool AllowBooklet { get; set; }
        public bool AllowCalculator { get; set; }
        public bool AllowAppendix { get; set; }
        public bool AllowOpenbook { get; set; }
        public string SenderTypeText  { get; set; }    
        public DateTime? ApprovedAt { get; set; }
        public long? RoomId { get; set; }
        public string ApprovedAtText { get; set; }
        public string ApprovedBy { get; set; }
        public string ApprovedByFullNameEn { get; set; }
        public string DateText => Status == "ne" ? "" : (Date == new DateTime() || Date == null) ?  "-" : DateString;
        public string DateString => Status == "ne" ? "" : $"{ Date.ToString(StringFormat.ShortDate) }";
        public string Time => Status == "ne" ? "" : $"{ StartTime.ToString(StringFormat.TimeSpan) } - { EndTime.ToString(StringFormat.TimeSpan) }";
        public string CourseCodeAndCredit => $"{ CourseCode } { CourseCredit.ToString(StringFormat.GeneralDecimal) } ({ CourseLecture.ToString(StringFormat.GeneralDecimal) }-{ CourseLab.ToString(StringFormat.GeneralDecimal) }-{ CourseOther.ToString(StringFormat.GeneralDecimal) })";
        public List<JointSectionCourseToBeOfferedViewModel> JointSections { get; set; } = new List<JointSectionCourseToBeOfferedViewModel>();
        public string JointSectionsString { get; set; }
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

        public string StatusText
        {
            get 
            {
                switch (Status)
                {
                    case "w":
                    return "Waiting";

                    case "a":
                    return "Approved";

                    case "c":
                    return "Confirm";

                    case "r":
                    return "Reject";

                    case "ne":
                    return "No Exam";

                    default:
                    return "Waiting";
                }
            }
        }
    }

    public class UpdateExaminationReservationViewModel
    {
        public bool IsSaveSuccess { get; set; }
        //public string Status { get; set; }// oe = overlap exam status = (a or w), o = overlap section slot, a = exam approval , e = overlap exam status = a
        public string Message { get; set; }
        public UpdateExamStatus Status { get; set; }
        public ExaminationReservation ExaminationReservation { get; set; } 
    }

    public enum UpdateExamStatus
    {
        OverlapExam,
        OverlapExamOnlyStatusApproval,
        OverlapSectionSlot,
        ExaminationAlreadyApproved,
        UpdateExamFail,
        UpdateExamSuccess,
        SaveExamSucceed,
        SaveExamFail
    }
}