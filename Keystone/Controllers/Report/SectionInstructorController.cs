using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.Report;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.Report
{
    public class SectionInstructorController : BaseController
    {
        public SectionInstructorController(ApplicationDbContext db,
                                           IFlashMessage flashMessage,
                                           ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider) { }
        
        public IActionResult Index(Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId);
            if (criteria.AcademicLevelId == 0 || criteria.TermId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var slots = new HashSet<long>(_db.SectionSlots.Where(x => criteria.InstructorId == 0 
                                                                      || x.InstructorId == criteria.InstructorId)
                                                          .Select(x => x.SectionId));

            var results = _db.Sections.Include(x => x.Course)
                                           .ThenInclude(x => x.Faculty)
                                      .Include(x => x.SectionSlots)
                                           .ThenInclude(x => x.Instructor)
                                      .Where(x => x.TermId == criteria.TermId
                                                  && (criteria.FacultyId == 0 
                                                      || x.Course.FacultyId == criteria.FacultyId)
                                                  && (criteria.DepartmentId == 0 
                                                      || x.Course.DepartmentId == criteria.DepartmentId)
                                                  && (string.IsNullOrEmpty(criteria.CourseCode)
                                                      || x.Course.Code.StartsWith(criteria.CourseCode))
                                                  && ((criteria.SectionFrom ?? 0) == 0
                                                      || x.NumberValue >= criteria.SectionFrom)
                                                  && ((criteria.SectionTo ?? 0) == 0
                                                      || x.NumberValue <= criteria.SectionTo)
                                                  && (criteria.InstructorId == 0 
                                                      || slots.Any(y => y == x.Id))
                                                  && (string.IsNullOrEmpty(criteria.HaveFinal)
                                                      || Convert.ToBoolean(criteria.HaveFinal) ? x.FinalDate.HasValue : !x.FinalDate.HasValue)
                                                  && (string.IsNullOrEmpty(criteria.HaveMidterm)
                                                      || Convert.ToBoolean(criteria.HaveMidterm) ? x.MidtermDate.HasValue : !x.MidtermDate.HasValue)
                                                  && (string.IsNullOrEmpty(criteria.Status)
                                                      || x.Status == criteria.Status))
                                      .OrderBy(x => x.Course.Faculty.Abbreviation)
                                      .ThenBy(x => x.Course.Code)
                                      .ThenBy(x => x.Number)
                                      .ToList();
                                      
            var model = new SectionInstructorViewModel
                        {
                            Criteria = criteria,
                            Results = results
                        };

            return View(model);
        }

        private void CreateSelectList(long academicLevelId, long facultyId)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Instructors = _selectListProvider.GetInstructors();
            ViewBag.YesNoAnswer = _selectListProvider.GetYesNoAnswer();
            ViewBag.Statuses = _selectListProvider.GetSectionStatuses();
            if (academicLevelId > 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
                ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);

                if (facultyId > 0)
                {
                    ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);
                }
            }
        }
    }
}