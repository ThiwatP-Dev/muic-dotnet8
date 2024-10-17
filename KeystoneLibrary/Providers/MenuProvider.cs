using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace KeystoneLibrary.Providers
{
    public class MenuProvider : BaseProvider, IMenuProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public MenuProvider(ApplicationDbContext db, IHttpContextAccessor httpContextAccessor) : base(db) 
        {
            _httpContextAccessor = httpContextAccessor;
         }

        public List<MenuViewModel> GetAllMenu()
        {
            List<MenuViewModel> menuViewModels = new List<MenuViewModel>();
            var menuGroups = _db.MenuGroups.AsNoTracking()
                                           .OrderBy(x => x.SequenceNo)
                                                .ThenBy(x => x.Name)
                                           .ToList();

            foreach (var menuGroup in menuGroups.Where(x => x.Name != "Setting"))
            {
                MenuViewModel menuViewModel = new MenuViewModel();
                menuViewModel.header = menuGroup.Name;
                menuViewModel.icon = menuGroup.Icon;

                menuViewModel.submenus = new List<SubmenuViewModel>();
                var menuSubgroups = _db.MenuSubgroups.AsNoTracking()
                                                     .Where(x => x.MenuGroupId == menuGroup.Id)
                                                     .OrderBy(x => x.SequenceNo)
                                                        .ThenBy(x => x.Name)
                                                     .ToList();

                foreach (var menuSubgroup in menuSubgroups)
                {
                    SubmenuViewModel submenuViewModel = new SubmenuViewModel();
                    submenuViewModel.submenus_header = menuSubgroup.Name;
                    submenuViewModel.submenus_items = new List<MenuItemViewModel>();

                    var menus = _db.Menus.AsNoTracking()
                                         .Where(x => !x.IsDeleted 
                                                     && x.MenuGroupId == menuGroup.Id
                                                     && x.MenuSubgroupId == menuSubgroup.Id)
                                         .OrderBy(x => x.SequenceNo)
                                            .ThenBy(x => x.TitleEn)
                                         .ToList();
                                         
                    foreach (var menu in menus)
                    {
                        MenuItemViewModel menuItemViewModel = new MenuItemViewModel();
                        menuItemViewModel.title = menu.TitleEn;
                        menuItemViewModel.url = menu.Url;
                        submenuViewModel.submenus_items.Add(menuItemViewModel);
                    }
                    menuViewModel.submenus.Add(submenuViewModel);
                }
                menuViewModels.Add(menuViewModel);
            }

            // Setting
            var settingMenuGroup = menuGroups.SingleOrDefault(x => x.Name == "Setting");
            MenuViewModel settingMenuViewModel = new MenuViewModel();
            settingMenuViewModel.header = settingMenuGroup.Name;
            settingMenuViewModel.icon = settingMenuGroup.Icon;

            settingMenuViewModel.submenus = new List<SubmenuViewModel>();
            var settingSubgroups = _db.MenuSubgroups.Where(x => x.MenuGroupId == settingMenuGroup.Id).OrderBy(x => x.SequenceNo).ThenBy(x => x.Name).ToList();
            foreach (var settingSubgroup in settingSubgroups)
            {
                SubmenuViewModel submenuViewModel = new SubmenuViewModel();
                submenuViewModel.submenus_header = settingSubgroup.Name;
                submenuViewModel.submenus_items = new List<MenuItemViewModel>();

                var menus = _db.Menus.Where(x => x.MenuGroupId == settingMenuGroup.Id
                                                 && x.MenuSubgroupId == settingSubgroup.Id).OrderBy(x => x.SequenceNo).ThenBy(x => x.TitleEn).ToList();
                foreach (var menu in menus)
                {
                    MenuItemViewModel menuItemViewModel = new MenuItemViewModel();
                    menuItemViewModel.title = menu.TitleEn;
                    menuItemViewModel.url = menu.Url;
                    
                    submenuViewModel.submenus_items.Add(menuItemViewModel);
                }
                settingMenuViewModel.submenus.Add(submenuViewModel);
            }

            menuViewModels.Add(settingMenuViewModel);

            return menuViewModels;
        }

        public List<MenuViewModel> GetAllMenu(string userId, List<string> roleIds)
        {
            var menuPermissions = _db.MenuPermissions.AsNoTracking()
                                                     .Include(x => x.Menu)
                                                         .ThenInclude(x => x.MenuGroup)
                                                     .Include(x => x.Menu)
                                                         .ThenInclude(x => x.MenuSubgroup)
                                                     .Where(x => !x.Menu.IsDeleted
                                                                 && x.IsReadable
                                                                 && (x.UserId == userId
                                                                     || roleIds.Any(y => y == x.RoleId)))
                                                     .Select(x => new 
                                                                  {
                                                                      x.Menu.MenuGroupId,
                                                                      MenuGroupName = x.Menu.MenuGroup.Name,
                                                                      MenuGruopSequence = x.Menu.MenuGroup.SequenceNo,
                                                                      MenuGroupIcon = x.Menu.MenuGroup.Icon,
                                                                      x.Menu.MenuSubgroupId,
                                                                      MenuSubGroupName = x.Menu.MenuSubgroup.Name,
                                                                      MenuSubGroupSequence = x.Menu.MenuSubgroup.SequenceNo,
                                                                      x.MenuId,
                                                                      MenuSequence = x.Menu.SequenceNo,
                                                                      MenuTitle = x.Menu.TitleEn,
                                                                      MenuUrl = x.Menu.Url
                                                                  })
                                                     .ToList();

            List<MenuViewModel> menuViewModels = new List<MenuViewModel>();
            if (menuPermissions != null && menuPermissions.Any())
            {
                var menus = menuPermissions.Where(x => x.MenuGroupName != "Setting")
                                           .OrderBy(x => x.MenuGruopSequence)
                                           .ThenBy(x => x.MenuGroupName)
                                           .GroupBy(x => new { x.MenuGroupId, x.MenuGroupName, x.MenuGroupIcon })
                                           .ToList();

                foreach (var group in menus)
                {
                    MenuViewModel menuViewModel = new MenuViewModel
                                                {
                                                    header = group.Key.MenuGroupName,
                                                    icon = group.Key.MenuGroupIcon,
                                                    submenus = new List<SubmenuViewModel>()
                                                };
                    foreach (var subGroup in group.OrderBy(x => x.MenuSubGroupSequence)
                                                  .ThenBy(x => x.MenuSubGroupName)
                                                  .GroupBy(x => new { x.MenuSubgroupId, x.MenuSubGroupName }))
                    {
                        SubmenuViewModel submenuViewModel = new SubmenuViewModel
                                                            {
                                                                submenus_header = subGroup.Key.MenuSubGroupName,
                                                                submenus_items = new List<MenuItemViewModel>()
                                                            };
                        foreach (var menu in subGroup.OrderBy(x => x.MenuSequence)
                                                     .ThenBy(x => x.MenuTitle)
                                                     .GroupBy(x => new { x.MenuId, x.MenuTitle, x.MenuUrl }))
                        {
                            MenuItemViewModel menuItemViewModel = new MenuItemViewModel
                                                                  {
                                                                      title = menu.Key.MenuTitle,
                                                                      url = menu.Key.MenuUrl
                                                                  };
                            
                            submenuViewModel.submenus_items.Add(menuItemViewModel);
                        }

                        menuViewModel.submenus.Add(submenuViewModel);
                    }

                    menuViewModels.Add(menuViewModel);
                }

                var setting = menuPermissions.Where(x => x.MenuGroupName == "Setting")
                                             .OrderBy(x => x.MenuGruopSequence)
                                             .ThenBy(x => x.MenuGroupName)
                                             .GroupBy(x => new { x.MenuGroupId, x.MenuGroupName, x.MenuGroupIcon })
                                             .SingleOrDefault();
                if (setting != null)
                {
                    MenuViewModel settingMenuViewModel = new MenuViewModel
                                                         {
                                                             header = setting.Key.MenuGroupName,
                                                             icon = setting.Key.MenuGroupIcon,
                                                             submenus = new List<SubmenuViewModel>()
                                                         };
                    foreach (var subGroup in setting.OrderBy(x => x.MenuSubGroupSequence)
                                                    .ThenBy(x => x.MenuSubGroupName)
                                                    .GroupBy(x => new { x.MenuSubgroupId, x.MenuSubGroupName }))
                    {
                        SubmenuViewModel submenuViewModel = new SubmenuViewModel
                                                            {
                                                                submenus_header = subGroup.Key.MenuSubGroupName,
                                                                submenus_items = new List<MenuItemViewModel>()
                                                            };

                        foreach (var menu in subGroup.OrderBy(x => x.MenuSequence)
                                                     .ThenBy(x => x.MenuTitle)
                                                     .GroupBy(x => new { x.MenuId, x.MenuTitle, x.MenuUrl }))
                        {
                            MenuItemViewModel menuItemViewModel = new MenuItemViewModel
                                                                  {
                                                                      title = menu.Key.MenuTitle,
                                                                      url = menu.Key.MenuUrl
                                                                  };
                            
                            submenuViewModel.submenus_items.Add(menuItemViewModel);
                        }

                        settingMenuViewModel.submenus.Add(submenuViewModel);
                    }

                    menuViewModels.Add(settingMenuViewModel);
                }
            }

            return menuViewModels;
        }

        private Uri GetRootUrl()
        {
            string scheme = _httpContextAccessor.HttpContext.Request.Scheme;
            string host = _httpContextAccessor.HttpContext.Request.Host.ToUriComponent();

            if (Uri.TryCreate($"{ scheme }://{ host }", UriKind.Absolute, out var url))
            {
                return url;
            }
            else
            {
                throw new InvalidOperationException("Root url wrong format");
            }
        }
    }
}