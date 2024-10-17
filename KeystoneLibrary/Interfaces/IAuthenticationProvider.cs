using KeystoneLibrary.Models;

namespace KeystoneLibrary.Interfaces
{
    public interface IAuthenticationProvider
    {
        PermissionViewModel GetRolePermissionViewModel(string roleId);
        Task<PermissionViewModel> GetUserPermissionViewModel(string userId);
        Task<PermissionViewModel> GetUserPermissionViewModelSummary(string userId);
        Task<bool> IsRoleExist(string userId, string role);
        Task AssignUserRole(List<RoleViewModel> models, string userId);
        void AssignRolePermission(PermissionViewModel model);
        void AssignUserPermission(PermissionViewModel model);
    }
}