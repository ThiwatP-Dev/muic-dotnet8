namespace KeystoneLibrary.Models
{
    public class InstructorTeachingStatusViewModel
    {
        public long TermId { get; set; }
        public string Term { get; set; }
        public int AcademicYear { get; set; }
        public int AcademicTerm { get; set; }
        public List<InstructorTeachingDetail> InstructorTeachingDetails { get; set; }
    }

    public class InstructorTeachingDetail
    {
        public long AcademicLevelId { get; set; }
        public long TermId { get; set; }
        public long CourseId { get; set; }
        public long SectionId { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public string Section { get; set; }
        public string Midterm { get; set; }
        public string Final { get; set; }
        public bool IsClosed { get; set; }
        public List<string> SectionDetails { get; set; }
        public List<string> Rooms { get; set; }
    }
}