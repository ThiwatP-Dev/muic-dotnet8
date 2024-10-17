using KeystoneLibrary.Models.DataModels.Curriculums;

namespace KeystoneLibrary.Models.DataModels.Admission
{
    public class AdmissionCurriculumViewModel
    {
        public long Id { get; set; }
        public long AdmissionRoundId { get; set; }
        public string Term { get; set; }
        public int Round { get; set; }
        public string AcademicLevel { get; set; }
        public long AcademicLevelId { get; set; }
        public string Faculty { get; set; }
        public long FacultyId { get; set; }
        public List<CurriculumVersion> CurriculumVersions { get; set; }
    }
}