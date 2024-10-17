using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.MasterTables;

namespace KeystoneLibrary.Interfaces
{
    public interface ICacheProvider
    {
        List<Instructor> GetInstructors();
        Term GetRegistrationTerm(long academiclevelId);
        Term GetCurrentTerm(long academiclevelId);
        Term GetQuestionnaireTerm(long academiclevelId);
        List<Faculty> GetFaculties();
        List<Department> GetDepartments();
        List<Course> GetCourses();
        List<Course> GetCourseAndTransferCourse();
        List<Course> GetExternalCourses();
    }
}