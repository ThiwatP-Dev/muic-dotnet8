namespace KeystoneLibrary.Models
{
    public class ReportViewModel
    {
        public long TermId { get; set; }
        public long AcademicLevelId { get; set; }
        public string Title { get; set; }
        public string Subject { get; set; }
        public string Creator { get; set; }
        public string Author { get; set; }
        public string Language { get; set; }
        public string ProfileImageURL { get; set; }
        public object Body { get; set; }
        public Criteria Criteria { get; set; }
    }
}