using KeystoneLibrary.Models;

namespace KeystoneLibrary.Interfaces
{
    public interface IEquivalenceProvider
    {
        List<StudentCourseEquivalentViewModel> GetExternalEquivalentCourses(List<StudentTransferCourseViewModel> courseList, long universityId);

    }
}