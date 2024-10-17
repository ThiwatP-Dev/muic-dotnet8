using System.Net;

namespace KeystoneLibrary.Models.Api
{
    public class ApiResponse<T>
    {
        public HttpStatusCode StatusCode { get; set; }
        public T Result { get; set; }
        public ApiErrorResponse ErrorResponse { get; set; }
        public bool IsSucceeded
        {
            get
            {
                return StatusCode == HttpStatusCode.OK;
            }
        }
    }
}