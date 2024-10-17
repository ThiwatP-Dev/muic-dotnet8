using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.MasterTables;

namespace KeystoneLibrary.Models.DataModels.Profile
{
    public class GraduationInformation : UserTimeStamp
    {
        public long Id { get; set; }
        public Guid StudentId { get; set; }
        public long? CurriculumInformationId { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? GraduatedAt { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? CommissionApprovedAt { get; set; } // วันที่สภาอนุมัติ

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? GraduationRegisteredAt { get; set; } // วันที่รายงานตัวบัณฑิต

        [StringLength(100)]
        public string? Class { get; set; }
        public long? TermId { get; set; } // Graduation Term
        public long? HonorId { get; set; }

        [StringLength(500)]
        public string? Remark { get; set; }

        [StringLength(500)]
        public string? ThesisRemark { get; set; }

        [StringLength(1000)]
        public string? OtherRemark1 { get; set; }

        [StringLength(1000)]
        public string? OtherRemark2 { get; set; }

        [JsonIgnore]
        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        [JsonIgnore]
        [ForeignKey("CurriculumInformationId")]
        public virtual CurriculumInformation? CurriculumInformation { get; set; }

        [ForeignKey("TermId")]
        public virtual Term? Term { get; set; }

        [ForeignKey("HonorId")]
        public virtual AcademicHonor? AcademicHonor { get; set; }

        [NotMapped]
        public string GraduatedAtText => GraduatedAt?.ToString(StringFormat.ShortDate);

        [NotMapped]
        public string GraduatedAtThText => GraduatedAt == null ? "" : GraduatedAt?.AddYears(543).ToString(StringFormat.ShortDate);

        [NotMapped]
        public string GraduatedAtFullText {
            get {
                System.Globalization.CultureInfo _cultureENInfo = new System.Globalization.CultureInfo("en-EN");
                return GraduatedAt == null ? "" : GraduatedAt?.ToString("d MMMM yyyy", _cultureENInfo);
            }
        }

        [NotMapped]
        public string GraduatedAtThFullText {
            get {
                System.Globalization.CultureInfo _cultureENInfo = new System.Globalization.CultureInfo("th-TH");
                return GraduatedAt == null ? "" : GraduatedAt?.ToString("d MMMM yyyy", _cultureENInfo);
            }
        }

        [NotMapped]
        public int ClassInt
        {
            get
            {
                int graduateClass;
                bool success = Int32.TryParse(Class, out graduateClass);
                return success ? graduateClass : 0;
            }
        }
    }
}
