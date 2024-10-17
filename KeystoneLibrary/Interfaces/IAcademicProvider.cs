using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.Curriculums;
using KeystoneLibrary.Models.DataModels.MasterTables;
using KeystoneLibrary.Models.Report;

namespace KeystoneLibrary.Interfaces
{
    public interface IAcademicProvider
    {
        Term GetTerm(long id);
        Term GetTermByTermAndYear(long academicLevelId, int academicTerm, int academicYear);
        List<Term> GetTermsByAcademicLevelId(long id);
        List<Term> GetTermByAcademicYear(int year);
        Term GetCurrentTermByAcademicLevelId(long academicLevelId);
        List<Department> GetDepartmentsByFacultyIds(long academicLevelId, List<long> ids);
        List<Curriculum> GetCurriculumsByDepartmentId(long academicLevelId, long facultyId, long departmentId);
        List<Curriculum> GetCurriculumsByDepartmentIds(long academicLevelId, List<long> facultyIds, List<long> departmentIds);
        List<CurriculumVersion> GetCurriculumVersionsByCurriculumIds(long academicLevelId, List<long> curriculumIds);
        List<Faculty> GetFacultiesByAcademicLevelIdForAdmission(long id);
        List<Department> GetDepartmentsByFacultyIds(List<long> ids);
        List<Department> GetDepartmentsByAcademicLevelIdAndFacultyIdForAdmission(long academicLevelId, long facultyId);
        List<Faculty> GetFacultiesByAcademicLevelId(long id);
        List<Department> GetDepartmentsByAcademicLevelIdAndFacultyId(long academicLevelId);
        List<Department> GetDepartmentsByAcademicLevelIdAndFacultyId(long academicLevelId, long facultyId);
        List<Department> GetExceptionWithdrawalDepartments(long facultyId);
        AcademicLevel GetAcademicLevel(long id);
        string GetFacultyNameById(long id);
        string GetFacultyNameByIds(List<long> ids);
        string GetDepartmentNameByIds(List<long> ids);
        string GetFacultyShortNameById(long id, string language = "en");
        string GetDepartmentShortNameById(long id, string language = "en");
        string GetDepartmentNameById(long id, string language = "en");
        string GetAcademicLevelNameById(long id, string language = "en");
        Course GetCourseDetail(long id);
        Term GetCurrentTerm(long academiclevelId);
        Term GetQuestionnaireTerm(long academiclevelId);
        TranscriptInformation GetFacultyAndDepartmentByCurriculumVersionId(long curriculumVersionId, string language);
        
        //News
        List<Topic> GetTopicByChannelId(long id);
    }
}