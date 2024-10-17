using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.MasterTables;

namespace KeystoneLibrary.Models
{
    public class CoordinatorViewModel
    {
        public long AcademicLevelId { get; set; }
        public AcademicLevel AcademicLevel { get; set; }
        public long TermId { get; set; }
        public Term Term { get; set; }
        public long CourseId { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public List<long> CoordinatorIds { get; set; }
        public List<long> InstructorIds { get; set; }
        public List<Instructor> Instructors { get; set; }
        public long TotalCoordinators { get; set; }
    }
}