using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace KeystoneLibrary.Models.DataModels
{
    public class Refund : UserTimeStamp
    {
        public long Id { get; set; }
        public long? RegistrationCourseId { get; set; }
        public long ReceiptId { get; set; }
        public long ReceiptItemId { get; set; } // 10000 : pay 5000 | scholarship 2500
        public decimal? RefundPercentage { get; set; } // 50%
        public decimal Amount { get; set; } // 2500
        public decimal ScholarshipAmount { get; set; } // 2500

        [StringLength(500)]
        public string? Remark { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? RefundedAt { get; set; }

        [ForeignKey("RegistrationCourseId")]
        public virtual RegistrationCourse? RegistrationCourse { get; set; }

        [ForeignKey("ReceiptId")]
        public virtual Receipt Receipt { get; set; }

        [ForeignKey("ReceiptItemId")]
        public virtual ReceiptItem ReceiptItem { get; set; }

        [NotMapped]
        public string RefundPercentageText => RefundPercentage?.ToString(StringFormat.GeneralDecimal) + "%";

        [NotMapped]
        public string RefundAmountText => Amount.ToString(StringFormat.TwoDecimal);

        [NotMapped]
        public string RefundedAtText => RefundedAt?.ToString(StringFormat.ShortDate);
    }
}