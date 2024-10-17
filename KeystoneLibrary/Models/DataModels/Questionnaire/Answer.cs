using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels.Questionnaire
{
    public class Answer : UserTimeStamp
    {
        public long Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string AnswerEn { get; set; }

        [Required]
        [StringLength(200)]
        public string AnswerTh { get; set; }

        [Required]
        [StringLength(200)]
        public string Value { get; set; }
        public long? NextQuestionId { get; set; }
        public long QuestionId { get; set; }

        [JsonIgnore]
        [ForeignKey("NextQuestionId")]
        public virtual Question NextQuestion { get; set; }

        [JsonIgnore]
        [ForeignKey("QuestionId")]
        public virtual Question Question { get; set; }

        [NotMapped]
        public decimal ValueDecimal => Convert.ToDecimal(Value);
    }
}