namespace KeystoneLibrary.Models
{
    public class LatePaymentConfigurationViewModel
    {
        public long AcademicLevelId { get; set; }
        public long Id { get; set; }
        public long FromTermId { get; set; }
        public long? ToTermId { get; set; }
        public decimal AmountPerDay { get; set; }
        public int MaximumDays { get; set; }
        public bool IsActive { get; set; }
    }
}