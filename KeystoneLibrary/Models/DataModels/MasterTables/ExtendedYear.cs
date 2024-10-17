using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels.MasterTables
{
    public class ExtendedYear : UserTimeStamp
    {
        public long Id { get; set; }
        public long AcademicLevelId { get; set; }
        public int Year { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime EffectivedAt { get; set; } = DateTime.Now;

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? ExpiredAt { get; set; }

        [ForeignKey("AcademicLevelId")]
        public virtual AcademicLevel AcademicLevel { get; set; }

        [NotMapped]
        public string EffectivedAtText => EffectivedAt.ToShortDateString();

        [NotMapped]
        public string ExpiredAtText => ExpiredAt == null ? "N/A" : ExpiredAt?.ToShortDateString();
    }
}