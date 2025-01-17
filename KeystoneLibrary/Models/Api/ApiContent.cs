namespace KeystoneLibrary.Models.Api
{
    public class ApiContent<T>
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
    }
}