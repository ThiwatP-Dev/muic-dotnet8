using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.Prerequisites;

namespace KeystoneLibrary.Interfaces
{
    public interface IPrerequisiteProvider
    {
        StudentInfoForPrerequisiteViewModel GetStudentInfoForPrerequisite(Guid studentId);
        StudentInfoForPrerequisiteViewModel GetStudentInfoForPrerequisite(string studentCode);
        //bool CheckPrerequisite(StudentInfoForPrerequisiteViewModel studentInfo, long courseId);
        bool CheckPrerequisite(StudentInfoForPrerequisiteViewModel studentInfo, long courseId, long curriculumVersionId, out string message);
        CheckPrerequisiteConditionViewModel IsConditionAlreadyExits(long id, string type);
        List<PrerequisiteCheckDetailViewModel> CheckPrerequisite(StudentInfoForPrerequisiteViewModel studentInfo, long courseId, long curriculumVersionId);
        string GetPrerequisiteByCourseId(long courseId);
        List<string> GetPrerequisitesByCourseId(long courseId, long curriculumVersionId);
        List<Prerequisite> GetPrerequisiteByCurriculumVersionId(long curriculumVersionId);
        Course CheckCourseCorequisite(StudentInfoForPrerequisiteViewModel studentInfo, long courseId);
        List<CoursePrerequisiteViewModel> GetPrerequisiteCurriculumVersion(long curriculumVersionId);
        GradeCondition GetGradeCondition(long id);
        CreditCondition GetCreditCondition(long id);
        GPACondition GetGPACondition(long id);
        CourseGroupCondition GetCourseGroupCondition(long id);
        Corequisite GetCorequisite(long id);
        TotalCourseGroupCondition GetTotalCourseGroupCondition(long id);
        TermCondition GetTermCondition(long id);
        AndCondition GetAndConditionById(long id);
        OrCondition GetOrConditionById(long id);
        BatchCondition GetBatchConditionById(long id);
        AbilityCondition GetAbilityConditionById(long id);
        bool IsExistsPrerequisite(long courseId);
        PredefinedCourse GetPredefinedCourse(long id);
        bool IsExistGradeCondition(GradeCondition grade);
        bool IsExistBatchCondition(BatchCondition batch);
        bool IsExistGPACondition(GPACondition gpa);
        bool IsExistCourseGroupCondition(CourseGroupCondition courseGroup);
        bool IsExistCourseCorequisite(Corequisite corequisite);
        bool IsExistCreditCondition(CreditCondition creditCondition);
        bool IsExistTotalCourseGroupCondition(TotalCourseGroupCondition totalCourseGroupCondition);
        bool IsExistTermCondition(TermCondition termCondition);
        bool IsExistAndCondition(AndCondition andCondition);
        bool IsExistOrCondition(OrCondition orCondition);
        bool IsExistedPredefinedCourse(PredefinedCourse course);
        bool IsExistAbilityCondition(AbilityCondition ability);
        List<AndCondition> GetAndConditions();
        List<OrCondition> GetOrConditions();
        List<CourseGroupCondition> GetCourseGroupConditions();
        List<CreditCondition> GetCreditConditions();
        List<GPACondition> GetGPAConditions();
        List<GradeCondition> GetGradeConditions();
        List<TermCondition> GetTermConditions();
        List<TotalCourseGroupCondition> GetTotalCourseGroupConditions();
        List<BatchCondition> GetBatchConditions();
        List<AbilityCondition> GetAbilityConditions();
        List<AndCondition> GetAndConditionNames(List<AndCondition> conditions);
        List<OrCondition> GetOrConditionNames(List<OrCondition> conditions);
        void GetPrerequisiteNames(ref List<Prerequisite> conditions);
        PrerequisiteFormViewModel GetPrerequisiteFormViewModel(long id, long? curriculumVersionId);
        Prerequisite GetPrerequisite(long id);
        PrerequisiteGraphViewModel GetPrerequisiteGraph(long curriculumVersionId);
        string GetConditionDescription(string conditionType, long conditionId, bool isGetDeeperLevel);
        string GetAndConditionDescription(AndCondition condition, bool isGetDeeperLevel);
        string GetOrConditionDescription(OrCondition condition, bool isGetDeeperLevel);
        string GetCorequisiteDescription(Corequisite condition);
        string GetPredefinedCourseDescription(PredefinedCourse condition);
        string GetGPAConditionDescription(GPACondition model);
        string GetCreditConditionDescription(CreditCondition model);
        string GetCourseGroupConditionDescription(CourseGroupCondition model);
        string GetGradeConditionDescription(GradeCondition model, bool isGetDeeperLevel);
        string GetTermConditionDescription(TermCondition model);
        string GetTotalCourseGroupConditionDescription(TotalCourseGroupCondition model);
        string GetAbilityConditionDescription(AbilityCondition model);
        string GetBatchConditionDescription(BatchCondition model);
        string GetUpdateAndConditionDescription(AndCondition condition, bool isGetDeeperLevel);
        string GetUpdateOrConditionDescription(OrCondition condition, bool isGetDeeperLevel);
    }
}