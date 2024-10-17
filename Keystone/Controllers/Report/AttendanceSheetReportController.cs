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

namespace Keystone.Controllers.Report
{
    [PermissionAuthorize("AttendanceSheetReport", "")]
    public class AttendanceSheetReportController : BaseController
    {
        protected readonly IInstructorProvider _instructorProvider;
        protected readonly IRegistrationProvider _registrationProvider;
        protected readonly IStudentPhotoProvider _studentPhotoProvider;

        public AttendanceSheetReportController(ApplicationDbContext db,
                                               IMapper mapper,
                                               IFlashMessage flashMessage,
                                               ISelectListProvider selectListProvider,
                                               IRegistrationProvider registrationProvider,
                                               IInstructorProvider instructorProvider,
                                               IStudentPhotoProvider studentPhotoProvider) : base(db, flashMessage, mapper, selectListProvider) 
        {
            _instructorProvider = instructorProvider;
            _registrationProvider = registrationProvider;
            _studentPhotoProvider = studentPhotoProvider;
        }

        public async Task<IActionResult> Index(Criteria criteria, int page)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.TermId, criteria.CourseId);
            var model = new AttendanceSheetReportViewModel
                        {
                            Criteria = criteria
                        };

            if (criteria.AcademicLevelId == 0 || criteria.TermId == 0 || criteria.CourseId == 0 || criteria.SectionId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }
            
            model = _db.Sections.AsNoTracking()
                                .Include(x => x.Course)
                                .Include(x => x.MainInstructor)
                                    .ThenInclude(x => x.Title)
                                .Include(x => x.Term)
                                    .ThenInclude(x => x.AcademicLevel)
                                .Where(x => (criteria.SectionId == 0
                                             || x.Id == criteria.SectionId)
                                             && x.TermId == criteria.TermId)
                                .Select(x => _mapper.Map<Section, AttendanceSheetReportViewModel>(x))
                                .SingleOrDefault();

            if (model != null)
            {
                var instructor = criteria.InstructorId == 0 ? model.MainInstructor
                                                            : _db.Instructors.Include(x => x.Title).SingleOrDefault(y => y.Id == criteria.InstructorId);
                model.InstructorFullName = instructor?.FullNameEn;
                model.InstructorCode = instructor?.Code;
                model.Criteria = criteria;

                var jointSectionIds = _db.Sections.Where(x => x.ParentSectionId == model.SectionId)
                                                  .Select(x => x.Id)
                                                  .ToList();

                var students = _registrationProvider.CallUSparkAPIGetStudentsBySectionId(model.SectionId);
                var studentList = _registrationProvider.GetAttendanceSheetStudentListsByStudents(students, model.SectionId);
                model.StudentList.AddRange(studentList);
                foreach(var jointId in jointSectionIds)
                {
                    students = _registrationProvider.CallUSparkAPIGetStudentsBySectionId(jointId);
                    studentList = _registrationProvider.GetAttendanceSheetStudentListsByStudents(students, jointId);
                    if(students.Any())
                    {
                        model.StudentList.AddRange(studentList);
                    }                
                }
                if (criteria.Type == "i")
                {
                    model.StudentList = model.StudentList.OrderBy(x => x.StudentCode)
                                                         .ToList();
                }
                else
                {
                    model.StudentList = model.StudentList.OrderBy(x => x.FirstName)
                                                             .ThenBy(x => x.LastName)
                                                         .ToList();
                    if (criteria.Type == "p")
                    {
                        foreach (var student in model.StudentList)
                        {
                            student.ProfileImageURL = await _studentPhotoProvider.GetStudentImg(student.StudentCode); ;
                        }
                    }
                }
            }
            
            return View(model);
        }

        private void CreateSelectList(long academicLevelId = 0, long termId = 0, long courseId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.AttendanceTypes = _selectListProvider.GetAttendanceType();
            ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            ViewBag.Courses = _selectListProvider.GetCoursesByTerm(termId);
            ViewBag.Instructors = _selectListProvider.GetInstructorByCourseId(termId, courseId);
            ViewBag.Sections = _selectListProvider.GetSectionByCourseId(termId, courseId);
        }
    }
}