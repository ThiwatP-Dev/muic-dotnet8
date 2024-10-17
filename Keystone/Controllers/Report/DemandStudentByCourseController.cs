using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.Report;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.Report
{
    public class DemandStudentByCourseController : BaseController
    {
        public DemandStudentByCourseController(ApplicationDbContext db,
                                               IFlashMessage flashMessage,
                                               ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider) { }
        
        public IActionResult Index(Criteria criteria)
        {
            CreateSelectList();
            if (criteria.CourseId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            ViewBag.CourseAndCredit = _db.Courses.Single(x => x.Id == criteria.CourseId)?.CourseAndCredit;

            var courses = from course in _db.CurriculumCourses
                          join courseGroup in _db.CourseGroups on course.CourseGroupId equals courseGroup.Id
                          join grade in _db.Grades on course.RequiredGradeId equals grade.Id
                          where course.CourseId == criteria.CourseId
                          group new { course.IsRequired, grade.Weight } by courseGroup.CurriculumVersionId into g
                          select new 
                                 {
                                     CurriculumVersionId = g.Key,
                                     g.FirstOrDefault().IsRequired,
                                     g.FirstOrDefault().Weight
                                 };
            
            var results = (from student in _db.Students
                           join academicInfo in _db.AcademicInformations on student.Id equals academicInfo.StudentId
                           join curriculumInfo in _db.CurriculumInformations on student.Id equals curriculumInfo.StudentId
                           join curriculumVersion in _db.CurriculumVersions on curriculumInfo.CurriculumVersionId equals curriculumVersion.Id
                           join course in courses on curriculumInfo.CurriculumVersionId equals course.CurriculumVersionId
                           let registrationCourses = _db.RegistrationCourses.Include(x => x.Grade)
                                                                            .Where(x => x.CourseId == criteria.CourseId
                                                                                        && x.StudentId == student.Id
                                                                                        && (x.Status == "a" || x.Status == "r"))
                                                                            .AsNoTracking()
                                                                            .Select(x => x.Grade.Weight ?? 0)
                           orderby curriculumVersion.NameEn, academicInfo.Batch
                           select new DemandStudentByCourse
                                  {
                                      CurriculumVersionName = curriculumVersion.NameEn,
                                      Batch = academicInfo.Batch,
                                      IsRegistered = course.IsRequired ? registrationCourses.Any() : true,
                                      IsPassed = registrationCourses.Any(x => x >= (course.Weight ?? 0))
                                  })
                           .AsNoTracking()
                           .ToList();
            
            var model = new DemandStudentByCourseViewModel
                        {
                            Criteria = criteria,
                            Results = results
                        };

            return View(model);
        }

        private void CreateSelectList()
        {
            ViewBag.Courses = _selectListProvider.GetCourses();
        }
    }
}