using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.Admission;
using KeystoneLibrary.Models.DataModels.Curriculums;
using KeystoneLibrary.Models.DataModels.MasterTables;

namespace KeystoneLibrary.Models.DataModels.Profile
{
    public class AdmissionInformation : UserTimeStamp
    {
        public long Id { get; set; }
        public Guid StudentId { get; set; }
        public long? PreviousSchoolId { get; set; }
        public long? EducationBackgroundId { get; set; }
        public long? GraduatedCountryId { get; set; }
        public long? PreviousBachelorSchoolId { get; set; }
        public long? PreviousMasterSchoolId { get; set; }
        public int? PreviousGraduatedYear { get; set; }
        public decimal? PreviousSchoolGPA { get; set; }
        public int? PreviousBachelorGraduatedYear { get; set; }
        public decimal? PreviousBachelorSchoolGPA { get; set; }
        public int? PreviousMasterGraduatedYear { get; set; }
        public decimal? PreviousMasterSchoolGPA { get; set; }

        public long? AdmissionTypeId { get; set; }

        public long? AdmissionTermId { get; set; }

        [StringLength(50)] 
        public string? ApplicationNumber { get; set; }

        [StringLength(100)] 
        public string? Password { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? AdmissionDate { get; set; } = DateTime.Now;

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? AppliedAt { get; set; } // วันที่ขึ้นทะเบียนนักศึกษาใหม่

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? CheckDated { get; set; } // Confirmation from pre-school date 

        [StringLength(200)] 
        public string? CheckReferenceNumber { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? ReplyDate { get; set; } 

        [StringLength(200)] 
        public string? ReplyReferenceNumber { get; set; }

        // Fields from Admission Program
        public long? AdmissionRoundId { get; set; } // change to non-nullable after migrate data and finish admission module
        public long? AdmissionDocumentGroupId { get; set; }
        public long? AcademicLevelId { get; set; } 
        public long? FacultyId { get; set; } // Faculty and department at first admission
        public long? DepartmentId { get; set; }
        public long? CurriculumVersionId { get; set; }
        public long? AgencyId { get; set; }

        [StringLength(200)] 
        public string? OfficerName { get; set; }

        [StringLength(200)] 
        public string? OfficerPhone { get; set; }
        public long? AdmissionPlaceId { get; set; }
        public long? ChannelId { get; set; } // admission channel = online, walk-in

        [StringLength(2)] 
        public string? EntranceExamResult { get; set; } // p = pass, f = fail, n = non test or no result
        
        [JsonIgnore]
        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        [ForeignKey("PreviousSchoolId")]
        public virtual PreviousSchool? PreviousSchool { get; set; }

        [ForeignKey("GraduatedCountryId")]
        public virtual Country? GraduatedCountry { get; set; }

        [ForeignKey("EducationBackgroundId")]
        public virtual EducationBackground? EducationBackground { get; set; }

        [ForeignKey("AdmissionTypeId")]
        public virtual AdmissionType? AdmissionType { get; set; }

        [ForeignKey("AdmissionTermId")]
        public virtual Term? AdmissionTerm { get; set; }

        [ForeignKey("AdmissionRoundId")]
        public virtual AdmissionRound? AdmissionRound { get; set; }

        [ForeignKey("AdmissionDocumentGroupId")]
        public virtual AdmissionDocumentGroup? AdmissionDocumentGroup { get; set; }

        [ForeignKey("FacultyId")]
        public virtual Faculty? Faculty { get; set; }

        [ForeignKey("DepartmentId")]
        public virtual Department? Department { get; set; }

        [ForeignKey("CurriculumVersionId")]
        public virtual CurriculumVersion? CurriculumVersion { get; set; }

        [ForeignKey("AgencyId")]
        public virtual Agency? Agency { get; set; }

        [ForeignKey("AdmissionPlaceId")]
        public virtual AdmissionPlace? AdmissionPlace { get; set; }

        [ForeignKey("ChannelId")]
        public virtual AdmissionChannel? AdmissionChannel { get; set; }

        [NotMapped]
        public List<AdmissionExamination> AdmissionExaminations { get; set; }

        [NotMapped]
        public List<StudentExemptedExamScore> StudentExemptedExamScores { get; set; }

        [NotMapped]
        public long CurriculumId => CurriculumVersion == null ? 0 : CurriculumVersion.CurriculumId;

        [NotMapped]
        public string CheckDatedText => CheckDated?.ToString(StringFormat.ShortDate);

        [NotMapped]
        public string ReplyDateText => ReplyDate?.ToString(StringFormat.ShortDate);

        [NotMapped]
        public string AdmissionDateText => AdmissionDate?.ToString(StringFormat.ShortDate);

        [NotMapped]
        public string AdmissionDateThText => AdmissionDate == null ? "" : AdmissionDate?.AddYears(543).ToString(StringFormat.ShortDate);

        [NotMapped]
        public long PreviousDocumentId { get; set; }

        [NotMapped]
        public string AdmissionTermRound => AdmissionTerm?.TermText + (AdmissionRound != null ? " Round " + AdmissionRound?.Round : "");
    }
}