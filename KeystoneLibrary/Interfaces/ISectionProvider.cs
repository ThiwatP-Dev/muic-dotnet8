using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.Schedules;
using Microsoft.EntityFrameworkCore.Storage;

namespace KeystoneLibrary.Interfaces
{
    public interface ISectionProvider
    {
        SectionViewModel GetSectionBySemester(long sectionId, long semesterId);
        Section GetSection(long id);
        bool CloseSection(Section section, out string errorMessage);
        bool CloseSection(Section section, out string errorMessage, IDbContextTransaction transaction);
        bool OpenSection(Section section);
        List<SectionSlot> GenerateSectionSlots(int totalWeeks, Section section);
        List<SectionSlot> GetSectionSlotsBySectionId(long sectionId);
        List<RoomScheduleSectionDetailViewModel> GetSectionDetailsByRoomAndTerm(long roomId, long termId, string sectionStatus = "");
        List<SectionDetail> GetSectionDetailsBySectionId(long sectionId);
        bool UpdateOpenedOrClosedAtSection(Section section, out string errorMessage);
        bool HaveStudentsInSection(long sectionId);
        bool IsSectionSlotExisted(long sectionId, long InstructorId, long roomId, DateTime date, TimeSpan? start, TimeSpan? end);
        bool IsSectionSlotExisted(long InstructorId, long roomId, DateTime date, TimeSpan? start, TimeSpan? end);
        List<Section> GetJointSections(long sectionId);
        string GetNextSectionNumber(long courseId, long termId);
        void ReCalculateSeatAvailable(long sectionId);
        List<SectionReportViewModel> GetSectionReportViewModel(Criteria criteria);
    }
}