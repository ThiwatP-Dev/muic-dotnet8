using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.MasterTables;

namespace KeystoneLibrary.Models.DataModels.Admission
{
    public class AdmissionExamination : UserTimeStamp
    {
        public long Id { get; set; }
        public long AcademicLevelId { get; set; }
        public long FacultyId { get; set; }
        public long? DepartmentId { get; set; }
        public long AdmissionRoundId { get; set; }

        [StringLength(500)]
        public string? NameEn { get; set; }
        
        [StringLength(500)]
        public string? NameTh { get; set; }

        [StringLength(1000)]
        public string? Remark { get; set; }

        [ForeignKey("AcademicLevelId")]
        public virtual AcademicLevel AcademicLevel { get; set; }
        
        [ForeignKey("FacultyId")]
        public virtual Faculty Faculty { get; set; }

        [ForeignKey("DepartmentId")]
        public virtual Department? Department { get; set; }

        [ForeignKey("AdmissionRoundId")]
        public virtual AdmissionRound AdmissionRound { get; set; }
        public virtual List<AdmissionExaminationSchedule> AdmissionExaminationSchedules { get; set; }
    }   
}