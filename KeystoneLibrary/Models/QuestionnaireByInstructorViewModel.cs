namespace KeystoneLibrary.Models
{
    public class QuestionnaireByInstructorAndSectionViewModel
    {
        public decimal TotalAverage { get; set; }
        public Criteria Criteria { get; set; }
        public List<QuestionnaireByInstructorAndSectionGroup> Header { get; set; }
        public List<QuestionnaireByInstructorAndSectionStudent> Students { get; set; }
    }

    public class QuestionnaireByInstructorAndSectionStudent
    {
        public Guid StudentId { get; set; }
        public long TermId { get; set; }
        public long SectionId { get; set; }
        public long InstructorId { get; set; }
        public long QuestionGroupId { get; set; }
        public long QuestionId { get; set; }
        public string Title { get; set; }
        public string StudentCode { get; set; }
        public string FirstName { get; set; }
        public string MidName { get; set; }
        public string LastName { get; set; }
        public string ValueText { get; set; }
        public string Type { get; set; }
        public decimal Value { get; set; }
        public decimal Average { get; set; }
        public bool IsCalulate { get; set; }
        public string FullName => string.IsNullOrEmpty(MidName) ? $"{ Title } { FirstName } { LastName }"
                                                                : $"{ Title } { FirstName } { MidName } { LastName }";
        public List<QuestionnaireByInstructorAndSectionQuestion> Questions { get; set; }
    }

    public class QuestionnaireByInstructorAndSectionGroup
    {
        public long QuestionGroupId { get; set; }
        public string GroupName { get; set; }
        public List<QuestionnaireByInstructorAndSectionQuestion> Questions { get; set; }
    }

    public class QuestionnaireByInstructorAndSectionQuestion
    {
        public long QuestionId { get; set; }
        public int Order { get; set; }
        public string Type { get; set; }
        public decimal Answer { get; set; }
        public string AnswerText { get; set; }
        public bool IsCalculate { get; set; }
    }
}