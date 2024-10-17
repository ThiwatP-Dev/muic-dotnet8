using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.MasterTables;
using System.ComponentModel;

namespace KeystoneLibrary.Models.DataModels.Curriculums
{
    public class CurriculumVersion : UserTimeStamp
    {
        public long Id { get; set; }

        [StringLength(50)]
        public string? Code { get; set; } //Curriculum ID
        public long CurriculumId { get; set; }
        public long? AcademicProgramId { get; set; }
        public long ImplementedTermId { get; set; }
        public int TotalCredit { get; set; }
        public int? ExpectCredit { get; set; }

        [Required]
        [StringLength(200)]
        public string NameEn { get; set; }

        [Required]
        [StringLength(200)]
        public string NameTh { get; set; }

        [StringLength(200)]
        public string? DegreeNameEn { get; set; } // 'Bachelor of Arts' <-- combine Academic Level + 'of' + Faculty Degree Name

        [StringLength(200)]
        public string? DegreeNameTh { get; set; }

        [StringLength(200)]
        public string? DegreeAbbreviationEn { get; set; } // majoring in 'Business English'

        [StringLength(200)]
        public string? DegreeAbbreviationTh { get; set; }
        public long? OpenedTermId { get; set; } //Open Year/Term
        public long? ClosedTermId { get; set; } //Close Year/Term
        public int MinimumTerm { get; set; } //Minimum term to staudy
        public int MaximumTerm { get; set; } //Maximum term to study
        public int? StartBatch { get; set; }
        public int? EndBatch { get; set; }

        [StringLength(500)]
        public string? Remark { get; set; }
        public bool IsLumpsumPayment { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? ApprovedDate { get; set; } //University approved

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? OHECApprovedAt { get; set; } //Office of the Higher Education Commission approved

        [ForeignKey("CurriculumId")]
        public virtual Curriculum Curriculum { get; set; }

        [ForeignKey("AcademicProgramId")]
        public virtual AcademicProgram? AcademicProgram { get; set; }

        [ForeignKey("ImplementedTermId")]
        public virtual Term Term { get; set; }

        [ForeignKey("OpenedTermId")]
        public virtual Term? OpenedTerm { get; set; }

        [ForeignKey("ClosedTermId")]
        public virtual Term? ClosedTerm { get; set; }

        [JsonIgnore]
        public virtual List<CourseGroup> CourseGroups { get; set; }

        [JsonIgnore]
        public virtual List<StudyPlan> StudyPlans { get; set; }

        [JsonIgnore]
        public virtual List<CurriculumInstructor> CurriculumInstructors { get; set; }

        [JsonIgnore]
        public virtual List<CurriculumBlacklistCourse> CurriculumBlacklistCourses { get; set; }

        [NotMapped]
        public List<long> CurriculumInstructorIds { get; set; }

        [NotMapped]
        public List<long> ThesisInstructorIds { get; set; }

        [NotMapped]
        public List<long> InstructorIds { get; set; }

        [NotMapped]
        public string CodeAndName => $"{ Code } - { Curriculum?.AbbreviationEn ?? "" } { NameEn }";

        [NotMapped]
        public string CodeAndNameTh => $"{ Code } - { Curriculum?.AbbreviationTh ?? "" } { NameTh }";

        [NotMapped]
        public string ApprovedDateText => ApprovedDate?.ToString(StringFormat.ShortDate);

        [NotMapped]
        public List<CurriculumStudyPlanViewModel> PlanGroups { get; set; }

        [NotMapped]
        public List<CurriculumSpecializationGroup> CurriculumSpecializationGroups { get; set; } = new List<CurriculumSpecializationGroup>();
    }
}