namespace KeystoneLibrary.Models
{
    public class ExtendedStudentViewModel
    {
        public Criteria Criteria { get; set; }
        public List<ExtendedStudentDetail> Students { get; set; }
    }
    public class ExtendedStudentDetail
    {
        public bool IsCheck { get; set; }
        public bool IsSendEmail { get; set; }
        public Guid StudentId { get; set; }
        public string StudentCode { get; set; }
        public string StudentTitle {get; set; }
        public string StudentFirstName {get; set; }
        public string StudentMidName { get; set; }
        public string StudentLastName { get; set; }
        public string Department { get; set; }
        public string AdmissionTerm { get; set; }
        public decimal StudiedYear { get; set; }
        public string StudentEmail { get; set; }
        public int CreditComp { get; set; }
        public int ExtendedYear { get; set; }
        public string StudentName => string.IsNullOrEmpty(StudentMidName) ? $"{ StudentTitle } { StudentFirstName } { StudentLastName }"
                                                                          : $"{ StudentTitle } { StudentFirstName } { StudentMidName } { StudentLastName }";
    }
}