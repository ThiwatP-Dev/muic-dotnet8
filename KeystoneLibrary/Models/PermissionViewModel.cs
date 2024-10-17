using KeystoneLibrary.Models.DataModels;

namespace KeystoneLibrary.Models
{
    public class PermissionViewModel
    {
        public PermissionViewModel()
        {
            Menus = new List<MenuPermissionViewModel>();
            User = new ApplicationUser();
            Roles = new List<RoleViewModel>();
        }

        public string RoleId { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public List<RoleViewModel> Roles { get; set; }
        public List<MenuPermissionViewModel> Menus { get; set; }
    }

    public class RoleViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class MenuPermissionViewModel : IEquatable<MenuPermissionViewModel>
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string GroupName { get; set; }
        public string SubgroupName { get; set; }
        public bool IsWriteAble { get; set; }
        public bool IsReadAble { get; set; }
        public List<TabPermissionViewModel> Tabs { get; set; }

        public bool Equals(MenuPermissionViewModel other)
        {
            if (other is null)
                return false;

            return this.Id == other.Id;
        }

        public override bool Equals(object obj) => Equals(obj as MenuPermissionViewModel);
        public override int GetHashCode() => (Id).GetHashCode();
    }

    public class TabPermissionViewModel : IEquatable<TabPermissionViewModel>
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public bool IsWriteAble { get; set; }
        public bool IsReadAble { get; set; }

        public bool Equals(TabPermissionViewModel other)
        {
            if (other is null)
                return false;

            return this.Id == other.Id;
        }

        public override bool Equals(object obj) => Equals(obj as MenuPermissionViewModel);
        public override int GetHashCode() => (Id).GetHashCode();
    }
}