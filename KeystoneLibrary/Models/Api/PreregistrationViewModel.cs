namespace KeystoneLibrary.Models.Api
{
    public class PreregistrationViewModel
    {
        public Guid Id { get; set; }
        public int Year { get; set; }
        public int Semester { get; set; }
        public long KSSectionId { get; set; }
        public long KSCourseId { get; set; }
        public string CourseCode { get; set; }
        public string SectionNumber { get; set; }
        public string StudentCode { get; set; }
        public int Sequence { get; set; }
        public bool PaidFlag { get; set; }
        public bool LockFlag { get; set; }
        public string Channel { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}