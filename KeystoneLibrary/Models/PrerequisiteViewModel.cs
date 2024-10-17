using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.Profile;

namespace KeystoneLibrary.Models
{
    public class PrerequisiteViewModel
    {
        public bool ResultBool { get; set; }
        public string ResultString { get; set; }
        public List<CoursePrerequisiteViewModel> CurriculumVersionPrerequisite { get; set; }
    }

    public class StudentInfoForPrerequisiteViewModel
    {
        public Student Student { get; set; }
        public AcademicInformation AcademicInformation { get; set; }
        public List<RegistrationCourse> RegistrationCourses { get; set; }
    }

    public class CoursePrerequisiteViewModel
    {
        public Course Course { get; set; }
        public string PrerequisiteDetail { get; set; }
    }

    public class PrerequisiteFormViewModel
    {
        public long Id { get; set; }
        public long? CurriculumVersionId { get; set; }
        public string CurriculumVersionName { get; set; }
        public long CourseId { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public string Condition { get; set; }
        public bool IsActive { get; set; }
        public List<long> AndConditionIds { get; set; }
        public List<long> OrConditionIds { get; set; }
        public List<long> CourseGroupConditionIds { get; set; }
        public List<long> TermConditionIds { get; set; }
        public List<long> GPAConditionIds { get; set; }
        public List<long> CreditConditionIds { get; set; }
        public List<long> GradeConditionIds { get; set; }
        public List<long> TotalCourseGroupConditionIds { get; set; }
        public List<long> BatchConditionIds { get; set; }
        public List<long> AbilityConditionIds { get; set; }
    }
}