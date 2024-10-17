using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace KeystoneLibrary.Models
{
    public class ChangeStudentStatusViewModel
    {
        public Guid StudentId { get; set; }
        public string StudentStatus { get;set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime EffectiveAt { get;set; }
        public string Remark { get; set; }
    }
} 