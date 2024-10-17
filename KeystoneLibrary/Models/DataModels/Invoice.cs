using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using KeystoneLibrary.Models.DataModels.Profile;

namespace KeystoneLibrary.Models.DataModels
{
    public class Invoice : UserTimeStamp
    {
        public long Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Number { get; set; }
        public int RunningNumber { get; set; } // ex. 00001
        public int Year { get; set; } // ex. 2021
        public int Month { get; set; } // ex. 04
        public long? TermId { get; set; }
        public Guid? StudentId { get; set; }

        [StringLength(200)]
        public string? Name { get; set; } // Name of customer

        [StringLength(500)]
        public string? Address { get; set; }

        [StringLength(500)]
        public string? Address2 { get; set; }
        public bool IsCancel { get; set; }
        public string? CancelRemark { get; set; }
        public bool IsPaid { get; set; }
        public bool IsPrint { get; set; } 
        public bool IsConfirm { get; set; } 

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? PrintedAt { get; set; }

        [StringLength(100)]
        public string? PrintedBy { get; set; }

        public DateTime? PaymentExpireAt { get; set; }
        public decimal Amount { get; set; } 

        public decimal TotalScholarshipAmount { get; set; }
        public decimal TotalItemsScholarshipAmount { get; set; }
        public decimal TotalItemsDiscount { get; set; } 
        public decimal TotalDeductAmount { get; set; } 

        /// <summary>
        /// Sepecific discount for this invoice [extra from discount in each item]
        /// </summary>
        public decimal TotalDiscount { get; set; }        
        public string? DiscountRemark { get; set; }
        public decimal TotalVATAmount { get; set; }

        /// <summary>
        /// Amount that need to be pay. 
        /// = Amount - TotalScholarshipAmount - TotalItemsDiscount - TotalDeductAmount - TotalDiscount
        /// </summary>
        public decimal TotalAmount { get; set; }
        public decimal CreditNoteBalance { get; set; } // For keeping track remaining balance of credit note (invoice cr for Add/Drop)
        public bool IsPackagePayment { get; set; }
        [StringLength(200)]
        public string? Reference1 { get; set; }
        [StringLength(200)]
        public string? Reference2 { get; set; }
        [StringLength(200)]
        public string? Reference3 { get; set; }
        public string? Base64Barcode { get; set; }
        public string? Barcode { get; set; }
        public long? USOrderId { get; set; }

        [StringLength(100)]
        public string? Type { get; set; } // r = registration, a = adding, cr = credit note, au = add from USpark

        [ForeignKey("StudentId")]
        public virtual Student? Student { get; set; }

        [ForeignKey("TermId")]
        public virtual Term? Term { get; set; }

        public List<InvoiceItem> InvoiceItems { get; set; }
        public virtual List<InvoicePrintLog> InvoicePrintLogs { get; set; }

        public string RunningNumberText
        {
            get
            {
                var runningYearString =  (Year % 100).ToString();
                var runningMonthString = Month.ToString("00");
                var runningNumberString = RunningNumber.ToString("00000");
                var formattedRunningNumber = $"{ runningYearString }{ runningMonthString }{ runningNumberString }";
                return string.IsNullOrEmpty(formattedRunningNumber) ? "0" : formattedRunningNumber;
            }
        }
        public int AddDropSequence { get; set; } // For storing how many times student do Add/Drop
        
        [NotMapped]
        public string AmountText => Amount.ToString(StringFormat.Money);

        [NotMapped]
        public string TotalVATAmountText => TotalVATAmount.ToString(StringFormat.Money);

        [NotMapped]
        public string TotalAmountText => TotalAmount.ToString(StringFormat.Money);

        [NotMapped]
        public string TotalItemsDiscountText => TotalItemsDiscount.ToString(StringFormat.Money);

        [NotMapped]
        public string TotalDeductAmountText => TotalDeductAmount.ToString(StringFormat.Money);

        [NotMapped]
        public string TotalDiscountText => TotalDiscount.ToString(StringFormat.Money);

        [NotMapped]
        public static readonly string TYPE_UNCONFIRM = "Unconfirm";
        [NotMapped]
        public static readonly string TYPE_REGISTRATION = "Registration";
        [NotMapped]
        public static readonly string TYPE_ADD_DROP = "Add/Drop";
        [NotMapped]
        public static readonly string TYPE_OTHER = "Other";

        [NotMapped]
        public static readonly string REFUND_TYPE_NON_REFUND = "Normal Invoice";
        [NotMapped]
        public static readonly string REFUND_TYPE_REFUND = "Refund";
        [NotMapped]
        public static readonly string REFUND_TYPE_BALANCE = "Balanced";

        [NotMapped]
        public string TypeText 
        {
            get
            {
                if (IsAddDrop.HasValue)
                {
                    switch (Type)
                    {
                        case "r":
                        case "a":
                        case "cr":
                            return IsAddDrop.Value ? TYPE_ADD_DROP : TYPE_REGISTRATION;
                        case "o":
                            return TYPE_OTHER;
                        default:
                            return "N/A";
                    }
                }
                else
                {
                    switch (Type)
                    {
                        case "r":
                            return TYPE_REGISTRATION;
                        case "a":
                            return TYPE_ADD_DROP;
                        case "cr":
                            return TotalAmount == 0 && TotalScholarshipAmount > 0 ? TYPE_REGISTRATION : TYPE_ADD_DROP;
                        case "o":
                            return TYPE_OTHER;
                        default:
                            return "N/A";
                    }
                }
            }
        }

        [NotMapped]
        public decimal ScholarshipAmount => InvoiceItems?.Sum(x => (decimal?)x.ScholarshipAmount) ?? 0;

        [NotMapped]
        public string ScholarshipAmountText => ScholarshipAmount.ToString(StringFormat.Money);

        /// <summary>
        /// TotalDiscount [Discount specific for this Invoice]
        /// +
        /// TotalItemsDiscount [Sum of Discount for each Invoice Item]
        /// </summary>
        [NotMapped]
        public string AllDiscountAmountText => (TotalDiscount + TotalItemsDiscount).ToString(StringFormat.Money);

        /// <summary>
        /// [NotMapped]
        /// Special Flag to override the invoice Type for this case of invoice
        /// </summary>
        [NotMapped]
        public bool? IsAddDrop { get; set; }

        [NotMapped]
        public bool? IsLateRegis { get; set; }
    }
}