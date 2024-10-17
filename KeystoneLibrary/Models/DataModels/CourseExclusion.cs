using KeystoneLibrary.Models.DataModels.Curriculums;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels
{
    public class CourseExclusion : UserTimeStamp
    {
        public long Id { get; set; }
        public long CourseId { get; set; }
        public long ExcludingCourseId { get; set; }
        public long CurriculumVersionId { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime EffectivedAt { get; set; } = DateTime.Now;

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? EndedAt { get; set; }

        [StringLength(1000)]
        public string? Remark { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }

        [ForeignKey("ExcludingCourseId")]
        public virtual Course ExcludingCourse { get; set; }

        [ForeignKey("CurriculumVersionId")]
        public virtual CurriculumVersion CurriculumVersion { get; set; }

        [NotMapped]
        public string EffectivedAtText => EffectivedAt.ToString(StringFormat.ShortDate);
        
        [NotMapped]
        public string EndedAtText => EndedAt == null ? "" : EndedAt.Value.ToString(StringFormat.ShortDate);

        [NotMapped]
        public List<long> ExcludingCourseIds { get; set; }

        [NotMapped]
        public long AcademicLevelId { get; set; }
        
        [NotMapped]
        public long CurriculumId { get; set; }
    }
}