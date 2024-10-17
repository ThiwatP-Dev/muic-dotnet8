namespace KeystoneLibrary.Models.Api
{
    public class SectionDetailViewModel
    {  
        public long? KSSectionDetailId { get; set; }
        public string InstructorCode { get; set; }
        public string RoomName { get; set; }
        public int? Day { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
    }
}