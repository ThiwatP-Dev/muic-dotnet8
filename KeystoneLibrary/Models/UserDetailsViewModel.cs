using KeystoneLibrary.Models.DataModels;

namespace KeystoneLibrary.Models
{
    public class UserDetailsViewModel
    {
        public UserDetailsViewModel() 
        {
            User = new ApplicationUser()
            {
                UserName = "N/A",
                Email = "N/A",
                PhoneNumber = "N/A"
            };
        }
        
        public ApplicationUser User { get; set; } 
        public List<IdentityRole> Roles { get; set; }
        public List<PermissionViewModel> Permissions { get; set; }
        public List<PermissionViewModel> UserPermissions { get; set; }
    }
}