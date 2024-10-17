namespace KeystoneLibrary.Models
{
    public class ScheduleViewModel
    {
        public long Id { get; set; }
        public long? ParentSectionId { get; set; }
        public long CourseId { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public string CourseCodeAndCredit { get; set; }
        public string MainInstructorFullNameEn { get; set; }
        public string PreRequisite { get; set; }
        public string Section { get; set; }
        public int SeatUsed { get; set; }
        public string ColorCode { get; set; }
        public string MidtermDate { get; set; }
        public string MidtermTime { get; set; }
        public string FinalDate { get; set; }
        public string FinalTime { get; set; }
        public bool IsClosed { get; set; }
        public int CourseCredit { get; set; }
        public int RegistrationCourseCredit { get; set; }
        public string CourseCreditText { get; set; }
        public string MainInstructor { get; set; }
        public string SectionType { get; set; }
        public string Remark => StartedAt == null || EndedAt == null
                                ? "" : $"{ StartedAt?.ToString(StringFormat.ShortDate) } - { EndedAt?.ToString(StringFormat.ShortDate) }";

        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public int SeatLimit { get; set; }
        public int SeatAvailable { get; set; }
        public List<ClassScheduleTimeViewModel> ScheduleTimes { get; set; }
        public int NumberValue
        {
            get
            {
                int number;
                bool success = Int32.TryParse(Section, out number);
                return success ? number : 0;
            }
        }
    }

    public class ClassScheduleTimeViewModel
    {
        private string _instructorNameEn;
        private string _instructorNameTh;
        private string _room;
        public long SectionId { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public string SectionNumber { get; set; }
        public int Day { get; set; }
        public string DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; } 
        public TimeSpan EndTime { get; set; } 
        public string TimeText { get; set; }
        public string Type { get; set; }
        public string Instructors { get; set; }
        public string Period { get; set; }
        public string Hours { get; set; }
        public string MidtermTime { get; set; }
        public string FinalTime { get; set; }
        public string ExaminationDate { get; set; }
        public string ExaminationTime { get; set; }
        public string InstructorShortName { get; set; }
        public List<long> InstructorIds => Instructors == null ? new List<long>() : JsonConvert.DeserializeObject<List<long>>(Instructors);
        public string Time => $"{ StartTime.ToString(StringFormat.TimeSpan) } - { EndTime.ToString(StringFormat.TimeSpan) }";
        public int TimeStartHours => StartTime.Hours;
        public int TimeEndHours => EndTime.Hours;
        public int TimeStartMinutes => StartTime.Minutes;
        public int TimeEndMinutes => EndTime.Minutes;
        public string Room
        {
            get
            {
                return String.IsNullOrEmpty(_room) ? "N/A" : _room;
            }
            set
            {
                _room = value;
            }
        }

        public string InstructorNameEn 
        {
            get
            {
                return String.IsNullOrEmpty(_instructorNameEn) ? "N/A" : _instructorNameEn;
            }
            set
            {
                _instructorNameEn = value;
            }
         }

        public string InstructorNameTh 
        {
            get
            {
                return String.IsNullOrEmpty(_instructorNameTh) ? "N/A" : _instructorNameTh;
            }
            set
            {
                _instructorNameTh = value;
            }
        }

    }
}