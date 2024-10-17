using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace KeystoneLibrary.Models.DataModels
{
    public class CourseEquivalent : UserTimeStamp
    {
        public long Id { get; set; }
        public long CourseId { get; set; }
        public long EquilaventCourseId { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime EffectivedAt { get; set; } = DateTime.Now;

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? EndedAt { get; set; }

        [StringLength(1000)]
        public string? Remark { get; set; }
        public long? MUICId { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }

        [ForeignKey("EquilaventCourseId")]
        public virtual Course EquilaventCourse { get; set; }

        [NotMapped]
        public string EffectivedAtText => EffectivedAt.ToString(StringFormat.ShortDate);
        
        [NotMapped]
        public string EndedAtText => EndedAt == null ? "" : EndedAt.Value.ToString(StringFormat.ShortDate);
    }
}