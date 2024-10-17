using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace KeystoneLibrary.Models.DataModels
{
    public class Slot : UserTimeStamp
    {
        public long Id { get; set; }

        [StringLength(200)]
        public string? Name { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }
        public long RegistrationTermId { get; set; }
        public bool IsSpecialSlot { get; set; }

        [StringLength(5)]
        public string Type { get; set; } // r = registration, a = add/drop, p = payment

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime StartedAt { get; set; } = DateTime.Now;// ex. 08/10/2021 10:00-10:45

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime EndedAt { get; set; } = DateTime.Now;

        [ForeignKey("RegistrationTermId")]
        public virtual RegistrationTerm RegistrationTerm { get; set; }
        public virtual List<RegistrationSlotCondition> RegistrationSlotConditions { get; set; }

        [NotMapped]
        public string StartDate => StartedAt.ToString(StringFormat.ShortDate);

        [NotMapped]
        public string EndDate => EndedAt.ToString(StringFormat.ShortDate);

        [NotMapped]
        public string Start => StartedAt.ToString(StringFormat.TimeSpanFullHour);

        [NotMapped]
        public string End => EndedAt.ToString(StringFormat.TimeSpanFullHour);

        [NotMapped]
        public string SlotText => $"{ StartDate } { Start } - { EndDate } { End }";
        public string StartedAtText => StartedAt.ToString(StringFormat.ShortDateTime);
        public string EndedAtText => EndedAt.ToString(StringFormat.ShortDateTime);
    }
}