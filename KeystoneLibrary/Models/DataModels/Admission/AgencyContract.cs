using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels.Admission
{
    public class AgencyContract
    {
        public long Id { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime SignedAt { get; set; }
        
        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? ExpiredAt { get; set; }

        [StringLength(1000)]
        public string? Remark { get; set; }
        public long AgencyId { get; set; }

        [ForeignKey("AgencyId")]
        public virtual Agency Agency { get; set; }

        [NotMapped]
        public string SignedAtText => SignedAt.ToString(StringFormat.ShortDate);

        [NotMapped]
        public string ExpiredAtText => ExpiredAt?.ToString(StringFormat.ShortDate);
    }
}