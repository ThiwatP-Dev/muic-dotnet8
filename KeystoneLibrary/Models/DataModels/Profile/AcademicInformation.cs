using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.MasterTables;
using KeystoneLibrary.Models.DataModels.Curriculums;

namespace KeystoneLibrary.Models.DataModels.Profile
{
    public class AcademicInformation : UserTimeStamp
    {
        public long Id { get; set; }
        public Guid StudentId { get; set; }
        public long? AdvisorId { get; set; }

        [StringLength(20)]
        public string? OldStudentCode { get; set; }
        public int Batch { get; set; }

        [Required]
        public long StudentGroupId { get; set; }

        [Required]
        public decimal GPA { get; set; }

        [Required]
        public int CreditComp { get; set; } // not count F
        public int? CreditExempted { get; set; } // Transfer credits form ACC
        public int? CreditEarned { get; set; } // total credit, accumulated credit
        public int? CreditTransfer { get; set; } // grade TR, CS, CP, PR, CE, CT
        public int? MaximumCredit { get; set; } // For registration
        public int? MinimumCredit { get; set; } // For registration
        public long? CurriculumVersionId { get; set; }
        public long? StudyPlanId { get; set; }
        public long AcademicProgramId { get; set; } // Day, Night, Fast track
        public long AcademicLevelId { get; set; } // Bachelor, Master, Doctor
        
        [StringLength(200)]
        public string? DegreeName { get; set; }
        public long FacultyId { get; set; }
        public long? DepartmentId { get; set; }
        public bool IsAthlete { get; set; }
        public bool IsGraduating { get; set; }
        public int ChangedMajorCount { get; set; }
        public bool IsHasGradeUpdate { get; set; }

        [JsonIgnore]
        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        [ForeignKey("StudentGroupId")]
        public virtual StudentGroup StudentGroup { get; set; }

        [ForeignKey("CurriculumVersionId")]
        public virtual CurriculumVersion? CurriculumVersion { get; set; }

        [ForeignKey("StudyPlanId")]
        public virtual StudyPlan? StudyPlan { get; set; }

        [ForeignKey("AcademicProgramId")]
        public virtual AcademicProgram AcademicProgram { get; set; }

        [ForeignKey("AcademicLevelId")]
        public virtual AcademicLevel AcademicLevel { get; set; }

        [JsonIgnore]
        [ForeignKey("FacultyId")]
        public virtual Faculty Faculty { get; set; }

        [JsonIgnore]
        [ForeignKey("DepartmentId")]
        public virtual Department? Department { get; set; }

        [ForeignKey("AdvisorId")]
        public virtual Instructor? Advisor { get; set; }

        [NotMapped]
        public string GPAString => GPA.ToString(StringFormat.TwoDecimal);
    }
}