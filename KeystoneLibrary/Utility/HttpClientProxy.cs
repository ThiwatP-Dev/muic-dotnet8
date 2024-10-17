namespace KeystoneLibrary.Utility
{
    public class HttpClientProxy : IHttpClientProxy
    {
        private readonly HttpClient _httpClient;

        public HttpClientProxy(HttpClient client)
        {
            _httpClient = client;
        }

        public async Task<HttpResponseMessage> PostAsync(string endpoint, IDictionary<string, string> headers, HttpContent content)
        {
            var requestMessage = GenerateHttpRequestMessage(HttpMethod.Post, endpoint, headers, content);
            return await _httpClient.SendAsync(requestMessage, CancellationToken.None);
        }

        public async Task<HttpResponseMessage> GetAsync(string endpoint, IDictionary<string, string> headers)
        {
            var requestMessage = GenerateHttpRequestMessage(HttpMethod.Get, endpoint, headers);
            return await _httpClient.SendAsync(requestMessage, CancellationToken.None);
        }

        public async Task<HttpResponseMessage> PutAsync(string endpoint, IDictionary<string, string> headers, HttpContent content)
        {
            var requestMessage = GenerateHttpRequestMessage(HttpMethod.Put, endpoint, headers, content);
            return await _httpClient.SendAsync(requestMessage, CancellationToken.None);
        }

        public async Task<HttpResponseMessage> DeleteAsync(string endpoint, IDictionary<string, string> headers, HttpContent content)
        {
            var requestMessage = GenerateHttpRequestMessage(HttpMethod.Delete, endpoint, headers, content);
            return await _httpClient.SendAsync(requestMessage, CancellationToken.None);
        }

        private static HttpRequestMessage GenerateHttpRequestMessage(
            HttpMethod method, string endpoint,
            IDictionary<string, string> headers, HttpContent content = null)
        {
            var requestMessage = new HttpRequestMessage(method, endpoint);

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    if (header.Key == "Authorization")
                    {
                        requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                    else
                    {
                        requestMessage.Headers.Add(header.Key, header.Value);
                    }
                }
            }

            requestMessage.Content = content;

            return requestMessage;
        }
    }
}