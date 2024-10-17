using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.MasterTables;

namespace KeystoneLibrary.Models.DataModels
{
    public class SectionDetail : UserTimeStamp
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public long SectionId { get; set; }
        public long TeachingTypeId { get; set; }
        public long? RoomId { get; set; }
        public int Day { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public long? InstructorId { get; set; }

        [StringLength(100)]
        public string? InstructorIds { get; set; } // delete for muic

        [StringLength(1000)]
        public string? Remark { get; set; }

        [NotMapped]
        public List<long> Instructors { get; set; }
        
        [JsonIgnore]
        [ForeignKey("SectionId")]
        public virtual Section Section { get; set; }

        [JsonIgnore]
        public virtual List<InstructorSection> InstructorSections { get; set; }

        [JsonIgnore]
        [ForeignKey("InstructorId")]
        public virtual Instructor? Instructor { get; set; }

        [JsonIgnore]
        [ForeignKey("TeachingTypeId")]
        public virtual TeachingType TeachingType { get; set; }
        
        [JsonIgnore]
        [ForeignKey("RoomId")]
        public virtual Room? Room { get; set; }

        [NotMapped]
        public string Time => $"{ StartTime.ToString(StringFormat.TimeSpan) } - { EndTime.ToString(StringFormat.TimeSpan) }";

        [NotMapped]
        public string StartTimeText => $"{ StartTime.ToString(StringFormat.TimeSpan) }";

        [NotMapped]
        public string EndTimeText => $"{ EndTime.ToString(StringFormat.TimeSpan) }";
        
        [NotMapped]
        public string Dayofweek
        {
            get
            {
                if (Day != -1)
                {
                    var day = Enum.GetName(typeof(DayOfWeek),Day)?.Substring(0,3).ToUpper();
                    return day;
                }
                return "";
            }
        }

        [NotMapped]
        public string DayofweekAndTime => $"{ Dayofweek } { Time }";

        [NotMapped]
        public List<long> InstructorIdList => string.IsNullOrEmpty(InstructorIds) || InstructorIds == "null" ? new List<long>() : JsonConvert.DeserializeObject<List<long>>(InstructorIds);

        [NotMapped]
        public TimeSpan? StartTimeNullAble { get; set; }

        [NotMapped]
        public TimeSpan? EndTimeNullAble { get; set; }

        [NotMapped]
        public long TempId { get; set; }
    }
}