namespace KeystoneLibrary.Models
{
    public class PrerequisiteCheckDetailViewModel
    {
        public string Description { get; set; }
        public string Curriculum { get; set; }
        public bool IsPass { get; set; }
        public string IsPassText => IsPass ? "Pass" : "Not Pass";

        public bool IsNotFound { get; internal set; }
    }
}