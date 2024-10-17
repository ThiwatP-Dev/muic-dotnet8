namespace KeystoneLibrary.Models
{
    public class TeachingLoadViewModel
    {
        public long Id { get; set; }
        public long TermId { get; set; }
        public long InstructorId { get; set; }
        public long CourseId { get; set; }
        public int TotalSectionsCount { get; set; }
        public long TeachingTypeId { get; set; }
        public decimal Load { get; set; }
        public bool IsExtraLoad { get; set; }
    }
}