using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.Profile;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    [PermissionAuthorize("StudentChangeCurriculum", "")]
    public class StudentChangeCurriculumController : BaseController
    {
        protected readonly ICacheProvider _cacheProvider;
        protected readonly ICurriculumProvider _curriculumProvider;
        private UserManager<ApplicationUser> _userManager { get; }
        
        public StudentChangeCurriculumController(ApplicationDbContext db,
                                                 ISelectListProvider selectListProvider,
                                                 IFlashMessage flashMessage,
                                                 ICacheProvider cacheProvider,
                                                 ICurriculumProvider curriculumProvider,
                                                 UserManager<ApplicationUser> user) : base(db, flashMessage, selectListProvider)
        {
            _cacheProvider = cacheProvider;
            _curriculumProvider = curriculumProvider;
            _userManager = user;
        }

        public IActionResult Index(Criteria criteria, int page = 1)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.CurriculumId);
            var model = new StudentChangeCurriculumViewModel();
            model.Criteria = criteria;
            if (criteria.AcademicLevelId == 0 || criteria.CurriculumVersionId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }
            var curriculumVersion = _curriculumProvider.GetCurriculumVersion(criteria.CurriculumVersionId);
            ViewBag.CurriculumsByOpenedTermAndClosedTerm = _selectListProvider.GetCurriculumByOpenedTermAndClosedTerm(criteria.AcademicLevelId, curriculumVersion.OpenedTermId, curriculumVersion.ClosedTermId);

            model.CurriculumStudents = _db.Students.Include(x => x.AcademicInformation)
                                                   .Where(x => x.AcademicInformation.CurriculumVersionId == criteria.CurriculumVersionId
                                                               && (x.RegistrationCourses == null
                                                                   || !x.RegistrationCourses.Any(y => y.Status == "a" || y.Status == "r")))
                                                   .Select(x => new CurriculumStudent
                                                                {
                                                                    StudentId = x.Id,
                                                                    Code = x.Code,
                                                                    FullNameEn = x.FullNameEn,
                                                                    GPA = x.AcademicInformation.GPA,
                                                                    CreditComp = x.AcademicInformation.CreditComp,
                                                                    Email = x.Email
                                                                })
                                                   .ToList();
            return View(model);
        }

        [PermissionAuthorize("StudentChangeCurriculum", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(long academicLevelId, long curriculumId, long curriculumVersionId, Guid[] studentIds)
        {
            var academicInformations = Find(studentIds);
            var curriculumInformations = _db.CurriculumInformations.Where(x => studentIds.Contains(x.StudentId) && x.IsActive).ToList();
            var studentLogs = new List<StudentLog>();
            var userId = _userManager.GetUserId(User);
            var curriculumVersion =_curriculumProvider.GetCurriculumVersion(curriculumVersionId);
            var curriculum = _curriculumProvider.GetCurriculum(curriculumId);
            var term = _cacheProvider.GetCurrentTerm(academicLevelId);

            foreach (var student in academicInformations)
            {
                var curInfo = curriculumInformations.FirstOrDefault(x => x.StudentId == student.StudentId && x.CurriculumVersionId == student.CurriculumVersionId);

                student.CurriculumVersionId = curriculumVersionId;
                student.FacultyId = curriculum.FacultyId;
                student.DepartmentId = curriculum.DepartmentId;

                studentLogs.Add(new StudentLog
                                {
                                    StudentId = student.StudentId,
                                    TermId = term.Id,
                                    Source = "Student Change Curriculum",
                                    Log = $"{{\"Curriculum\" : \"{ curriculum.NameEn }\",\"Curriculum Version\" : \"{ curriculumVersion.NameEn }\"}}",
                                    UpdatedAt = DateTime.UtcNow,
                                    UpdatedBy = userId
                                });

                if (curInfo != null)
                {
                    curInfo.CurriculumVersionId = curriculumVersionId;
                    curInfo.FacultyId = curriculum.FacultyId;
                    curInfo.DepartmentId = curriculum.DepartmentId;
                }
            }

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    _db.StudentLogs.AddRange(studentLogs);
                    _db.SaveChanges();
                    transaction.Commit();
                    CreateSelectList(academicLevelId, curriculumId);
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return View();
                }
                catch
                {
                    CreateSelectList(academicLevelId, curriculumId);
                    transaction.Rollback();
                    _flashMessage.Danger(Message.UnableToEdit);
                    return View();
                }
            }
        }

        private List<AcademicInformation> Find(Guid[] studentIds)
        {
            var model = _db.AcademicInformations.Where(x => studentIds.Contains(x.StudentId))
                                                .ToList();
            return model;
        }

        public void CreateSelectList(long academicLevelId = 0, long curriculumId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            if (academicLevelId > 0)
            {
                ViewBag.Curriculums = _selectListProvider.GetCurriculumByAcademicLevelId(academicLevelId);
                if (curriculumId > 0)
                {
                    ViewBag.CurriculumVersions = _selectListProvider.GetCurriculumVersion(curriculumId);
                }
            }
        }
    }
}