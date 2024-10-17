using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels.Profile
{
    public class ExtendedStudent : UserTimeStamp
    {
        public long Id { get; set; }
        public Guid StudentId { get; set; }
        public long TermId { get; set; } // current term
        public decimal StudiedYear { get; set; }
        public decimal Credit { get; set; } // credit complete when save

        [StringLength(1000)]
        public string? Remark { get; set; }
        public bool SendEmail { get; set; }
        
        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        [ForeignKey("TermId")]
        public virtual Term Term { get; set; }

        [NotMapped]
        public string CreditText => Credit.ToString(StringFormat.TwoDecimal);
    }
}
