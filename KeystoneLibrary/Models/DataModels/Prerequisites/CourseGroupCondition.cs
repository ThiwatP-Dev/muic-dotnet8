using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.Curriculums;

namespace KeystoneLibrary.Models.DataModels.Prerequisites
{
    public class CourseGroupCondition : UserTimeStamp
    {
        public long Id { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }
        public long CourseGroupId { get; set; }
        public int Credit { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? ExpiredAt { get; set; }
        public long? MUICId { get; set; }

        [ForeignKey("CourseGroupId")]
        public virtual CourseGroup CourseGroup { get; set; }

        [NotMapped]
        public long CurriculumVersionId { get; set; }

        [NotMapped]
        public long CurriculumId { get; set; }

        [NotMapped]
        public long AcademicLevelId { get; set; }

        [NotMapped]
        public string ExpiredAtText => ExpiredAt?.ToString(StringFormat.ShortDate);

        [NotMapped]
        public string CourseGroupConditionName => $"{ CourseGroup?.NameEn } { Credit } credits";
        // notmapp --> name --> Major requires 20 credit
    }
}