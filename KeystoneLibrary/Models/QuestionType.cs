namespace KeystoneLibrary.Models
{
    public enum QuestionType
    {
        RANKING,
        ONE_CHOICE,
        MULTIPLE_CHOICE,
        SHORT_ANSWER
    }

    public static class QuestionTypeExtensions
    {
        public static string ToStringValue(this QuestionType questionType)
        {
            string value = string.Empty;
            switch(questionType)
            {
                case QuestionType.RANKING:
                    value = "r";
                    break;

                case QuestionType.ONE_CHOICE:
                    value = "o";
                    break;

                case QuestionType.MULTIPLE_CHOICE:
                    value = "m";
                    break;
                case QuestionType.SHORT_ANSWER:
                    value = "s";
                    break;
            }

            return value;
        }
    }
}
