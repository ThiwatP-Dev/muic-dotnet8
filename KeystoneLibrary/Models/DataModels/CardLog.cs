using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace KeystoneLibrary.Models.DataModels
{
    public class CardLog
    {
        public long Id { get; set; }
        public Guid StudentId { get; set; }
        public long TermId { get; set; }

        [StringLength(200)]
        public string? CardType { get; set; } // subtitue card, temporary card

        [StringLength(3000)]
        public string? Log { get; set; } //Jsonstring of card model
        public int RunningNumber { get; set; }
        public int Year { get; set; }

        [StringLength(200)]
        public string? ReferenceNumber { get; set; }

        [StringLength(10)]
        public string? Language { get; set; } // THAI, ENG

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? RequestedAt { get; set; }

        [StringLength(200)]
        public string? RequestedBy { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? PrintedAt { get; set; }

        [StringLength(200)]
        public string? PrintedBy { get; set; }

        [NotMapped]
        public string PrintedAtText => PrintedAt?.ToString(StringFormat.ShortDate);
    }
}