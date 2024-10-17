namespace KeystoneLibrary.Models
{
    public class QuestionnaireCourseGroupViewModel
    {
        public long QuestionnaireCourseGroupId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public List<long> CourseIds { get; set; }
        public string CourseText => Courses != null ? string.Join(", ", Courses.Select(x => x.CodeAndCredit)) : "N/A";
        public List<QuestionnaireCourseGroupCourse> Courses { get; set; }
    }

    public class QuestionnaireCourseGroupCourse
    {
        public long QuestionnaireCourseGroupId { get; set; }
        public long CourseRateId { get; set; }
        public string CourseCode { get; set; }
        public string CodeAndCredit { get; set; } 
        public string CourseName { get; set; }
        public int Credit { get; set; }
        public decimal Lecture { get; set; }
        public decimal Lab { get; set; }
        public decimal Other { get; set; }
        public string SpecialChar => CourseRateId == 2 ? "**" : "";  
        public string CodeAndName => $"{ CourseCode }{ SpecialChar } { CourseName }";
        public string CreditText => $"{ Credit.ToString(StringFormat.GeneralDecimal) }({ Lecture.ToString(StringFormat.GeneralDecimal) }-{ Lab.ToString(StringFormat.GeneralDecimal) }-{ Other.ToString(StringFormat.GeneralDecimal) })";
    }
}