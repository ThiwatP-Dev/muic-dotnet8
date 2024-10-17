using KeystoneLibrary.Models.DataModels;

namespace KeystoneLibrary.Models
{
    public class ActiveRoomReportViewModel
    {
        public Criteria Criteria { get; set; }
        public List<ActiveRoomReportDetail> Details { get; set; }
    }

    public class ActiveRoomReportDetail
    {
        public string Name { get; set; }
        public string Campus { get; set; }
        public string Building { get; set; }
        public int Floor { get; set; }
        public List<RoomSlot> RoomSlots { get; set; }
        public RoomSlot RoomSlot { get; set; }
    }
}