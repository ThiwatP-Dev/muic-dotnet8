namespace KeystoneLibrary.Models.USpark
{
    public class UsparkRoomReservation
    {
        [JsonProperty("termId")]
        public long TermId { get; set; }

        [JsonProperty("studentCode")]
        public string StudentCode { get; set; }

        [JsonProperty("phoneNumber")]
        public string PhoneNumber { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("purpose")]
        public string Purpose { get; set; }

        [JsonProperty("date")]
        public DateTime Date { get; set; }

        [JsonProperty("startedTime")]
        public TimeSpan StartedTime { get; set; }

        [JsonProperty("endedTime")]
        public TimeSpan EndedTime { get; set; }

        [JsonProperty("roomId")]
        public long RoomId { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("remark")]
        public string Remark { get; set; }
    }

    public class UsparkRoomReservationViewModel
    {
        public long RoomReservationId { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Title { get; set; }
        public string NameEn { get; set; }
        public string NameTh { get; set; }
        public string Purpose { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan StartedTime { get; set; }
        public TimeSpan EndedTime { get; set; }
        public string Description { get; set; }
        public string Remark { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ReservationDateTime
    {
        [JsonProperty("date")]
        public DateTime Date { get; set; }

        [JsonProperty("startedTime")]
        public TimeSpan StartedTime { get; set; }

        [JsonProperty("endedTime")]
        public TimeSpan EndedTime { get; set; }
    }

    public class StudentRoomReservationDelete
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("studentCode")]
        public string StudentCode { get; set; }
    }
}