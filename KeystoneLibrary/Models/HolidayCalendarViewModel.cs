using KeystoneLibrary.Models.DataModels;

namespace KeystoneLibrary.Models
{
    public class HolidayCalendarViewModel
    {
        public Criteria Criteria { get; set; }
        public List<HolidayCalendarDetailViewModel> Details { get; set; }
    }

    public class HolidayCalendarDetailViewModel
    {
        public long Id { get; set; }
        public long MuicId { get; set; }
        public string Title { get; set; }
        //public DateTime Date { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string AcademicLevel { get; set; }
        public string Type { get; set; }
        public string Color { get; set; }
        public string Remark { get; set; }
        public bool IsCancel { get; set; }
        public bool IsAllowMakeup { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public List<SectionSlot> SectionSlots { get; set; }
    }
}