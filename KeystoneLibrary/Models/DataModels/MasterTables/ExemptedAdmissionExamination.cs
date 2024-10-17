using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels.MasterTables
{
    public class ExemptedAdmissionExamination : UserTimeStamp
    {
        // ex. IELTS TOFEL SAT etc.
        public long Id { get; set; }

        [Required]
        [StringLength(300)]
        public string NameEn { get; set; }
        
        [StringLength(300)]
        public string? NameTh { get; set; }
        public decimal MinimumScore { get; set; }
        public decimal MaximumScore { get; set; }
        public bool DisplayAdmissionForm { get; set; }

        [NotMapped]
        public decimal Score { get; set; }
    }
}