using KeystoneLibrary.Models.DataModels.Profile;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels.Scholarship
{
    public class Voucher : UserTimeStamp
    {
        public long Id { get; set; }
        public Guid StudentId { get; set; }
        public long StudentScholarshipId { get; set; }
        public long TermId { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime RequestedAt { get; set; }
        public decimal TotalAmount { get; set; }

        [StringLength(5)]
        public string? Status { get; set; } // r = request, p = university paid this money return to student

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? PaidAt { get; set; }

        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        [ForeignKey("StudentScholarshipId")]
        public virtual ScholarshipStudent ScholarshipStudent { get; set; }

        [ForeignKey("TermId")]
        public virtual Term Term { get; set; }
        public virtual List<VoucherLog> VoucherLogs { get; set; }

        [NotMapped]
        public string TotalAmountText => TotalAmount.ToString(StringFormat.Money);

        [NotMapped]
        public string RequestedAtText => RequestedAt.ToString(StringFormat.ShortDate);

        [NotMapped]
        public string PaidAtText => PaidAt?.ToString(StringFormat.ShortDate);

        [NotMapped]
        public string StatusText 
        {
            get
            {
                switch (Status)
                {
                    case "r":
                        return "Request";
                    case "p":
                        return "Paid";
                    default:
                        return "N/A";
                }
            }
        }

        [NotMapped]
        public string Code { get; set; }
    }
}