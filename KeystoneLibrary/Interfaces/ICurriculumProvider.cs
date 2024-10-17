using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.Curriculums;
using KeystoneLibrary.Models.DataModels.MasterTables;
using KeystoneLibrary.Models.DataModels.Profile;
using KeystoneLibrary.Models.DataModels.Graduation;
using KeystoneLibrary.Models.Report;

namespace KeystoneLibrary.Interfaces
{
    public interface ICurriculumProvider
    {
        string GetCurriculumVersionName(long versionId);
        Curriculum GetCurriculum(long id);
        CurriculumVersion GetCurriculumVersion(long id);
        Curriculum GetCurriculumByVersionId(long curriculumVersionId);
        CurriculumInformation GetCurriculumInformation(long id);
        List<CourseGroup> GetParentCourseGroupsByVersionId(long curriculumVersionId);
        List<CourseGroup> GetCourseGroupsByVersionId(long curriculumVersionId);
        List<CourseGroup> GetCourseGroupRecursiveByVersionId(long versionId);
        List<CourseGroup> GetCourseGroupsForChangeCurriculum(long versionId, long minorId, long concentrationId);
        List<CourseGroup> GetCourseGroupsWithCourses(long versionId, long minorId, long concentrationId);
        List<CourseGroup> GetCourseGroups(long versionId, long minorId, long concentrationId);
        List<CourseGroup> GetCourseGroupsAndChildren(long versionId, long minorId, long concentrationId);
        List<SpecializationGroup> GetSpecializationInformations();
        List<SpecializationGroup> GetSpecializationInformationByCurriculumVersionId(long curriculumVersionId);
        List<CourseGroup> GetRegistrationCourseGroups(long versionId, List<CourseGroupingDetail> courses);
        List<CourseGroup> GetCourseGroupsBySpecializationGroupId(long specializationGroupId);
        bool IsExistCurriculumCode(string code);
        List<CurriculumInstructor> SetCurriculumInstructor(List<long> instructorIds, string type, long versionId);
        List<CurriculumStudyPlanViewModel> GetStudyPlansByCurriculumVersion(long versionId);
        List<CorequisiteDetail> GetCurriculumCorequisites(long versionId);
        List<CourseEquivalentDetail> GetCurriculumCourseEquivalents(long versionId);
        StudyPlan GetStudyPlanById(long id);
        List<StudyCourse> GetStudyCoursesByPlanId(long planId);
        List<Course> GetCurriculumCourse(long versionId);
        List<CurriculumCourse> GetCurriculumCourses(List<long> courseGroupIds);
        void CopyCurriculumVersion(long curriculumVersionId, long oldCurriculumVersionId, List<CourseGroup> courseGroups, bool isCopyPrerequisite, bool isCopySpecializeGroup
            , bool isCopyBlacklistCourses, bool isCopyCoRequisiteAndCourseEquivalent);
        List<Curriculum> GetCurriculumByAcademicLevelId(long id);
        string GetCurriculumNameByIds(List<long> ids);
        CurriculumVersion GetVersionIdByCourseGroup(long id);
        List<CourseGroup> GetCourseGroupChilds(long parentId);
        List<CurriculumCourse> FindCurriculumCourses(long courseGroupId, long? curriculumVersionId);
        CourseGroup FindCourseGroup(long? id);
        List<CurriculumInstructor> GetCurriculumInstructors(long versionId);
        List<CurriculumCourseGroup> GetCurriculumCourseGroups();
        List<CurriculumVersion> GetCurriculumVersionsByCurriculumIdAndStudentId(Guid studentId, long curriculumId);
        List<CurriculumVersion> GetImplementedCurriculumVersionsByStudentId(Guid studentId, long facultyId = 0, long departmentId = 0);
        bool IsExistCourseExclusion(CourseExclusion model);
        bool IsExistCurriculumInformation(CurriculumInformation model);
        bool IsActiveCurriculumInformation(Guid studentId, long curriculumInformationId);
        CurriculumInformationViewModel GetMinorAndConcentration(Guid studentId, string language);
        // List<CourseGroupViewModel> GetCourseGroupWithRegistrationCourses(Guid studentId, long versionId, out int totalCourseGroup);
        List<CourseGroupViewModel> GetCurriculumCourseGroups(long versionId, Guid studentId, ref List<RegistrationCourse> regisCourses, out List<CourseGroupCourseViewModel> curriculumCourses, out int totalCourseGroup);
        List<CourseGroupViewModel> GetCourseGroupModifications(long versionId, Guid studentId, long graduatingRequestId, out int totalCourseGroup);
        CourseGroupViewModel GetOtherCourseGroupRegistrations(Guid studentId, List<CourseGroupCourseViewModel> curriculumCourses);
        List<CourseGroupViewModel> GetCourseSpecializationGroupRegistrations(Guid studentId, string specializationGroupType, long? specializationGroupId);
        List<CourseGroupViewModel> GetCourseGroupRegistrationLogs(long versionId, Guid studentId);

        List<CourseGroupViewModel> GetCourseGroups(Guid studentId, long versionId, out int totalCourseGroup);
        List<CourseGroup> GetCourseGroupBySpecialGroup(long specialGroupId);
        List<CourseGroupViewModel> GetCourseGroupWithRegistrationCourses(Guid studentId, long versionId, out int totalCourseGroup);

        // Ability
        List<CurriculumCourse> GetAbilityCourses(long courseGroupId);
        List<SpecializationGroupBlackList> GetAbilityBlacklistDepartments(long specializationGroupId);

        void SaveCurriculumVersionExpectCredit(List<CurriculumVersion> model);

        // Curriculum Version Structure
        string ExportCurriculum(long facultyId, long departmentId);
        CurriculumVersionStructureViewModel GetCurriculumVersionStructureByStudent(string studentId);
        CurriculumVersionStructureViewModel GetCurriculumVersionStructure(long curriculumVersionId);
        string GetCurriculumVersionStructureJson(long curriculumVersionId);
        string GetCurriculumVersionCourseStructuresJson(long curriculumVersionId);

        void CopyCourseGroupFromSpecilizationGroup(CourseGroup parentGroup);
        List<CurriculumSpecializationGroup> GetSpecializationGroupsByCurriculumVersion(long curriculumVersionId);
    
    }
}