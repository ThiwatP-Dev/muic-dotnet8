using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.Authentication;

namespace KeystoneLibrary.Providers
{
    public class AuthenticationProvider : IAuthenticationProvider
    {
        protected readonly ApplicationDbContext _db;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthenticationProvider(ApplicationDbContext db,
                                      RoleManager<IdentityRole> roleManager,
                                      UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public PermissionViewModel GetRolePermissionViewModel(string roleId)
        {
            var menus = (from menu in _db.Menus
                         join menuGroup in _db.MenuGroups on menu.MenuGroupId equals menuGroup.Id
                         join menuSubgroup in _db.MenuSubgroups on menu.MenuSubgroupId equals menuSubgroup.Id
                         join menuPermission in _db.MenuPermissions on new { RoleId = roleId, MenuId = menu.Id }
                                                                    equals new { menuPermission.RoleId, menuPermission.MenuId } into joinGroup
                         from menuPermission in joinGroup.DefaultIfEmpty()
                         where (menuPermission == null || menuPermission.RoleId == roleId)
                         select new MenuPermissionViewModel
                         {
                             Id = menu.Id,
                             Name = menu.TitleEn,
                             GroupName = menuGroup.Name,
                             SubgroupName = menuSubgroup.Name,
                             IsReadAble = menuPermission == null ? false : menuPermission.IsReadable,
                             IsWriteAble = menuPermission == null ? false : menuPermission.IsWritable,
                            //  Tabs = (from tab in _db.Tabs
                            //          join tabPermission in _db.TabPermissions on new { RoleId = roleId, TabId = tab.Id }
                            //                                                   equals new { tabPermission.RoleId, tabPermission.TabId } into joinGroup
                            //          from tabPermission in joinGroup.DefaultIfEmpty()
                            //          where tab.MenuId == menu.Id
                            //                && (tabPermission == null || tabPermission.RoleId == roleId)
                            //          select new TabPermissionViewModel
                            //          {
                            //              Id = tab.Id,
                            //              Name = tab.TitleEn,
                            //              IsReadAble = tabPermission == null ? false : tabPermission.IsReadable,
                            //              IsWriteAble = tabPermission == null ? false : tabPermission.IsWritable,
                            //          }).OrderBy(x => x.Name)
                            //            .ToList(),
                         }).OrderBy(x => x.GroupName)
                            .ThenBy(x => x.SubgroupName)
                                .ThenBy(x => x.Name)
                           .ToList();

            var model = new PermissionViewModel()
            {
                RoleId = roleId,
                Menus = menus
            };
            return model;
        }

        public async Task<PermissionViewModel> GetUserPermissionViewModel(string userId)
        {
            var user = _db.Users.Find(userId);
            var userRoles = await _userManager.GetRolesAsync(user);
            List<RoleViewModel> roleModels = new List<RoleViewModel>();
            foreach (var userRole in userRoles)
            {
                var role = await _roleManager.FindByNameAsync(userRole);
                roleModels.Add(new RoleViewModel()
                {
                    Id = role.Id,
                    Name = role.Name
                });
            }

            var menus = (from menu in _db.Menus
                         join menuGroup in _db.MenuGroups on menu.MenuGroupId equals menuGroup.Id
                         join menuSubgroup in _db.MenuSubgroups on menu.MenuSubgroupId equals menuSubgroup.Id
                         join menuPermission in _db.MenuPermissions on new { UserId = userId, MenuId = menu.Id }
                                                                    equals new { menuPermission.UserId, menuPermission.MenuId } into joinGroup
                         from menuPermission in joinGroup.DefaultIfEmpty()
                         where (menuPermission == null || menuPermission.UserId == userId)
                         select new MenuPermissionViewModel
                         {
                             Id = menu.Id,
                             Name = menu.TitleEn,
                             GroupName = menuGroup.Name,
                             SubgroupName = menuSubgroup.Name,
                             IsReadAble = menuPermission == null ? false : menuPermission.IsReadable,
                             IsWriteAble = menuPermission == null ? false : menuPermission.IsWritable,
                             Tabs = (from tab in _db.Tabs
                                     join tabPermission in _db.TabPermissions on new { UserId = userId, TabId = tab.Id }
                                                                              equals new { tabPermission.UserId, tabPermission.TabId } into joinGroup
                                     from tabPermission in joinGroup.DefaultIfEmpty()
                                     where tab.MenuId == menu.Id
                                           && (tabPermission == null || tabPermission.UserId == userId)
                                     select new TabPermissionViewModel
                                     {
                                         Id = tab.Id,
                                         Name = tab.TitleEn,
                                         IsReadAble = tabPermission == null ? false : tabPermission.IsReadable,
                                         IsWriteAble = tabPermission == null ? false : tabPermission.IsWritable,
                                     }).OrderBy(x => x.Name)
                                       .ToList(),
                         }).OrderBy(x => x.GroupName)
                           .ThenBy(x => x.Name)
                           .ToList();

            var model = new PermissionViewModel()
            {
                UserId = userId,
                Menus = menus,
                Roles = roleModels
            };
            return model;
        }

        public async Task<PermissionViewModel> GetUserPermissionViewModelSummary(string userId)
        {
            List<MenuPermissionViewModel> menuPermissionViewModels = new List<MenuPermissionViewModel>();
            var user = _db.Users.Find(userId);
            var userRoles = await _userManager.GetRolesAsync(user);
            List<string> roleIds = new List<string>();

            foreach (var userRole in userRoles)
            {
                var role = await _roleManager.FindByNameAsync(userRole);
                roleIds.Add(role.Id);
            }
            menuPermissionViewModels = GetReadableMenuPermission(userId, roleIds);

            var model = new PermissionViewModel()
            {
                UserId = userId,
                Menus = menuPermissionViewModels
            };
            return model;
        }

        public async Task<bool> IsRoleExist(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return await _userManager.IsInRoleAsync(user, role);
        }

        public async Task AssignUserRole(List<RoleViewModel> models, string userId)
        {
            models = models.Where(x => x.Id != null).ToList();
            if (models != null && models.Any())
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (_db.UserRoles.Any(x => x.UserId == userId))
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    var result = await _userManager.RemoveFromRolesAsync(user, roles.ToArray());
                }

                if (models.Any(x => x.Name != ""))
                {
                    await _userManager.AddToRolesAsync(user, models.Where(x => x.Name != "").Select(x => x.Name));
                }
            }
        }

        public void AssignRolePermission(PermissionViewModel model)
        {
             // Delete
            var existingMenus = _db.MenuPermissions.Where(x => x.RoleId == model.RoleId);
            _db.MenuPermissions.RemoveRange(existingMenus);

            var existingTabs = _db.TabPermissions.Where(x => x.RoleId == model.RoleId);
            _db.TabPermissions.RemoveRange(existingTabs);

            // Add
            if (model.Menus != null && model.Menus.Any(x => x.IsReadAble))
            {
                foreach (var menu in model.Menus.Where(x => x.IsReadAble))
                {
                    _db.MenuPermissions.Add(new MenuPermission
                                            {
                                                RoleId = model.RoleId,
                                                MenuId = menu.Id,
                                                IsReadable = menu.IsReadAble,
                                                IsWritable = menu.IsWriteAble,
                                                IsActive = true
                                            });

                    if (menu.Tabs != null && menu.Tabs.Any(x => x.IsReadAble))
                    {
                        foreach (var tab in menu.Tabs.Where(x => x.IsReadAble))
                        {
                            _db.TabPermissions.Add(new TabPermission
                                                   {
                                                        RoleId = model.RoleId,
                                                        TabId = tab.Id,
                                                        IsWritable = tab.IsWriteAble,
                                                        IsReadable = tab.IsReadAble,
                                                        IsActive = true
                                                   });
                        }
                    }
                }
            }
            _db.SaveChanges();
        }
        public void AssignUserPermission(PermissionViewModel model)
        {
            // Delete
            var existingMenus = _db.MenuPermissions.Where(x => x.UserId == model.UserId);
            _db.MenuPermissions.RemoveRange(existingMenus);

            var existingTabs = _db.TabPermissions.Where(x => x.UserId == model.UserId);
            _db.TabPermissions.RemoveRange(existingTabs);

            // Add
            if (model.Menus != null && model.Menus.Any(x => x.IsReadAble))
            {
                foreach (var menu in model.Menus.Where(x => x.IsReadAble))
                {
                    _db.MenuPermissions.Add(new MenuPermission
                                            {
                                                UserId = model.UserId,
                                                MenuId = menu.Id,
                                                IsReadable = menu.IsReadAble,
                                                IsWritable = menu.IsWriteAble,
                                                IsActive = true
                                            });

                    if (menu.Tabs != null && menu.Tabs.Any(x => x.IsReadAble))
                    {
                        foreach (var tab in menu.Tabs.Where(x => x.IsReadAble))
                        {
                            _db.TabPermissions.Add(new TabPermission
                                                   {
                                                        UserId = model.UserId,
                                                        TabId = tab.Id,
                                                        IsWritable = tab.IsWriteAble,
                                                        IsReadable = tab.IsReadAble,
                                                        IsActive = true
                                                   });
                        }
                    }
                }
            }
            _db.SaveChanges();
        }
        private List<MenuPermissionViewModel> GetReadableMenuPermission(string userId, List<string> roleIds)
        {
            var menus = (from menu in _db.Menus
                         join menuGroup in _db.MenuGroups on menu.MenuGroupId equals menuGroup.Id
                         join menuSubgroup in _db.MenuSubgroups on menu.MenuSubgroupId equals menuSubgroup.Id
                         join menuPermission in _db.MenuPermissions on menu.Id equals menuPermission.MenuId
                         where (menuPermission.UserId == userId || roleIds.Contains(menuPermission.RoleId))
                               && menuPermission.IsReadable
                         group menuPermission by new
                         {
                             menu.Id,
                             Name = menu.TitleEn,
                             GroupName = menuGroup.Name,
                             SubgroupName = menuSubgroup.Name
                         } into joinGroup
                         select new MenuPermissionViewModel
                         {
                             Id = joinGroup.Key.Id,
                             Name = joinGroup.Key.Name,
                             GroupName = joinGroup.Key.GroupName,
                             SubgroupName = joinGroup.Key.SubgroupName,
                             IsReadAble = true,
                             IsWriteAble = joinGroup.Any(x => x.IsWritable),
                             Tabs = (from tab in _db.Tabs
                                     join tabPermission in _db.TabPermissions on tab.Id equals tabPermission.TabId
                                     where tab.MenuId == joinGroup.Key.Id
                                           && (tabPermission.UserId == userId || roleIds.Contains(tabPermission.RoleId))
                                           && tabPermission.IsReadable
                                     group tabPermission by new
                                     {
                                         tab.Id,
                                         Name = tab.TitleEn
                                     } into g2
                                     select new TabPermissionViewModel
                                     {
                                         Id = g2.Key.Id,
                                         Name = g2.Key.Name,
                                         IsReadAble = true,
                                         IsWriteAble = g2.Any(x => x.IsWritable)
                                     }).OrderBy(x => x.Name)
                                       .ToList(),
                         }).OrderBy(x => x.GroupName)
                           .ThenBy(x => x.SubgroupName)
                           .ThenBy(x => x.Name)
                           .ToList();
            return menus == null ? new List<MenuPermissionViewModel>() : menus;
        }
    }
}