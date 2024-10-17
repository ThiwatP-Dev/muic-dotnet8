using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels.Profile
{
    public class StudentStateLog : UserTimeStamp
    {
        public long Id { get; set; }

        [Required]
        public Guid StudentId { get; set; }

        [Required]
        public long TermId { get; set; }

        [Required]
        public string State { get; set; }

        [Required]
        public string UpdateFrom { get; set; } // KS or US (via API)
        
        [JsonIgnore]
        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        [ForeignKey("TermId")]
        public virtual Term Term { get; set; }
    }
}
