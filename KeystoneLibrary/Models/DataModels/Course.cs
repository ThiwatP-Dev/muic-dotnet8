using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.MasterTables;
using KeystoneLibrary.Models.DataModels.Withdrawals;

namespace KeystoneLibrary.Models.DataModels
{
    public class Course : UserTimeStamp
    {
        public long Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Code { get; set; }
        public long AcademicLevelId { get; set; }  
        public long? FacultyId { get; set; }
        public long? DepartmentId { get; set; }

        [Required]
        [StringLength(200)]
        public string NameEn { get; set; }  

        [StringLength(200)]
        public string? NameTh { get; set; }  

        [StringLength(80)]
        public string? ShortNameEn { get; set; }  

        [StringLength(80)]
        public string? ShortNameTh { get; set; }  

        [StringLength(80)]
        public string? TranscriptNameEn1 { get; set; }  

        [StringLength(80)]
        public string? TranscriptNameEn2 { get; set; }  

        [StringLength(80)]
        public string? TranscriptNameEn3 { get; set; }  

        [StringLength(80)]
        public string? TranscriptNameTh1 { get; set; }  

        [StringLength(80)]
        public string? TranscriptNameTh2 { get; set; }  

        [StringLength(80)]
        public string? TranscriptNameTh3 { get; set; }  

        [StringLength(500)]
        public string? DescriptionEn { get; set; }  

        [StringLength(500)]
        public string? DescriptionTh { get; set; }  

        public int Credit { get; set; }  
        
        public int RegistrationCredit { get; set; } = 0;

        public int PaymentCredit { get; set; } = 0;
        public long? TeachingTypeId { get; set; }
        public decimal Hour { get; set; } = 0;

        //show in curriculum
        public decimal Lecture { get; set; } = 0;  
        public decimal Lab { get; set; } = 0;  
        public decimal Other { get; set; } = 0; // Self study  
        public bool IsShowInTranscript { get; set; }  
        public bool IsCalculateCredit { get; set; } // Calculate accumulated credit  
        public bool IsIntensiveCourse { get; set; } = false;
        public bool IsSectionGroup { get; set; } = false; // Big lecture room with many sections
        public int MinimumSeat { get; set; } = 0; // If section's seat used less than minimum seat, Officer will close this section.
        public long? CourseRateId { get; set; }
        public long? GradeTemplateId { get; set; }
        public long? TransferUniversityId { get; set; }

        //KS-1971 New Flag to check for Course to be Offer Drop Down List
        public bool IsAllowAddNewSection { get; set; }

        [StringLength(100)]
        public string? ExcludingNativeLanguageId { get; set; } // ex Thai language course, not allow thai people

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy HH:mm}")]
        public DateTime? ExpiredAt { get; set; }

        // For migration
        public long? MUICId { get; set; }

        [NotMapped]
        public string SpecialChar => CourseRateId == 2 ? "**" : "";  

        [NotMapped]
        public string CodeAndSpecialChar => $"{ Code }{SpecialChar}";  

        [NotMapped]
        public string CodeAndName => $"{ Code }{SpecialChar} { NameEn }";  

        [NotMapped]
        public string CodeAndNameTh => $"{ Code }{SpecialChar} { NameTh }";  

        [NotMapped]
        public string CreditText => $"{ Credit.ToString(StringFormat.GeneralDecimal) }({ Lecture.ToString(StringFormat.GeneralDecimal) }-{ Lab.ToString(StringFormat.GeneralDecimal) }-{ Other.ToString(StringFormat.GeneralDecimal) })";

        [NotMapped]
        public string RegisCreditText => $"{ RegistrationCredit.ToString(StringFormat.GeneralDecimal) }({ Lecture.ToString(StringFormat.GeneralDecimal) }-{ Lab.ToString(StringFormat.GeneralDecimal) }-{ Other.ToString(StringFormat.GeneralDecimal) })";
        
        [NotMapped]
        public string RegistrationCreditText => $"{ RegistrationCredit.ToString(StringFormat.GeneralDecimal) }";

        [NotMapped]
        public string CourseAndCredit => $"{ CodeAndName } { CreditText }";

        [NotMapped]
        public string NameEnAndCredit => $"{ NameEn } { CreditText }";

        [NotMapped]
        public string CodeAndCredit => $"{ Code }{SpecialChar} { CreditText }";

        [NotMapped]
        public string NameThAndCredit => $"{ NameTh } { CreditText }";  

        [JsonIgnore]
        [ForeignKey("AcademicLevelId")]
        public virtual AcademicLevel AcademicLevel { get; set; }  

        [JsonIgnore]
        [ForeignKey("FacultyId")]
        public virtual Faculty? Faculty { get; set; }

        [JsonIgnore]
        [ForeignKey("DepartmentId")]
        public virtual Department? Department { get; set; }

        [JsonIgnore]
        [ForeignKey("TeachingTypeId")]
        public virtual TeachingType? TeachingType { get; set; }

        [ForeignKey("CourseRateId")]
        public virtual CourseRate? CourseRate { get; set; }

        [ForeignKey("GradeTemplateId")]
        public virtual GradeTemplate? GradeTemplate { get; set; }

        [JsonIgnore]
        [ForeignKey("TransferUniversityId")]
        public virtual TransferUniversity? TransferUniversity { get; set; }  
 
        [JsonIgnore]
        public virtual List<Section> Sections { get; set; }

        [JsonIgnore]
        public virtual List<CurriculumBlacklistCourse> CurriculumBlacklistCourses { get; set; }

        [JsonIgnore]
        public virtual WithdrawalException ExceptionalCourse { get; set; }

        [NotMapped]
        public List<long> ExcludingNativeLanguageIds { get; set; } = new List<long>();

        [NotMapped]
        public string ExcludingNativeLanguageText { get; set; }

        [NotMapped]
        public string CreditGeneralText => Credit.ToString(StringFormat.GeneralDecimal);

        [NotMapped]
        public string LectureText => Lecture.ToString(StringFormat.GeneralDecimal);

        [NotMapped]
        public string LabText => Lab.ToString(StringFormat.GeneralDecimal);

        [NotMapped]
        public string OtherText => Other.ToString(StringFormat.GeneralDecimal);

        [NotMapped]
        public string EquivalentCourseCode { get; set; }

        // Fields For External Course
        // Code*
        // AcademicLevelId*
        // TransferUniversityId
        // NameEn*
        // NameTh*
        // ShortNameEn
        // ShortNameTh
        // TranscriptNameEn1*
        // TranscriptNameEn2
        // TranscriptNameEn3
        // TranscriptNameTh1*
        // TranscriptNameTh2
        // TranscriptNameTh3
        // DescriptionEn
        // DescriptionTh
        // Credit*
        // Lecture*
        // Lab*
        // Other*
        // IsShowInTranscript*
        // IsCalculateCredit*
    }
}