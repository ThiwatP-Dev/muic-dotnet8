using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Profile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;
using AutoMapper;
using KeystoneLibrary.Models.DataModels.Advising;
using Microsoft.AspNetCore.Identity;
using KeystoneLibrary.Models.DataModels;
using Keystone.Permission;

namespace Keystone.Controllers
{
    [PermissionAuthorize("Advisor", "")]
    public class AdvisorController : BaseController
    {
        protected readonly IStudentProvider _studentProvider;
        protected readonly ICacheProvider _cacheProvider;
        protected readonly IMasterProvider _masterProvider;
        protected readonly IRegistrationProvider _registrationProvider;
        private UserManager<ApplicationUser> _userManager { get; }

        public AdvisorController(ApplicationDbContext db,
                                 IFlashMessage flashMessage,
                                 IMapper mapper,
                                 IStudentProvider studentProvider,
                                 ISelectListProvider selectListProvider,
                                 ICacheProvider cacheProvider,
                                 IRegistrationProvider registrationProvider,
                                 IMasterProvider masterProvider,
                                 UserManager<ApplicationUser> user) : base(db, flashMessage, mapper, selectListProvider)
        {
            _studentProvider = studentProvider;
            _cacheProvider = cacheProvider;
            _masterProvider = masterProvider;
            _registrationProvider = registrationProvider;
            _userManager = user;
        }

        public IActionResult Index(int page, Criteria criteria)
        {
            CreateSelectList();
            if (criteria.Id == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }
            
            var models = _db.Students.Include(x => x.AcademicInformation)
                                         .ThenInclude(x => x.AcademicLevel)
                                     .Include(x => x.AcademicInformation)
                                         .ThenInclude(x => x.CurriculumVersion)
                                     .Include(x => x.AcademicInformation)
                                         .ThenInclude(x => x.Faculty)
                                     .Include(x => x.AcademicInformation)
                                         .ThenInclude(x => x.Department)
                                     .Include(x => x.Title)
                                     .Where(x => x.AcademicInformation.AdvisorId == criteria.Id
                                                 && (string.IsNullOrEmpty(criteria.CodeAndName)
                                                     || x.Code.Contains(criteria.CodeAndName)
                                                     || x.FirstNameEn.Contains(criteria.CodeAndName)
                                                     || x.FirstNameTh.Contains(criteria.CodeAndName)
                                                     || x.LastNameEn.Contains(criteria.CodeAndName)
                                                     || x.LastNameTh.Contains(criteria.CodeAndName))
                                                 && (string.IsNullOrEmpty(criteria.StudentStatus)
                                                     || x.StudentStatus == criteria.StudentStatus))
                                     .IgnoreQueryFilters()
                                     .OrderByDescending(x => x.Code)
                                     .GetPaged(criteria, page);

            return View(models);
        }

        public ActionResult Details(Guid id, string returnUrl)
        {
            CreateSelectList();
            ViewBag.ReturnUrl = returnUrl;
            var student = _studentProvider.GetStudentInformationById(id);
            var model = new AdvisorViewModel();
            if (student != null)
            {
                model = _mapper.Map<Student, AdvisorViewModel>(student);
                var currentTerm = _cacheProvider.GetCurrentTerm(student.AcademicInformation.AcademicLevelId);
                var registrationTerm = _cacheProvider.GetRegistrationTerm(student.AcademicInformation.AcademicLevelId);
                var registration = _db.RegistrationCourses.Include(x => x.Student)
                                                          .Include(x => x.Course)
                                                          .Include(x => x.Section)
                                                          .Include(x => x.Term)
                                                          .Include(x => x.Grade)
                                                          .Where(x => x.Student.Id == id)
                                                          .IgnoreQueryFilters()
                                                          .ToList();

                model.CurrentTermId = currentTerm.Id;
                model.CurrentTerm = currentTerm.TermText;
                model.CurrentTermCourses = registration.Where(x => x.TermId == currentTerm.Id)
                                                       .Select(x => new RegistrationCourseDetail
                                                                    {
                                                                        CourseCode = x.Course.Code,
                                                                        CourseName = x.Course.CourseAndCredit,
                                                                        Section = x.Section == null ? "" : x.Section.Number,
                                                                        Credit = x.Course.Credit,
                                                                        GradeName = x.GradeName,
                                                                        GradeWeight = x.Grade?.Weight
                                                                    })
                                                       .OrderBy(x => x.CourseCode)
                                                       .ToList();

                model.CurrentTermTotalCredit = model.CurrentTermCourses.Sum(x => x.Credit);

                model.RegistrationTermId = registrationTerm.Id;
                model.RegistrationTerm = registrationTerm.TermText;
                model.RegistrationCourses = registration.Where(x => x.TermId == registrationTerm.Id)
                                                        .Select(x => new RegistrationCourseDetail
                                                                     {
                                                                         CourseCode = x.Course.Code,
                                                                         CourseName = x.Course.CourseAndCredit,
                                                                         Section = x.Section == null ? "" : x.Section.Number,
                                                                         Credit = x.Course.Credit
                                                                     })
                                                        .OrderBy(x => x.CourseCode)
                                                        .ToList();

                model.AdvisingCourses = _db.AdvisingCourses.Include(x => x.Course)
                                                           .Include(x => x.Section)
                                                           .Where(x => x.StudentId == student.Id
                                                                       && x.InstructorId == student.AcademicInformation.AdvisorId)
                                                           .ToList();

                foreach(var item in model.AdvisingCourses) 
                {
                    item.Sections = _registrationProvider.GetSectionByCourseId(model.RegistrationTermId, item.CourseId);
                }

                if (!model.AdvisingCourses.Any())
                {
                    model.AdvisingCourses = null;
                }
                
                model.AdvisorDetail.AdvisorId = student.AcademicInformation.AdvisorId ?? 0;
                model.AdvisingLogs = _db.AdvisingLogs.Include(x => x.Instructor)
                                                     .Where(x => x.StudentId == student.Id
                                                                 && x.InstructorId == student.AcademicInformation.AdvisorId)
                                                     .ToList();

                var advisingStatus = _db.AdvisingStatuses.SingleOrDefault(x => x.StudentId == student.Id);
                if (advisingStatus != null)
                {
                    model.AdvisorDetail.AdvisingStatusId = advisingStatus.Id;
                    model.IsAdvise = advisingStatus.IsAdvised;
                    model.IsRegistration = advisingStatus.IsRegistrationAllowed;
                    model.IsPayment = advisingStatus.IsPaymentAllowed;
                }
            }

            return View(model);
        }

        [PermissionAuthorize("Advisor", PolicyGenerator.Write)]
        [HttpPost]
        public ActionResult CreateNote(Guid studentId, long advisorId, string note, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            try
            {
                var academicLevelId = _studentProvider.GetAcademicInformationByStudentId(studentId).AcademicLevelId;
                var currentTerm = _cacheProvider.GetCurrentTerm(academicLevelId);
                var advisorLog = new AdvisingLog
                                 {
                                     TermId = currentTerm.Id,
                                     StudentId = studentId,
                                     InstructorId = advisorId,
                                     Message = note
                                 };

                _db.AdvisingLogs.Add(advisorLog);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToCreate);
            }
            
            return RedirectToAction(nameof(Details), new { id = studentId, returnUrl = returnUrl });
        }

        [PermissionAuthorize("Advisor", PolicyGenerator.Write)]
        [HttpPost]
        public ActionResult EditCourse(AdvisorViewModel model, string returnUrl)
        {
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var courses = _db.AdvisingCourses.Where(x => x.TermId == model.RegistrationTermId 
                                                                 && x.StudentId == model.StudentId
                                                                 && x.InstructorId == model.AdvisorDetail.AdvisorId)
                                                     .ToList();
                    _db.AdvisingCourses.RemoveRange(courses);
                    _db.SaveChanges();

                    _db.AdvisingCourses.AddRange(model.AdvisingCourses.Where(x => x.CourseId > 0));
                    
                    _db.SaveChanges();
                    transaction.Commit();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                }
                catch
                {
                    transaction.Rollback();
                    _flashMessage.Danger(Message.UnableToEdit);
                }
            }
            
            return RedirectToAction(nameof(Details), new { id = model.StudentId, returnUrl = returnUrl });
        }

        [PermissionAuthorize("Advisor", PolicyGenerator.Write)]
        [HttpPost]
        public ActionResult Save(AdvisorViewModel model, string returnUrl)
        {
            var advisingStatus = _masterProvider.GetAdvisingStatus(model.AdvisorDetail.AdvisingStatusId ?? 0);
            if (advisingStatus != null)
            {
                try
                {
                    var userId = _userManager.GetUserId(User);

                    advisingStatus.TermId = model.RegistrationTermId;
                    advisingStatus.IsAdvised = model.IsAdvise;
                    advisingStatus.IsPaymentAllowed = model.IsPayment;
                    advisingStatus.IsRegistrationAllowed = model.IsRegistration;
                    advisingStatus.UpdatedAt = DateTime.Now;
                    advisingStatus.UpdatedBy = userId;

                    _db.SaveChanges();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Details), new { id = model.StudentId, returnUrl = returnUrl });
                }
                catch
                {
                    _flashMessage.Danger(Message.UnableToEdit);
                    return RedirectToAction(nameof(Details), new { id = model.StudentId, returnUrl = returnUrl });
                }
            }
            else
            {
                try
                {
                    var data = new AdvisingStatus
                               {
                                   TermId = model.RegistrationTermId,
                                   StudentId = model.StudentId,
                                   InstructorId = model.AdvisorDetail.AdvisorId,
                                   IsAdvised = model.IsAdvise,
                                   IsPaymentAllowed = model.IsPayment,
                                   IsRegistrationAllowed = model.IsRegistration,
                                   CreatedAt = DateTime.Now,
                                   UpdatedAt = DateTime.Now,
                               };

                    _db.AdvisingStatuses.Add(data);
                    _db.SaveChanges();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                }
                catch
                {
                    _flashMessage.Danger(Message.UnableToSave);
                }
            }

            return RedirectToAction(nameof(Details), new { id = model.StudentId, returnUrl = returnUrl });
        }

        private void CreateSelectList()
        {
            ViewBag.Instructors = _selectListProvider.GetInstructors();
            ViewBag.Courses = _selectListProvider.GetCourses();
            ViewBag.YesNoAnswer = _selectListProvider.GetYesNoAnswer();
            ViewBag.Statuses = _selectListProvider.GetStudentStatuses();
        }
    }
}