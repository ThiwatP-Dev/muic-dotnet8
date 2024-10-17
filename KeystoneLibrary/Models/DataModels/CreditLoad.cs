using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.MasterTables;

namespace KeystoneLibrary.Models.DataModels
{
    public class CreditLoad : UserTimeStamp
    {
        public long Id { get; set; }
        public long AcademicLevelId { get; set; }
        public long TermTypeId { get; set; }
        public long? FacultyId { get; set; }
        public long? DepartmentId { get; set; }
        public bool IsGraduating { get; set; }
        public decimal MaxGPA { get; set; }
        public decimal MinGPA { get; set; }
        public int MaxCredit { get; set; }
        public int MinCredit { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime EffectivedAt { get; set; } = DateTime.Now;

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? EndedAt { get; set; }

        [ForeignKey("AcademicLevelId")]
        public virtual AcademicLevel AcademicLevel { get; set; }

        [ForeignKey("TermTypeId")]
        public virtual TermType TermType { get; set; }

        [ForeignKey("FacultyId")]
        public virtual Faculty Faculty { get; set; }

        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; }

        [NotMapped]
        public string EffectivedAtString => EffectivedAt.ToString(StringFormat.ShortDate);

        [NotMapped]
        public string EndedAtString => EndedAt == null ? "" : EndedAt.Value.ToString(StringFormat.ShortDate);
    }
}