using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;

namespace KeystoneLibrary.Interfaces
{
    public interface IInstructorProvider
    {
        Instructor GetInstructor(long id);
        Instructor GetInstructorByCode(string code);
        List<Instructor> GetInstructors();
        List<InstructorInfoViewModel> GetInstructors(Criteria criteria);
        Instructor GetInstructorProfile(long id);
        List<Instructor> GetInstructors(List<long> ids);
        List<Instructor> GetInstructorsByFacultyId(long id);
        List<Instructor> GetTermInstructorsByCourseId(long termId, long courseId);
        List<Instructor> GetTermInstructorsBySectionId(long sectionId);
        bool CheckCourseCoordinator(long termId, long courseId, long instructorId);
        List<Section> GetInstructorSection(long termId, long courseId, long instructorId);
        bool IsExistInstructor(string code);
        List<Instructor> GetInstructorBySection(List<long> sectionIds);
        void AssignAdvisee(List<Guid> studentListId, long instructorId);
    }
}