using KeystoneLibrary.Models.DataModels.Profile;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels.Scholarship
{
    public class FinancialTransaction : UserTimeStamp // FinancialTransaction
    {
        public long Id { get; set; }
        public Guid StudentId { get; set; }
        public long StudentScholarshipId { get; set; }
        public long TermId { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime PaidAt { get; set; }
        public long? ReceiptId { get; set; } 
        public long? ReceiptItemId { get; set; } // 12000
        public long? VoucherId { get; set; } // voucher transaction
        public decimal UsedScholarship { get; set; } // add: +6000 | refund: -6000
        public decimal PersonalPay { get; set; } // 6000
        public decimal Balance { get; set; } // add: remaining limitedamount - usedscholarship | refund: remaining limitedamount - (-usedscholarship)

        [StringLength(5)]
        public string? Type { get; set; } // a = add, r = refund, c = cancel receipt

        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        [ForeignKey("StudentScholarshipId")]
        public virtual ScholarshipStudent ScholarshipStudent { get; set; }

        [ForeignKey("TermId")]
        public virtual Term Term { get; set; }

        [ForeignKey("ReceiptId")]
        public virtual Receipt? Receipt { get; set; }

        [ForeignKey("ReceiptItemId")]
        public virtual ReceiptItem? ReceiptItem { get; set; }

        [ForeignKey("VoucherId")]
        public virtual Voucher? Voucher { get; set; }

        [NotMapped]
        public string PaidAtText => PaidAt.ToString(StringFormat.ShortDate);

        [NotMapped]
        public string BalanceText => Balance.ToString(StringFormat.Money);

        [NotMapped]
        public string UsedScholarshipText => UsedScholarship.ToString(StringFormat.Money);

        [NotMapped]
        public string PersonalPayText => PersonalPay.ToString(StringFormat.Money);

        [NotMapped]
        public string TypeText
        {
            get
            {
                switch(Type)
                {
                    case "a":
                        return "Active";
                    case "r":
                        return "Refund";
                    case "c":
                        return "Cancel Receipt";
                    default:
                        return "N/A";
                }
            }
        }
    }
}