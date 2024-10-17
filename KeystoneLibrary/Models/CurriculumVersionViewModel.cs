using KeystoneLibrary.Models.DataModels.Curriculums;

namespace KeystoneLibrary.Models
{
    public class CurriculumVersionViewModel
    {
        public CurriculumVersion CurriculumVersion { get; set; }
        public long MinorId { get; set; }
        public long ConcentrationId { get; set; }
        public long CurriculumVersionId { get; set; }
        public List<CorequisiteDetail> Corequisites { get; set; }
        public List<CourseEquivalentDetail> CourseEquivalents { get; set; }
    }

    public class CorequisiteDetail
    {
        public long CorequisiteId { get; set; }
        public long CurriculumVersionId { get; set; }
        public string FirstCourse { get; set; }
        public string SecondCourse { get; set; }
        public string ExpiredDate { get; set; }
        public string Description { get; set; }
    }

    public class CourseEquivalentDetail
    {
        public long CourseId { get; set; }
        public long CourseEquivalentId { get; set; }
        public long CurriculumVersionId { get; set; }
        public string Course { get; set; }
        public string EquivalentCourse { get; set; }
        public string EffectiveDate { get; set; }
        public string EndDate { get; set; }
        public string Remark { get; set; }
    }
}