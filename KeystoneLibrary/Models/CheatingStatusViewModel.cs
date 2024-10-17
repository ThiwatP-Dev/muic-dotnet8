namespace KeystoneLibrary.Models
{
    public class CheatingStatusViewModel
    {
        public long Id { get; set; }
        public long AcademicLevelId { get; set; }
        public long TermId { get; set; }
        public string StudentCode { get; set; }
        public string StudentFullName { get; set; }
        public string ExaminationType { get; set; }
        public string PunishType { get; set; }
        public string InvestigatedAtText { get; set; }
        public string RegistrationApprovedAtText { get; set; }
        public bool IsCheating { get; set; }
    }
}