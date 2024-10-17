using System.ComponentModel.DataAnnotations;

namespace KeystoneLibrary.Models
{
    public class ModifyRegistrationCourseViewModel
    {
        public Criteria Criteria { get; set; }
        public StudentInformationViewModel Student { get; set; } = new StudentInformationViewModel();
        public List<ModifyRegistrationCourseDetailViewModel> CouresByTerms { get; set; }
    }

    public class ModifyRegistrationCourseDetailViewModel
    {
        public int AcademicYear { get; set; }
        public int AcademicTerm { get; set; }
        public string TermText => $"{ AcademicTerm }/{ AcademicYear }";
        public List<ModifyRegistrationCourse> Courses { get; set; }
    }

    public class ModifyRegistrationCourse
    {
        public long RegistrationCourseId { get; set; }
        public long? CourseRateId { get; set; }
        public string SpecialChar => CourseRateId == 2 ? "**" : "";
        public string CodeAndName => $"{ Code }{SpecialChar}  { NameEn }";  
        public string Code { get; set; }
        public string NameEn { get; set; }
        public int Credit { get; set; }
        public decimal Lecture { get; set; } = 0;  
        public decimal Lab { get; set; } = 0;  
        public decimal Other { get; set; } = 0;
        public string CreditText => $"{ Credit.ToString(StringFormat.GeneralDecimal) }({ Lecture.ToString(StringFormat.GeneralDecimal) }-{ Lab.ToString(StringFormat.GeneralDecimal) }-{ Other.ToString(StringFormat.GeneralDecimal) })";
        public string CourseAndCredit => $"{ CodeAndName } { CreditText }";
        public string SectionNumber { get; set; }
        public string GradeName { get; set; }
        public bool IsGradePublished { get; set; }
        public bool IsPaid { get; set; }
        public bool IsStarCourse { get; set; }
        public bool IsTransfer { get; set; }

        [Required]
        public long ToCourseId { get; set; }
        public bool ToStarCourse { get; set; }
        public bool ToTransferCourse { get; set; }

        public List<RegistrationChangeCourseLogViewModel> Logs { get; set; }
    }

    public class RegistrationChangeCourseLogViewModel
    {
        public ModifyRegistrationCourse Previous { get; set; }
        public ModifyRegistrationCourse Changed { get; set; }
        
    }
}