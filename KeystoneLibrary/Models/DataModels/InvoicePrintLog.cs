using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace KeystoneLibrary.Models.DataModels
{
    public class InvoicePrintLog
    {
        public long Id { get; set; }
        public long InvoiceId { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime PrintedAt { get; set; }

        [StringLength(200)]
        public string? PrintedBy { get; set; }

        [StringLength(1000)]
        public string? Remark { get; set; }

        [ForeignKey("InvoiceId")]
        public virtual Invoice Invoice { get; set; }

        [NotMapped]
        public string PrintedAtText => PrintedAt.ToString(StringFormat.ShortDate);
    }
}