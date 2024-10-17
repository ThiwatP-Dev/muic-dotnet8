namespace KeystoneLibrary.Models.Report
{
    public class StudentIncidentReportViewModel : StudentInformationViewModel
    {
        public string Incident { get; set; }
        public int AcademicTerm { get; set; }
        public int AcademicYear { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string ApprovedBy { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public bool LockedDocument { get; set; }
        public bool LockedRegistration { get; set; }
        public bool LockedPayment { get; set; }
        public bool LockedVisa { get; set; }
        public bool LockedGraduation { get; set; }
        public bool LockedChangeFaculty { get; set; }
        public bool LockedSignIn { get; set; }
        public string Remark { get; set; }
        public string ApprovedAtText => ApprovedAt?.AddHours(7).ToString(StringFormat.ShortDate);
        public string CreatedAtText => CreatedAt.AddHours(7).ToString(StringFormat.ShortDate);
        public string UpdatedAtText => UpdatedAt.AddHours(7).ToString(StringFormat.ShortDate);
        public string TermText => $"{ AcademicTerm }/{ AcademicYear }";
    }
}