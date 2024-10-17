using KeystoneLibrary.Exceptions;

namespace KeystoneLibrary.Models.Api.ApiResponse
{
    public class APIResponse
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public dynamic Data { get; set; }
        [JsonIgnore]
        public int HTTPStatusCode { get; set; }
        [JsonIgnore]
        public bool IsSuccessful { get; set; }
    }

    public static class APIResponseBuilder
    {
        public static APIResponse Success(dynamic data, string message = "Success") => new APIResponse
        {
            HTTPStatusCode = 200,
            Code = "200",
            Message = message,
            Data = data,
            IsSuccessful = true
        };

        public static APIResponse Error(ResultException error, dynamic data = null)
        {
            var statusCode = 0;
            var isExceptionCodeParsable = Int32.TryParse(error.Code.Substring(0, 3), out statusCode);
            if (!isExceptionCodeParsable)
                statusCode = 500;

            return new APIResponse
            {
                HTTPStatusCode = statusCode,
                Code = error.Code,
                Message = error.Message,
                Data = data,
                IsSuccessful = false
            };
        }
    }
}