using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels.Withdrawals
{
    public class WithdrawalLog : UserTimeStamp
    {
        public long Id { get; set; }
        public long WithdrawalId { get; set; }

        [StringLength(5)]
        public string? Status { get; set; } // w = waiting, a = approved, r = reject, c = cancel

        [StringLength(1000)]
        public string? Remark { get; set; }

        [ForeignKey("WithdrawalId")]
        public virtual Withdrawal Withdrawal { get; set; }

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
    }
}