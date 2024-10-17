using KeystoneLibrary.Data;
using KeystoneLibrary.Models.DataModels.MasterTables;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;
using Microsoft.EntityFrameworkCore;
using KeystoneLibrary.Interfaces;
using Keystone.Permission;

namespace Keystone.Controllers.MasterTables
{
    [PermissionAuthorize("StudentFeeType", "")]
    public class StudentFeeTypeController : BaseController
    {
        protected readonly IMasterProvider _masterProvider;

        public StudentFeeTypeController(ApplicationDbContext db,
                                        IFlashMessage flashMessage,
                                        IMasterProvider masterProvider) : base(db, flashMessage)
        {
            _masterProvider = masterProvider;
        }

        public ActionResult Index(Criteria criteria, int page)
        {
            var types = _db.StudentFeeTypes.Where(x => string.IsNullOrEmpty(criteria.CodeAndName)
                                                      || x.NameEn.StartsWith(criteria.CodeAndName)
                                                      || x.NameTh.StartsWith(criteria.CodeAndName))
                                           .IgnoreQueryFilters()
                                           .GetPaged(criteria ,page, true);
            return View(types);
        }

        [PermissionAuthorize("StudentFeeType", PolicyGenerator.Write)]
        public ActionResult Create(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(new StudentFeeType());
        }

        [PermissionAuthorize("StudentFeeType", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(StudentFeeType model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _db.StudentFeeTypes.Add(model);
                    _db.SaveChanges();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index), new
                                                           {
                                                               CodeAndName = model.NameEn
                                                           });
                }
                catch
                {
                    ViewBag.ReturnUrl = returnUrl;
                    _flashMessage.Danger(Message.UnableToCreate);
                    return View(model);
                }
            }

            ViewBag.ReturnUrl = returnUrl;
            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public ActionResult Edit(long id, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var model = _masterProvider.GetStudentFeeType(id);
            return View(model);
        }

        [PermissionAuthorize("StudentFeeType", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id, string returnUrl)
        {
            var model = _masterProvider.GetStudentFeeType(id ?? 0);
            if (ModelState.IsValid && await TryUpdateModelAsync<StudentFeeType>(model))
            {
                try
                {
                    await _db.SaveChangesAsync();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index), new
                                                           {
                                                               CodeAndName = model.NameEn
                                                           });
                }
                catch
                {
                    ViewBag.ReturnUrl = returnUrl;
                    _flashMessage.Danger(Message.UnableToEdit);
                    return View(model);
                }
            }

            ViewBag.ReturnUrl = returnUrl;
            _flashMessage.Danger(Message.UnableToEdit);
            return View(model);
        }

        [PermissionAuthorize("StudentFeeType", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            var model = _masterProvider.GetStudentFeeType(id);
            try
            {
                _db.StudentFeeTypes.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}