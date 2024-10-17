using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using KeystoneLibrary.Models.DataModels.Profile;

namespace KeystoneLibrary.Models.DataModels
{
    public class Installment : UserTimeStamp
    {
        public long Id { get; set; }
        public Guid StudentId { get; set; }
        public long TermId { get; set; }

        [StringLength(1000)]
        public string? Remark { get; set; }

        [JsonIgnore]
        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        [ForeignKey("TermId")]
        public virtual Term Term { get; set; }
        public virtual List<InstallmentTransaction> InstallmentTransactions { get; set; }
    }
}