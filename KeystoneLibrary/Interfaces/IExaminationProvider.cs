using KeystoneLibrary.Models.DataModels;

namespace KeystoneLibrary.Interfaces
{
    public interface IExaminationProvider
    {
        ExaminationPeriod GetExaminationPeriod(long id);
        ExaminationCoursePeriod GetExaminationCoursePeriod(long id);
        ExaminationCourseCondition GetExaminationCourseCondition(long id);
        bool IsPeriodExists(long id, long termId, int period);
        bool IsCoursePeriodExists(long id, long courseId, bool isEvening);
        string GetExaminationCoursePeriodCondition(long courseId);
    }
}