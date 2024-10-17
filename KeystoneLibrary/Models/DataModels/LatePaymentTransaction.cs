using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.Profile;

namespace KeystoneLibrary.Models.DataModels
{
    public class LatePaymentTransaction : UserTimeStamp
    {
        public long Id { get; set; }
        public Guid StudentId { get; set; }
        public long TermId { get; set; }
        public string? Type { get; set; } // p = student petition, m = memo from instructor, c = contact admin

        [StringLength(1000)]
        public string? Remark { get; set; }
        public bool IsPaid { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime ApprovedAt { get; set; }
        
        [StringLength(100)]
        public string? ApprovedBy { get; set; }
        
        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        [ForeignKey("TermId")]
        public virtual Term Term { get; set; }

        [NotMapped]
        public string ApprovedAtText
        {
            get
            {
                return ApprovedAt.ToString(StringFormat.ShortDate);
            }
        }

        [NotMapped]
        public string TypeText
        {
            get
            {
                switch (Type)
                {
                    case "p":
                        return "Petition";
                    case "m":
                        return "Memo";
                    case "c":
                        return "Contact";
                    default:
                        return "N/A";
                }
            }
        }
    }
}