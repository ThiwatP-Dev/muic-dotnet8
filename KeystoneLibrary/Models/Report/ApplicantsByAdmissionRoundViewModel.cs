using KeystoneLibrary.Models.DataModels.Admission;

namespace KeystoneLibrary.Models
{
    public class ApplicantsByAdmissionRoundViewModel
    {
        public Criteria Criteria { get; set; }
        public List<TermDetail> TermDetails { get; set; } = new List<TermDetail>();
        public List<ApplicantsByAdmissionRoundDetail> ApplicantsByAdmissionRoundDetails { get; set; } = new List<ApplicantsByAdmissionRoundDetail>();
    }

    public class TermDetail 
    {
        public long TermId { get; set; }
        public string TermName { get; set; }
        public string TermNameForSort { get; set; }
        public int RoundColspan { get; set; }
        public List<AdmissionRound> Rounds { get; set; } = new List<AdmissionRound>();
        public string RoundTotalColName { get; set; }
        public string ReEnterColName { get; set; }
        public string RoundTotalAndReEnterColName { get; set; }
        public int ApplyStudentSum { get; set; }
        public int IntensiveStudentSum { get; set; }
        public int RegistrationSum { get; set; }
    }

    public class ApplicantsByAdmissionRoundDetail
    {
        public long DepartmentId { get; set; }
        public long FacultyId { get; set; }
        public string DepartmentName { get; set; }
        public string FacultyName { get; set; }
        public bool IsFaculty { get; set; }
        public List<StudentInRound> StudentInRounds { get; set; } = new List<StudentInRound>();
    }

    public class StudentInRound 
    {
        public bool IsSummary { get; set; }
        public int ApplyStudent { get; set; }
        public int IntensiveStudent { get; set; }
        public int Registration { get; set; }
        public int ReEnterRegistration { get; set; }
        public int ReEnterApplyStudent { get; set; }
        public int TotalRegistration { get; set; }
    }
}