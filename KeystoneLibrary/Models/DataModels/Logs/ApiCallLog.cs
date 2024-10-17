namespace KeystoneLibrary.Models.DataModels.Logs
{
    public class ApiCallLog
    {
        public Guid Id { get; set; }
        public string? Endpoint { get; set; }
        public string? HttpMethod { get; set; }
        public string? RequestPayload { get; set; }
        public string? ResponsePayload { get; set; }
        public int ResponseStatusCode { get; set; }
        public DateTime Timestamp { get; set; }
        public string? ClientIpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? ErrorMessage { get; set; }
        public bool IsSuccess { get; set; }
        public string? ReferId { get; set; }
        public string? ReferTable { get; set; }

        public ApiCallLog()
        {
            Timestamp = DateTime.UtcNow; // Automatically set the timestamp to the current time
        }

        public override string ToString()
        {
            return $"[{Timestamp}] {HttpMethod} {Endpoint} - Status: {ResponseStatusCode} - Success: {IsSuccess}\n" +
                   $"Request: {RequestPayload}\n" +
                   $"Response: {ResponsePayload}\n" +
                   $"Client IP: {ClientIpAddress} - User Agent: {UserAgent}\n" +
                   $"Error: {ErrorMessage}";
        }
    }
}
