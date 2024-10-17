using KeystoneLibrary.Models.DataModels.Withdrawals;

namespace KeystoneLibrary.Models
{
    public class WithdrawalExceptionViewModel
    {
        public long AcademicLevelId { get; set; }
        public long FacultyId { get; set; }
        public long DepartmentId { get; set; }
        public long CourseId { get; set; }
        public string CourseName { get; set; }
        public string RespondStatus { get; set; } // SaveSuccess, InvalidInput, DataDuplicate
        public List<WithdrawalException> ExceptionalCourses { get; set; }
        public List<WithdrawalException> ExceptionalFaculty { get; set; }
    }
}