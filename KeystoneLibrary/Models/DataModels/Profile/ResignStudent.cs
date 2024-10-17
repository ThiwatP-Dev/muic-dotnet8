using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.MasterTables;
using System.ComponentModel;

namespace KeystoneLibrary.Models.DataModels.Profile
{
    public class ResignStudent : UserTimeStamp
    {
        public long Id { get; set; }
        public Guid StudentId { get; set; }
        public long TermId { get; set; } // current term
        public long EffectiveTermId { get; set; } // current term
        public long ResignReasonId { get; set; }

        [StringLength(1000)]
        public string? Remark { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime ApprovedAt { get; set; } = DateTime.Now;

        [StringLength(200)]
        public string? ApprovedBy { get; set; }
        
        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        [ForeignKey("TermId")]
        public virtual Term Term { get; set; }

        [ForeignKey("EffectiveTermId")]
        public virtual Term EffectiveTerm { get; set; }

        [ForeignKey("ResignReasonId")]
        public virtual ResignReason ResignReason { get; set; }

        [NotMapped]
        public string ApprovedAtText => ApprovedAt.ToString(StringFormat.ShortDate);
    }
}
