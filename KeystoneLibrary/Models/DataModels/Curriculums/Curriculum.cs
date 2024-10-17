using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.MasterTables;

namespace KeystoneLibrary.Models.DataModels.Curriculums
{
    public class Curriculum : UserTimeStamp
    {
        public long Id { get; set; }

        [StringLength(50)]
        public string? ReferenceCode { get; set; } // Code from MHESI
        public long AcademicLevelId { get; set; }
        public long FacultyId { get; set; }
        public long? DepartmentId { get; set; }

        [Required]
        [StringLength(5)]
        public string TermType { get; set; } // s = semester, t = trimester

        [Required]
        [StringLength(200)]
        public string NameEn { get; set; }

        [Required]
        [StringLength(200)]
        public string NameTh { get; set; }

        [StringLength(100)]
        public string? AbbreviationEn { get; set; }

        [StringLength(100)]
        public string? AbbreviationTh { get; set; }

        [StringLength(500)]
        public string? DescriptionEn { get; set; }

        [StringLength(500)]
        public string? DescriptionTh { get; set; }
        public double MinimumGPA { get; set; } //Minimum GPA to pass

        [NotMapped]
        public string MinimumGPAText
        {
            get
            {
                return MinimumGPA.ToString(StringFormat.TwoDecimal);
            }
        }

        [JsonIgnore]
        public virtual List<CurriculumVersion> CurriculumVersions { get; set; }

        [ForeignKey("AcademicLevelId")]
        public virtual AcademicLevel AcademicLevel { get; set; }
        
        [ForeignKey("FacultyId")]
        public virtual Faculty Faculty { get; set; }
        
        [ForeignKey("DepartmentId")]
        public virtual Department? Department { get; set; }

        [NotMapped]
        public string CodeAndName
        { 
            get 
            {
                return $"{ AbbreviationEn } - { NameEn }"; 
            }
        }

        [NotMapped]
        public string TermTypeText
        { 
            get 
            {
                switch (TermType)
                {
                    case "s":
                    return "Semester";

                    case "t":
                    return "Trimester";

                    default:
                    return "N/A";
                }
            }
        }
    }
}