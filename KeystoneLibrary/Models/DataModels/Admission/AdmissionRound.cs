using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.MasterTables;
using System.ComponentModel;

namespace KeystoneLibrary.Models.DataModels.Admission
{
    public class AdmissionRound : UserTimeStamp
    {
        public long Id { get; set; }
        public long AcademicLevelId { get; set; }
        public long AdmissionTermId { get; set; }
        public int Round { get; set; } = 1;

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? StartedAt { get; set; } = DateTime.Now;

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? EndedAt { get; set; } = DateTime.Now;

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? FirstClassAt { get; set; } = DateTime.Now; // Admission Date

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? StudentCardCreatedAt { get; set; } = DateTime.Now;

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? TemporaryCardExpiredAt { get; set; } = DateTime.Now;

        [StringLength(1000)]
        public string? Remark { get; set; }

        [ForeignKey("AcademicLevelId")]
        public virtual AcademicLevel AcademicLevel { get; set; }
        
        [ForeignKey("AdmissionTermId")]
        public virtual Term AdmissionTerm { get; set; }

        [NotMapped]
        public string StartedAtText => StartedAt?.ToString(StringFormat.ShortDate);
        
        [NotMapped]
        public string EndedAtText => EndedAt?.ToString(StringFormat.ShortDate);

        [NotMapped]
        public string FirstClassAtText => FirstClassAt?.ToString(StringFormat.ShortDate);

        [NotMapped]
        public string TemporaryCardExpiredAtText => TemporaryCardExpiredAt?.ToString(StringFormat.ShortDate);

        [NotMapped]
        public string StudentCardCreatedAtText => StudentCardCreatedAt?.ToString(StringFormat.ShortDate);

        [NotMapped]
        public string TermRoundText => $"{ AdmissionTerm?.TermText ?? "" } Round { Round }";
    }   
}