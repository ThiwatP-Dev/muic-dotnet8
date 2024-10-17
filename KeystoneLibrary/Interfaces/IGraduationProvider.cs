using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.Graduation;
using KeystoneLibrary.Models.USpark;

namespace KeystoneLibrary.Interfaces
{
    public interface IGraduationProvider
    {
        long GetGraduatingRequestByStaff(Guid studentId);
        Task<GraduatingRequestViewModel> GetGraduatingRequestDetail(long graduatingRequestId);
        GraduatingRequestViewModel GetCourseGroupingDetail(long graduatingRequestId);
        GraduatingRequestViewModel GetCourseGroupingLogDetail(long courseGroupingLogId);
        GraduatingRequestViewModel GetCourseGroupingLogMoveGroup(long courseGroupingLogId);
        void ChangeStatus(long id, string status, string remark, bool isPublish);
        List<CoursePrediction> GetCoursePredictions(long graduatingRequestId);
        void SaveCoursePredictions(long graduatingRequestId, List<CoursePrediction> coursePredictions);
        GraduatingRequestViewModel GetCourseGroupingLogs(long graduatingRequestId);
        long DeleteCourseGroupingLog(long CourseGroupingLogId);
        long UpdateCourseGroupingLogTogglePublish(long CourseGroupingLogId, bool IsPublished);
        long UpdateCourseGroupingLogToggleApprove(long CourseGroupingLogId, bool IsApproved);
        long SaveEqualCourses(GraduatingRequestViewModel model);
        void SaveGroupingCourseMoveGroup(GraduatingRequestViewModel model);
        GraduatingRequestViewModel GetGroupingCourseMoveGroupForPrint(long courseGroupingLogId);
        bool SaveGraduation(Guid studentId, string studentStatus, DateTime? graduatedAt, long? termId, long? honorId, string remark, List<GraduationHonor> honors);
        List<GraduationHonor> GetGraduationHonors(Guid studentId);
        GraduatingRequest CreateGraduatingRequest(Guid studentId);
        CourseGroupModification AddCourseToGraduationCourseGroup(Guid studentId, long courseId, long courseGroupId, long? requiredGradeId);
        void DisabledCourseToGraduationCourseGroup(Guid studentId, long courseId, long courseGroupId);
        List<USparkGraduatingTerm> GetGraduatingTerms(long academicLevelId, int totalTerms, int termPerYear);
        IOrderedQueryable<GraduatingRequestExcelViewModel> GetGraduatingRequest(Criteria criteria);
    }
}