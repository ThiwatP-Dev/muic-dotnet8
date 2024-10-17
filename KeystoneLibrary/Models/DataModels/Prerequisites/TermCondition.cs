using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels.Prerequisites
{
    public class TermCondition : UserTimeStamp
    {
        public long Id { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }
        public int Term { get; set; } // Term 10 terms 

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? ExpiredAt { get; set; }
        public long? MUICId { get; set; }

        [NotMapped]
        public string ExpiredAtText => ExpiredAt?.ToString(StringFormat.ShortDate);

        [NotMapped]
        public string TermConditionName => $"{ Term } term{(Term > 1 ? "s" : string.Empty)}";
    }
}