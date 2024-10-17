using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels.Prerequisites
{
    public class GPACondition : UserTimeStamp
    {
        public long Id { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }
        public decimal GPA { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? ExpiredAt { get; set; }
        public long? MUICId { get; set; }

        [NotMapped]
        public string ExpiredAtText => ExpiredAt?.ToString(StringFormat.ShortDate);

        [NotMapped]
        public string GPAConditionName => $"GPA { GPA }";
    }
}