namespace KeystoneLibrary.Exceptions
{
    public static class GraduatingAPIException
    {
        private static readonly string KEY = "API";

        public static ResultException RequestExists()
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY) }001",
                       Message = "Request exists"
                   };
        }

        public static ResultException RequestNotFound()
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY) }002",
                       Message = "Request not found"
                   };
        }

        public static ResultException CurriculumVersionNotFound()
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY) }003",
                       Message = "Curriculum version not found"
                   };
        }

        public static ResultException RequestAttemped()
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY) }004",
                       Message = "Request already attempted"
                   };
        }

        public static ResultException ExpectedCreditNotReach()
        {
            return new ResultException
            {
                Code = $"{ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, KEY)}005",
                Message = "Expected credit not reached"
            };
        }
    }
}