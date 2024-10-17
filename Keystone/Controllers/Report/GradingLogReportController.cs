using AutoMapper;
using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.Report;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;

namespace Keystone.Controllers
{
    [PermissionAuthorize("GradingLogReport", "")]
    public class GradingLogReportController : BaseController
    {

        protected readonly IUserProvider _userProvider;
        public GradingLogReportController(ApplicationDbContext db,
                                          ISelectListProvider selectListProvider,
                                          IFlashMessage flashMessage,
                                          IUserProvider userProvider,
                                          IMapper mapper) : base(db, flashMessage, mapper, selectListProvider)
        { 
            _userProvider = userProvider;
        }

        public IActionResult Index(int page, Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.TermId);
            if (string.IsNullOrEmpty(criteria.CodeAndName) && criteria.AcademicLevelId == 0 && criteria.TermId == 0 
                && criteria.FacultyId == 0 && criteria.DepartmentId == 0 && criteria.CourseId == 0)
            {
                _flashMessage.Danger(Message.RequiredData);
                return View();
            }

            var models = _db.GradingLogs.Where(x => (string.IsNullOrEmpty(criteria.CodeAndName)
                                                     || x.RegistrationCourse.Student.FirstNameEn.Contains(criteria.CodeAndName, StringComparison.OrdinalIgnoreCase)
                                                     || x.RegistrationCourse.Student.LastNameEn.Contains(criteria.CodeAndName, StringComparison.OrdinalIgnoreCase)
                                                     || x.RegistrationCourse.Student.FirstNameTh.Contains(criteria.CodeAndName, StringComparison.OrdinalIgnoreCase)
                                                     || x.RegistrationCourse.Student.LastNameTh.Contains(criteria.CodeAndName, StringComparison.OrdinalIgnoreCase)
                                                     || x.RegistrationCourse.Student.Code.Contains(criteria.CodeAndName, StringComparison.OrdinalIgnoreCase))
                                                     && (criteria.AcademicLevelId == 0
                                                         || criteria.AcademicLevelId == x.RegistrationCourse.Student.AcademicInformation.AcademicLevelId)
                                                     && (criteria.TermId == 0
                                                         || criteria.TermId == x.RegistrationCourse.TermId)
                                                     && (criteria.FacultyId == 0
                                                         || criteria.FacultyId == x.RegistrationCourse.Student.AcademicInformation.FacultyId)
                                                     && (criteria.DepartmentId == 0
                                                         || criteria.DepartmentId == x.RegistrationCourse.Student.AcademicInformation.DepartmentId)
                                                     && (criteria.CourseId == 0
                                                         || criteria.CourseId == x.RegistrationCourse.CourseId)
                                                     && x.Type == "m")
                                        .Select(x => new GradingLogReportViewModel
                                                     {
                                                         StudentCode = x.RegistrationCourse.Student.Code,
                                                         StudentTitle = x.RegistrationCourse.Student.Title.NameEn,
                                                         StudentFirstNameEn = x.RegistrationCourse.Student.FirstNameEn,
                                                         StudentLastNameEn = x.RegistrationCourse.Student.LastNameEn,
                                                         StudentMidNameEn = x.RegistrationCourse.Student.MidNameEn,
                                                         Faculty = x.RegistrationCourse.Student.AcademicInformation.Faculty.ShortNameEn,
                                                         Department = x.RegistrationCourse.Student.AcademicInformation.Department.ShortNameEn,
                                                         CourseCode = x.RegistrationCourse.Course.Code,
                                                         CourseCredit = x.RegistrationCourse.Course.Credit,
                                                         CourseLecture = x.RegistrationCourse.Course.Lecture,
                                                         CourseLab = x.RegistrationCourse.Course.Lab,
                                                         CourseOther = x.RegistrationCourse.Course.Other,
                                                         CourseNameEn = x.RegistrationCourse.Course.NameEn,
                                                         CourseRateId = x.RegistrationCourse.Course.CourseRateId,
                                                         Section = x.RegistrationCourse.Section.Number,
                                                         PreviousGrade = x.PreviousGrade,
                                                         CurrentGrade = x.CurrentGrade,
                                                         ApprovedAt = x.ApprovedAtText,
                                                         ApprovedBy = x.ApprovedBy,
                                                         Remark = x.Remark
                                                     })
                                        .ToList();

            var userIds = models.Select(x => x.ApprovedBy).ToList();
            var users = _userProvider.GetCreatedFullNameByIds(userIds);
            foreach(var item in models)
            {
                item.ApprovedBy = users.Where(x => x.CreatedBy == item.ApprovedBy).FirstOrDefault()?.CreatedByFullNameEn ?? "-";
                item.PreviousGrade = string.IsNullOrEmpty(item.PreviousGrade) ? "-" : item.PreviousGrade;
            }
            var result = models.GroupBy(x => x.StudentCode)
                               .Select(x => new GradingLogGroupReportViewModel
                                            {
                                                Code = x.Key,
                                                Name = x.First().StudentFullName,
                                                Faculty = x.First().Faculty,
                                                Department = x.First().Department,
                                                Details = x.Select(y => new GradingLogDetail
                                                                        {
                                                                            ApprovedAt = y.ApprovedAt,
                                                                            ApprovedBy = y.ApprovedBy,
                                                                            PreviousGrade = y.PreviousGrade,
                                                                            CurrentGrade = y.CurrentGrade,
                                                                            Course = y.CourseCodeAndCredit,
                                                                            CourseName = y.CourseNameEn,
                                                                            Section = y.Section,
                                                                            Remark = y.Remark
                                                                        })
                                                           .OrderBy(y => y.Course)
                                                           .OrderBy(y => y.ApprovedAt)
                                                           .ToList()
                                            })
                               .AsQueryable()
                               .GetPaged(criteria, page, true);

            // var modelPageResult = models.AsQueryable().GetPaged(criteria, page);
            return View(result);
            // return View(modelPageResult);
        }

        private void CreateSelectList(long academicLevelId = 0, long termId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
            if (academicLevelId != 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            }

            if (termId != 0)
            {
                ViewBag.Courses = _selectListProvider.GetCoursesByTerm(termId);
            }
        }
    }
}