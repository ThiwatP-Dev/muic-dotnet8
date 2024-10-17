using KeystoneLibrary.Models.DataModels;

namespace KeystoneLibrary.Models
{
    public class ExaminationPeriodViewModel
    {
        public Criteria Criteria { get; set; }
        public long CourseId { get; set; }
        public long TermId { get; set; }
        public DateTime MidtermDate { get; set; } = DateTime.Now;
        public TimeSpan MidtermStart { get; set; }
        public TimeSpan MidtermEnd { get; set; }
        public DateTime FinalDate { get; set; } = DateTime.Now;
        public TimeSpan FinalStart { get; set; }
        public TimeSpan FinalEnd { get; set; }
        public bool IsEvening { get; set; }
        public List<ExaminationPeriod> examPeriods { get; set; }
    }
}