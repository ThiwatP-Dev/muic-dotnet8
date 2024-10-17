using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;

namespace Keystone.Controllers
{
    [PermissionAuthorize("Graduation", "")]
    public class GraduationController : BaseController
    {
        protected readonly IStudentProvider _studentProvider;
        protected readonly ICurriculumProvider _curriculumProvider;
        protected readonly ICacheProvider _cacheProvider;
        protected readonly IDateTimeProvider _dateTimeProvider;

        public GraduationController(ApplicationDbContext db,
                                    IFlashMessage flashMessage,
                                    IStudentProvider studentProvider,
                                    ICurriculumProvider curriculumProvider,
                                    ICacheProvider cacheProvider,
                                    IDateTimeProvider dateTimeProvider,
                                    ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider)
        {
            _studentProvider = studentProvider;
            _curriculumProvider = curriculumProvider;
            _cacheProvider = cacheProvider;
            _dateTimeProvider = dateTimeProvider;
        }
        
        public IActionResult Index(int page, Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId, criteria.CurriculumId);
            if (criteria.AcademicLevelId == 0)
            {
                criteria.AcademicLevelId = _db.AcademicLevels.SingleOrDefault(x => x.NameEn.ToLower().Contains("bachelor")).Id;
                criteria.TermId = _cacheProvider.GetCurrentTerm(criteria.AcademicLevelId).Id;
                CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId, criteria.CurriculumId);
                return View(new PagedResult<GraduationViewModel>()
                            {
                                Criteria = criteria
                            });
            }
            
            DateTime? startedAt = _dateTimeProvider.ConvertStringToDateTime(criteria.StartedAt);
            DateTime? endedAt = _dateTimeProvider.ConvertStringToDateTime(criteria.EndedAt);

            var model = (from graduation in _db.GraduationInformations
                         join student in _db.Students on graduation.StudentId equals student.Id
                         join academicInfo in _db.AcademicInformations on student.Id equals academicInfo.StudentId
                         join title in _db.Titles on student.TitleId equals title.Id
                         join faculty in _db.Faculties on academicInfo.FacultyId equals faculty.Id
                         join department in _db.Departments on academicInfo.DepartmentId equals department.Id
                         join curriculumnInfo in _db.CurriculumInformations on student.Id equals curriculumnInfo.StudentId
                         join version in _db.CurriculumVersions on curriculumnInfo.CurriculumVersionId equals version.Id
                         join honor in _db.AcademicHonors on graduation.HonorId equals honor.Id into honors
                         from honor in honors.DefaultIfEmpty()
                         where academicInfo.AcademicLevelId == criteria.AcademicLevelId
                               && (criteria.TermId == 0
                                   || graduation.TermId == criteria.TermId)
                               && (startedAt == null
                                   || graduation.GraduatedAt.Value.Date >= startedAt.Value.Date)
                               && (endedAt == null
                                   || graduation.GraduatedAt.Value.Date <= endedAt.Value.Date)
                               && (criteria.FacultyId == 0
                                   || academicInfo.FacultyId == criteria.FacultyId)
                               && (criteria.DepartmentId == 0 
                                   || academicInfo.DepartmentId == criteria.DepartmentId)
                               && ((criteria.StartStudentBatch ?? 0) == 0
                                   || academicInfo.Batch == criteria.StartStudentBatch)
                               && (criteria.CurriculumId == 0
                                   || version.CurriculumId == criteria.CurriculumId)
                               && (criteria.CurriculumVersionId == 0
                                   || curriculumnInfo.CurriculumVersionId == criteria.CurriculumVersionId)
                         orderby academicInfo.GPA descending
                         select new GraduationViewModel
                                {
                                    StudentId = student.Id,
                                    StudentCode = student.Code,
                                    Title = title.NameEn,
                                    TitleTh = title.NameTh,
                                    StudentFullName = student.FullNameEn,
                                    StudentFullNameTh = student.FullNameTh,
                                    StudentStatus = student.StudentStatusText,
                                    FacultyName = faculty.NameEn,
                                    DepartmentName = department.NameEn,
                                    FacultyNameTh = faculty.NameTh,
                                    DepartmentNameTh = department.NameTh,
                                    Credit = academicInfo.CreditComp,
                                    GPA = academicInfo.GPA,
                                    GraduatingDateText = graduation.GraduatedAtText,
                                    Remark = honor == null ? graduation.OtherRemark1 : honor.NameEn + " (" + graduation.OtherRemark1 + ")"
                                }).GetPaged(criteria, page, true);

            return View(model);
        }

        private void CreateSelectList(long academicLevelId = 0, long facultyId = 0, long curriculumId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            if (academicLevelId > 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
                ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
                ViewBag.Curriculums = _selectListProvider.GetCurriculumByAcademicLevelId(academicLevelId);
                if (facultyId > 0)
                {
                    ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);
                }

                if (curriculumId > 0)
                {
                    ViewBag.CurriculumVersions = _selectListProvider.GetCurriculumVersion(curriculumId);
                }
            }
        }
    }
}