using System.Net;
using System.Text;
using KeystoneLibrary.Models.Api;

namespace KeystoneLibrary.Helpers
{
    public static class APIHelper
    {
        static readonly HttpClient client = new HttpClient();
        private static readonly string contentTypeJson = "application/json";
        private static string _endpoint = "http://keystoneregistration.azurewebsites.net/api";

        public static async Task<ApiResponse<T>> SubmitRegistration<T>(string code, string registrationRequest, string registrationChannel)
        {
            var requestBody = new 
                              {
                                  studentCode = code,
                                  request = registrationRequest,
                                  channel = registrationChannel
                              };

            var requestBodyJson = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(requestBodyJson, Encoding.UTF8, contentTypeJson);
            var response = await client.PostAsync($"{_endpoint}/v1/registrations/submit", content);

            var apiResponse = await TransformApiResponse<T>(response);
            return apiResponse;
        }

        private static async Task<ApiResponse<T>> TransformApiResponse<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return new ApiResponse<T>
                {
                    StatusCode = response.StatusCode,
                    Result = JsonConvert.DeserializeObject<T>(content),
                    ErrorResponse = null
                };
            }
            else
            {
                return new ApiResponse<T>
                {
                    StatusCode = response.StatusCode,
                    Result = default(T),
                    ErrorResponse = JsonConvert.DeserializeObject<ApiErrorResponse>(content)
                };
            }
        }
    }
}