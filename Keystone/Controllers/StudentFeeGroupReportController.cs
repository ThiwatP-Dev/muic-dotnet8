using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.Report;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    [PermissionAuthorize("StudentFeeGroupReport", "")]
    public class StudentFeeGroupReportController : BaseController
    {
        protected readonly IExceptionManager _exceptionManager;
        protected readonly IFeeProvider _feeProvider;

        public StudentFeeGroupReportController(ApplicationDbContext db,
                                         IFlashMessage flashMessage,
                                         IFeeProvider feeProvider,
                                         IExceptionManager exceptionManager,
                                         ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider)
        {
            _exceptionManager = exceptionManager;
            _feeProvider = feeProvider;
        }

        public ActionResult Index(int page, Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId, criteria.DepartmentId, criteria.CurriculumId);
            if (criteria.AcademicLevelId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }
            
            var model = (from feeGroup in _db.StudentFeeGroups
                         join academicLevel in _db.AcademicLevels on feeGroup.AcademicLevelId equals academicLevel.Id into academicLevels
                         from academicLevel in academicLevels.DefaultIfEmpty()
                         join studentFeeType in _db.StudentFeeTypes on feeGroup.StudentFeeTypeId equals studentFeeType.Id into studentFeeTypes
                         from studentFeeType in studentFeeTypes.DefaultIfEmpty()
                         join termFee in _db.TermFees on feeGroup.Id equals termFee.StudentFeeGroupId into termFees
                         from termFee in termFees.DefaultIfEmpty()
                         join feeItem in _db.FeeItems on termFee.FeeItemId equals feeItem.Id into feeItems
                         from feeItem in feeItems.DefaultIfEmpty()
                         join termType in _db.TermTypes on termFee.TermTypeId equals termType.Id into termTypes
                         from termType in termTypes.DefaultIfEmpty()
                         join startedTerm in _db.Terms on termFee.StartedTermId equals startedTerm.Id into startedTerms
                         from startedTerm in startedTerms.DefaultIfEmpty()
                         join endedTerm in _db.Terms on termFee.EndedTermId equals endedTerm.Id into endedTerms
                         from endedTerm in endedTerms.DefaultIfEmpty()
                         where feeGroup.AcademicLevelId == criteria.AcademicLevelId
                               && (string.IsNullOrEmpty(criteria.CodeAndName)
                                   || feeGroup.Code.StartsWith(criteria.CodeAndName)
                                   || feeGroup.Name.StartsWith(criteria.CodeAndName))
                               && (criteria.IsThai == null || feeGroup.IsThai == criteria.IsThai)
                               && (criteria.StudentFeeTypeId == 0
                                   || feeGroup.StudentFeeTypeId == criteria.StudentFeeTypeId)
                         select new StudentFeeGroupReportViewModel 
                                {
                                    Code = feeGroup.Code,
                                    Name = feeGroup.Name,
                                    AcademicLevel = academicLevel.NameEn,
                                    StartedBatch = termFee.StartedBatch,
                                    EndedBatch = termFee.EndedBatch,
                                    StartedTerm = startedTerm.TermText,
                                    EndedTerm = endedTerm.TermText,
                                    IsThai = feeGroup.IsThai,
                                    IsLumpsumPayment = feeGroup.IsLumpsumPayment,
                                    FeeItem = feeItem.NameEn,
                                    IsOneTime = termFee.IsOneTime,
                                    IsPerYear = termFee.IsPerYear,
                                    IsPerTerm = termFee.IsPerTerm,
                                    TermType = termType.NameEn,
                                    Remark = feeGroup.Remark,
                                    Term = termFee.Term,
                                    Amount = termFee.Amount,
                                    StudentFeeType = studentFeeType.NameEn
                                }).OrderByDescending(x => x.StartedTerm)
                                      .ThenByDescending(x => x.Code)
                                  .IgnoreQueryFilters()
                                  .GetPaged(criteria, page, true);
                                  
            return View(model);
        }

        private void CreateSelectList(long academicLevelId = 0, long facultyId = 0, long departmentId = 0, long curriculumId = 0)
        {
            ViewBag.TermFees = _feeProvider.GetTermFees();
            ViewBag.CalculateTypes = _selectListProvider.GetTermFeeCalculateTypes();
            ViewBag.ThaiStatuses = _selectListProvider.GetThaiStatuses();
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Nationalities = _selectListProvider.GetNationalities();
            ViewBag.TermTypes = _selectListProvider.GetTermTypes();
            ViewBag.FeeItems = _selectListProvider.GetFeeItems();
            ViewBag.StudentFeeTypes = _selectListProvider.GetStudentFeeTypes();

            if (academicLevelId != 0)
            {
                ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            }

            if (facultyId != 0)
            {
                ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);
            }

            if (departmentId != 0)
            {
                ViewBag.Curriculums = _selectListProvider.GetCurriculumByDepartment(academicLevelId, facultyId, departmentId);
            }

            if (curriculumId != 0)
            {
                ViewBag.CurriculumVersions = _selectListProvider.GetCurriculumVersion(curriculumId);
            }
        }
    }
}