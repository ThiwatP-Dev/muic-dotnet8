namespace KeystoneLibrary.Models.USpark
{
    public class USparkQuestionnaireViewModel
   {
       public long Id { get; set; }
       public long KSSectionId { get; set; }
       public List<long> InstructorIds { get; set; }
       public List<USparkQuestionGroupViewModel> QuestionGroups { get; set; }
   }

   public class USparkQuestionGroupViewModel
   {
       public long Id { get; set; }
       public string NameEn { get; set; }
       public List<USparkQuestionViewModel> Questions { get; set; }
   }

   public class USparkQuestionViewModel
   {
       public int Order { get; set; }
       public string NameEn { get; set; }
       public string QuestionType { get; set; }
       public List<USparkAnswerViewModel> Answers { get; set; }
   }
   
   public class USparkAnswerViewModel
   {
       public long Id { get; set; }
       public string Value { get; set; }
       public string AnswerEn { get; set; }
   }
    
    public class USparkQuestionnaireResponseViewModel
    {
        [JsonProperty("ksSectionId")]
        public long KSSectionId { get; set; }

        [JsonProperty("studentCode")]
        public string StudentCode { get; set; }

        [JsonProperty("questionnaireId")]
        public long QuestionnaireId { get; set; }

        [JsonProperty("responses")]
        public List<QuestionnaireResponseViewModel> Responses { get; set; }
    }

    public class QuestionnaireResponseViewModel
    {
        [JsonProperty("questionGroupId")]
        public long QuestionGroupId { get; set; }

        [JsonProperty("instructorId")]
        public long InstructorId { get; set; }

        [JsonProperty("answerId")]
        public long AnswerId { get; set; }

        [JsonProperty("answerRemark")]
        public string AnswerRemark { get; set; }
    }
}