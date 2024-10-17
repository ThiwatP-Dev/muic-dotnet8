using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.MasterTables;

namespace KeystoneLibrary.Models.DataModels.Curriculums
{
    public class CourseGroup : UserTimeStamp
    {
        public CourseGroup()
        {
            ChildCourseGroups = new List<CourseGroup>();
        }

        public long Id { get; set; }
        public long? CurriculumVersionId { get; set; }
        public long? CourseGroupId { get; set; }
        public long? RequiredGradeId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string NameEn { get; set; }

        [Required]
        [StringLength(200)]
        public string NameTh { get; set; }

        [StringLength(4000)]
        public string? DescriptionEn { get; set; }

        [StringLength(4000)]
        public string? DescriptionTh { get; set; }
        public int Credit { get; set; }

        [NotMapped]
        public string NameEnAndCredit => $"{ NameEn } ({ Credit.ToString(StringFormat.GeneralDecimal) } credits)";  
        public long? SpecializationGroupId { get; set; }

        [StringLength(2)]
        public string? Type { get; set; } // r = required, e = elective
        public int Sequence { get; set; }
        public bool IsForceTrack { get; set; } // Must Take, Should Take

        [StringLength(1000)]
        public string? Remark { get; set; }

        // For migration
        public long? MUICId { get; set; }

        public bool IsAutoAssignGraduationCourse { get; set; }

        [NotMapped]
        public bool HasChild { get; set; }

        [ForeignKey("CurriculumVersionId")]
        public virtual CurriculumVersion? CurriculumVersion { get; set; }

        [JsonIgnore]
        [ForeignKey("CourseGroupId")]
        public virtual CourseGroup? ParentGroup { get; set; }

        [ForeignKey("SpecializationGroupId")]
        public virtual SpecializationGroup? SpecializationGroup { get; set; }
        public virtual ICollection<CourseGroup> ChildCourseGroups { get; set; }
        public virtual ICollection<CurriculumCourse> CurriculumCourses { get; set; }

        [NotMapped]
        public string TypeText
        {
            get
            {
                if (Type == "r")
                {
                    return "Required";
                }
                else
                {
                    return "Elective";
                }
            }
        }

        [NotMapped]
        public string RequiredGradeText { get; set; }

        [NotMapped]
        public bool IsSelected { get; set; }

        [NotMapped]
        public string SpecializationGroupType { get; set; }

        [NotMapped]
        public long? MinorId { get; set; }

        [NotMapped]
        public long? ConcentrationId { get; set; }

        [NotMapped]
        public string FullPathName { get; set; }
    }
}