using KeystoneLibrary.Models.DataModels.Profile;

namespace KeystoneLibrary.Models.Report
{
    public class EntranceExaminationReportViewModel
    {
        public Criteria Criteria { get; set; }
        public long AdmissionRoundId { get; set; }
        public string Term { get; set; }
        public string Faculty { get; set; }
        public string Department { get; set; }
        public List<Student> Students { get; set; }
        public string Rooms { get; set; }
        public string TestDates { get; set; }
        public string TestTimes { get; set; }
        public List<EntranceExaminationSchedule> EntranceExaminationSchedules { get; set; }
    }

    public class EntranceExaminationSchedule
    {
        public long Id { get; set; }
        public string Room { get; set; }
        public string TestDate { get; set; }
        public string TestTime { get; set; }
        public long AdmissionExaminationTypeId { get; set; }
        public string AdmissionExaminationType { get; set; }
    }
}