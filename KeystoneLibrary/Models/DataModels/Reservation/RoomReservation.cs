using KeystoneLibrary.Models.DataModels.MasterTables;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels
{
    public class RoomReservation : UserTimeStamp
    {
        public long Id { get; set; }
        public long TermId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [Required]
        [StringLength(100)]
        public string PhoneNumber { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }
        public long RoomId { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime DateFrom { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime DateTo { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(1000)]
        public string? Remark { get; set; }

        [StringLength(5)]
        public string? UsingType { get; set; } // s = studying, a = activity, e = examination, m = meeting

        [StringLength(5)]
        public string? Status { get; set; } // w = waiting, a = approved, r = reject, c = cancel

        [StringLength(5)]
        public string? SenderType { get; set; } // s = student, i = instructor
        public bool IsSunday { get; set; } = true;
        public bool IsMonday { get; set; } = true;
        public bool IsTuesday { get; set; } = true;
        public bool IsWednesday { get; set; } = true;
        public bool IsThursday { get; set; } = true;
        public bool IsFriday { get; set; } = true;
        public bool IsSaturday { get; set; } = true;

        [StringLength(200)]
        public string? ApprovedBy { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? ApprovedAt { get; set; }

        [JsonIgnore]
        [ForeignKey("TermId")]
        public virtual Term Term { get; set; }
        
        [JsonIgnore]
        [ForeignKey("RoomId")]
        public virtual Room Room { get; set; }

        [NotMapped]
        public string DateFromText => DateFrom.ToString(StringFormat.ShortDate);

        [NotMapped]
        public string DateToText => DateTo.ToString(StringFormat.ShortDate);

        [NotMapped]
        public string StartTimeText { get; set; }

        [NotMapped]
        public string EndTimeText { get; set; }
        [NotMapped]
        public string StartTimeString => $"{ StartTime.ToString(StringFormat.TimeSpan) }";

        [NotMapped]
        public string EndTimeString => $"{ EndTime.ToString(StringFormat.TimeSpan) }";

        [NotMapped]
        public string TimeDisplay => $"{ StartTimeString } - { EndTimeString }";

        [NotMapped]
        public string SenderTypeText
        {
            get
            {
                switch (SenderType)
                {
                    case "s":
                        return "Student";
                    case "i":
                        return "Instructor";
                    case "a":
                        return "Admin/Staff";
                    default:
                        return "N/A";
                }
            }
        }

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
        public long AcademicLevelId { get; set; }

        [NotMapped]
        public string ApprovedAtText { get; set; }
    }
}