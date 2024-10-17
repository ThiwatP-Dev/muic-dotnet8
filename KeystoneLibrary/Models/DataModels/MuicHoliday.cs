using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace KeystoneLibrary.Models.DataModels
{
    public class MuicHoliday : UserTimeStamp
    {
        public long Id { get; set; }
        public long MuicId { get; set; }

        [StringLength(500)]
        public string? Name { get; set; }

        [StringLength(1000)]
        public string? Remark { get; set; }

        public bool IsMakeUpAble { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy HH:mm}", ApplyFormatInEditMode=true)]
        public DateTime StartedAt { get; set; } = DateTime.Now;

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy HH:mm}", ApplyFormatInEditMode=true)]
        public DateTime EndedAt { get; set; } = DateTime.Now;

    }
}