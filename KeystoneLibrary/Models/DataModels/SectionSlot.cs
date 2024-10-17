using KeystoneLibrary.Models.DataModels.MasterTables;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels
{
    public class SectionSlot : UserTimeStamp
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public long SectionId { get; set; }
        public long TeachingTypeId { get; set; }
        public long? RoomId { get; set; }
        public int Day { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public long? InstructorId { get; set; }

        [StringLength(5)]
        public string? Status { get; set; } // w = waiting, p = passed, c = cancel
        public bool IsMakeUpClass { get; set; }

        [StringLength(1000)]
        public string? Remark { get; set; }
        public long? MUICId { get; set; }
        
        [JsonIgnore]
        [ForeignKey("SectionId")]
        public virtual Section Section { get; set; }

        [JsonIgnore]
        [ForeignKey("TeachingTypeId")]
        public virtual TeachingType TeachingType { get; set; }
        
        [JsonIgnore]
        [ForeignKey("RoomId")]
        public virtual Room? Room { get; set; }
        
        [JsonIgnore]
        [ForeignKey("InstructorId")]
        public virtual Instructor? Instructor { get; set; }

        [NotMapped]
        public string Time => $"{ StartTime.ToString(StringFormat.TimeSpan) } - { EndTime.ToString(StringFormat.TimeSpan) }";

        [NotMapped]
        public string StartTimeText => $"{ StartTime.ToString(StringFormat.TimeSpan) }";

        [NotMapped]
        public string EndTimeText => $"{ EndTime.ToString(StringFormat.TimeSpan) }";

        [NotMapped]
        public string DateText => $"{ Date.ToString(StringFormat.ShortDate) }";
        
        [NotMapped]
        public string Dayofweek
        {
            get
            {
               var day = Enum.GetName(typeof(DayOfWeek), Date.DayOfWeek).Substring(0, 3).ToUpper();
               return day ;
            }
        }

        [NotMapped]
        public string DayofweekAndTime => $"{ Dayofweek } { Time }";

        [NotMapped]
        public string StatusText
        {
            get
            {
                switch (Status)
                {
                    case "w":
                        return "Waiting";
                    case "p":
                        return "Passed";
                    case "c":
                        return "Cancel";
                    default:
                        return "N/A";
                }
            }
        }

        [NotMapped]
        public string IsMakeUpClassString { get; set; }
    }
}