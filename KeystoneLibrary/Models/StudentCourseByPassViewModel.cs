namespace KeystoneLibrary.Models
{
    public class StudentCourseByPassViewModel 
    {
        public long Id { get; set; }
        public long AcademicLevelId { get; set; }
        public long TermId { get; set; }
        public string StudentId { get; set; }
        public List<long> CourseIds { get; set; }
        public bool IsActive { get; set; }
        public string Remark { get; set; }
    }
    public class StudentCourseByPassesViewModel 
    {
        public long Id { get; set; }
        public string AcademicLevelNameEn { get; set; }
        public string TermText { get; set; }
        public string StudentCode { get; set; }
        public string Title { get; set; }
        public string FirstNameEn { get; set; }
        public string LastNameEn { get; set; }
        public string MidNameEn { get; set; }
        public string CourseCodes { get; set; }
        public string CourseIdsString { get; set; }
        public string Remark { get; set; }
        public bool IsActive { get; set; }
        public List<long> CourseIds => string.IsNullOrEmpty(CourseIdsString) ? new List<long>() : JsonConvert.DeserializeObject<List<long>>(CourseIdsString);
        public string FullNameEn => string.IsNullOrEmpty(MidNameEn) ? $"{ Title } { FirstNameEn } { LastNameEn }"
                                                                    : $"{ Title } { FirstNameEn } { MidNameEn } { LastNameEn }";    
        public string FullNameEnNoTitle => string.IsNullOrEmpty(MidNameEn) ? $"{ FirstNameEn } { LastNameEn }"
                                                                           : $"{ FirstNameEn } { MidNameEn } { LastNameEn }";
    }

    public class BodyUpdateStudentCourseByPass
    {
        public string StudentCode { get; set; }
        public long AcademicYear { get; set; }
        public long Term { get; set; }
        public List<long> KSCourseIds { get; set; }
    }
}