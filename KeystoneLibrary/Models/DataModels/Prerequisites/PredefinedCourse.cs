using KeystoneLibrary.Models.DataModels.Curriculums;
using KeystoneLibrary.Models.DataModels.MasterTables;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels.Prerequisites
{
    public class PredefinedCourse : UserTimeStamp
    {
        public long Id { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }
        public long CurriculumVersionId { get; set; }

        [Required]
        [StringLength(5)]
        public string RequiredType { get; set; }
        public long FirstCourseId { get; set; }
        public long SecondCourseId { get; set; }
        public long? GradeId { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? ExpiredAt { get; set; }

        [ForeignKey("CurriculumVersionId")]
        public virtual CurriculumVersion CurriculumVersion { get; set; }

        [ForeignKey("FirstCourseId")]
        public virtual Course FirstCourse { get; set; }

        [ForeignKey("SecondCourseId")]
        public virtual Course SecondCourse { get; set; }

        [ForeignKey("GradeId")]
        public virtual Grade? Grade { get; set; }

        [NotMapped]
        public string RequiredTypeText
        {
            get
            {
                switch (RequiredType)
                {
                    case "rq":
                        return "Required"; // ต้องลงต่อเนื่อง
                    case "rc":
                        return "Recomment"; // ขึ้นโชว์แต่ไม่บังคับ
                    default:
                        return "N/A";
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