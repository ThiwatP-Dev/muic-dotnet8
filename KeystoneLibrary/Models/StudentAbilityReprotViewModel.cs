namespace KeystoneLibrary.Models
{
    public class StudentAbilityReprotViewModel
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public string StudentFirstName { get; set; }
        public string StudentLastName { get; set; }
        public string StudentMidName { get; set; }
        public string IntakeTerm { get; set; }
        public string FullName => string.IsNullOrEmpty(StudentMidName) ? $"{ StudentFirstName } { StudentLastName }"
                                                                       : $"{ StudentFirstName } { StudentMidName } { StudentLastName }";
        public string Major { get; set; }
        public List<string> Abilities { get; set; } = new List<string>();
        public List<string> PredefindCourses { get; set; }  = new List<string>();
    }

    public class PredefindCoursesViewModel
    {
        public Guid StudentId { get; set; }
        public long? CourseRateId { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public int Credit { get; set; }
        public decimal Lecture { get; set; }
        public decimal Lab { get; set; }
        public decimal Other { get; set; }
        public string Type { get; set; }
        public string SpecialChar => CourseRateId == 2 ? "**" : "";
        public string CodeAndSpecialChar => $"{ CourseCode }{ SpecialChar }";
        public string CreditText => $"{ Credit.ToString(StringFormat.GeneralDecimal) } ({ Lecture.ToString(StringFormat.GeneralDecimal) }-{ Lab.ToString(StringFormat.GeneralDecimal) }-{ Other.ToString(StringFormat.GeneralDecimal) })";
        public string CourseAndCreditAndType => $"{ CodeAndSpecialChar } { CourseName } { CreditText } [{ Type }]";
    }
}