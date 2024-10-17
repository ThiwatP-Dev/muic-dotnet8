using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace KeystoneLibrary.Models.DataModels
{
    public class InstructorSection : UserTimeStamp
    {
        public long Id { get; set; }
        public long SectionDetailId { get; set; }
        public long InstructorId { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? StartedAt { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? EndedAt { get; set; }
        public decimal Hours { get; set; }
        
        [JsonIgnore]
        [ForeignKey("SectionDetailId")]
        public virtual SectionDetail SectionDetail { get; set; }

        [JsonIgnore]
        [ForeignKey("InstructorId")]
        public virtual Instructor Instructor { get; set; }

        [NotMapped]
        public string Period => $"{ StartedAt?.ToString(StringFormat.ShortDate) } - { EndedAt?.ToString(StringFormat.ShortDate) }";

        [NotMapped]
        public string HoursText => $"{ Hours.ToString(StringFormat.GeneralDecimal) }";
    }
}