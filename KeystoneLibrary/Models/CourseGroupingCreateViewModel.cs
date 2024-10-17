namespace KeystoneLibrary.Models
{
    public class CourseGroupingCreateViewModel
    {
        public long CourseGroupId { get; set; }
        public string CourseGroupName { get; set; }
        public long CourseId { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public string CourseCodeAndName { get; set; }
        public string Grades { get; set; }
        public long? EquivalentCourseId { get; set; }
        public List<long> RegistrationCourseIds { get; set; }
        public string GradeName { get; set; }
    }
}