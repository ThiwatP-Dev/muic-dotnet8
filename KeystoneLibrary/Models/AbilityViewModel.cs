using KeystoneLibrary.Models.DataModels.Curriculums;
using KeystoneLibrary.Models.DataModels.MasterTables;

namespace KeystoneLibrary.Models
{
    public class AbilityViewModel
    {
        public string Code { get; set; }
        public string NameEn { get; set; }
        public string NameTh { get; set; }
        public bool IsForceTrack { get; set; }
        public List<CurriculumCourse> CurriculumCourses { get; set; }
        public List<SpecializationGroupBlackList> SpecializationGroupBlackLists { get; set; }
        public AbilityViewModel()
        {
            CurriculumCourses = new List<CurriculumCourse>();
            SpecializationGroupBlackLists = new List<SpecializationGroupBlackList>();
        }
    }

    public class AbilityCourseViewModel
    {
        public long Id { get; set; }
        public long CurriculumCourseId { get; set; }
        public long CourseId { get; set; }
        public int Sequence { get; set; }
        public bool IsMustTake { get; set; }
    }

    public class AbilityBlacklistDepartmentViewModel
    {
        public long Id { get; set; }
        public long SpecializationGroupBlackListId { get; set; }
        public long DepartmentId { get; set; }
    }
}