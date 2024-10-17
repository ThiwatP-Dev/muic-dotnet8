namespace KeystoneLibrary.Exceptions
{
    public class ResultException
    {
        public string Code { get; set; }
        public string Message { get; set; }
    }

    public static class ResultExceptionHelper
    {
        public static string Compose(string prefix, string key) => $"{ prefix }{ key }"; 
    }

    public static class ResultExceptionPrefix
    {
        public static string Created = "201";
        public static string NoContent = "204";
        public static string BadRequest = "400";
        public static string Unauthorized = "401";
        public static string Forbidden = "403";
        public static string NotFound = "404";
        public static string Unprocessed = "422";
        public static string ServerError = "500";
    }
}