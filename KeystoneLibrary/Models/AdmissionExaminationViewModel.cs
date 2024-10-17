using KeystoneLibrary.Models.DataModels.Admission;

namespace KeystoneLibrary.Models
{
    public class AdmissionExaminationViewModel
    {
        public long Id { get; set; }
        public long AcademicLevelId { get; set; }
        public string AcademicLevel { get; set; }
        public long AdmissionTermId { get; set; }
        public string AdmissionTerm { get; set; }
        public long AdmissionRoundId { get; set; }
        public string AdmissionRound { get; set; }

        public long FacultyId { get; set; }
        public long? DepartmentId { get; set; }
        public string NameEn { get; set; }
        public string NameTh { get; set; }
        public string Remark { get; set; }
        public bool IsActive { get; set; } = true;
        public string Faculty { get; set; }
        public string Department { get; set; }
        public List<AdmissionExaminationSchedule> AdmissionExaminationSchedules { get; set; } = new List<AdmissionExaminationSchedule>();
        public List<AdmissionExaminationDetail> AdmissionExaminationDetails { get; set; } = new List<AdmissionExaminationDetail>();
    }

    public class AdmissionExaminationDetail
    {
        public long FacultyId { get; set; }
        public List<long?> DepartmentIds { get; set; }
    }
}