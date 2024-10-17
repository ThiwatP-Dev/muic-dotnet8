using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.MasterTables;

namespace KeystoneLibrary.Models.DataModels.Prerequisites
{
    public class AbilityCondition : UserTimeStamp
    {
        public long Id { get; set; }
        public long AbilityId { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? ExpiredAt { get; set; }

        [JsonIgnore]
        [ForeignKey("AbilityId")]
        public virtual SpecializationGroup Ability { get; set; }

        [NotMapped]
        public string ExpiredAtText => ExpiredAt?.ToString(StringFormat.ShortDate);
    }
}