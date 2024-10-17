using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.Curriculums;
using KeystoneLibrary.Models.DataModels.MasterTables;

namespace KeystoneLibrary.Models.DataModels.Prerequisites
{
    public class CreditCondition : UserTimeStamp
    {
        public long Id { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }
        public int Credit { get; set; }
        public long? TeachingTypeId { get; set; } // course type in skyplus
        public long? CurriculumVersionId { get; set; }
        public long? CourseGroupId { get; set; }

        [StringLength(20)]
        public string? CreditType { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? ExpiredAt { get; set; }
        public long? MUICId { get; set; }

        [ForeignKey("CurriculumVersionId")]
        public virtual CurriculumVersion? CurriculumVersion { get; set; }

        [ForeignKey("CourseGroupId")]
        public virtual CourseGroup? CourseGroup { get; set; }

        [ForeignKey("TeachingTypeId")]
        public virtual TeachingType? TeachingType { get; set; }

        [NotMapped]
        public string CreditConditionName => $"{ CurriculumVersion?.NameEn } { Credit } credits";

        [NotMapped]
        public string CreditTypeText
        {
            get
            {
                switch (CreditType)
                {
                    case "all":
                        return "All";
                    case "academic":
                        return "Academic";
                    case "lecture":
                        return "Lecture";
                    case "lab":
                        return "Lab";
                    case "self":
                        return "Self Study";
                    case "registration":
                        return "Registration";
                    case "free":
                        return "Free Elective";
                    default:
                        return "";
                }
            }
        }

        [NotMapped]
        public string ExpiredAtText => ExpiredAt?.ToString(StringFormat.ShortDate);

        [NotMapped]
        public long AcademicLevelId { get; set; }

        [NotMapped]
        public long CurriculumId { get; set; }
    }
}