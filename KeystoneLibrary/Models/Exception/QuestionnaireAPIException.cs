namespace KeystoneLibrary.Exceptions
{
    public static class QuestionnaireException
    {
        private static readonly string KEY = "API";
        public static ResultException StudentNotFound()
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY) }001",
                       Message = "Student Not Found"
                   };
        }

        public static ResultException SectionNotFound()
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY) }002",
                       Message = "Section Not Found"
                   };
        }

        public static ResultException ObjectIsNull()
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY) }003",
                       Message = "Object something value is null"
                   };
        }

        public static ResultException SurveyedAlreadyExist()
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY) }004",
                       Message = "Surveyed already exist in database"
                   };
        }
    }
}