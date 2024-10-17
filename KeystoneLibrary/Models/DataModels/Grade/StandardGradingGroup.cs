using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace KeystoneLibrary.Models.DataModels
{
    public class StandardGradingGroup : UserTimeStamp
    {
        public long Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [StringLength(10)]
        public string? Type { get; set; }
        public decimal Minimum { get; set; }
        public decimal Maximum { get; set; }
        public long? GradeTemplateId { get; set; }
        public virtual List<StandardGradingScore> StandardGradingScores { get; set; }

        [ForeignKey("GradeTemplateId")]
        public virtual GradeTemplate? GradeTemplate { get; set; }

        [NotMapped]
        public string TypeText
        {
            get
            {
                switch (Type)
                {
                    case "s":
                        return "Score";
                    case "p":
                        return "Percentage";
                    default:
                        return "N/A";
                }
            }
        }
    }
}