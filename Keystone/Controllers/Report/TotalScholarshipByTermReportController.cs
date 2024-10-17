using AutoMapper;
using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.Report;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.Report
{
    [PermissionAuthorize("TotalScholarshipByTermReport", "")]
    public class TotalScholarshipByTermReportController : BaseController
    {
        protected readonly IAcademicProvider _academicProvider;
        protected readonly IScholarshipProvider _scholarshipProvider;

        public TotalScholarshipByTermReportController(ApplicationDbContext db,
                                                      IFlashMessage flashMessage,
                                                      IMapper mapper,
                                                      IAcademicProvider academicProvider,
                                                      IScholarshipProvider scholarshipProvider,
                                                      ISelectListProvider selectListProvider) : base(db, flashMessage, mapper, selectListProvider) 
        { 
            _academicProvider = academicProvider;
            _scholarshipProvider = scholarshipProvider;
        }

        public IActionResult Index(int page, Criteria criteria)
        {
            CreateSelectList(criteria.ScholarshipTypeId);
            if (criteria.StartYear == null || criteria.EndYear == null || criteria.StartTerm == null || criteria.EndTerm == null
                || criteria.StartTerm <= 0 || criteria.EndTerm <= 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }
            
            var scholarships = _db.FinancialTransactions.Include(x => x.ScholarshipStudent)
                                                            .ThenInclude(x => x.Scholarship)
                                                            .ThenInclude(x => x.ScholarshipType)
                                                        .Include(x => x.Term)
                                                        .Where(x => (criteria.ScholarshipTypeId == 0
                                                                     || x.ScholarshipStudent.Scholarship.ScholarshipTypeId == criteria.ScholarshipTypeId)
                                                                     && (criteria.ScholarshipId == 0
                                                                         || x.ScholarshipStudent.ScholarshipId == criteria.ScholarshipId)
                                                                     && (criteria.AcademicLevelId == 0
                                                                         || x.Term.AcademicLevelId == criteria.AcademicLevelId)
                                                                     && (x.Term.AcademicYear > criteria.StartYear
                                                                         || (x.Term.AcademicYear == criteria.StartYear 
                                                                             && x.Term.AcademicTerm >= criteria.StartTerm))
                                                                     && (x.Term.AcademicYear < criteria.EndYear
                                                                         || (x.Term.AcademicYear == criteria.EndYear
                                                                             && x.Term.AcademicTerm <= criteria.EndTerm)))
                                                        .IgnoreQueryFilters()
                                                        .GroupBy(x => x.Term)
                                                        .Select(x => new TotalScholarshipByTermViewModel
                                                                     {
                                                                         Term = x.Key.TermText,
                                                                         TotalScholarshipByTermDetails = x.GroupBy(y => y.ScholarshipStudent.Scholarship.ScholarshipType)
                                                                                                          .Select(y => new TotalScholarshipByTermDetail
                                                                                                                       {
                                                                                                                           ScholarshipTypeId = y.Key.Id,
                                                                                                                           ScholarshipTypeNameEn = y.Key.NameEn,
                                                                                                                           ScholarshipTypeNameTh = y.Key.NameTh,
                                                                                                                           TermId = x.Key.Id,
                                                                                                                           TotalScholarship = y.Select(z => z.ScholarshipStudent.ScholarshipId)
                                                                                                                                               .Distinct()
                                                                                                                                               .Count(),
                                                                                                                           TotalStudent = y.Select(z => z.Student)
                                                                                                                                           .Distinct()
                                                                                                                                           .Count(),
                                                                                                                           TotalUsedAmount = y.Sum(z => z.UsedScholarship)
                                                                                                                       }
                                                                                                                 )
                                                                                                          .OrderBy(y => y.ScholarshipTypeNameEn)
                                                                                                          .ToList()
                                                                     })
                                                        .ToList();

            foreach (var scholarship in scholarships)
            {
                scholarship.TotalScholarshipByTerm = scholarship.TotalScholarshipByTermDetails.Sum(x => x.TotalUsedAmount);
                scholarship.TotalScholarship = scholarship.TotalScholarshipByTermDetails.Sum(x => x.TotalScholarship);
                scholarship.TotalStudent = scholarship.TotalScholarshipByTermDetails.Sum(x => x.TotalStudent);
            }

            var models = scholarships.AsQueryable().GetPaged(criteria, page, true);
            return View(models);
        }

        public IActionResult Details(long termId, long scholarshipTypeId, string totalUsedAmount, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var model = new TotalStudentDetailViewModel();
            model.Term = _academicProvider.GetTerm(termId)?.TermText ?? "N/A";
            model.ScholarshipTypeNameEn = _scholarshipProvider.GetScholarshipTypeById(scholarshipTypeId)?.NameEn ?? "N/A";
            model.TotalUsedAmountText = totalUsedAmount;
            model.TotalStudentDetails = _db.FinancialTransactions.Include(x => x.Student)
                                                                     .ThenInclude(x => x.AcademicInformation)
                                                                     .ThenInclude(x => x.Faculty)
                                                                 .Include(x => x.Student)
                                                                     .ThenInclude(x => x.AcademicInformation)
                                                                     .ThenInclude(x => x.Department)
                                                                 .Include(x => x.ScholarshipStudent)
                                                                     .ThenInclude(x => x.Scholarship)
                                                                     .ThenInclude(x => x.ScholarshipType)
                                                                 .Include(x => x.Term)
                                                                 .Where(x => x.Term.Id == termId
                                                                             && x.ScholarshipStudent.Scholarship.ScholarshipTypeId == scholarshipTypeId)
                                                                 .GroupBy(x => x.Student)
                                                                 .Select(x => new TotalStudentDetail
                                                                              {
                                                                                  Code = x.Key.Code,
                                                                                  FullNameEn = x.FirstOrDefault().Student.FullNameEn,
                                                                                  FacultyNameEn = x.FirstOrDefault().Student.AcademicInformation.Faculty.NameEn,
                                                                                  DepartmentNameEn = x.FirstOrDefault().Student.AcademicInformation.Department.NameEn,
                                                                                  ScholarshipNameEn = x.FirstOrDefault().ScholarshipStudent.Scholarship.NameEn,
                                                                                  TotalAmount = x.Sum(y => y.UsedScholarship)
                                                                              })
                                                                 .ToList();
            
            return View(model);
        }
        
        private void CreateSelectList(long scholarshipTypeId = 0)
        {
            ViewBag.ScholarShipTypes = _selectListProvider.GetScholarshipTypes();
            ViewBag.Scholarships = scholarshipTypeId == 0 ? _selectListProvider.GetScholarships()
                                                          : _selectListProvider.GetScholarshipsByScholarshipTypeId(scholarshipTypeId);
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
        }
    }
}