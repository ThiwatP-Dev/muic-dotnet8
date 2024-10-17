using KeystoneLibrary.Models.DataModels;

namespace KeystoneLibrary.Models.Report
{
    public class SectionInstructorViewModel
    {
        public Criteria Criteria { get; set; }
        public List<Section> Results { get; set; }
        public int RowCount => Results == null ? 0 : Results.Count();
    }
}