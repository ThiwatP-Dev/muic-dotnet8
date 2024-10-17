using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using KeystoneLibrary.Models.DataModels.Profile;

namespace KeystoneLibrary.Models.DataModels
{
    public class Receipt : UserTimeStamp
    {
        public long Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Number { get; set; }
        public int RunningNumber { get; set; } // ex. 00001
        public int Year { get; set; } // ex. 2021
        public int Month { get; set; } // ex. 04
        public long? TermId { get; set; }
        public long? InvoiceId { get; set; }
        public Guid? StudentId { get; set; }

        [StringLength(200)]
        public string? Name { get; set; } // Name of customer

        [StringLength(500)]
        public string? Address { get; set; }

        [StringLength(500)]
        public string? Address2 { get; set; }

        [StringLength(100)]
        public string? TaxNumber { get; set; }
        public bool IsCancel { get; set; }
        public bool IsEmailed { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? EmailedAt { get; set; }

        [StringLength(100)]
        public string? EmailedBy { get; set; }
        public bool IsPrint { get; set; } // For master document

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? PrintedAt { get; set; }

        [StringLength(100)]
        public string? PrintedBy { get; set; }

        [StringLength(5)]
        public string? Round { get; set; } // r = registration, a = adding
        public decimal Amount { get; set; }
        public decimal TotalScholarshipAmount { get; set; }
        public decimal TotalItemsScholarshipAmount { get; set; }
        public decimal TotalItemsDiscount { get; set; } 
        public decimal TotalDeductAmount { get; set; } 
        public decimal TotalDiscount { get; set; } 
        public bool IsVAT { get; set; }
        public decimal TotalVATAmount { get; set; }
        public decimal TotalAmount { get; set; }

        [ForeignKey("StudentId")]
        public virtual Student? Student { get; set; }

        [ForeignKey("TermId")]
        public virtual Term? Term { get; set; }

        [ForeignKey("InvoiceId")]
        public virtual Invoice? Invoice { get; set; }
        public virtual List<ReceiptItem> ReceiptItems { get; set; }
        public virtual List<ReceiptPrintLog> ReceiptPrintLogs { get; set; }
        public virtual List<ReceiptPaymentMethod> ReceiptPaymentMethods { get; set; }
        
        [NotMapped]
        public string RoundText 
        {
            get
            {
                switch (Round)
                {
                    case "r":
                        return "Registration";
                    case "a":
                        return "Adding";
                    default:
                        return "N/A";
                }
            }
        }

        [NotMapped]
        public string EmailedAtText => EmailedAt == null ? "N/A" : PrintedAt?.ToString(StringFormat.ShortDate);

        [NotMapped]
        public string PrintedAtText => PrintedAt == null ? "N/A" : PrintedAt?.ToString(StringFormat.ShortDate);

        [NotMapped]
        public string AmountText => Amount.ToString(StringFormat.TwoDecimal);

        [NotMapped]
        public string TotalVATAmountText => TotalVATAmount.ToString(StringFormat.TwoDecimal);

        [NotMapped]
        public string TotalAmountText => TotalAmount.ToString(StringFormat.TwoDecimal);

        [NotMapped]
        public int PrintLogsCount => ReceiptPrintLogs == null ? 0 : ReceiptPrintLogs.Count;

        // [NotMapped]
        // public decimal ScholarshipAmount => ReceiptItems?.Sum(x => (decimal?)x.ScholarshipAmount) ?? 0;

        [NotMapped]
        public string ScholarshipAmountText => TotalScholarshipAmount.ToString(StringFormat.Money);

        /// <summary>
        /// TotalDiscount [Discount specific for this Invoice]
        /// +
        /// TotalItemsDiscount [Sum of Discount for each Invoice Item]
        /// </summary>
        [NotMapped]
        public string AllDiscountAmountText => (TotalDiscount + TotalItemsDiscount).ToString(StringFormat.Money);

        [NotMapped]
        public string PrintedByFullName { get; set; }

        [NotMapped]
        public decimal ExtraFee { get; set; }

        [NotMapped]
        public string InvoicePaymentMethod { get; set; }
    }
}