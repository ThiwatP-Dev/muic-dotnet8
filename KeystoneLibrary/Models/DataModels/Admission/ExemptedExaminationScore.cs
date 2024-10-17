using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.MasterTables;
using System.ComponentModel;

namespace KeystoneLibrary.Models.DataModels.Admission
{
    public class ExemptedExaminationScore : UserTimeStamp
    {
        public long Id { get; set; }
        public long ExemptedAdmissionExaminationId { get; set; }
        public long? FacultyId { get; set; }
        public long? DepartmentId { get; set; }
        public long? AdmissionTypeId { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? StartedAt { get; set; }  = DateTime.Now;

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? EndedAt { get; set; } = DateTime.Now;
        public long CourseId { get; set; }
        public decimal MinimumScore { get; set; }
        public decimal MaximumScore { get; set; }

        [StringLength(1000)]
        public string? Remark { get; set; }

        [ForeignKey("ExemptedAdmissionExaminationId")]
        public virtual ExemptedAdmissionExamination ExemptedAdmissionExamination { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }

        [ForeignKey("FacultyId")]
        public virtual Faculty? Faculty { get; set; }

        [ForeignKey("DepartmentId")]
        public virtual Department? Department { get; set; }

        [ForeignKey("AdmissionTypeId")]
        public virtual AdmissionType? AdmissionType { get; set; }
    }   
}