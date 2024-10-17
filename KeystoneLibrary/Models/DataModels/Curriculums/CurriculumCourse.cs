using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.MasterTables;

namespace KeystoneLibrary.Models.DataModels.Curriculums
{
    public class CurriculumCourse : UserTimeStamp
    {
        public long Id { get; set; }
        public long CourseId { get; set; }
        public long CourseGroupId { get; set; }
        public bool IsRequired { get; set; }
        public long? RequiredGradeId { get; set; }
        public long GradeTemplateId { get; set; }
        public bool IsMustTake { get; set; }
        public int? Sequence { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }

        [ForeignKey("GradeTemplateId")]
        public virtual GradeTemplate GradeTemplate { get; set; }

        [JsonIgnore]
        [ForeignKey("CourseGroupId")]
        public virtual CourseGroup CourseGroup { get; set; }

        [ForeignKey("RequiredGradeId")]
        public virtual Grade Grade { get; set; }

        [NotMapped]
        public long? CurriculumVersionId { get; set; }
        
        [NotMapped]
        public string IsRequiredText { get; set; }

        [NotMapped]
        public string RequiredGradeText { get; set; }

        [NotMapped]
        public string RegistrationGradeText { get; set; }
        [NotMapped]
        public long? SpecializationGroupId { get; set; }

        [NotMapped]
        public string SpecializationGroupType { get; set; }
    }
}