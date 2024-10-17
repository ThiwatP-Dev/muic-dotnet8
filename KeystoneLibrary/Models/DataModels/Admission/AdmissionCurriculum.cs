using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.Curriculums;
using KeystoneLibrary.Models.DataModels.MasterTables;

namespace KeystoneLibrary.Models.DataModels.Admission
{
    public class AdmissionCurriculum : UserTimeStamp
    {
        public long Id { get; set; }
        public long AdmissionRoundId { get; set; }
        public long AcademicLevelId { get; set; }
        public long FacultyId { get; set; }
        public long DepartmentId { get; set; }
        public long CurriculumId { get; set; }
        public long CurriculumVersionId { get; set; }

        [ForeignKey("AdmissionRoundId")]
        public virtual AdmissionRound AdmissionRound { get; set; }

        [ForeignKey("AcademicLevelId")]
        public virtual AcademicLevel AcademicLevel { get; set; }
        
        [ForeignKey("FacultyId")]
        public virtual Faculty Faculty { get; set; }

        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; }

        [ForeignKey("CurriculumId")]
        public virtual Curriculum Curriculum { get; set; }

        [ForeignKey("CurriculumVersionId")]
        public virtual CurriculumVersion CurriculumVersion { get; set; }

        // For CRUD Form
        [NotMapped]
        public List<CurriculumVersion> CurriculumVersions { get; set; }

        [NotMapped]
        public long AdmissionTermId { get; set; }
    }   
}