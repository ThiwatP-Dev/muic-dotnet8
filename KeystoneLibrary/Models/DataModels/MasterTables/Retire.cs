using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels.MasterTables
{
    public class Retire : UserTimeStamp
    {
        public long Id { get; set; }

        [Required]
        [StringLength(300)]
        public string Name { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime EffectivedAt { get; set; } = DateTime.Now;

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? ExpiredAt { get; set; }

        [NotMapped]
        public string EffectivedAtText => EffectivedAt.ToShortDateString();

        [NotMapped]
        public string ExpiredAtText => ExpiredAt?.ToShortDateString();
    }
}