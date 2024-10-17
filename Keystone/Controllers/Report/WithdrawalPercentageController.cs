using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.Report;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    public class WithdrawalPercentageController : BaseController
    {
        protected readonly IRegistrationProvider _registrationProvider;
        protected readonly IAcademicProvider _academicProvider;
        protected readonly ICalculationProvider _calculationProvider;

        public WithdrawalPercentageController(ApplicationDbContext db,
                                              ISelectListProvider selectListProvider,
                                              IFlashMessage flashMessage,
                                              IRegistrationProvider registrationProvider,
                                              IAcademicProvider academicProvider,
                                              ICalculationProvider calculationProvider) : base(db, flashMessage, selectListProvider)
        {
            _registrationProvider = registrationProvider;
            _academicProvider = academicProvider;
            _calculationProvider = calculationProvider;
        }

        public IActionResult Index(Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.TermId);

            var model = new WithdrawalPercentageViewModel
                        {
                            Criteria = criteria
                        };

            if (criteria.AcademicLevelId == 0 || criteria.TermId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View(model);
            }

            var term = _academicProvider.GetTerm(criteria.TermId);
            model.Term = term.TermText;
            model.Type = criteria.Type;

            var withdrawals = _db.Withdrawals.Include(x => x.RegistrationCourse)
                                             .Where(x => x.RegistrationCourse.TermId == criteria.TermId
                                                         && ((criteria.CourseIds == null
                                                              || !criteria.CourseIds.Any())
                                                             || criteria.CourseIds.Contains(x.RegistrationCourse.CourseId))
                                                         && (string.IsNullOrEmpty(criteria.Type)
                                                             || x.Type == criteria.Type)
                                                         && ((x.RegistrationCourse.Status == "r"
                                                              || x.RegistrationCourse.Status == "a")
                                                             && x.RegistrationCourse.IsPaid))
                                             .GroupBy(x => x.RegistrationCourse.CourseId)
                                             .ToList();
            
            model.WithdrawalPercentageDetails = withdrawals.Select(x => new WithdrawalPercentageDetail
                                                                        {
                                                                            CourseId = x.Key,
                                                                            NoOfWithdrawalStudent = x.Count(),
                                                                            WithdrawalStudents = x.GroupBy(y => y.TypeText)
                                                                                                  .Select(y => new WithdrawalStudent
                                                                                                               {
                                                                                                                   Type = y.Key,
                                                                                                                   NoOfStudents = y.Count()
                                                                                                               })
                                                                                                  .ToList()
                                                                        })
                                                           .ToList();

            model.WithdrawalPercentageDetails.Select(x => {
                                                              var course = _registrationProvider.GetCourse(x.CourseId);
                                                              var registrationCourse = _registrationProvider.GetRegistrationCoursesByCourseIdAndTermId(x.CourseId, criteria.TermId);

                                                              x.CourseCode = course.Code;
                                                              x.CourseName = course.NameEn;
                                                              x.NoOfStudents = registrationCourse.Count();
                                                              x.WithdrawalStudentPercentage = _calculationProvider.GetPercentage(x.NoOfWithdrawalStudent, x.NoOfStudents);
                                                              x.ApplicationWithdrawalStudentPercentage = _calculationProvider.GetPercentage(x.WithdrawalStudents.Where(y => y.Type == "Application")
                                                                                                                                                                .Sum(y => y.NoOfStudents), x.NoOfStudents);
                                                              x.DebarmentWithdrawalStudentPercentage = _calculationProvider.GetPercentage(x.WithdrawalStudents.Where(y => y.Type == "Debarment")
                                                                                                                                                              .Sum(y => y.NoOfStudents), x.NoOfStudents);
                                                              return x;
                                                          })
                                             .ToList();

            return View(model);
        }

        private void CreateSelectList(long academicLevelId, long termId) 
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.WithdrawalTypes = _selectListProvider.GetWithdrawalTypes();
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