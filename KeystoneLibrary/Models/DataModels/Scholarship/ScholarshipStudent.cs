using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.Profile;

namespace KeystoneLibrary.Models.DataModels.Scholarship
{
    public class ScholarshipStudent : UserTimeStamp
    {
        public long Id { get; set; }
        public Guid StudentId { get; set; }
        public long ScholarshipId { get; set; }
        public long EffectivedTermId { get; set; }
        public long? ExpiredTermId { get; set; }
        public int? ExpiredAcademicTerm { get; set; }
        public int? ExpiredAcademicYear { get; set; }
        public bool IsApproved { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? ApprovedAt { get; set; }

        [StringLength(200)]
        public string? ApprovedBy { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? ReferenceDate { get; set; }
        public bool SendContract { get; set; }
        public bool AllowRepeatedRegistration { get; set; }
        public bool HaveScholarshipHour { get; set; }
        public decimal LimitedAmount { get; set; } // used amount, personal pay, refundable amount, balance --> Financial Transaction

        [StringLength(2000)]
        public string? Remark { get; set; }

        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        [ForeignKey("ScholarshipId")]
        public virtual Scholarship Scholarship { get; set; }

        [ForeignKey("EffectivedTermId")]
        public virtual Term EffectivedTerm { get; set; }

        [ForeignKey("ExpiredTermId")]
        public virtual Term? ExpiredTerm { get; set; }
        public virtual List<FinancialTransaction> FinancialTransactions { get; set; }

        [NotMapped]
        public string DefaultAmountText => LimitedAmount.ToString(StringFormat.TwoDecimal);
        
        [NotMapped]
        public string ApprovedAtText => ApprovedAt?.ToString(StringFormat.ShortDate);
        
        [NotMapped]
        public string ReferenceDateText => ReferenceDate?.ToString(StringFormat.ShortDate);

        [NotMapped]
        public string LimitedAmountText => LimitedAmount.ToString(StringFormat.Money);

        [NotMapped]
        public string BalanceText => FinancialTransactions == null || !FinancialTransactions.Any()
                                     ? LimitedAmount.ToString(StringFormat.Money)
                                     : FinancialTransactions.LastOrDefault()?.Balance.ToString(StringFormat.Money);

        [NotMapped]
        public long ScholarshipTypeId { get; set; }

        [NotMapped]
        public bool Active { get; set; }
    }
}