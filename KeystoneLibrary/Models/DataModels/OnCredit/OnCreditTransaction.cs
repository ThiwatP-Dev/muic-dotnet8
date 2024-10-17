using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels
{
    public class OnCreditTransaction : UserTimeStamp
    {
        public long Id { get; set; }
        public long OnCreditId { get; set; }
        public decimal TotalAmount { get; set; }

        [StringLength(10)]
        public string? Channel { get; set; }
        public bool IsPaid { get; set; }
        public bool IsTransfered { get; set; }
        public DateTime? PaidAt { get; set; }

        [ForeignKey("OnCreditId")]
        public virtual OnCredit OnCredit { get; set; }
    }
}