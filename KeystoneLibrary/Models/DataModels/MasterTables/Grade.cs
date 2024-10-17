using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.Curriculums;

namespace KeystoneLibrary.Models.DataModels.MasterTables
{
    public class Grade : UserTimeStamp
    {
        public long Id { get; set; }

        [Required]
        [StringLength(10)]
        public string Name { get; set; }
        public long? AcademicLevelId { get; set; }
        public long? FacultyId { get; set; }
        public long? DepartmentId { get; set; }
        public long? CurriculumVersionId { get; set; }
        public decimal? Weight { get; set; }
        public bool IsCalculateGPA { get; set; }
        public bool IsCalculateCredit { get; set; }
        public bool IsShowInTranscript { get; set; }
        public bool IsCalculateRegistrationCredit { get; set; }

        [StringLength(100)]
        public string? DescriptionEn { get; set; }

        [StringLength(100)]
        public string? DescriptionTh { get; set; }

        [NotMapped]
        public string WeightText
        { 
            get
            {
                return Weight?.ToString(StringFormat.TwoDecimal);
            }
        }

        [ForeignKey("AcademicLevelId")]
        public virtual AcademicLevel? AcademicLevel { get; set; }

        [ForeignKey("FacultyId")]
        public virtual Faculty? Faculty { get; set; }

        [ForeignKey("DepartmentId")]
        public virtual Department? Department { get; set; }

        [ForeignKey("CurriculumVersionId")]
        public virtual CurriculumVersion? CurriculumVersion { get; set; }
    }
}