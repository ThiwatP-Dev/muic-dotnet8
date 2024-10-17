using AutoMapper;
using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.Profile;
using KeystoneLibrary.Models.DataModels.Scholarship;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    [PermissionAuthorize("ScholarshipProfile", "")]
    public class ScholarshipProfileController : BaseController
    {
        protected readonly IExceptionManager _exceptionManager;
        protected readonly ICacheProvider _cacheProvider;
        protected readonly IStudentProvider _studentProvider;
        protected readonly IScholarshipProvider _scholarshipProvider;
        protected readonly IVoucherProvider _voucherProvider;

        public ScholarshipProfileController(ApplicationDbContext db, 
                                            IFlashMessage flashMessage,
                                            IMapper mapper,
                                            ICacheProvider cacheProvider,
                                            IStudentProvider studentProvider,
                                            IScholarshipProvider scholarshipProvider,
                                            IExceptionManager exceptionManager,
                                            IVoucherProvider voucherProvider,
                                            ISelectListProvider selectListProvider) : base(db, flashMessage, mapper, selectListProvider)
        {
            _cacheProvider = cacheProvider;
            _exceptionManager = exceptionManager;
            _studentProvider = studentProvider;
            _scholarshipProvider = scholarshipProvider;
            _voucherProvider = voucherProvider;
        }

        public ActionResult Index(string keyword, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (string.IsNullOrEmpty(keyword))
            {
                _flashMessage.Warning(Message.RequiredData);
                return View(new ScholarshipProfileViewModel());
            }
            else if (!_studentProvider.IsExistAllStudent(keyword))
            {
                _flashMessage.Danger(Message.StudentNotFound);
                return View(new ScholarshipProfileViewModel()
                            {
                                StudentCode = keyword
                            });
            }

            var model = _db.Students.Include(x => x.AcademicInformation)
                                        .ThenInclude(x => x.AcademicProgram)
                                    .Include(x => x.AcademicInformation)
                                        .ThenInclude(x => x.Faculty)
                                    .Include(x => x.AcademicInformation)
                                        .ThenInclude(x => x.Department)
                                    .Include(x => x.AcademicInformation)
                                        .ThenInclude(x => x.CurriculumVersion)
                                        .ThenInclude(x => x.Curriculum)
                                    .Include(x => x.GraduationInformations)
                                    .Include(x => x.MaintenanceStatuses)
                                    .Include(x => x.RegistrationCourses)
                                    .Include(x => x.ScholarshipStudents)
                                        .ThenInclude(x => x.Scholarship)
                                    .Include(x => x.ScholarshipStudents)
                                        .ThenInclude(x => x.EffectivedTerm)
                                    .Include(x => x.ScholarshipStudents)
                                        .ThenInclude(x => x.ExpiredTerm)
                                    .Include(x => x.ScholarshipStudents)
                                        .ThenInclude(x => x.FinancialTransactions)
                                    .IgnoreQueryFilters()
                                    .SingleOrDefault(x => x.Code == keyword);
            
            if (model == null)
            {
                return View(new ScholarshipProfileViewModel());
            }

            var term = _cacheProvider.GetCurrentTerm(model.AcademicInformation.AcademicLevelId);
            var viewModel = MappingViewModel(model, term);
            CreateSelectList(model.AcademicInformation.AcademicLevelId);
            ViewBag.AcademicLevelId = model.AcademicInformation.AcademicLevelId;
            return View(viewModel);
        }

        [PermissionAuthorize("ScholarshipProfile", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddScholarshipStudent(ScholarshipStudent model)
        {
            if (model.StudentId == Guid.Empty)
            {
                _flashMessage.Danger(Message.StudentNotFound);
                return RedirectToAction(nameof(Index));
            }

            var student = _db.Students.SingleOrDefault(x => x.Id == model.StudentId);
            model.IsActive = model.Active;
            if (ModelState.IsValid)
            {
                if (_scholarshipProvider.IsScholarshipLimitExceeded(model))
                {
                    _flashMessage.Danger(Message.ScholarshipLimitExceeded);
                }
                else if (_scholarshipProvider.IsAnyScholarshipActive(model) && model.IsActive)
                {
                    _flashMessage.Danger(Message.ScholarshipActive);
                }
                else
                {
                    try
                    {
                        _db.ScholarshipStudents.Add(model);
                        _db.SaveChanges();
                        _flashMessage.Confirmation(Message.SaveSucceed);
                    }
                    catch
                    {
                        _flashMessage.Danger(Message.UnableToCreate);
                    }
                }

                return RedirectToAction(nameof(Index), new { keyword = student.Code });
            }
            
            _flashMessage.Danger(Message.UnableToCreate);
            return RedirectToAction(nameof(Index), new { keyword = student.Code });
        }

        public ActionResult EditScholarshipStudent(long id)
        {
            var model = _scholarshipProvider.GetScholarshipStudentById(id);
            CreateSelectList(model.Student.AcademicInformation.AcademicLevelId, model.ScholarshipTypeId);
            return PartialView("~/Views/ScholarshipProfile/_Form.cshtml", model);
        }

        [PermissionAuthorize("ScholarshipProfile", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditScholarshipStudent(ScholarshipStudent model)
        {
            if (model.StudentId == Guid.Empty)
            {
                _flashMessage.Danger(Message.StudentNotFound);
                return RedirectToAction(nameof(Index));
            }
            
            var studentCode = _db.Students.AsNoTracking()
                                          .SingleOrDefault(x => x.Id == model.StudentId).Code;
            
            if (_scholarshipProvider.IsScholarshipLimitExceeded(model))
            {
                _flashMessage.Danger(Message.ScholarshipLimitExceeded);
                return RedirectToAction(nameof(Index), new { keyword = studentCode });
            }
            else if (_scholarshipProvider.IsAnyScholarshipActive(model) && model.IsActive)
            {
                _flashMessage.Danger(Message.ScholarshipActive);
                return RedirectToAction(nameof(Index), new { keyword = studentCode });
            }

            var scholarshipStudent = _scholarshipProvider.GetScholarshipStudentById(model.Id);
            scholarshipStudent.ScholarshipId = model.ScholarshipId;
            scholarshipStudent.EffectivedTermId = model.EffectivedTermId;
            scholarshipStudent.ExpiredTermId = model.ExpiredTermId;
            scholarshipStudent.LimitedAmount = model.LimitedAmount;
            scholarshipStudent.Remark = model.Remark;
            scholarshipStudent.SendContract = model.SendContract;
            scholarshipStudent.AllowRepeatedRegistration = model.AllowRepeatedRegistration;
            scholarshipStudent.IsApproved = model.IsApproved;
            scholarshipStudent.ApprovedBy = model.ApprovedBy;
            scholarshipStudent.ApprovedAt = model.ApprovedAt;
            scholarshipStudent.ReferenceDate = model.ReferenceDate;
            scholarshipStudent.IsActive = model.Active;

            try
            {
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
                return RedirectToAction(nameof(Index), new { keyword = studentCode });
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToEdit);
                return RedirectToAction(nameof(Index), new { keyword = studentCode });
            }
        }

        [PermissionAuthorize("ScholarshipProfile", PolicyGenerator.Write)]
        public IActionResult Delete(long id)
        {
            var scholarshipStudent = _scholarshipProvider.GetScholarshipStudentById(id);
            try
            {
                scholarshipStudent.IsActive = false;
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }

            return RedirectToAction(nameof(Index), new { keyword = scholarshipStudent.Student.Code });
        }

        private void CreateSelectList(long academicLevelId, long scholarshipTypeId = 0)
        {
            ViewBag.ScholarshipTypes = _selectListProvider.GetScholarshipTypes();
            ViewBag.Scholarships = scholarshipTypeId == 0 ? _selectListProvider.GetScholarships()
                                                          : _selectListProvider.GetScholarshipsByScholarshipTypeId(scholarshipTypeId);
            ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            ViewBag.Signatories = _selectListProvider.GetSignatories();
        }

        public JsonResult GetLimitBugetByScholarshipId(long id, Guid studentId)
        {
            var student = _db.Students.Include(x => x.AcademicInformation)
                                      .SingleOrDefault(x => x.Id == studentId);

            if (student.AcademicInformation != null)
            {
                var scholarshipBudget = _db.ScholarshipBudgets.Include(x => x.Scholarship)
                                                              .SingleOrDefault(x => x.ScholarshipId == id
                                                                                    && (x.FacultyId == student.AcademicInformation.FacultyId
                                                                                        || x.FacultyId == null)
                                                                                    && (x.DepartmentId == student.AcademicInformation.DepartmentId
                                                                                        || x.DepartmentId == null));
                                                    
                return Json(scholarshipBudget?.AmountText ?? "0");
            }
            
            return Json(0);
        }

        public ScholarshipProfileViewModel MappingViewModel(Student student, Term term)
        {   
            var viewModel = new ScholarshipProfileViewModel();
            viewModel = _mapper.Map<Student, ScholarshipProfileViewModel>(student);
            viewModel.ScholarshipStudents = viewModel.ScholarshipStudents.OrderBy(x => x.CreatedAt)
                                                                         .ToList();
            viewModel.Vouchers = _voucherProvider.GetVoucherByStudentId(viewModel.StudentId);
            viewModel.AllowRegistration = true;
            viewModel.IsFinishedRegistration = student.RegistrationCourses.Any(x => x.TermId == term.Id);
            viewModel.AllowAdvising = true;
            viewModel.IsGraduating = student.GraduationInformation?.GraduatedAt != null;
            viewModel.IsMaintainedStatus = student.MaintenanceStatuses.Any(x => x.TermId == term.Id);
            viewModel.Transactions = _scholarshipProvider.GetTransactionsByStudent(viewModel.StudentId);
            
            return viewModel;
        }
    }
}