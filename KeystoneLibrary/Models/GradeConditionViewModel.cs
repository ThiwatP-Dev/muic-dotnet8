namespace KeystoneLibrary.Models
{
    public class GradeConditionViewModel
    {
        public long Id { get; set; }
        public string GradeName { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public decimal Lecture { get; set; } = 0;  
        public decimal Lab { get; set; } = 0;  
        public decimal Other { get; set; } = 0;
        public long? CourseRateId { get; set; }
        public int Credit { get; set; }  
        public bool IsActive { get; set; }
        public DateTime? ExpiredAt { get; set; }
        public string ExpiredAtText => ExpiredAt?.ToString(StringFormat.ShortDate);
        public string CodeAndName => $"{ CourseCode }{SpecialChar} { CourseName }"; 
        public string SpecialChar => CourseRateId == 2 ? "**" : "";  
        public string CreditText => $"{ Credit.ToString(StringFormat.GeneralDecimal) }({ Lecture.ToString(StringFormat.GeneralDecimal) }-{ Lab.ToString(StringFormat.GeneralDecimal) }-{ Other.ToString(StringFormat.GeneralDecimal) })";
        public string CourseAndCredit => $"{ CodeAndName } { CreditText }";
    }
}