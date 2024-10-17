namespace KeystoneLibrary.Models
{
    public class RepeatStudentDetailReprotViewModel
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public string StudentFirstName { get; set; }
        public string StudentLastName { get; set; }
        public string StudentMidName { get; set; }
        public string Faculty { get; set; }
        public string Department { get; set; }
        public long CourseId { get; set; }
        public long? CourseRateId { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public int Credit { get; set; }
        public decimal Lecture { get; set; }
        public decimal Lab { get; set; }
        public decimal Other { get; set; }
        public string Type { get; set; }
        public long TermId { get; set; }
        public int AcademicTerm { get; set; }
        public int AcademicYear { get; set; }
        public long GradeId { get; set; }
        public string GradeName { get; set; }
        public string GradePassing { get; set; }
        public string FullName => string.IsNullOrEmpty(StudentMidName) ? $"{ StudentFirstName } { StudentLastName }"
                                                                       : $"{ StudentFirstName } { StudentMidName } { StudentLastName }";
        public string SpecialChar => CourseRateId == 2 ? "**" : "";
        public string CodeAndSpecialChar => $"{ CourseCode }{ SpecialChar }";
        public string CreditText => $"{ Credit.ToString(StringFormat.GeneralDecimal) } ({ Lecture.ToString(StringFormat.GeneralDecimal) }-{ Lab.ToString(StringFormat.GeneralDecimal) }-{ Other.ToString(StringFormat.GeneralDecimal) })";
        public string CourseAndCredit => $"{ CodeAndSpecialChar } { CourseName } { CreditText }";
        public string TermText => $"{ AcademicTerm }/{ AcademicYear }";

        public string AdvisorTitleEn { get; set; }
        public string AdvisorFirstNameEn { get; set; }
        public string AdvisorLastNameEn { get; set; }
        public string AdvisorFullName => !string.IsNullOrEmpty(AdvisorFirstNameEn) ? $"{ AdvisorFirstNameEn } { AdvisorLastNameEn }": "";
    }

    public class RepeatStudentReprotViewModel
    {
        public string StudentCode { get; set; }
        public string FullName { get; set; }
        public string Title { get; set; }
        public string StudentFirstName { get; set; }
        public string StudentLastName { get; set; }
        public string StudentMidName { get; set; }
        public string AdvisorTitleEn { get; set; }
        public string AdvisorFirstNameEn { get; set; }
        public string AdvisorLastNameEn { get; set; }
        public string Department { get; set; }
        public string CourseAndCredit { get; set; }
        public List<string> GradeNameAndTerms { get; set;} = new List<string>();
        public string AdvisorFullName { get; set; }
        public string GradePassing { get; set; }
    }
}