using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.MasterTables;

namespace KeystoneLibrary.Interfaces
{
    public interface IRoomProvider
    {
        Room GetRoom(long roomId);
        List<Building> GetBuildingByCampusId(long campusId);
        List<Room> GetRoomByBuildingId(long buildingId);
        List<RoomSlot> GetRoomSlotsBySectionSlotIds(List<long> sectionSlotIds);
        Campus GetCampus(long id);
        List<Room> GetAvailableRoom(DateTime date, TimeSpan start, TimeSpan end, string type);
        RoomSlot IsExistedRoomSlot(long roomId, DateTime date, TimeSpan start, TimeSpan end);
        RoomSlot IsExistedRoomSlot(long roomId, DateTime date, TimeSpan start, TimeSpan end, long examReservationId = 0);
        List<Room> GetAvailableRoomBySectionSlotIds(List<long> sectionSlotIds);
        List<Room> GetAvailableRoomAndRoomInSectionSlot(List<long> sectionSlotIds);
        bool CreateRoomSlotBySectionSlot(SectionSlot sectionsSlot);
        bool CancelRoomSlotBySectionSlotId(long sectionsSlotId);
        bool CancelAndInActiveRoomSlotBySectionSlotId(long sectionsSlotId);
        bool CancelRoomSlotByRoomReservationId(long roomReservationId);
        bool IsBuildingOpen(long roomId, TimeSpan start, TimeSpan end);
        string GetRoomSlotDetail(DateTime startDate, DateTime endDate);
        List<RoomSlot> GetRoomSlotsByRoomAndTermAndDate(long roomId, long termId, string date);
        List<RoomSlot> GetWeeklyRoomSlotsByRoomAndTermAndDate(long roomId, long termId, string date, string isMakeUp, string type = "");
        bool IsHaveRoomInSectionSlot(long sectionId);
        List<Room> GetAvailableRoomByDates(List<DateTime> dates, TimeSpan? start, TimeSpan? end, long id = 0);
    }
}