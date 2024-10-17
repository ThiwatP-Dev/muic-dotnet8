using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels.Profile
{
    public class StudentLog : UserTimeStamp
    {
        public long Id { get; set; }
        public Guid StudentId { get; set; }
        public long TermId { get; set; }

        [StringLength(100)]
        public string Source { get; set; } // ChangeCurriculum

        [Required]
        [StringLength(5000)]
        public string Log { get; set; } // ChangeCurriculum = Old, New
        
        [JsonIgnore]
        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        [ForeignKey("TermId")]
        public virtual Term Term { get; set; }

        [NotMapped]
        public Dictionary<string, string> LogDictionary => JsonConvert.DeserializeObject<Dictionary<string, string>>(Log);
    }
}
