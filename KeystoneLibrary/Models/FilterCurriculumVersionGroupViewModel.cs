namespace KeystoneLibrary.Models
{
    public class FilterCurriculumVersionGroupViewModel
    {
        public Criteria Criteria { get; set; }
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int CurriculumVersionCount { get; set; }
        public List<FilterCurriculumVersionGroupDetailViewModel> CurriculumVersions { get; set; } = new List<FilterCurriculumVersionGroupDetailViewModel>();
    }

    public class FilterCurriculumVersionGroupDetailViewModel
    {
        public string IsChecked { get; set; }
        public long FilterCurriculumVersionGroupDetailId { get; set; }
        public long FilterCurriculumVersionGroupId { get; set; }
        public long CurriculumVersionId { get; set; }
        public string Code { get; set; }
        public string NameEn { get; set; }
        public string Faculty { get; set; }
        public string Department { get; set; }
        public string Curriculum { get; set; }
        public string CodeAndName => $"{ Code } - { NameEn }";
    }
}