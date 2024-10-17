using KeystoneLibrary.Models.DataModels;

namespace KeystoneLibrary.Models
{
    public class SeatAvailableViewModel
    {
        public Criteria Criteria { get; set; }
        public List<CoursesSeatAvailable> Courses { get; set; } = new List<CoursesSeatAvailable>();
        public int TotalSeatAvailable 
        {
            get
            {
                return Courses == null || !Courses.Any() ? 0 : Courses.Sum(x => x.SeatAvailable);
            }
        }

        public int TotalSeatUsed 
        {
            get
            {
                return Courses == null || !Courses.Any() ? 0 : Courses.Sum(x => x.SeatUsed);
            }
        }

        public int TotalSeatLimit
        {
            get
            {
                return Courses == null || !Courses.Any() ? 0 : Courses.Sum(x => x.SeatLimit);
            }
        }

        public int TotalSeatPayment
        {
            get
            {
                return Courses == null || !Courses.Any() ? 0 : Courses.Sum(x => x.SeatPayment);
            }
        }

        public int TotalSeatWithdraw
        {
            get
            {
                return Courses == null || !Courses.Any() ? 0 : Courses.Sum(x => x.SeatWithdraw);
            }
        }

        public int TotalSeatLeft
        {
            get
            {
                return Courses == null || !Courses.Any() ? 0 : Courses.Sum(x => x.SeatLeft);
            }
        }
    }

    public class CoursesSeatAvailable
    {
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public int CourseCredit { get; set; }
        public decimal CourseLecture { get; set; }
        public decimal CourseLab { get; set; }
        public decimal CourseOther { get; set; }
        public string TitleNameEn { get; set; }
        public string FirstNameEn { get; set; }
        public string LastNameEn { get; set; }
        public string SectionNumber { get; set; }
        public int SeatAvailable { get; set; }
        public int SeatUsed { get; set; }
        public int SeatLimit { get; set; }
        public int SeatPayment { get; set; }
        public int SeatWithdraw { get; set; }
        public long SectionId { get; set; }
        public bool IsOutbound { get; set; }
        public bool IsSpecialCase { get; set; }
        public long? ParentSectionId { get; set; }
        public string Remark { get; set; }
        public int SeatLeft
        {
            get
            {
                return SeatAvailable < SeatWithdraw ? 0 : SeatAvailable - SeatWithdraw;
            }
        }
        
        public Course Course { get; set; }
        public List<RegistrationCourse> RegistrationCourses { get; set; } = new List<RegistrationCourse>();
        public List<SectionDetail> SectionDetails { get; set; } = new List<SectionDetail>();
        public List<long> InstructorIds { get; set; } = new List<long>();
        public List<string> Instructors { get; set; } = new List<string>();
        public string MainInstructorFullNameEn => $"{ TitleNameEn }{ FirstNameEn } { LastNameEn }";
        public string CourseCodeAndCredit => $"{ CourseCode } { CourseCredit.ToString(StringFormat.GeneralDecimal) } ({ CourseLecture.ToString(StringFormat.GeneralDecimal) }-{ CourseLab.ToString(StringFormat.GeneralDecimal) }-{ CourseOther.ToString(StringFormat.GeneralDecimal) })";
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

    public class RegistrationCourseWithDrawals
    {
        public long? SectionId { get; set; }
        public bool IsWithDrawal { get; set; }
    }
}