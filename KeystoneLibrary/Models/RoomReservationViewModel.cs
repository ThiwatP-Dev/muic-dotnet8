namespace KeystoneLibrary.Models
{
    public class RoomReservationViewModel
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Room { get; set; }
        public string CreatedAtText { get; set; }
        public string CreatedBy { get; set; }
        public string DateFromText { get; set; }
        public string DateToText { get; set; }
        public string StartedAtText { get; set; }
        public string EndedAtText { get; set; }
        public string SenderTypeText { get; set; }
        public string UsingTypeText { get; set; }
        public string Description { get; set; }
        public string Remark { get; set; }
        public string TimeDisplay { get; set; }
        public string Status { get; set; }
        public string AcademicLevelNameEn { get; set; }
        public string TermText { get; set; }
        public bool IsSunday { get; set; } = false;
        public bool IsMonday { get; set; } = false;
        public bool IsTuesday { get; set; } = false;
        public bool IsWednesday { get; set; } = false;
        public bool IsThursday { get; set; } = false;
        public bool IsFriday { get; set; } = false;
        public bool IsSaturday { get; set; } = false;
        public List<RoomReservationSlotViewModel> Results { get; set; } = new List<RoomReservationSlotViewModel>();
    }

    public class RoomReservationSlotViewModel
    {
        public string IsChecked { get; set; }
        public long Id { get; set; }
        public long? RoomReservationId { get; set; }
        public string RoomNameEn { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get ; set; }
        public bool IsCancel { get; set; }
        public int Day { get; set; }

        public string Dayofweek
        {
            get
            {
               var day = Enum.GetName(typeof(DayOfWeek),Day).Substring(0,3).ToUpper();
               return day ;
            }
        }
        public string StartTimeText => StartTime.ToString(StringFormat.TimeSpan);
        public string EndTimeText => EndTime.ToString(StringFormat.TimeSpan);
        public string DateText => Date.ToString(StringFormat.ShortDate);
        public string Time => $"{ StartTimeText } - { EndTimeText }";
    }
}