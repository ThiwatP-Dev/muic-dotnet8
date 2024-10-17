using KeystoneLibrary.Data;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;
using KeystoneLibrary.Interfaces;
using Microsoft.EntityFrameworkCore;
using KeystoneLibrary.Models.Enums;
using KeystoneLibrary.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Keystone.Permission;

namespace Keystone.Controllers.MasterTables
{

    public class ChangeCurriculumPetitionManagementController : BaseController
    {
        protected readonly IPetitionProvider _petitionProvider;
        private readonly IAcademicProvider _academicProvider;

        public ChangeCurriculumPetitionManagementController(ApplicationDbContext db,
                                                            IFlashMessage flashMessage,
                                                            ISelectListProvider selectListProvider,
                                                            IPetitionProvider petitionProvider,
                                                            IAcademicProvider academicProvider) : base(db, flashMessage, selectListProvider)
        {
            _petitionProvider = petitionProvider;
            _academicProvider = academicProvider;
        }

        [PermissionAuthorize("ChangeCurriculumPetitionManagement", "")]
        public ActionResult Index(Criteria criteria, int page)
        {
            CreateSelectList(criteria.AcademicLevelId);
            if (criteria.AcademicLevelId == 0 || criteria.TermId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var petition = _db.ChangingCurriculumPetitions.AsNoTracking()
                                                          .Include(x => x.CurrentCurriculumVersion)
                                                          .Include(x => x.NewCurriculumVersion)
                                                          .Include(x => x.Student)
                                                              .ThenInclude(x => x.Title)
                                                          .Include(x => x.CurrentDepartment)
                                                          .Include(x => x.NewDepartment)
                                                          .Where(x => (criteria.TermId == 0
                                                                       || x.TermId == criteria.TermId)
                                                                       && (string.IsNullOrEmpty(criteria.Code)
                                                                           || x.Student.Code.StartsWith(criteria.Code))
                                                                       && (string.IsNullOrEmpty(criteria.CodeAndName)
                                                                           || x.Student.FirstNameEn.StartsWith(criteria.CodeAndName)
                                                                           || x.Student.FirstNameTh.StartsWith(criteria.CodeAndName)
                                                                           || x.Student.MidNameEn.StartsWith(criteria.CodeAndName)
                                                                           || x.Student.MidNameTh.StartsWith(criteria.CodeAndName)
                                                                           || x.Student.LastNameEn.StartsWith(criteria.CodeAndName)
                                                                           || x.Student.LastNameTh.StartsWith(criteria.CodeAndName))
                                                                       && (criteria.RequestedFrom == null
                                                                           || x.CreatedAt.Date >= criteria.RequestedFrom.Value.Date)
                                                                       && (criteria.RequestedTo == null
                                                                           || x.CreatedAt.Date <= criteria.RequestedTo.Value.Date)
                                                                       && (string.IsNullOrEmpty(criteria.Status)
                                                                           || x.Status.ToString() == criteria.Status))
                                                          .OrderBy(x => x.Student.Code)
                                                          .IgnoreQueryFilters()
                                                          .GetPaged(criteria, page, true);
            return View(petition);
        }

        [PermissionAuthorize("ChangeCurriculumPetitionManagement", "")]
        public ActionResult Edit(long id, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList();
            var petition = _db.ChangingCurriculumPetitions.Include(x => x.Student)
                                                              .ThenInclude(x => x.Title)
                                                          .SingleOrDefault(x => x.Id == id);
            if (petition == null)
                return Redirect(returnUrl);

            string status = string.Empty;
            switch (petition.Status)
            {
                case PetitionStatusEnum.PENDING:
                    status = "PENDING";
                    break;
                case PetitionStatusEnum.CANCEL:
                    status = "CANCEL";
                    break;
                case PetitionStatusEnum.APPROVED:
                    status = "APPROVED";
                    break;
                case PetitionStatusEnum.REJECTED:
                    status = "REJECTED";
                    break;
            }

            var model = new ChangeCurriculumPetitionViewModel
                        {
                            Id = id,
                            Code = petition.Student?.Code,
                            Name = petition.Student?.FullNameEn,
                            Status = status,
                            Remark = petition.Remark
                        };

            return View(model);
        }

        [PermissionAuthorize("ChangeCurriculumPetitionManagement", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(ChangeCurriculumPetitionViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var petition = _db.ChangingCurriculumPetitions.SingleOrDefault(x => x.Id == model.Id);
                    var status = PetitionStatusEnum.PENDING;
                    switch (model.Status)
                    {
                        case "PENDING":
                            status = PetitionStatusEnum.PENDING;
                            break;
                        case "CANCEL":
                            status = PetitionStatusEnum.CANCEL;
                            break;
                        case "APPROVED":
                            status = PetitionStatusEnum.APPROVED;
                            break;
                        case "REJECTED":
                            status = PetitionStatusEnum.REJECTED;
                            break;
                    }

                    if (petition != null)
                    {
                        petition.Status = status;
                        petition.Remark = model.Remark;
                    }

                    await _db.SaveChangesAsync();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return Redirect(returnUrl);
                }
                catch
                {
                    ViewBag.ReturnUrl = returnUrl;
                    _flashMessage.Danger(Message.UnableToEdit);
                    CreateSelectList();
                    return View(model);
                }
            }

            ViewBag.ReturnUrl = returnUrl;
            _flashMessage.Danger(Message.UnableToEdit);
            CreateSelectList();
            return View(model);
        }

        [PermissionAuthorize("ChangeCurriculumPetitionManagement", "")]
        public ActionResult Details(long id)
        {
            var petition = _petitionProvider.GetChangingCurriculumPetition(id);
            return PartialView("_Details", petition);
        }

        private void CreateSelectList(long academicLevelId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Petitions = _selectListProvider.GetPetitions();
            ViewBag.Statuses = _selectListProvider.GetChangeDepartmentPetitionStatuses();
            if (academicLevelId > 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            }
        }

        #region USpark

        [AllowAnonymous]
        [HttpPost("uspark/petition/changeMajor")]
        public IActionResult CreateChangeMajorPetition([FromBody] CreateUsparkChangingCurriculumPetitionViewModel request)
        {
            try
            {
                _petitionProvider.CreateChangingCurriculumPetition(request);
            }
            catch (ArgumentNullException ex)
            {
                return StatusCode(400, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(409, ex.Message);
            }

            return StatusCode(201);
        }

        [AllowAnonymous]
        [HttpGet("uspark/petition/changeMajor/{studentCode}")]
        public IActionResult GetLastChangeMajorPetition(string studentCode)
        {
            var criteria = new Criteria
            {
                StudentCode = studentCode
            };

            var petitions = _petitionProvider.SearchChangingCurriculumPetition(criteria, 1, 1000);

            if (!petitions.Items.Any())
            {
                return StatusCode(204);
            }

            var lastAvailablePetition = petitions.Items.FirstOrDefault();

            return StatusCode(200, lastAvailablePetition);
        }
        #endregion
    }
}