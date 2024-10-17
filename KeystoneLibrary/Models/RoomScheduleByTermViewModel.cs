using KeystoneLibrary.Models.DataModels;

namespace KeystoneLibrary.Models
{
    public class RoomScheduleByTermViewModel
    {
        public Criteria Criteria { get; set; }
        public List<long> RoomIds { get; set; }
        public long TermId { get; set; }
        public string Term { get; set; }
        public string Name { get; set; }
        public string BuildingNameEn { get; set; }
        public string BuildingNameTh { get; set; }
        public string CampusNameEn { get; set; }
        public string CampusNameTh { get; set; }
        public string PrintDateString { get; set; }
        public List<RoomScheduleInfoByTermViewModel> Rooms { get; set; }
        public List<RoomSlot> RoomSlots { get; set; }
        public List<ScheduleViewModel> Schedules { get; set; }
        public List<RoomScheduleDetailByTermPreview> Preview { get; set; }
    }

    public class RoomScheduleDetailByTermPreview
    {
        public string Name { get; set; }
        public string Term { get; set; }
        public string BuildingName { get; set; }
        public string CampusName { get; set; }
        public string PrintDateString { get; set; }
        public long RoomId { get; set; }
        public long TermId { get; set; }
        public string SectionStatus { get; set; }
        public List<ScheduleViewModel> Schedules { get; set; }
    }

    public class RoomScheduleInfoByTermViewModel
    {
        public long Id { get; set; }
        public string NameEn { get; set; }
        public string BuildingName { get; set; }
        public int Floor { get; set; }
        public int Capacity { get; set; }
        public string RoomType { get; set; }
    }

    public class RoomScheduleSectionDetailViewModel
    {
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public bool IsParent { get; set; }
        public string SectionNumber { get; set; }
        public int SeatUsed { get; set; }
        public DateTime? MidtermDate { get; set; }
        public DateTime? FinalDate { get; set; }
        public TimeSpan? MidtermStart { get; set; }
        public TimeSpan? MidtermEnd { get; set; }
        public TimeSpan? FinalStart { get; set; }
        public TimeSpan? FinalEnd { get; set; }
        public long SectionId { get; set; }
        public int Day { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string RoomName { get; set; }
        public string TitleName { get; set; }
        public string FirstNameEn { get; set; }
        public string LastNameEn { get; set; }
        public string Time => $"{ StartTime.ToString(StringFormat.TimeSpan) } - { EndTime.ToString(StringFormat.TimeSpan) }";
        public string FinalString => $"{ FinalDate?.ToString(StringFormat.ShortDate) }";
        public string FinalTime => $"({ FinalStart?.ToString(StringFormat.TimeSpan) } - { FinalEnd?.ToString(StringFormat.TimeSpan) })";

        public string MidtermString => $"{ MidtermDate?.ToString(StringFormat.ShortDate) }";
        public string MidtermTime => $"({ MidtermStart?.ToString(StringFormat.TimeSpan) } - { MidtermEnd?.ToString(StringFormat.TimeSpan) })";

        public string InstructorNameEn => $"{ TitleName } { FirstNameEn } { LastNameEn }";

        public string InstructorShortNameEn 
        { 
            get 
            { 
                var lastname = String.IsNullOrEmpty(LastNameEn) ? "" : $"{ LastNameEn.Substring(0,1) }.";
                return $"{ TitleName } { FirstNameEn } { lastname }";
            } 
        }

        public string Dayofweek
        {
            get
            {
                if (Day != -1)
                {
                    var day = Enum.GetName(typeof(DayOfWeek),Day)?.Substring(0,3).ToUpper();
                    return day;
                }
                return "";
            }
        }
    }
}