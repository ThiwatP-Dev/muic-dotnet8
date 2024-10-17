using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.Report;

namespace KeystoneLibrary.Interfaces
{
    public interface IReservationProvider
    {
        RoomReservation GetRoomReservation(long? id);
        List<RoomSlot> GenerateRoomSlotByRoomReservation(RoomReservation roomReservation);
        bool IsExamExisted(long id, long examiantionTypeId, long instructorId, long roomId, DateTime date, TimeSpan? start, TimeSpan? end);
        bool IsRoomExisted(RoomReservation roomReservation);
        List<RoomSlot> GetExaminationRoomSlots(long examinationReservationId);
        bool CancelRoomSlot(List<RoomSlot> roomSlots);
        ExaminationReservation GetExaminationReservation(long id);
        bool IsExistedExaminationReservation(long sectionId, long examinationTypeId, long id = 0);
        List<RoomReservationSlotViewModel> GetRoomSlotByReservation(long id);
        bool IsMeetingRoom(long roomId);
        bool IsPeriodReservationExisted(RoomReservation model);
        UpdateExaminationReservationViewModel UpdateExaminationReservation(ExaminationReservation model);
        List<DateTime> GenerateSelectedDate(RoomReservation roomReservation);
        bool IsOtherExamReservationExist(long instructorId, DateTime date, TimeSpan startTime, TimeSpan endTime, long notInSectionId);
        bool IsExamReservationExist(long sectionId, long instructorId, DateTime date, TimeSpan startTime, TimeSpan endTime);

        AllSectionTimeSlotReportViewModel GetAllSectionTimeSlotReport(Criteria criteria);
        List<ExportListReportViewModel> GetExaminationListReport(Criteria criteria);
    }
}