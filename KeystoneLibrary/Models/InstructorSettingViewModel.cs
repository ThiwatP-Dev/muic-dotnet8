namespace KeystoneLibrary.Models
{
    public class InstructorSettingViewModel
    {
        public List<RoleViewModel> MajorRoles { get; set; }
        public List<RoleViewModel> UniversityRoles { get; set; }
        
        public InstructorSettingViewModel()
        {
            MajorRoles = new List<RoleViewModel>();
            UniversityRoles = new List<RoleViewModel>();
        }
    }
}