namespace KeystoneLibrary.Models
{
    public class FilterCourseGroupViewModel
    {
        public Criteria Criteria { get; set; }
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int CourseCount { get; set; }
        public List<FilterCourseGroupDetailViewModel> Courses { get; set; }
    }

    public class FilterCourseGroupDetailViewModel
    {
        public string IsChecked { get; set; }
        public long FilterCourseGroupDetailId { get; set; }
        public long FilterCourseGroupId { get; set; }
        public long CourseId { get; set; }
        public string Code { get; set; }
        public string NameEn { get; set; }
        public string Faculty { get; set; }
        public string Department { get; set; }
        public decimal Lecture { get; set; } = 0;
        public decimal Lab { get; set; } = 0;
        public decimal Other { get; set; } = 0; // Self study
        public int Credit { get; set; }
        public long? CourseRateId { get; set; }
        public string SpecialChar => CourseRateId == 2 ? "**" : "";

        public string CodeAndSpecialChar => $"{ Code }{SpecialChar}";

        public string CodeAndName => $"{ Code }{SpecialChar}  { NameEn }";

        public string CreditText => $"{ Credit.ToString(StringFormat.GeneralDecimal) }({ Lecture.ToString(StringFormat.GeneralDecimal) }-{ Lab.ToString(StringFormat.GeneralDecimal) }-{ Other.ToString(StringFormat.GeneralDecimal) })";
        public string CourseAndCredit => $"{ CodeAndName } { CreditText }";
    }
}