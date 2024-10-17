namespace KeystoneLibrary.Models
{
    public class AddDropFeeConfigurationViewModel
    {
        public long AcademicLevelId { get; set; }
        public long Id { get; set; }
        public long FromTermId { get; set; }
        public long? ToTermId { get; set; }
        public decimal Amount { get; set; }
        public int FreeAddDropCount { get; set; }
        public bool IsActive { get; set; }
    }
}