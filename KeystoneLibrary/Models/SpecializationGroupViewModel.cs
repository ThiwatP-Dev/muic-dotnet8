using KeystoneLibrary.Models.DataModels.Curriculums;

namespace KeystoneLibrary.Models
{
    public class SpecializationGroupViewModel
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public string NameEn { get; set; }
        public string NameTh { get; set; }
        public string ShortNameEn { get; set; }
        public string ShortNameTh { get; set; }
        public bool IsActive { get; set; }
        public List<CourseGroup> CourseGroups { get; set; }
    }
}