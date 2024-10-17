using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Keystone.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class MenuController : ControllerBase
    {
        private IMenuProvider _menuProvider;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        
        public MenuController(IMenuProvider menuProvider,
                              UserManager<ApplicationUser> userManager,
                              RoleManager<IdentityRole> roleManager)
        {
            _menuProvider = menuProvider;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet("GetAllMenu")]
        public List<MenuViewModel> GetAllMenu()
        {
            return _menuProvider.GetAllMenu();
        }

        [HttpGet("GetMenu")]
        public async Task<List<MenuViewModel>> GetMenu()
        {
            var user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user);
            var roleIds = new List<string>();
            foreach (var userRole in roles)
            {
                var role = await _roleManager.FindByNameAsync(userRole);
                roleIds.Add(role.Id);
            }

            if (roles.Contains("Admin"))
            {
                return _menuProvider.GetAllMenu();
            }
            else 
            {
                return _menuProvider.GetAllMenu(user.Id, roleIds);
            }
        }
    }
}