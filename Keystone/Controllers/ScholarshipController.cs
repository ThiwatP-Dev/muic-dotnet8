using AutoMapper;
using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.Profile;
using KeystoneLibrary.Models.DataModels.Scholarship;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Vereyon.Web;

namespace Keystone.Controllers
{
    [PermissionAuthorize("Scholarship", "")]
    public class ScholarshipController : BaseController 
    {
        protected readonly ICacheProvider _cacheProvider;
        protected readonly IAcademicProvider _academicProvider;
        protected readonly IStudentProvider _studentProvider;
        protected readonly IScholarshipProvider _scholarshipProvider;
        private UserManager<ApplicationUser> _userManager { get; }
        
        public ScholarshipController(ApplicationDbContext db,
                                     IFlashMessage flashMessage,
                                     IMemoryCache memoryCache,
                                     ISelectListProvider selectListProvider,
                                     ICacheProvider cacheProvider,
                                     IAcademicProvider academicProvider,
                                     IStudentProvider studentProvider,
                                     UserManager<ApplicationUser> user,
                                     IScholarshipProvider scholarshipProvider,
                                     IMapper mapper) : base(db, flashMessage, mapper, memoryCache, selectListProvider) 
        { 
            _cacheProvider = cacheProvider;
            _academicProvider = academicProvider;
            _studentProvider = studentProvider;
            _userManager = user;
            _scholarshipProvider = scholarshipProvider;
        }

        public IActionResult Index(Criteria criteria, int page = 1)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId, criteria.ScholarshipTypeId);
            if (criteria.ScholarshipTypeId == 0 && criteria.SponsorId == 0 && criteria.ScholarshipId == 0 
                && string.IsNullOrEmpty(criteria.Status) && criteria.StartTermId == 0 && criteria.EndTermId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }
            
            var models = _db.Scholarships.Include(x => x.StartedTerm)
                                         .Include(x => x.EndedTerm)
                                         .Include(x => x.ScholarshipType)
                                         .Include(x => x.Sponsor)
                                         .IgnoreQueryFilters()
                                         .Where(x => (string.IsNullOrEmpty(criteria.Status)
                                                      || x.IsActive == Convert.ToBoolean(criteria.Status))
                                                      && (criteria.ScholarshipId == 0
                                                          || x.Id == criteria.ScholarshipId)
                                                      && (criteria.StartTermId == 0
                                                          || x.StartedTermId == criteria.StartTermId)
                                                      && (criteria.EndTermId == 0
                                                          || x.EndedTermId == criteria.EndTermId)
                                                      && (criteria.ScholarshipTypeId == 0
                                                          || x.ScholarshipTypeId == criteria.ScholarshipTypeId)
                                                      && (criteria.SponsorId == 0
                                                          || x.SponsorId == criteria.SponsorId))
                                         .OrderBy(x => x.NameEn)
                                         .GetPaged(criteria, page);

            return View(models);
        }

        public IActionResult Details(long id, string returnUrl)
        {
            var model = Find(id);
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        [PermissionAuthorize("Scholarship", PolicyGenerator.Write)]
        public IActionResult Create(string returnUrl)
        {
            CreateSelectList();
            ViewBag.ReturnUrl = returnUrl;
            return View(new Scholarship());
        }

        [PermissionAuthorize("Scholarship", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Scholarship model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var budgets = model.BudgetDetails.Where(x => x.FeeGroupId > 0).ToList();
            model.BudgetDetails = budgets.Any() ? budgets : null;

            var feeItems = model.ScholarshipFeeItems.Where(x => x.FeeItemId > 0).ToList();
            model.ScholarshipFeeItems = feeItems.Any() ? feeItems : null;

            model.AcademicLevelId = _academicProvider.GetTerm(model.StartedTermId ?? 0)?.AcademicLevelId;
            if (!model.IsFullAmount && !feeItems.Any())
            {
                CreateSelectList(model.AcademicLevelId ?? 0);
                _flashMessage.Warning(Message.RequiredData);
                return View(model);
            }

            try
            {
                model.AllowRepeatedRegistration = true;
                _db.Scholarships.Add(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
                return RedirectToAction(nameof(Index), new Criteria
                                                           {
                                                               ScholarshipId = model.Id
                                                           });
            }
            catch
            {
                CreateSelectList(model.AcademicLevelId ?? 0);
                _flashMessage.Danger(Message.UnableToCreate);
                return View(model);
            }
        }

        public IActionResult Edit(long id, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var model = Find(id);
            model.AcademicLevelId = _academicProvider.GetTerm(model.StartedTermId ?? 0)?.AcademicLevelId;
            CreateSelectList(model.AcademicLevelId ?? 0);
            return View(model);
        }

        [PermissionAuthorize("Scholarship", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Scholarship model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var budgets = model.BudgetDetails.Where(x => x.FeeGroupId > 0).ToList();
            model.BudgetDetails = budgets.Any() ? budgets : null;

            var feeItems = model.ScholarshipFeeItems.Where(x => x.FeeItemId > 0).ToList();
            model.ScholarshipFeeItems = feeItems.Any() ? feeItems : null;

            model.AcademicLevelId = _academicProvider.GetTerm(model.StartedTermId ?? 0)?.AcademicLevelId;
            if (!model.IsFullAmount && !feeItems.Any())
            {
                CreateSelectList(model.AcademicLevelId ?? 0);
                _flashMessage.Warning(Message.RequiredData);
                return View(model);
            }

            var scholarship = Find(model.Id);
            using (var transaction = _db.Database.BeginTransaction())
            {
                if (await TryUpdateModelAsync<Scholarship>(scholarship))
                {
                    try
                    {
                        var _budgets =  scholarship.BudgetDetails.Where(x => x.FeeGroupId > 0).ToList();
                        scholarship.BudgetDetails = _budgets.Any() ? _budgets : null;

                        var _feeItems = scholarship.ScholarshipFeeItems.Where(x => x.FeeItemId > 0).ToList();
                        scholarship.ScholarshipFeeItems = _feeItems.Any() ? _feeItems : null;

                        await _db.SaveChangesAsync();
                        transaction.Commit();
                        _flashMessage.Confirmation(Message.SaveSucceed);
                        return RedirectToAction(nameof(Index), new Criteria
                                                               {
                                                                   ScholarshipId = model.Id
                                                               });
                    }
                    catch
                    {
                        transaction.Rollback();
                        CreateSelectList(model.AcademicLevelId ?? 0);
                        _flashMessage.Danger(Message.UnableToEdit);
                        return View(model);
                    }
                }
                else
                {
                    transaction.Rollback();
                    CreateSelectList(model.AcademicLevelId ?? 0);
                    _flashMessage.Danger(Message.UnableToEdit);
                    return View(model);
                }
                
            }
        }

        [PermissionAuthorize("Scholarship", PolicyGenerator.Write)]
        public ActionResult Delete(long id, string returnUrl)
        {
            var model = Find(id);
            try
            {
                _db.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                model.AcademicLevelId = _academicProvider.GetTerm(model.StartedTermId ?? 0)?.AcademicLevelId;
                CreateSelectList(model.AcademicLevelId ?? 0);
                _flashMessage.Danger(Message.UnableToDelete);
            }
            
            return Redirect(returnUrl);
        }

        public ActionResult Students(Criteria criteria, string returnUrl, int page = 1)
        {
            ViewBag.AllYesNoAnswer = _selectListProvider.GetAllYesNoAnswer();
            var models = _db.ScholarshipStudents.Include(x => x.Student)
                                                    .ThenInclude(x => x.AcademicInformation)
                                                .Include(x => x.Scholarship)
                                                .IgnoreQueryFilters()
                                                .Where(x => (string.IsNullOrEmpty(criteria.Code)
                                                             || x.Student.Code == criteria.Code)
                                                            && (string.IsNullOrEmpty(criteria.Status)
                                                                || (Convert.ToBoolean(criteria.Status)
                                                                    ? x.Student.AcademicInformation.GPA < x.Scholarship.MinimumGPA
                                                                    : x.Student.AcademicInformation.GPA > x.Scholarship.MinimumGPA))
                                                             && x.ScholarshipId == criteria.ScholarshipId)
                                                .OrderBy(x => x.Student.Code)
                                                .GetPaged(criteria, page);

            ViewBag.Scholarships = _scholarshipProvider.GetScholarshipById(criteria.ScholarshipId);
            ViewBag.ReturnUrl = returnUrl;

            return View(models);
        }

        [PermissionAuthorize("Scholarship", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Active(long scholarshipId, long studentId, bool isActive, DateTime approvedAt, string remark)
        {
            var userId = _userManager.GetUserId(User);
            var student = _db.ScholarshipStudents.IgnoreQueryFilters()
                                                 .SingleOrDefault(x => x.Id == studentId);
            var studentInformation = _studentProvider.GetStudentInformationById(student.StudentId);
            var term = _cacheProvider.GetCurrentTerm(studentInformation.AcademicInformation.AcademicLevelId);
            var log = new ScholarshipActiveLog
                      {
                          StudentId = student.StudentId,
                          ScholarshipId = scholarshipId,
                          TermId = term.Id,
                          IsActive = isActive,
                          ApprovedAt = approvedAt,
                          Remark = remark,
                          UpdatedAt = DateTime.UtcNow,
                          UpdatedBy = userId
                      };

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    student.IsActive = isActive;
                    _db.ScholarshipActiveLogs.Add(log);
                    _db.SaveChanges();

                    transaction.Commit();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                }
                catch
                {
                    transaction.Rollback();
                    _flashMessage.Danger(Message.UnableToDelete);
                }
            }

            return RedirectToAction(nameof(Students), new { id = scholarshipId });
        }

        public ActionResult ActiveAll(long id)
        {
            return RedirectToAction(nameof(Students));
        }

        public ActionResult AddStudents(Criteria criteria, string returnUrl, int page)
        {
            var model = new AddStudentScholarshipViewModel();
            ViewBag.ReturnUrl = returnUrl;
            model.Criteria = criteria;
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId);
            if (criteria.AcademicLevelId == 0 && criteria.StartStudentBatch == null && criteria.EndStudentBatch == null)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View(model);
            }

            var students = _db.Students.Include(x => x.AcademicInformation)
                                           .ThenInclude(x => x.AcademicLevel)
                                       .Include(x => x.AdmissionInformation)
                                           .ThenInclude(x => x.AdmissionTerm)
                                       .Include(x => x.ScholarshipStudents)
                                       .Where(x => (string.IsNullOrEmpty(criteria.Code)
                                                        || x.Code == criteria.Code)
                                                    && (string.IsNullOrEmpty(criteria.FirstName)
                                                        || x.FirstNameEn == criteria.FirstName || x.FirstNameTh == criteria.FirstName)
                                                    && (criteria.AcademicLevelId == 0
                                                        || criteria.AcademicLevelId == x.AcademicInformation.AcademicLevelId)
                                                    && (criteria.FacultyId == 0
                                                        || criteria.FacultyId == x.AcademicInformation.FacultyId)
                                                    && (criteria.DepartmentId == 0
                                                        || criteria.DepartmentId == x.AcademicInformation.DepartmentId)
                                                    && (criteria.StartTermId == 0
                                                        || criteria.StartTermId <= x.AdmissionInformation.AdmissionTermId)
                                                    && (criteria.EndTermId == 0
                                                        || criteria.EndTermId >= x.AdmissionInformation.AdmissionTermId)
                                                    && x.AcademicInformation != null
                                                    && criteria.StartStudentBatch <= x.AcademicInformation.Batch
                                                    && criteria.EndStudentBatch >= x.AcademicInformation.Batch
                                                    && (string.IsNullOrEmpty(criteria.IsAthlete)
                                                        || Convert.ToBoolean(criteria.IsAthlete) == x.AcademicInformation.IsAthlete))
                                       .Select(x => _mapper.Map<Student, ScholarshipStudentViewModel>(x))
                                       .ToList();

            students.Select(x => { 
                                     x.IsExist = x.ScolarshipIds.Any(y => y == criteria.ScholarshipId);
                                     return x;
                                 }).ToList();
            
            model.AddStudents = students;
            return View(model);
        }

        [PermissionAuthorize("Scholarship", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddStudents(long id, Guid[] studentIds, string returnUrl)
        {
            var scholarship = Find(id);
            var scholarshipStudents = new List<ScholarshipStudent>();
            var studentLogs = new List<ScholarshipActiveLog>();
            var userId = _userManager.GetUserId(User);
            ViewBag.ReturnUrl = returnUrl;
            foreach (var studentId in studentIds)
            {
                var student = _db.Students.Include(x => x.AcademicInformation)
                                          .SingleOrDefault(x => x.Id == studentId);
                                          
                var term = _cacheProvider.GetCurrentTerm(student.AcademicInformation.AcademicLevelId);
                scholarshipStudents.Add(new ScholarshipStudent
                                        {
                                            ScholarshipId = id,
                                            StudentId = student.Id,
                                            EffectivedTermId = term.Id,
                                            ExpiredTermId = scholarship.TotalYear.HasValue
                                                            ? GetExpiredTerm(student.AcademicInformation.AcademicLevelId, scholarship.TotalYear.Value)
                                                            : null,
                                            LimitedAmount = scholarship.DefaultAmount ?? 0,
                                            SendContract = false,
                                            IsApproved = true
                                        });

                studentLogs.Add(new ScholarshipActiveLog
                                {
                                    StudentId = student.Id,
                                    ScholarshipId = id,
                                    TermId = term.Id,
                                    IsActive = true,
                                    ApprovedAt = DateTime.UtcNow,
                                    Remark = "Add new scholarship",
                                    UpdatedAt = DateTime.UtcNow,
                                    UpdatedBy = userId
                                });
            }
            
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    _db.ScholarshipStudents.AddRange(scholarshipStudents);
                    _db.ScholarshipActiveLogs.AddRange(studentLogs);
                    _db.SaveChanges();

                    transaction.Commit();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Students), new { ScholarshipId = id, ReturnUrl = returnUrl });
                }
                catch
                {
                    CreateSelectList();
                    transaction.Rollback();
                    _flashMessage.Danger(Message.UnableToSave);
                    return RedirectToAction(nameof(AddStudents), new { ScholarshipId = id, ReturnUrl = returnUrl });
                }
            }
        }

        [PermissionAuthorize("Scholarship", PolicyGenerator.Write)]
        public ActionResult DeleteStudent(long routeId, string returnUrl, Criteria criteria)
        {
            var model = _db.ScholarshipStudents.Find(routeId);
            try
            {
                model.IsActive = false;
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                CreateSelectList();
                _flashMessage.Danger(Message.UnableToDelete);
            }
            
            return RedirectToAction(nameof(Students), new { ScholarshipId = model?.ScholarshipId ?? 0, returnUrl = returnUrl });
        }

        private long? GetExpiredTerm(long academicLevelId, int totalYear)
        {
            var effectivedTerm = _cacheProvider.GetCurrentTerm(academicLevelId);
            var term = _academicProvider.GetTermsByAcademicLevelId(academicLevelId)
                                        .SingleOrDefault(x => x.AcademicYear == (effectivedTerm.AcademicYear + totalYear)
                                                              && x.AcademicTerm == effectivedTerm.AcademicTerm);
            return term?.Id;
        }

        private Scholarship Find(long id)
        {
            var model = _db.Scholarships.Include(x => x.ScholarshipFeeItems)
                                            .ThenInclude(x => x.FeeItem)
                                        .Include(x => x.StartedTerm)
                                        .Include(x => x.EndedTerm)
                                        .Include(x => x.Sponsor)
                                        .Include(x => x.ScholarshipType)
                                        .Include(x => x.BudgetDetails)
                                             .ThenInclude(x => x.FeeGroup)
                                        .IgnoreQueryFilters()
                                        .SingleOrDefault(x => x.Id == id);

            model.BudgetDetails = model.BudgetDetails.Any() ? model.BudgetDetails : null;
            model.ScholarshipFeeItems = model.ScholarshipFeeItems.Any() ? model.ScholarshipFeeItems : null;

            return model;
        }

        private void CreateSelectList(long academicLevelId = 0, long facultyId = 0, long scholarshipTypeId = 0)
        {
            ViewBag.FeeItems = _selectListProvider.GetFeeItems();
            ViewBag.Percentages = _selectListProvider.GetPercentages();
            ViewBag.Sponsors = _selectListProvider.GetSponsors();
            ViewBag.ScholarshipTypes = _selectListProvider.GetScholarshipTypes();
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Percentages = _selectListProvider.GetScholarshipPercentages();
            ViewBag.Statuses = _selectListProvider.GetActiveStatuses();
            ViewBag.YesNoAnswers = _selectListProvider.GetAllYesNoAnswer();
            ViewBag.FeeGroups = _selectListProvider.GetFeeGroups();
            ViewBag.Scholarships = scholarshipTypeId > 0 ? _selectListProvider.GetScholarshipsByScholarshipTypeId(scholarshipTypeId)
                                                         : _selectListProvider.GetScholarships();
            if (academicLevelId > 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            }
                                                
            if (facultyId > 0)
            {
                ViewBag.Departments = _selectListProvider.GetDepartments();
            }
        }
    }
}