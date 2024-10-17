using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.MasterTables;
using KeystoneLibrary.Models.DataModels.Profile;

namespace KeystoneLibrary.Models.DataModels.Admission
{
    public class StudentExemptedExamScore : UserTimeStamp
    {
        public long Id { get; set; }
        public Guid StudentId { get; set; }
        public long ExemptedExaminationId { get; set; }
        public decimal Score { get; set; }
        public long? StudentDocumentId { get; set; }

        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }
        
        [ForeignKey("ExemptedExaminationId")]
        public virtual ExemptedAdmissionExamination ExemptedAdmissionExamination { get; set; }

        [ForeignKey("StudentDocumentId")]
        public virtual StudentDocument StudentDocument { get; set; }
    }
}