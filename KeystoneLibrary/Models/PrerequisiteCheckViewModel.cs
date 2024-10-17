using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.Profile;

namespace KeystoneLibrary.Models
{
    public class PrerequisiteCheckViewModel
    {
        public Criteria Criteria { get; set; }
        public List<PrerequisiteCheckDetailViewModel> Result { get; set; }
        public List<PrerequisiteCheckDetailViewModel> Prerequisites { get; set; }
        public List<RegistrationCourse> RegistrationCourses { get; set; }
        public List<CurriculumInformation> CurriculumInformations { get; set; }
    }
}