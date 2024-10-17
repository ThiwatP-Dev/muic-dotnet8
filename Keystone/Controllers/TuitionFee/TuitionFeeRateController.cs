using KeystoneLibrary.Data;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Fee;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;
using KeystoneLibrary.Interfaces;
using Newtonsoft.Json;
using Keystone.Permission;

namespace Keystone.Controllers
{
    [PermissionAuthorize("TuitionFeeRate", "")]
    public class TuitionFeeRateController : BaseController
    {
        private readonly IFeeProvider _iFeeProvider;
        private readonly IAcademicProvider _academicProvider;

        public TuitionFeeRateController(ApplicationDbContext db,
                                        IFlashMessage flashMessage,
                                        ISelectListProvider selectListProvider,
                                        IFeeProvider iFeeProvider,
                                        IAcademicProvider academicProvider) : base(db, flashMessage, selectListProvider)
        {
            _iFeeProvider = iFeeProvider;
            _academicProvider = academicProvider;
        }

        public IActionResult Index(int page, Criteria criteria)
        {
            CreateSelectList();
            var models = _db.TuitionFeeRates.Include(x => x.TuitionFeeType)
                                            .Include(x => x.StudentFeeType)
                                            .Include(x => x.CustomCourseGroup)
                                            .Where(x => (string.IsNullOrEmpty(criteria.CodeAndName)
                                                         || x.Name.StartsWith(criteria.CodeAndName))
                                                         && (criteria.Batch == 0
                                                             || x.StartedBatch <= criteria.Batch
                                                             || x.EndedBatch >= criteria.Batch)
                                                         && (criteria.TuitionFeeTypeId == 0
                                                             || x.TuitionFeeTypeId == criteria.TuitionFeeTypeId)
                                                         && (criteria.StudentFeeTypeId == 0
                                                             || x.StudentFeeTypeId == criteria.StudentFeeTypeId)
                                                         && (criteria.CustomCourseGroupId == 0
                                                             || x.CustomCourseGroupId == criteria.CustomCourseGroupId))
                                            .IgnoreQueryFilters()
                                            .OrderBy(x => x.Name)
                                            .ToList();

            models = models.Select(x => {
                                            x.WhitelistMajors = string.IsNullOrEmpty(x.WhitelistMajorIds) 
                                                                ? new List<long>()
                                                                : JsonConvert.DeserializeObject<List<long>>(x.WhitelistMajorIds);
                                            x.WhitelistMajorsText = x.WhitelistMajors != null 
                                                                    ? string.Join(", ", _academicProvider.GetDepartmentNameByIds(x.WhitelistMajors)) : "";
                                            return x;
                                        })
                           .Where(x => (criteria.DepartmentId == 0
                                        || x.WhitelistMajors.Contains(criteria.DepartmentId)))
                           .ToList();

            var modelPagedResult = models.AsQueryable()
                                         .GetPaged(criteria, page, true);

            return View(modelPagedResult);
        }

        [PermissionAuthorize("TuitionFeeRate", PolicyGenerator.Write)]
        public ActionResult Create(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList();
            return View(new TuitionFeeRate());
        }

        [PermissionAuthorize("TuitionFeeRate", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(TuitionFeeRate model, string returnUrl)
        {
            CreateSelectList();
            try
            {
                model.WhitelistMajorIds = model.WhitelistMajors != null ? JsonConvert.SerializeObject(model.WhitelistMajors) : null;
                _db.TuitionFeeRates.Add(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                ViewBag.ReturnUrl = returnUrl;
                _flashMessage.Danger(Message.UnableToCreate);
                return View(model);
            }
        }

        public ActionResult Edit(long id, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList();
            var model = _iFeeProvider.GetTuitionFeeRate(id);
            model.WhitelistMajors = string.IsNullOrEmpty(model.WhitelistMajorIds) ? new List<long>()
                                                                                  : JsonConvert.DeserializeObject<List<long>>(model.WhitelistMajorIds);
            return View(model);
        }

        [PermissionAuthorize("TuitionFeeRate", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(TuitionFeeRate model, string returnUrl)
        {
            CreateSelectList();
            var feeRate = _iFeeProvider.GetTuitionFeeRate(model.Id);
            feeRate.WhitelistMajorIds = model.WhitelistMajors != null ? JsonConvert.SerializeObject(model.WhitelistMajors) : null;

            if (ModelState.IsValid && await TryUpdateModelAsync<TuitionFeeRate>(feeRate))
            {
                try
                {
                    await _db.SaveChangesAsync();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index));
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

        [PermissionAuthorize("TuitionFeeRate", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            var model = _iFeeProvider.GetTuitionFeeRate(id);
            try
            {
                _db.TuitionFeeRates.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }

            return RedirectToAction(nameof(Index));
        }
        
        private void CreateSelectList()
        {
            ViewBag.Departments = _selectListProvider.GetDepartments();
            ViewBag.Batches = _selectListProvider.GetBatches();
            ViewBag.TuitionFeeTypes = _selectListProvider.GetTuitionFeeTypes();
            ViewBag.StudentFeeTypes = _selectListProvider.GetStudentFeeTypes();
            ViewBag.CustomCourseGroups = _selectListProvider.GetCustomCourseGroups();
        }
    }
}