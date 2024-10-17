using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.MasterTables;

namespace KeystoneLibrary.Models.DataModels.Profile
{
    public class MaintenanceStatus : UserTimeStamp
    {
        public long Id { get; set; }
        public Guid StudentId { get; set; }
        public long TermId { get; set; }
        public long MaintenanceFeeId { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? PaidDate { get; set; }

        [StringLength(200)]
        public string? InvoiceNumber { get; set; }

        [StringLength(200)]
        public string? ApprovedBy { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime ApprovedAt { get; set; } = DateTime.Now;

        [StringLength(1000)]
        public string? Remark { get; set; }

        [NotMapped]
        public string ApprovedAtText => ApprovedAt.ToString(StringFormat.ShortDate);

        [NotMapped]
        public string PaidDateText
        {
            get
            {
                return PaidDate?.ToString(StringFormat.ShortDate);
            }
        }

        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        [ForeignKey("TermId")]
        public virtual Term Term { get; set; }

        [ForeignKey("MaintenanceFeeId")]
        public virtual MaintenanceFee MaintenanceFee { get; set; }
    }
}