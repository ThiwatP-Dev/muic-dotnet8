namespace KeystoneLibrary.Models
{
    public class AcademicCalendarViewModel
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string AcademicLevel { get; set; }
        public string Color { get; set; }
        public string Remark { get; set; }
    }
}