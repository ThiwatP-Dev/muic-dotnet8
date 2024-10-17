using AutoMapper;
using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.Report;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    [PermissionAuthorize("CanceledReceiptReport", "")]
    public class CanceledReceiptReportController : BaseController
    {
        public CanceledReceiptReportController(ApplicationDbContext db,
                                               IFlashMessage flashMessage,
                                               IMapper mapper,
                                               ISelectListProvider selectListProvider) : base(db, flashMessage, mapper, selectListProvider) { }

        public IActionResult Index(int page, Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId);
            if (criteria.AcademicLevelId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var reports = _db.Receipts.Include(x => x.Student)
                                          .ThenInclude(x => x.AcademicInformation)
                                      .Where(x => x.IsCancel == true
                                                  && criteria.AcademicLevelId == x.Student.AcademicInformation.AcademicLevelId
                                                  && (criteria.TermId == 0 
                                                      || criteria.TermId == x.TermId)
                                                  && (criteria.StartStudentBatch == null
                                                      || criteria.StartStudentBatch == 0
                                                      || criteria.StartStudentBatch >= x.Student.AcademicInformation.Batch)
                                                  && (criteria.EndStudentBatch == null
                                                      || criteria.EndStudentBatch == 0
                                                      || criteria.EndStudentBatch <= x.Student.AcademicInformation.Batch)
                                                  && (criteria.FacultyId == 0
                                                      || criteria.FacultyId == x.Student.AcademicInformation.FacultyId)
                                                  && (criteria.DepartmentId == 0
                                                      || criteria.DepartmentId == x.Student.AcademicInformation.DepartmentId))
                                      .Select(x => _mapper.Map<Receipt, CanceledReceiptReportViewModel>(x))
                                      .IgnoreQueryFilters()
                                      .GetPaged(criteria, page);

            return View(reports);
        }

        public void CreateSelectList(long academicLevelId = 0, long facultyId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
            ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);
        }
    }
}