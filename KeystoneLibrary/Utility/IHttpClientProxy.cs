namespace KeystoneLibrary.Utility
{
    public interface IHttpClientProxy
    {
        Task<HttpResponseMessage> DeleteAsync(string endpoint, IDictionary<string, string> headers, HttpContent content);
        Task<HttpResponseMessage> GetAsync(string endpoint, IDictionary<string, string> headers);
        Task<HttpResponseMessage> PostAsync(string endpoint, IDictionary<string, string> headers, HttpContent content);
        Task<HttpResponseMessage> PutAsync(string endpoint, IDictionary<string, string> headers, HttpContent content);
    }
}