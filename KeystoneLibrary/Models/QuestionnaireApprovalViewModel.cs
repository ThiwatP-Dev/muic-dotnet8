namespace KeystoneLibrary.Models
{
    public class QuestionnaireApprovalViewModel
    {
        public long AcademicLevelId { get; set; }
        public long TermId { get; set; }
        public string Status { get; set; }
        public Criteria Criteria { get; set; }
        public List<QuestionnaireApprovalSection> Sections { get; set; }
    }

    public class QuestionnaireApprovalSection
    {
        public long QuestionnaireApprovalId { get; set; }
        public long? FacultyId { get; set; }
        public long? DepartmentId { get; set; }
        public long CourseId { get; set; }
        public long SectionId { get; set; }
        public long InstructorId { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public string Faculty { get; set; }
        public string SectionType { get; set; }
        public string InstructorTitle { get; set; }
        public string InstructorName { get; set; }
        public int CourseCredit { get; set; }
        public decimal CourseLecture { get; set; }
        public decimal CourseLab { get; set; }
        public decimal CourseOther { get; set; }
        public long? CourseRateId { get; set; }
        public string Section { get; set; }
        public string Status { get; set; }
        public bool IsChecked { get; set; }
        public int StudentServey { get; set; }
        public int StudentServeyThatCount { get; set; } //student who fill at least 1 score
        public int RegisteredStudents { get; set; }
        public decimal Total { get; set; }
        public decimal TotalRelatedSection { get; set; }
        public decimal TotalSD { get; set; }
        public decimal TotalRelatedSectionSD { get; set; }
        public bool IsDisabled { get; set; }
        public string SpecialChar => CourseRateId == 2 ? "**" : "";
        public string CodeAndSpecialChar => $"{ CourseCode }{SpecialChar}";
        public string Course => $"{ CodeAndSpecialChar } { CourseCredit.ToString(StringFormat.GeneralDecimal) } ({ CourseLecture.ToString(StringFormat.GeneralDecimal) }-{ CourseLab.ToString(StringFormat.GeneralDecimal) }-{ CourseOther.ToString(StringFormat.GeneralDecimal) })";
        public string StatusText => Status == "w" ? "WAITING STAFF APPROVE" : Status == "s" ? "WAITING PD APPROVE" : Status == "p" ? "PD APPROVE" : "";
    }

    public class QuestionnaireApprovalResponseViewModel
    {
        public Guid StudentId { get; set; }
        public long? SectionId { get; set; }
        public string Answer { get; set; }
        public decimal TotalQuestion { get; set; }
        public decimal AnswerValue         
        {
            get
            {
                decimal number;
                bool success = Decimal.TryParse(Answer, out number);
                return success ? number : 0;
            }
        }
    }
    public class QuestionnaireApprovalLogDetail
    {
        public DateTime UpdateDate { get; set; }
        public string Status { get; set; }
        public string UserId { get; set; }
        public string UpdateBy { get; set; }
        public string StatusText => Status == "w" ? "WAITING STAFF APPROVE" : Status == "s" ? "STAFF APPROVE" : Status == "p" ? "PD APPROVE" : "";
        public string UpdateDateText => UpdateDate.ToString(StringFormat.ShortDateTime);
    }
}