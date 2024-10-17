using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.Curriculums;

namespace KeystoneLibrary.Models.DataModels.Prerequisites
{
    public class Prerequisite : UserTimeStamp
    {
        public long Id { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [StringLength(50)]
        public string ConditionType { get; set; } // and, or, gpa, credit, grade, coursegroup, term, totalcoursegroup
        public long CourseId { get; set; }
        public long? CurriculumVersionId { get; set; }
        public long ConditionId { get; set; }
        
        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? ExpiredAt { get; set; }
        public long? MUICId { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }

        [ForeignKey("CurriculumVersionId")]
        public virtual CurriculumVersion? CurriculumVersion { get; set; }

        [NotMapped]
        public string PrerequisiteName { get; set; }
    }
}