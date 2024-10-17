using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.Profile;

namespace KeystoneLibrary.Models.DataModels
{
    public class OnCredit : UserTimeStamp
    {
        public long Id { get; set; }
        public Guid StudentId { get; set; }
        public long TermId { get; set; }
        public int TotalMonth { get; set; }
        public decimal AmountPerMonth { get; set; }
        public decimal TotalAmount { get; set; }

        [StringLength(1000)]
        public string? Remark { get; set; }

        [JsonIgnore]
        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        [ForeignKey("TermId")]
        public virtual Term Term { get; set; }
        public virtual List<OnCreditTransaction> OnCreditTransactions { get; set; }
    }
}