namespace KeystoneLibrary.Models
{
    public class LateRegistrationConfigurationViewModel
    {
        public long AcademicLevelId { get; set; }        
        public List<LateRegistrationViewModel> LateRegistrations { get; set; }
    }

    public class LateRegistrationViewModel
    {
        public long Id { get; set; }
        public long AcademicLevelId { get; set; }     
        public string AcademicLevelNameEn { get; set; }
        public long FromTermId { get; set; }
        public string FromTermText { get;set; }
        public long? ToTermId { get; set; }
        public string ToTermText { get; set; }
        public decimal Amount { get; set; }
        public bool IsActive { get; set; }
    }
}