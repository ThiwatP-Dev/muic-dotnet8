using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels
{
    public class InvoiceDeductTransaction : UserTimeStamp
    {
        public long Id { get; set; }
        public long? InvoiceId { get; set; }
        public long? InvoiceItemId { get; set; }
        public long InvoiceCreditNoteId { get; set; }

        [StringLength(50)]
        public string? Type { get; set; } // n = New, u = Using, c = Cancel
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }

        [ForeignKey("InvoiceId")]
        public virtual Invoice? Invoice { get; set; }

        [ForeignKey("InvoiceItemId")]
        public virtual InvoiceItem? InvoiceItem { get; set; }

        [ForeignKey("InvoiceCreditNoteId")]
        public virtual Invoice InvoiceCreditNote { get; set; }

        [NotMapped]
        public string AmountText => Amount.ToString(StringFormat.Money);

        [NotMapped]
        public string BalanceText => Balance.ToString(StringFormat.Money);
    }
}