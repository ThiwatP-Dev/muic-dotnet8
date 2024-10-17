using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.MasterTables;
using System.ComponentModel;

namespace KeystoneLibrary.Models.DataModels
{
    public class AcademicCalendar : UserTimeStamp
    {
        public long Id { get; set; }
        public long EventId { get; set; }
        public long? AcademicLevelId { get; set; } // Null is for all academic level

        [StringLength(500)]
        public string? Remark { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy HH:mm}", ApplyFormatInEditMode=true)]
        public DateTime StartedAt { get; set; } = DateTime.Now;

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy HH:mm}", ApplyFormatInEditMode=true)]
        public DateTime EndedAt { get; set; } = DateTime.Now;

        [StringLength(2)]
        public string? ViewLevel { get; set; } // Admission, Student, Instructor, Guardian, Admin

        [JsonIgnore]
        [ForeignKey("EventId")]
        public virtual Event Event { get; set; }
        
        [JsonIgnore]
        [ForeignKey("AcademicLevelId")]
        public virtual AcademicLevel? AcademicLevel { get; set; }

        [NotMapped]
        public long? EventCategoryId { get; set; }
    }
}