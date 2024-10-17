using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels.Prerequisites
{
    public class Corequisite : UserTimeStamp
    {
        public long Id { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }
        public long FirstCourseId { get; set; }
        public long SecondCourseId { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? ExpiredAt { get; set; }
        public long? MUICId { get; set; }

        [ForeignKey("FirstCourseId")]
        public virtual Course FirstCourse { get; set; }

        [ForeignKey("SecondCourseId")]
        public virtual Course SecondCourse { get; set; }

        [NotMapped]
        public string ExpiredAtText => ExpiredAt?.ToString(StringFormat.ShortDate);
    }
}