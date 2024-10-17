using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.MasterTables;

namespace KeystoneLibrary.Models.DataModels.Admission
{
    public class StudentCodeRange : UserTimeStamp
    {
        public long Id { get; set; }
        public long AdmissionRoundId { get; set; }

        [Required]
        [StringLength(20)]
        public string StartedCode { get; set; }

        [Required]
        [StringLength(20)]
        public string EndedCode { get; set; }
        public long AcademicLevelId { get; set; }

        [ForeignKey("AdmissionRoundId")]
        public virtual AdmissionRound AdmissionRound { get; set; } // have startedAt and endedAt
        
        [ForeignKey("AcademicLevelId")]
        public virtual AcademicLevel AcademicLevel { get; set; }

        [NotMapped]
        public int StartedCodeInt 
        {
            get
            {
                int startedCode;
                bool success = Int32.TryParse(StartedCode, out startedCode);
                return success ? startedCode : 0;
            }
        }

        [NotMapped]
        public int EndedCodeInt 
        {
            get
            {
                int endedCode;
                bool success = Int32.TryParse(EndedCode, out endedCode);
                return success ? endedCode : 0;
            }
        }
    }   
}