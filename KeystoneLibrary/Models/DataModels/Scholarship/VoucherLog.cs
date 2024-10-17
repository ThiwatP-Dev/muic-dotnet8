using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.Fee;

namespace KeystoneLibrary.Models.DataModels.Scholarship
{
    public class VoucherLog : UserTimeStamp
    {
        public long Id { get; set; }
        public long VoucherId { get; set; }
        public long FeeItemId { get; set; }
        public decimal Amount { get; set; }

        [ForeignKey("VoucherId")]
        public virtual Voucher Voucher { get; set; }

        [ForeignKey("FeeItemId")]
        public virtual FeeItem FeeItem { get; set; }

        [NotMapped]
        public string AmountText => Amount.ToString(StringFormat.Money);
    }
}