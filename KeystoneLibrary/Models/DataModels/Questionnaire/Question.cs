using KeystoneLibrary.Models.Report;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels.Questionnaire
{
    public class Question : UserTimeStamp
    {
        public long Id { get; set; }
        public int Order { get; set; }

        [Required]
        [StringLength(200)]
        public string QuestionEn { get; set; }

        [Required]
        [StringLength(200)]
        public string QuestionTh { get; set; }

        [Required]
        [StringLength(2)]
        public string QuestionType { get; set; } // r = Ranking, s = Short Answer, o = One Choice, m = Multiple Choice
        public long? ParentQuestionId { get; set; }
        public long QuestionGroupId { get; set; }
        public bool IsCalculate { get; set; }

        [JsonIgnore]
        [ForeignKey("QuestionGroupId")]
        public virtual QuestionGroup QuestionGroup { get; set; }
        public virtual List<Answer> Answers { get; set; }

        [NotMapped]
        public List<QuestionAndAnswerViewModel> QuestionAndAnswers { get; set; }

        [NotMapped]
        public List<Response> Responses { get; set; }
    }
}