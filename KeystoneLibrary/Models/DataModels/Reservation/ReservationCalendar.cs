using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels
{
    public class ReservationCalendar : UserTimeStamp
    {
        public long Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? StartedAt { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? EndedAt { get; set; }

        public bool IsAllowed { get; set; }

        [NotMapped]
        public string StartedDate => StartedAt?.ToString(StringFormat.ShortDate);

        [NotMapped]
        public string EndedDate => EndedAt?.ToString(StringFormat.ShortDate);
    }
}