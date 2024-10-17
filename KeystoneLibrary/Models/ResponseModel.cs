namespace KeystoneLibrary.Models
{
    public class ResponseModel
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public Guid StudentId { get; internal set; }
    }
}