namespace KeystoneLibrary.Models
{
    public class AddStudentScholarshipViewModel
    {
        public Criteria Criteria { get; set; }
        public List<ScholarshipStudentViewModel> AddStudents { get; set; }
    }

    public class ScholarshipStudentViewModel
    {       
        public Guid StudentId { get; set; }
        public string Code { get; set; }
        public string FullNameEn { get; set; }
        public long FacultyId { get; set; }
        public long AcademicLevelId { get; set; }
        public long DepartmentId { get; set; }
        public long StartTermId { get; set; }
        public long EndTermId { get; set; }
        public string StartStudentBatch { get; set; }
        public string EndStudentBatch { get; set; }
        public bool IsAthlete { get; set; }
        public DateTime EffectiveAt { get; set; }
        public DateTime? ExpiredAt { get; set; }
        public bool IsApproved { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? ReferenceDate { get; set; }
        public bool SentContract { get; set; }
        public decimal LimitedAmount { get; set; }
        public decimal UsedAmount { get; set; }
        public decimal PersonalPay { get; set; }
        public decimal RefundableAmount { get; set; }
        public decimal Balance { get; set; }
        public List<long> ScolarshipIds { get; set; }
        public bool IsExist { get; set; }
    }
}