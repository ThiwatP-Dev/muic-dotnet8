using KeystoneLibrary.Models.DataModels.Curriculums;

namespace KeystoneLibrary.Models
{
    public class CurriculumViewModel
    {
        public Curriculum Curriculum { get; set; }
        public CurriculumVersion Version { get; set; }
        public List<long> InstructorIds { get; set; }
    }

    public class CopyCurriculumViewModel
    {
        public string ReturnUrl { get; set; }
        public long AcademicLevelId { get; set; }
        public long MasterCurriculumId { get; set; }
        public long CurriculumId { get; set; }
        public long CurriculumVersionId { get; set; }
        public Curriculum Curriculum { get; set; }
        public CurriculumVersion Version { get; set; }
        public List<CourseGroup> CourseGroup { get; set; }
        public string modelJson { get; set; }
        public bool IsCopyPrerequisite { get; set; }
        public bool IsCopySpecializeGroup { get; set; }
        public bool IsCopyBlacklistCourses { get; set; }
        public bool IsCopyCoRequisiteAndCourseEquivalent { get; set; }
    }

    public class CurriculumInformationViewModel
    {
        public string CurriculumVersion { get; set; }
        public string Minor { get; set; }
        public string Concentration { get; set; }
    }
}