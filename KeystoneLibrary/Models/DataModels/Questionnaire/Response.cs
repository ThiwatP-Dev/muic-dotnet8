using KeystoneLibrary.Models.DataModels.Profile;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeystoneLibrary.Models.DataModels.Questionnaire
{
    public class Response : UserTimeStamp
    {
        public long Id { get; set; }

        [Required]
        public Guid StudentId { get; set; }

        [Required]
        public long TermId { get; set; }
        public long? CourseId { get; set; }
        public long? SectionId { get; set; }
        public long? InstructorId { get; set; }
        public long QuestionnaireId { get; set; }
        public long QuestionGroupId { get; set; }
        public long AnswerId { get; set; }

        [StringLength(5000)]
        public string? AnswerRemark { get; set; } 

        [JsonIgnore]
        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        [JsonIgnore]
        [ForeignKey("QuestionnaireId")]
        public virtual Questionnaire Questionnaire { get; set; }

        [JsonIgnore]
        [ForeignKey("QuestionGroupId")]
        public virtual QuestionGroup QuestionGroup { get; set; }

        [JsonIgnore]
        [ForeignKey("AnswerId")]
        public virtual Answer Answer { get; set; }

        [JsonIgnore]
        [ForeignKey("CourseId")]
        public virtual Course? Course { get; set; }

        [JsonIgnore]
        [ForeignKey("SectionId")]
        public virtual Section? Section { get; set; }

        [JsonIgnore]
        [ForeignKey("InstructorId")]
        public virtual Instructor? Instructor { get; set; }

        // [NotMapped]
        // public List<QuestionAndAnswerViewModel> QuestionAndAnswers
        // {
        //     get 
        //     {
        //         var result = new JsonSerializerSettings { Error = (se, ev) => { ev.ErrorContext.Handled = true; } };
        //         var questions = JsonConvert.DeserializeObject<List<QuestionAndAnswerViewModel>>(Answer, result);
        //         return questions;
        //     }
        // }
    }
}