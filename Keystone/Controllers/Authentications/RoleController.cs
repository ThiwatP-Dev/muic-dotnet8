using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;

namespace Keystone.Controllers.Authentications
{
    public class RoleController : BaseController
    {
        private readonly IAuthenticationProvider _authenticationProvider;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleController(ApplicationDbContext db,
                              IFlashMessage flashMessage,
                              ISelectListProvider selectListProvider,
                              RoleManager<IdentityRole> roleManager,
                              IAuthenticationProvider authenticationProvider) : base(db, flashMessage, selectListProvider)
        {
            _roleManager = roleManager;
            _authenticationProvider = authenticationProvider;
        }

        [PermissionAuthorize("Role", "")]
        public IActionResult Index(int page)
        {
            var roles = _roleManager.Roles
                                    .OrderBy(x => x.Name)
                                    .GetPaged(page);
            return View(roles);
        }

        [PermissionAuthorize("Role", PolicyGenerator.Write)]
        public IActionResult Create()
        {
            return View(new IdentityRole());
        }

        [PermissionAuthorize("Role", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IdentityRole model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    model.NormalizedName = model.Name.ToUpper();
                    model.ConcurrencyStamp = Guid.NewGuid().ToString();
                    _db.Roles.Add(model);
                    _db.SaveChanges();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    _flashMessage.Danger(Message.UnableToCreate);
                    return View(model);
                }
            }

            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var role = await Find(id);
            if (role == null)
            {
                _flashMessage.Warning(Message.UnableToEdit);
                return RedirectToAction(nameof(Index));
            }

            return View(role);
        }

        [PermissionAuthorize("Role", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(IdentityRole model)
        {
            var role = await Find(model.Id);
            role.NormalizedName = model.Name.ToUpper();
            if (ModelState.IsValid && await TryUpdateModelAsync<IdentityRole>(role))
            {
                try
                {
                    await _db.SaveChangesAsync();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    _flashMessage.Danger(Message.UnableToEdit);
                    return View(role);
                }
            }

            _flashMessage.Danger(Message.UnableToEdit);
            return View(role);
        }

        [PermissionAuthorize("RoleManagePermission", "")]
        public ActionResult ManagePermission(string roleId)
        {
            ModelState.Clear();
            CreateSelectList();

            if (roleId == null)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }
            else
            {
                var model = _authenticationProvider.GetRolePermissionViewModel(roleId);
                return View(model);
            }
        }

        [PermissionAuthorize("RoleManagePermission", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestFormLimits(ValueCountLimit = Int32.MaxValue)]
        public ActionResult ManagePermission(PermissionViewModel model)
        {
            CreateSelectList();
            if (model.RoleId != null)
            {
                _authenticationProvider.AssignRolePermission(model);
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            return View(model);
        }

        [PermissionAuthorize("Role", PolicyGenerator.Write)]
        public async Task<ActionResult> Delete(string id)
        {
            IdentityRole model = await Find(id);
            try
            {
                _db.Roles.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<IdentityRole> Find(string id)
        {
            return await _roleManager.FindByIdAsync(id);
        }

        private void CreateSelectList()
        {
            ViewBag.Roles = _selectListProvider.GetRoles();
        }

        public async Task<IActionResult> GetUserRole(string userId, string roleId)
        {
            var role = await Find(roleId);
            if (role == null)
            {
                return StatusCode(500, "Role not found.");
            }
            else
            {
                var IsUserRoleExist = await _authenticationProvider.IsRoleExist(userId, role.Name);
                if (IsUserRoleExist)
                {
                    return StatusCode(500, "Role already exist.");
                }
                else
                {
                    return Ok(role);
                }
            }
        }

        public async Task<IActionResult> GetRole(string roleId)
        {
            var role = await Find(roleId);
            if (role == null)
            {
                return StatusCode(500, "Role not found.");
            }
            else
            {
                return Ok(role);
            }
        }
    }
}