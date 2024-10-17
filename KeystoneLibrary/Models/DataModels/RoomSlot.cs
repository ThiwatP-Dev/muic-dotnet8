using KeystoneLibrary.Models.DataModels.MasterTables;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels
{
    public class RoomSlot : UserTimeStamp
    {
        public long Id { get; set; }
        public long TermId { get; set; }
        public long RoomId { get; set; }
        public int Day { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        [StringLength(5)]
        public string? UsingType { get; set; } // s = studying, a = activity, e = examination, m = meeting
        public long? SectionSlotId { get; set; }
        public long? ExaminationReservationId { get; set; }
        public long? RoomReservationId { get; set; }
        public bool IsCancel { get; set; }

        [JsonIgnore]
        [ForeignKey("TermId")]
        public virtual Term Term { get; set; }
        
        [JsonIgnore]
        [ForeignKey("RoomId")]
        public virtual Room Room { get; set; }

        [ForeignKey("SectionSlotId")]
        public virtual SectionSlot? SectionSlot { get; set; }

        [ForeignKey("ExaminationReservationId")]
        public virtual ExaminationReservation? ExaminationReservation { get; set; }

        [ForeignKey("RoomReservationId")]
        public virtual RoomReservation? RoomReservation { get; set; }
        
        [NotMapped]
        public string Dayofweek
        {
            get
            {
               var day = Enum.GetName(typeof(DayOfWeek),Day).Substring(0,3).ToUpper();
               return day ;
            }
        }

        [NotMapped]
        public string DayofweekAndTime => $"{ Dayofweek } { Time }";

        [NotMapped]
        public string StartTimeText => StartTime.ToString(StringFormat.TimeSpan);

        [NotMapped]
        public string EndTimeText => EndTime.ToString(StringFormat.TimeSpan);

        [NotMapped]
        public string DateText => Date.ToString(StringFormat.ShortDate);

        [NotMapped]
        public string Time => $"{ StartTimeText } - { EndTimeText }";

        [NotMapped]
        public string DateAndTime => $"{ Dayofweek } { DateText } { Time }";

        [NotMapped]
        public bool IsSameDate => SectionSlot?.CreatedAt == Date
                                  || ExaminationReservation?.CreatedAt.Date == Date.Date
                                  || RoomReservation?.CreatedAt.Date == Date.Date ? true : false;

        [NotMapped]
        public string UsingTypeText
        {
            get
            {
                switch (UsingType)
                {
                    case "s":
                        return "Studying";
                    case "a":
                        return "Activity";
                    case "e":
                        return "Examination";
                    case "m":
                        return "Meeting";
                    default:
                        return "N/A";
                }
            }
        }

        [NotMapped]
        public double TotalHours => (EndTime - StartTime).TotalHours;
    }
}