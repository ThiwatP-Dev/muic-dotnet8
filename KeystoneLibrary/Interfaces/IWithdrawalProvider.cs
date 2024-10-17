using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Withdrawals;

namespace KeystoneLibrary.Interfaces
{
    public interface IWithdrawalProvider
    {
        bool IsInWithdrawalPeriod(long academiceLevelId, string type, DateTime requestedAt);
        bool IsPeriodExisted(WithdrawalPeriod model);
        Withdrawal GetWithdrawal(long id);
        List<Withdrawal> GetWithdrawalsByRegistrationCourses(List<long> registrationCourseIds, string type);
        List<WithdrawalException> GetExceptionalCourses(string CourseName);
        List<WithdrawalException> GetExceptionalFaculties(long facultyId, long departmentId = 0);
        List<WithdrawalException> GetWithdrawalExceptionsByDepartmentIds(List<long> departmentIds);
        bool IsExistExceptionCourse(long courseId);
        bool IsExistExceptionDepartment(long facultyId, long departmentId);
        WithdrawalReportViewModel GetWithdrawalReport(Criteria criteria);
    }
}