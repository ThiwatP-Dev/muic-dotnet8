namespace KeystoneLibrary.Models.Api
{
    public class SectionViewModel
    {
        public long KSSectionId { get; set; }
        public long? KSParentSectionId { get; set; }
        public string Number { get; set; }
        public long KSCourseId { get; set; }
        public long KSSemesterId { get; set; }
        public int SeatAvailable { get; set; }
        public int SeatLimit { get; set; }
        public int SeatUsed { get; set; }
        public string Remark { get; set; }
        public TimeSpan? MidtermStart { get; set; }
        public TimeSpan? MidtermEnd { get; set; }
        public DateTime? MidtermDate { get; set; }
        public string MidtermRoomName { get; set; }
        public TimeSpan? FinalStart { get; set; }
        public TimeSpan? FinalEnd { get; set; }
        public DateTime? FinalDate { get; set; }
        public string FinalRoomName { get; set; }
        public List<SectionDetailViewModel> SectionDetails { get; set; }
        public List<SectionSlotViewModel> SectionSlots { get; set; }
    }
}