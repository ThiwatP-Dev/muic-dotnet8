using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.MasterTables;

namespace KeystoneLibrary.Models
{
    public class RoomScheduleViewModel
    {
        public Criteria Criteria { get; set; }
        public long RoomId { get; set; }
        public long TermId { get; set; }
        public string Term { get; set; }
        public string Name { get; set; }
        public string BuildingNameEn { get; set; }
        public string BuildingNameTh { get; set; }
        public string CampusNameEn { get; set; }
        public string CampusNameTh { get; set; }
        public string Date { get; set; }
        public string DateString { get; set; }
        public string PrintDateString { get; set; }
        public string IsMakeUp { get; set; }
        public List<Room> Rooms { get; set; }
        public List<RoomSlot> RoomSlots { get; set; }
        public List<ScheduleViewModel> Schedules { get; set; }
        public List<RoomSchedulePreview> Preview { get; set; }
    }

    public class RoomSchedulePreview
    {
        public string Name { get; set; }
        public string Term { get; set; }
        public string BuildingName { get; set; }
        public string CampusName { get; set; }
        public string DateString { get; set; }
        public string PrintDateString { get; set; }
        public List<ScheduleViewModel> Schedules { get; set; }
    }
}