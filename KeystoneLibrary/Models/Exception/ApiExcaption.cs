namespace KeystoneLibrary.Exceptions
{
    public static class ApiException
    {
        private static readonly string KEY = "API";
        public static ResultException Unauthorized()
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.Unauthorized, KEY) }001",
                       Message = "Unauthorized"
                   };
        }

        public static ResultException InvalidParameter(string msg = "")
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.Unprocessed, KEY) }002",
                       Message = $"Invalid Parameters {msg}".Trim()
                   };
        }

        public static ResultException Forbidden()
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.Forbidden, KEY) }003",
                       Message = "Forbidden"
                   };
        }

        public static ResultException InvalidKey()
        {
            return new ResultException
                   {
                       Code = $"{ ResultExceptionHelper.Compose(ResultExceptionPrefix.Unauthorized, KEY) }004",
                       Message = "Invalid Key"
                   };
        }
    }
}