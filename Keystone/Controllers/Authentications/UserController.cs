using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;

namespace Keystone.Controllers.Authentications
{
    public class UserController : BaseController
    {
        private readonly IAuthenticationProvider _authenticationProvider;
        private readonly RoleManager<IdentityRole> _roleManager;
        private UserManager<ApplicationUser> _userManager { get; }

        public UserController(ApplicationDbContext db,
                              IFlashMessage flashMessage,
                              ISelectListProvider selectListProvider,
                              RoleManager<IdentityRole> roleManager,
                              UserManager<ApplicationUser> user,
                              IAuthenticationProvider authenticationProvider) : base(db, flashMessage, selectListProvider)
        {
            _roleManager = roleManager;
            _authenticationProvider = authenticationProvider;
            _userManager = user;
        }

        [PermissionAuthorize("User", "")]
        public IActionResult Index(int page)
        {
            var users = _userManager.Users.OrderBy(x => x.UserName)
                                          .GetPaged(page);
            return View(users);
        }

        [PermissionAuthorize("User", PolicyGenerator.Write)]
        public IActionResult Create()
        {
            return View(new ApplicationUser());
        }

        [PermissionAuthorize("User", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ApplicationUser model)
        {
            model.Id = Guid.NewGuid().ToString();
            if (ModelState.IsValid)
            {
                try
                {
                    await _userManager.CreateAsync(model);
                    await _userManager.AddPasswordAsync(model, "muic#12345");
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

        [PermissionAuthorize("User", "")]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await Find(id);
            if (user == null)
            {
                _flashMessage.Warning(Message.UnableToEdit);
                return RedirectToAction(nameof(Index));
            }

            return View(user);
        }

        [PermissionAuthorize("User", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(ApplicationUser model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.FindByIdAsync(model.Id);
                    user.FirstnameTH = model.FirstnameTH;
                    user.LastnameTH = model.LastnameTH;
                    user.FirstnameEN = model.FirstnameEN;
                    user.LastnameEN = model.LastnameEN;
                    user.UserName = model.UserName;
                    var result = await _userManager.UpdateAsync(user);
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var resultPassword = await _userManager.ResetPasswordAsync(user, token, "muic#12345");

                    if (result.Succeeded)
                    {
                        _flashMessage.Confirmation(Message.SaveSucceed);
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch
                {
                    _flashMessage.Danger(Message.UnableToEdit);
                    return View(model);
                }
            }

            _flashMessage.Danger(Message.UnableToEdit);
            return View(model);
        }

        [PermissionAuthorize("UserManagePermission", "")]
        public async Task<ActionResult> ManagePermission(string userId)
        {
            CreateSelectList();
            if (userId == null)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }
            else
            {
                var model = await _authenticationProvider.GetUserPermissionViewModel(userId);
                model.UserId = userId;
                model.User = await Find(userId);
                return View(model);
            }
        }

        [PermissionAuthorize("UserManagePermission", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestFormLimits(ValueCountLimit = Int32.MaxValue)]
        public async Task<ActionResult> ManagePermission(PermissionViewModel model)
        {
            CreateSelectList();
            if (model.UserId != null)
            {
                await _authenticationProvider.AssignUserRole(model.Roles, model.UserId);
                _authenticationProvider.AssignUserPermission(model);
                model.User = await Find(model.UserId);
                _flashMessage.Confirmation(Message.SaveSucceed);
            }

            return View(model);
        }

        [PermissionAuthorize("UserManagePermission", "")]
        public async Task<ActionResult> PermissionSummary(string userId)
        {
            CreateSelectList();
            if (userId == null)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }
            else
            {
                var model = await _authenticationProvider.GetUserPermissionViewModelSummary(userId);
                model.UserId = userId;
                model.User = await Find(userId);
                return View(model);
            }
        }

        [PermissionAuthorize("User", PolicyGenerator.Write)]
        public async Task<ActionResult> Delete(string id)
        {
            ApplicationUser model = await Find(id);
            try
            {
                _db.Users.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<ApplicationUser> Find(string id)
        {
            return await _userManager.FindByIdAsync(id);
        }

        private void CreateSelectList()
        {
            ViewBag.Roles = _selectListProvider.GetRoles();
            ViewBag.Users = _selectListProvider.GetUsers();
        }
    }
}