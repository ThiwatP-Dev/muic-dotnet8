using KeystoneLibrary.Models.DataModels.MasterTables;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels
{
    public class SectionPeriod : UserTimeStamp
    {
        public long Id { get; set; }
        public long AcademicLevelId { get; set; }
        public long TermId { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime StartedAt { get; set; } = DateTime.Now;
        public TimeSpan? StartedTime { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime EndedAt { get; set; } = DateTime.Now;
        public TimeSpan? EndedTime { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime NoMakeupClassFrom { get; set; } = DateTime.Now;

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime NoMakeupClassTo { get; set; } = DateTime.Now;

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime NoReserveableFrom { get; set; } = DateTime.Now;

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime NoReserveableTo { get; set; } = DateTime.Now;

        [NotMapped]
        public string StartedAtText => StartedAt.ToString(StringFormat.ShortDate);

        [NotMapped]
        public string EndedAtText => EndedAt.ToString(StringFormat.ShortDate);

        [NotMapped]
        public string NoMakeupClassFromText => NoMakeupClassFrom.ToString(StringFormat.ShortDate);

        [NotMapped]
        public string NoMakeupClassToText => NoMakeupClassTo.ToString(StringFormat.ShortDate);

        [NotMapped]
        public string NoReserveableFromText => NoReserveableFrom.ToString(StringFormat.ShortDate);

        [NotMapped]
        public string NoReserveableToText => NoReserveableTo.ToString(StringFormat.ShortDate);
        
        [ForeignKey("TermId")]
        public virtual Term Term { get; set; }

        [ForeignKey("AcademicLevelId")]
        public virtual AcademicLevel AcademicLevel { get; set; }
    }
}