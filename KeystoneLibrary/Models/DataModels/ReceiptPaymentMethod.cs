using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.MasterTables;

namespace KeystoneLibrary.Models.DataModels
{
    public class ReceiptPaymentMethod
    {
        public long Id { get; set; }
        public long ReceiptId { get; set; }
        public long PaymentMethodId { get; set; } // cash, card, e-payment (BAY_EP), ture money (TMW)
        public decimal Amount { get; set; }

        [ForeignKey("ReceiptId")]
        public virtual Receipt Receipt { get; set; }

        [ForeignKey("PaymentMethodId")]
        public virtual PaymentMethod PaymentMethod { get; set; }

        [NotMapped]
        public string AmountText => Amount.ToString(StringFormat.TwoDecimal);
    }
}