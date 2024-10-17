namespace KeystoneLibrary.Models.DataModels
{
    public class Calendar
    {
        public long Id { get; set; }
        public DateTime Date { get; set; }
        public int Year { get; set; }
        public int BuddhaYear { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public int DayOfWeek { get; set; }
        public int WeekNumber { get; set; }
        public int WeekOfYear { get; set; }
        public int LastDayOfMonth { get; set; }
        public DateTime LastDateOfMonth { get; set; }
    }
}