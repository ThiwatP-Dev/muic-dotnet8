using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using KeystoneLibrary.Models.DataModels.Profile;

namespace KeystoneLibrary.Models.DataModels.Scholarship
{
    public class ScholarshipActiveLog
    {
        public long Id { get; set; }
        public Guid StudentId { get; set; }
        public long ScholarshipId { get; set; }
        public long TermId { get; set; }
        public bool IsActive { get; set; }

        [StringLength(2000)]
        public string? Remark { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? ApprovedAt { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime UpdatedAt { get; set; }

        [StringLength(200)]
        public string? UpdatedBy { get; set; }

        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        [ForeignKey("ScholarshipId")]
        public virtual Scholarship Scholarship { get; set; }

        [ForeignKey("TermId")]
        public virtual Term Term { get; set; }
    }
}