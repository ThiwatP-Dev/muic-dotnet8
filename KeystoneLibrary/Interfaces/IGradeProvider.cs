using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.MasterTables;

namespace KeystoneLibrary.Interfaces
{
    public interface IGradeProvider
    {
        List<string> GetGradeOptions();
        List<ExaminationType> GetExaminationTypes();
        List<GradeSection> GetGradeSectionsByCourseIds(List<long> courseIds);
        List<Grade> GetGrades();
        List<Grade> GetWeightGrades();
        List<GradingLog> GetGradingLogsByRegistrationCourseId(long Id);
        List<Grade> GetNonWeightGrades();
        List<Coordinator> GetCoordinators(long courseId, long termId);
        List<StudentGradeRecord> GetStudentScoresByBarcodeId(long barcodeId);
        Grade GetGradeById(long Id);
        List<long> GetCoordinatorCourse(long instructorId, long termId);
        List<Section> GetSectionByInstructorId(long instructorId, long courseId, long termId);
        bool CheckCourseHaveCoordinator(long termId, long courseId);
        Grade GetGradeByName(string name);
        GradingLog SetGradingLog(long? studentRawScoreId, long registrationCourseId, string previousGrade, 
                                 string currentGrade, string userId, string remark, string type);
        List<Grade> GetUncalculatedGrade();
        List<Grade> GetShowTranscriptGrade();
        GradingLog GetLatestGradingLog(long studentRawScoreId);
        List<GradingFrequency> GetGradingFrequencies(List<StudentScoreAllocation> studentScoreAllocations);
        List<GradeNormalCurve> GetGradeNormalCurves(ClassStatistics classStatistics);
        GradingReportViewModel GetGradeScoreSummaryViewModelByCourseId(long courseId, long termId, long instructorId, bool isGradeMember = false);
        GradingReportViewModel GetGradeScoreSummaryViewModelByCourseIdForReport(long courseId, long termId, long instructorId, bool isGradeMember = false);
        List<GradingReportViewModel> GetGradeApprovalDetailForPreview(List<long> barcodeIds);
        List<StandardGradingGroup> GetStandardGradingGroups(List<long> ids);
        List<StudentRawScoreViewModel> GetStudentRawScoresBySections(List<long?> sectionIdsNullable, List<StudentRawScoreDetail> studentRawScoreDetails);
        List<Barcode> GenerateBarcode(long termId, List<long> sectionIds, Course course);
        ClassStatistics GetClassStatisticsGradeScoreSummary(List<StudentScoreAllocation> studentScoreAllocations);
        List<GradingRange> GetSummaryGradingCurves(long courseId, long termId, List<StudentScoreAllocation> students);
        List<GradingCurve> GetGradingCurveByCourseIdAndTermId(long courseId, long termId);
        List<StudentRawScoreViewModel> GetStudentRawScoresBySectionList(List<long?> sectionIdsNullable, List<StudentRawScoreDetail> studentRawScoreDetails, List<StudentRawScoreViewModel> studentRawScores);

        //Grade Template
        GradeTemplate FindGradeTemplate(long id);
        IEnumerable<GradeTemplate> GetTemplates();
        GradeTemplate AddGradeTemplate(GradeTemplate template);
        GradeTemplate UpdateGradeTemplate(GradeTemplate template);
        GradeTemplate DeleteGradeTemplate(long id);
        GradeTemplate GetGradeTemplateByCourses(List<long> courseIds);
        List<Grade> GetGradesByGradeTemplate(GradeTemplate gradeTemplate);

    }
}