using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace KeystoneLibrary.Models
{
    public class ResultSectionApiViewModel
    {
        public string Term { get; set; }
        public long AcademicYear { get; set; }
        public long AcademicTerm { get; set; }
        public List<SectionApiViewModel> Sections { get; set; }
    }
    
    public class SectionApiViewModel
    {
        public long SectionId { get; set; }
        public string SectionNumber { get; set; }
        public string SectionTypes 
        { 
            get 
            {
                string result = "";
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

                return result;
            }
         }
        public string TermText { get; set; }
        public int SeatAvailable { get; set; }
        public int SeatLimit { get; set; }
        public int SeatUsed { get; set; }
        public int TotalSeatUsed { get; set; }
        public int PlanningSeat { get; set; }
        public int ExtraSeat { get; set; }
        public int MinimumSeat { get; set; }
        public long? ParentSectionId { get; set; }
        public bool IsSpecialCase { get; set; }
        public bool IsOutbound { get; set; }
        public TimeSpan? MidtermStart { get; set; }
        public TimeSpan? MidtermEnd { get; set; }
        public DateTime? MidtermDate { get; set; }
        public bool IsDisabledMidterm { get; set; }
        public TimeSpan? FinalStart { get; set; }
        public TimeSpan? FinalEnd { get; set; }
        public DateTime? FinalDate { get; set; }
        public bool IsDisabledFinal { get; set; }
        public string FinalRoom { get; set; }
        public string MidtermRoom { get; set; }
        public string Username { get; set; }

        [StringLength(5)]
        public string Status { get; set; } // a = approve, w = waiting, r = reject

        [StringLength(500)]
        public string Remark { get; set; }

        public InstructorApiViewModel MainInstructor { get; set; }
        public MasterAndJointSectionViewModel MasterSection { get; set; }
        public List<MasterAndJointSectionViewModel> JointSections { get; set; }
        public List<InstructorApiViewModel> CoInstructors { get; set; }
        public CourseApiViewModel Course { get; set; }
        public List<StudentApiViewModel> StudentList { get; set; }
        public List<SectionSlotApiViewModel> SectionSlots{ get; set; }
        public List<SectionDetailApiViewModel> SectionDetails { get; set; }
        public List<ExamTypeApiViewModel> ExaminationReservation { get; set; }
    }

    public class MasterAndJointSectionViewModel
    {
        public long SectionId { get; set; }
        public string SectionNumber { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public string CourseCredit { get; set; }

        [JsonIgnore]
        public long ParentSectionId { get; set; }

        [JsonIgnore]
        public int SeatUsed { get; set; }
    }
    public class ExamTypeApiViewModel
    {
        public long ExaminationId { get; set; }
        public long SectionId { get; set; }
        public string  ExaminationType { get; set; }
        public string Room { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? Date { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public string StudentRemark { get; set; }
        public bool AbsentInstructor { get; set; }
        public bool AllowBooklet { get; set; }
        public bool AllowCalculator { get; set; }
        public bool AllowOpenbook { get; set; }
        public bool AllowAppendix { get; set; }
        public int TotalProctor { get; set; }
        public string ProctorRemark { get; set; }
        public InstructorApiViewModel Instructor { get; set; }
        public string Status { get; set; }
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

                    case "r":
                    return "Reject";

                    default:
                    return "N/A";
                }
            }
        }
        public string ExamStatus { get; set; }
        public string ExamStatusText
        {
            get
            {
                switch (ExamStatus)
                {
                    case "w":
                        return "Waiting";
                    case "p":
                        return "Taught";
                    case "c":
                        return "Cancel";
                    default:
                        return "N/A";
                }
            }
        }
    }
    public class InstructorApiViewModel
    {
        public string InstructorCode { get; set; }
        public string Title { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
    }
    public class CourseApiViewModel
    {
        public long? CourseId { get; set; }
        public string CourseCode { get; set; }
        public string CourseNameEn { get; set; }
        public int? CourseCredit { get; set; }
        public decimal? Lecture { get; set; }
        public decimal? Lab { get; set; }
        public decimal? Other { get; set; }

    }
    public class StudentApiViewModel
    {
        public string StudentCode { get; set; }
        public string TitleEn { get; set; }
        public string NameEn { get; set; }
        public string MidNameEn { get; set; }
        public string LastNameEn { get; set; }
        public string CourseCode { get; set; }
        public string SectionNumber { get; set;}
        public string Withdraw { get; set; }
        public string Email { get; set; }
        public string PersonalEmail { get; set; }
        public bool IsPaid { get; set; }
        public DepartmentApiViewModel Department { get; set; }
        public FacultyApiViewModel Faculty { get; set; }
        public CurriculumVersionApiViewModel CurriculumVersion { get; set; }

        [JsonIgnore]
        public long SectionId { get; set; }

        [JsonIgnore]
        public long? ParentSectionId { get; set; }

    }

    public class DepartmentApiViewModel
    {
        public string DepartmentCode { get; set; }
        public string DepartmentNameEn { get; set; }
        public string ShortNameEn { get; set; }
    }
    public class CurriculumVersionApiViewModel
    {        
        public string Code { get; set; }
        public string NameEn { get; set; }
    }

    public class FacultyApiViewModel
    {
        public string FacultyCode { get; set; }
        public string FacultyNameEn { get; set; }
        public string ShortNameEn { get; set; }
    }

    public class SectionSlotApiViewModel
    {
        public long SectionSlotId { get; set; }
        public long SectionId { get; set; }
        public string Room { get; set; }
        public string TeachingType { get; set; }
        public string Status { get; set; } // w = Waiting, p = Taught, c = Cancel
        public InstructorApiViewModel Instructor { get; set; }
        public string Day { get; set; }
        public string Remark { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int TotalTime { get; set; }
        public bool? AbsentInstructor { get; set; }
        public long? ExaminationId { get; set; }
        
        public string StatusText
        {
            get
            {
                if (ExaminationId.HasValue && ExaminationId.Value > 0)
                {
                    switch (Status) //This is actually a examstatus
                    {
                        case "w":
                            return "Waiting";
                        case "p":
                            return "Passed";
                        case "c":
                            return "Cancel";
                        case "r": //hack to be from normal status
                            return "Reject";
                        default:
                            return "N/A";
                    }
                }
                else
                {
                    switch (Status)
                    {
                        case "w":
                            return "Waiting";
                        case "p":
                            return "Taught";
                        case "c":
                            return "Cancel";
                        default:
                            return "N/A";
                    }
                }
            }
        }
    }

    public class SectionSlotWithDetailApiViewModel : SectionSlotApiViewModel
    {
        public string SectionNumber { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public bool IsMakeUp { get; set; }
    }

    public class SectionDetailApiViewModel
    {
        public long SectionDetailId { get; set; }
        public long SectionId { get; set; }
        public long? RoomId { get; set; }
        public string Room { get; set; }
        public string TeachingType { get; set; }
        public InstructorApiViewModel Instructor { get; set; }
        public string Day { get; set; }
        public string Remark { get; set; }
        
        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

    }

    public class CreateSectionSlotMakeUp
    {
        public long SectionId { get; set; }
        public long TeachingTypeId { get; set; }
        public long RoomId { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string InstructorCode { get; set; }
        public string Status { get; set; }
        public string Remark { get ; set; }

    }

    public class RoomApiViewModel
    {
        public long RoomId { get; set; }
        public string NameEn { get; set; }
        public int Floor { get; set; }
        public int Capacity { get; set; }
        public int ExaminationCapacity { get; set; }
        public string Building { get; set; }
        public string RoomType { get; set; }
        public bool IsOnline { get; set; }
    }

    public class TeachingTypeApiViewModel
    {
        public long TeachingTypeId { get; set; }
        public string NameEn { get; set; }
    }

    public class UpdateExamViewModel
    {
        public long SectionId { get; set; }
        public string InstructorCode { get; set; }
        public string ExamType { get; set; } // m, f
        public long RoomId { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string StudentRemark { get; set; }
        public bool AbsentInstructor { get; set; }
        public bool AllowBooklet { get; set; }
        public bool AllowCalculator { get; set; }
        public bool AllowOpenbook { get; set; }
        public bool AllowAppendix { get; set; }
        public int TotalProctor { get; set; }
        public string ProctorRemark { get; set; }
        public string Status { get; set; }
        public bool IsActive { get; set; }
    }
    public class DisabledExamViewModel
    {
        public long SectionId { get; set; }
        public bool IsDisabledFinal { get; set; }
        public bool IsDisabledMidterm { get; set; }
    }

    public class SectionManagementViewModel
    {
        public long Id { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public int CourseCredit { get; set; }
        public decimal CourseLecture { get; set; }
        public decimal CourseLab { get; set; }
        public decimal CourseOther { get; set; }
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
        public int ParentSeatUsed { get; set; }
        public int PlanningSeat { get; set; }
        public long? ParentSectionId { get; set; }
        public string ParentSectionCourseCode { get; set; }
        public string ParentSectionNumber { get; set; }
        public string Status { get; set; }
        public bool IsClosed { get; set; }
        public bool IsSpecialCase { get; set; }
        public bool IsOutbound { get; set; }
        public string ApprovedBy { get; set; }
        public string ApprovedAtText { get; set; }
        public string Remark { get; set; }
        public string FirstNameEn { get; set; }
        public string LastNameEn { get; set; }
        public string Title { get; set; }

        public string MainInstructorFullNameEn => $"{ Title } { FirstNameEn } { LastNameEn }";
        public List<SectionDetailManagementViewModel> SectionDetails { get; set; }
        public List<JointSectionManagementViewModel> JointSections { get; set; }
        public string MidtermDateTimeText => (MidtermDate == new DateTime() || MidtermDate == null) ?  "-" : MidtermDateTime;
        public string FinalDateTimeText => (FinalDate == new DateTime() || FinalDate == null) ?  "-" : FinalDateTime;
        public string FinalString => $"{ FinalDate?.ToString(StringFormat.ShortDate) }";
        public string MidtermString => $"{ MidtermDate?.ToString(StringFormat.ShortDate) }";
        public string MidtermDateTime => $"{ MidtermString } { MidtermText }";
        public string FinalDateTime => $"{ FinalString } { FinalText }";
        public string MidtermText => $"{ MidtermStart?.ToString(StringFormat.TimeSpan) } - { MidtermEnd?.ToString(StringFormat.TimeSpan) }";
        public string FinalText => $"{ FinalStart?.ToString(StringFormat.TimeSpan) } - { FinalEnd?.ToString(StringFormat.TimeSpan) }";
        public string CourseCodeAndCredit => $"{ CodeAndSpecialChar } { CourseCredit.ToString(StringFormat.GeneralDecimal) } ({ CourseLecture.ToString(StringFormat.GeneralDecimal) }-{ CourseLab.ToString(StringFormat.GeneralDecimal) }-{ CourseOther.ToString(StringFormat.GeneralDecimal) })";
        public long? CourseRateId { get; set; }
        public string SpecialChar => CourseRateId == 2 ? "**" : "";
        public string CodeAndSpecialChar => $"{ CourseCode }{SpecialChar}";
        public long? ParentCourseRateId { get; set; }
        public string ParentSpecialChar => ParentCourseRateId == 2 ? "**" : "";
        public string ParentCodeAndSpecialChar => $"{ ParentSectionCourseCode }{ParentSpecialChar}";
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

    public class JointSectionManagementViewModel
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

    public class SectionDetailManagementViewModel
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

    public class InstructorInfoApiViewModel
    {
        [JsonIgnore]
        public long Id { get; set; }

        public string InstructorCode { get; set; }

        [JsonIgnore]
        public long TitleId { get; set; }

        public string TitleEn { get; set; }
        public string TitleTh { get; set; }
        public string FirstNameEn { get; set; }
        public string LastNameEn { get; set; }
        public string FirstNameTh { get; set; }
        public string LastNameTh { get; set; }
        public string Email { get; set; }
        public string PersonalEmail { get; set; }

        // Instructor Work Status
        public string AcademicLevel { get; set; }

        [JsonProperty("divison")]
        public string Faculty { get; set; }

        [JsonProperty("divisonCode")]
        public string FacultyCode { get; set; }

        [JsonProperty("major")]
        public string Department { get; set; }
        
        [JsonProperty("majorCode")]
        public string DepartmentCode { get; set; }

        public string Type { get; set; }

        public List<CourseSectionInstructorApiViewModel> Courses { get; set; } = new List<CourseSectionInstructorApiViewModel>();
    }

    public class CourseSectionInstructorApiViewModel
    {
        public long SectionId { get; set; }
        public string SectionNumber { get; set; }
        public string SectionTypes 
        { 
            get 
            {
                string result = "";
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

                return result;
            }
         }
        public CourseApiViewModel Course { get; set; }
        public MasterAndJointSectionViewModel MasterSection { get; set; }
        public List<MasterAndJointSectionViewModel> JointSections { get; set; }

        [JsonIgnore]
        public long? InstructorId { get; set; }

        [JsonIgnore]
        public long? ParentSectionId { get; set; }
        [JsonIgnore]
        public bool IsSpecialCase { get; set; }
        [JsonIgnore]
        public bool IsOutbound { get; set; }

        [JsonIgnore]
        public long? MainInstructorId { get; set; }

        [JsonIgnore]
        public InstructorApiViewModel MainInstructor { get; set; }

        [JsonIgnore]
        public List<long?> CoInstructors { get; set; } = new List<long?>();

        public bool IsMainInstructor => InstructorId == MainInstructorId;
        public bool IsCoInstructor => CoInstructors.Any(x => x == InstructorId);
    }
}