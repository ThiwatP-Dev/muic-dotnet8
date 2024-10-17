using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;
using AutoMapper;
using Newtonsoft.Json;
using KeystoneLibrary.Models.DataModels;
using Keystone.Permission;

namespace Keystone.Controllers
{
    [PermissionAuthorize("StudentIdCard", "")]
    public class StudentIdCardController : BaseController
    {
        protected readonly ICardProvider _cardProvider;
        protected readonly ICacheProvider _cacheProvider;
        protected readonly IAcademicProvider _academicProvider;
        protected readonly IStudentProvider _studentProvider;
        protected readonly IFeeProvider _feeProvider;
        
        public StudentIdCardController(ApplicationDbContext db, 
                                       IFlashMessage flashMessage, 
                                       IMapper mapper,
                                       ISelectListProvider selectListProvider,
                                       ICardProvider cardProvider,
                                       ICacheProvider cacheProvider,
                                       IAcademicProvider academicProvider,
                                       IFeeProvider feeProvider,
                                       IStudentProvider studentProvider) : base(db, flashMessage, mapper, selectListProvider)
        {
            _cardProvider = cardProvider;
            _cacheProvider = cacheProvider;
            _academicProvider = academicProvider;
            _feeProvider = feeProvider;
            _studentProvider = studentProvider;
        }

        public IActionResult Index(Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId);
            var model = new StudentIdCardViewModel
            {
                CardType = "substitudecard"
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Preview(StudentIdCardViewModel model)
        {
            string targetPdf = "";
            var report = new ReportViewModel();
            if (model.CardType == "idcard")
            {
                targetPdf = "Preview/IdCardPreview";
                report.Body = model.FormDetails;
            } 

            else if (model.CardType == "substitudecard")
            {
                targetPdf = "Preview/SubstituteCardPreview";
                var studentSubstitudedCard = await _cardProvider.GetStudentSubstitudedCard(model);
                model.StudentIdCardDetail = studentSubstitudedCard;
            
                report.Body = model.StudentIdCardDetail;
            }
            
            return View(targetPdf, report);
        }

        public async Task<ActionResult> Finish(string model)
        {
            var viewModel = JsonConvert.DeserializeObject<StudentIdCardDetail>(model);
            var currentTerm = _cacheProvider.GetCurrentTerm(viewModel.AcademicLevelId);
            var user = GetUser();

            try
            {
                var cardLog = new CardLog
                              {
                                  StudentId = viewModel.StudentId,
                                  TermId = currentTerm.Id,
                                  CardType = "substitudecard",
                                  Log = model,
                                  Year = currentTerm.AcademicYear,
                                  PrintedAt = DateTime.Now,
                                  RequestedBy = user == null ? "" : user.NormalizedUserName
                              };
                _db.CardLogs.Add(cardLog);
                await _db.SaveChangesAsync();

                _flashMessage.Confirmation(Message.SaveSucceed);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToCreate);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> IdCard ([FromBody] StudentIdCardViewModel model)
        {
            if (string.IsNullOrEmpty(model.FromCode) && string.IsNullOrEmpty(model.ToCode) && model.AcademicLevelId == null && model.AdmissionRoundId == null)
            {
               model.ErrorMessage  = Message.RequiredData;
                return PartialView("~/Views/StudentIdCard/_Form.cshtml", model);
            }

            CreateSelectList(model.AcademicLevelId ?? 0);
            var students = _studentProvider.GetStudentFromCodeRanges(model.FromCodeInt, model.ToCodeInt, model.AcademicLevelId ?? 0, model.AdmissionRoundId ?? 0).ToList();
            model = await _cardProvider.GetStudentIdCardForm(students);
            if (model.FormDetails == null || !model.FormDetails.Any())
            {
                model.ErrorMessage  = Message.StudentNotFound;
                return PartialView("~/Views/StudentIdCard/_Form.cshtml", model);
            }

            return PartialView("~/Views/StudentIdCard/_Form.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SubstituteIdCard ([FromBody] StudentIdCardViewModel model)
        {
            if (string.IsNullOrEmpty(model.Code))
            {
                return PartialView("~/Views/StudentIdCard/_SubstituteForm.cshtml", new StudentIdCardViewModel { ErrorMessage = Message.RequiredData });
            }

            if (!_studentProvider.IsExistStudent(model.Code))
            {
                return PartialView("~/Views/StudentIdCard/_SubstituteForm.cshtml", new StudentIdCardViewModel { ErrorMessage = Message.StudentNotFound });
            }

            CreateSelectList(model.AcademicLevelId ?? 0);
            model.StudentIdCardDetail = await _cardProvider.GetStudentSubstitudedCard(model);
            return PartialView("~/Views/StudentIdCard/_SubstituteForm.cshtml", model);
        }

        private void CreateSelectList(long academicLevelId)
        {
            ViewBag.Titles = _selectListProvider.GetTitlesEn();
            ViewBag.ExaminationTypes = _selectListProvider.GetMidtermAndFinal();
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            if (academicLevelId != 0)
            {
                ViewBag.AdmissionRounds = _selectListProvider.GetAdmissionRoundByAcademicLevelId(academicLevelId);
            }
        }

        private void CreateSelectListAddress()
        {
            ViewBag.Countries = _selectListProvider.GetCountries();
            ViewBag.Provinces = _selectListProvider.GetProvinces();
            ViewBag.Districts = _selectListProvider.GetDistricts();
            ViewBag.SubDistricts = _selectListProvider.GetSubdistricts();
            ViewBag.States = _selectListProvider.GetStates();
        }
    }
}