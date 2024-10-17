using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels.Questionnaire
{
    public class QuestionGroup : UserTimeStamp
    {
        public long Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string NameEn { get; set; }

        [Required]
        [StringLength(200)]
        public string NameTh { get; set; }

        [StringLength(500)]
        public string? DescriptionEn { get; set; }

        [StringLength(500)]
        public string? DescriptionTh { get; set; }
        public long QuestionnaireId { get; set; }

        [JsonIgnore]
        [ForeignKey("QuestionnaireId")]
        public virtual Questionnaire Questionnaire { get; set; }
        public virtual List<Question> Questions { get; set; }
    }
}