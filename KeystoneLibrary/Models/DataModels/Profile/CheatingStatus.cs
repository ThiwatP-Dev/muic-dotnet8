using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.MasterTables;

namespace KeystoneLibrary.Models.DataModels.Profile
{
    public class CheatingStatus : UserTimeStamp
    {
       public long Id { get; set; }
       public Guid StudentId { get; set; }
       public long RegistrationCourseId { get; set; }
       public long ExaminationTypeId { get; set; }
       public long? IncidentId { get; set; }
       public long TermId { get; set; }
       public long? FromTermId { get; set; }
       public long? ToTermId { get; set; }
       
       [StringLength(500)]
       public string? Detail { get; set; }

       [StringLength(200)]
       public string? ApprovedBy { get; set; }

       [TypeConverter(typeof(DefaultDateTimeFormat))]
       [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
       public DateTime ApprovedAt { get; set; } = DateTime.Now;

       [ForeignKey("StudentId")]
       public virtual Student Student { get; set; }

       [ForeignKey("RegistrationCourseId")]
       public virtual RegistrationCourse RegistrationCourse { get; set; }

       [ForeignKey("ExaminationTypeId")]
       public virtual ExaminationType ExaminationType { get; set; }

       [ForeignKey("IncidentId")]
       public virtual Incident? Incident { get; set; }

       [ForeignKey("TermId")]
       public virtual Term Term { get; set; }

       [ForeignKey("FromTermId")]
       public virtual Term? FromTerm { get; set; }

       [ForeignKey("ToTermId")]
       public virtual Term? ToTerm { get; set; }

       [NotMapped]
       public string ApprovedAtText => ApprovedAt.ToString(StringFormat.ShortDate);

       [NotMapped]
       public string StudentCode { get; set; }
    }
}