using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.MasterTables;
using System.ComponentModel;
using KeystoneLibrary.Models.DataModels.Profile;

namespace KeystoneLibrary.Models.DataModels.Withdrawals
{
    public class Withdrawal : UserTimeStamp
    {
        public long Id { get; set; }
        public long RegistrationCourseId { get; set; }

        [Required]
        [StringLength(5)]
        public string Type { get; set; } // p = petition, u = uspark, d = debarment

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime RequestedAt { get; set; }

        [StringLength(500)]
        public string? Remark { get; set; }
        
        // Request by
        public long? InstructorId { get; set; }
        public Guid? StudentId { get; set; }
        public long? SignatoryId { get; set; }

        [Required]
        [StringLength(5)]
        public string Status { get; set; } // w = waiting, a = approved, r = reject, c = cancel

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
                        return "Cancel";
                    default:
                        return "N/A";
                }
            }
        }

        [NotMapped]
        public string TypeText 
        { 
            get
            {
                switch (Type)
                {
                    case "p":
                        return "Petition";
                    case "u":
                        return "Application";
                    case "d":
                        return "Debarment";
                    default:
                        return "";
                }
            }
        }

        [NotMapped]
        public string RequestedBy
        {
            get
            {
                switch (Type)
                {
                    case "u":
                        return Student == null ? "" : Student.CodeAndName;
                    case "d":
                        return Instructor == null ? "" : Instructor.CodeAndName;
                    default:
                        return "";
                }
            }
        }
        
        [ForeignKey("RegistrationCourseId")]
        public virtual RegistrationCourse RegistrationCourse { get; set; }

        [ForeignKey("StudentId")]
        public virtual Student? Student { get; set; }
        
        [JsonIgnore]
        [ForeignKey("InstructorId")]
        public virtual Instructor? Instructor { get; set; }

        [ForeignKey("SignatoryId")]
        public virtual Signatory? Signatory { get; set; }

        [NotMapped]
        public string RequestedAtText => RequestedAt.ToLocalTime().ToString(StringFormat.ShortDateTime);
    }
}