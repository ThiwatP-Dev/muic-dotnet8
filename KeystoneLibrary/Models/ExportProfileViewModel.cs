namespace KeystoneLibrary.Models
{
    public class ExportProfileViewModel
    {
        public Criteria Criteria { get; set; }
        public List<ExportProfileStudent> Results { get; set; }
    }

    public class ExportProfileStudent
    {
        public string IsChecked { get; set; }
        public string StudentCode { get; set; }
        public string StudentFullName { get; set; }
        public string ProfileImageURL { get; set; }
    }
}