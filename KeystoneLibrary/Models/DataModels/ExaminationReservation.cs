using KeystoneLibrary.Models.DataModels.MasterTables;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels
{
    public class ExaminationReservation : UserTimeStamp
    {
        public long Id { get; set; }
        public long TermId { get; set; }
        public long SectionId { get; set; }
        public long? InstructorId { get; set; }
        public long ExaminationTypeId { get; set; }
        public long? RoomId { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime Date { get; set; } = DateTime.Now;
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool UseProctor { get; set; }
        public int TotalProctor { get; set; }
        public bool AbsentInstructor { get; set; } // instructor proctor by themselves or not
        public bool AllowBooklet { get; set; }
        public bool AllowCalculator { get; set; }
        public bool AllowOpenbook { get; set; }
        public bool AllowAppendix { get; set; }

        [StringLength(1000)]
        public string? StudentRemark { get; set; } // show in student application

        [StringLength(1000)]
        public string? ProctorRemark { get; set; } // show to proctor only

        [StringLength(5)]
        public string? Status { get; set; } // w = waiting, a = approved, r = reject

        [StringLength(5)]
        public string? ExamStatus { get; set; } // w = waiting, p = passed, c = cancel

        [StringLength(5)]
        public string? SenderType { get; set; } // s = student, api or i = instructor, a = admin

        [StringLength(200)]
        public string? ApprovedBy { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? ApprovedAt { get; set; }

        [JsonIgnore]
        [ForeignKey("TermId")]
        public virtual Term Term { get; set; }

        [JsonIgnore]
        [ForeignKey("SectionId")]
        public virtual Section Section { get; set; }
        
        [JsonIgnore]
        [ForeignKey("InstructorId")]
        public virtual Instructor? Instructor { get; set; }
        
        [JsonIgnore]
        [ForeignKey("ExaminationTypeId")]
        public virtual ExaminationType ExaminationType { get; set; }
        
        [JsonIgnore]
        [ForeignKey("RoomId")]
        public virtual Room? Room { get; set; }

        [NotMapped]
        public long AcademicLevelId { get; set; }

        [NotMapped]
        public long CourseId { get; set; }

        [NotMapped]
        public string StatusText
        {
            get 
            {
                switch (Status)
                {
                    case "w":
                    return "Waiting";

                    case "a":
                    return "Approved";

                    case "r":
                    return "Reject";

                    case "c":
                    return "Confirm";

                    default:
                    return "N/A";
                }
            }
        }

        public string DateText => Date == new DateTime() ? "-" : Date.ToString(StringFormat.ShortDate);

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
                    case "api":
                        return "Instructor";
                    case "a":
                        return "Admin/Staff";
                    default:
                        return "N/A";
                }
            }
        }
        
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
        public string StartTimeText { get; set; }

        [NotMapped]
        public string EndTimeText { get; set; }
        [NotMapped]
        public string StartTimeString => StartTime.ToString(StringFormat.TimeSpan);

        [NotMapped]
        public string EndTimeString => EndTime.ToString(StringFormat.TimeSpan);

        [NotMapped]
        public string Time => StartTimeString == new TimeSpan().ToString(StringFormat.TimeSpan) ? "-" : $"{ StartTimeString } - { EndTimeString }";
    }
}