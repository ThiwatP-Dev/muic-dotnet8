using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.MasterTables;

namespace KeystoneLibrary.Models.DataModels.Profile
{
    public class StudentIncident : UserTimeStamp
    {
        public long Id { get; set; }
        public Guid StudentId { get; set; }
        public long IncidentId { get; set; }
        public long TermId { get; set; }
        public bool LockedDocument { get; set; }
        public bool LockedRegistration { get; set; }
        public bool LockedPayment { get; set; }
        public bool LockedVisa { get; set; }
        public bool LockedGraduation { get; set; }
        public bool LockedChangeFaculty { get; set; }
        public bool LockedSignIn { get; set; }

        [StringLength(500)]
        public string? Remark { get; set; }

        [Required]
        [StringLength(450)]
        public string? ApprovedBy { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime ApprovedAt { get; set; } = DateTime.UtcNow;
        
        [JsonIgnore]
        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        [ForeignKey("IncidentId")]
        public virtual Incident Incident { get; set; }

        [JsonIgnore]
        [ForeignKey("TermId")]
        public virtual Term Term { get; set; }

        [NotMapped]
        public long SignatoryId { get; set; }

        [NotMapped]
        public string UserId { get; set; }

        [NotMapped]
        public string ApprovedAtText => ApprovedAt.ToString(StringFormat.ShortDate);
    }
}
