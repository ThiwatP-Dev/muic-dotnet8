using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels.Withdrawals
{
    public class WithdrawalPeriod : UserTimeStamp
    {
        public long Id { get; set; }
        public long AcademicLevelId { get; set; }
        public long TermId { get; set; }

        [Required]
        [StringLength(1)]
        public string Type { get; set; } // p = petition, u = uspark, d = debarment

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy HH:mm}", ApplyFormatInEditMode=true)]
        public DateTime StartedAt { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy HH:mm}", ApplyFormatInEditMode=true)]
        public DateTime EndedAt { get; set; }
        
        [ForeignKey("TermId")]
        public virtual Term Term { get; set; }

        [NotMapped]
        public string StartedAtText
        { 
            get
            {
                return StartedAt.AddHours(7).ToString(StringFormat.ShortDateTime);
            }
        }

        [NotMapped]
        public string EndedAtText
        { 
            get
            {
                return EndedAt.AddHours(7).ToString(StringFormat.ShortDateTime);
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
                    case "u":
                        return "Application";
                    case "d":
                        return "Debarment";
                    default:
                        return "N/A";
                }
            }
        }
    }
}