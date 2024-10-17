using KeystoneLibrary.Models;
using KeystoneLibrary.Models.Schedules;
using KeystoneLibrary.Models.DataModels;

namespace KeystoneLibrary.Interfaces
{
    public interface IScheduleProvider
    {
        GeneratedSchedultResult GenerateSchedules(GenerateSchedule model);
        string CreateExaminationConflictMessage(List<long> sectionIds);
        List<ScheduleViewModel> GetSchedule(List<long> sectionIds);
        List<ScheduleViewModel> GetInstructorSchedule(long termId, long instructorId);
        List<ScheduleViewModel> GetExaminationSchedule(List<long> sectionIds, string scheduleType);
        List<ScheduleViewModel> GetTeachingSchedule(List<RoomSlot> roomSlots, string type);
        List<ScheduleViewModel> GetRoomSchedulePreview(List<RoomSlot> roomSlots, string type = "");
        List<ScheduleViewModel> GetRoomSchedulePreview(List<RoomScheduleSectionDetailViewModel> sectionDetails);
        string GetClassConflictMessage(List<long> sectionIds);
        string GetExamConflictMessage(List<long> sectionIds);
    }
}