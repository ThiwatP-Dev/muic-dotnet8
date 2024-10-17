using KeystoneLibrary.Models.DataModels;

namespace KeystoneLibrary.Models
{
    public class TeachingScheduleViewModel
    {
        public Criteria Criteria { get; set; }
        public string RoomName { get; set; }
        public string BuildingName { get; set; }
        public string Term { get; set; }
        public string Campus { get; set; }
        public DateTime Date { get; set; }
        public List<TeachingScheduleDetail> TeachingScheduleDetails { get; set; }
        public List<ScheduleViewModel> Schedules { get; set; }
    }

    public class TeachingScheduleDetail
    {
        public long TermId { get; set; }
        public long RoomId { get; set; }
        public long CampusId { get; set; }
        public long BuildingId { get; set; }
        public int Floor { get; set; }
        public DateTime Date { get; set; }
        public string UsingType { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public long SectionId { get; set; }
        public string Section { get; set; }
        public string Term { get; set; }
        public int SeatUsed { get; set; }
        public string Day { get; set; }
        public string Time { get; set; }
        public string SectionInstructor { get; set; }
        public string ExaminationInstructor { get; set; }
        public string RoomReservationName { get; set; }
        public List<RegistrationCourse> Registrations { get; set; }
    }
}