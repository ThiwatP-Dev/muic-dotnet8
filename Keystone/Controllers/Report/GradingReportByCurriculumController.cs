using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.Report;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;

namespace Keystone.Controllers
{
    [PermissionAuthorize("GradingReportByCurriculum", "")]
    public class GradingReportByCurriculumController : BaseController
    {
        protected readonly IStudentProvider _studentProvider;
        protected readonly ICurriculumProvider _curriculumProvider;

        public GradingReportByCurriculumController(ApplicationDbContext db,
                                                   ISelectListProvider selectListProvider,
                                                   IFlashMessage flashMessage,
                                                   IStudentProvider studentProvider,
                                                   ICurriculumProvider curriculumProvider) : base(db, flashMessage, selectListProvider)
        {
            _studentProvider = studentProvider;
            _curriculumProvider = curriculumProvider;
        }

        public IActionResult Index(Criteria criteria)
        {
            if (string.IsNullOrEmpty(criteria.Code))
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var student = _studentProvider.GetStudentByCode(criteria.Code);
            if (student == null)
            {
                _flashMessage.Warning(Message.StudentNotFound);
                return View();
            }

            var requestViewModel = new GraduatingRequestViewModel();
            long curriculumVersionId = student.AcademicInformation.CurriculumVersionId ?? 0;

            _studentProvider.UpdateCGPA(student.Id);

            var curriculumVersion = _curriculumProvider.GetCurriculumVersion(curriculumVersionId);
            int totalCourseGroup = 0;
            var courseGroups = _curriculumProvider.GetCourseGroupWithRegistrationCourses(student.Id
                                                                                         , curriculumVersionId
                                                                                         , out totalCourseGroup);
            requestViewModel.TotalCourseGroup = totalCourseGroup;  
            requestViewModel.TotalCurriculumVersionCredit = curriculumVersion.TotalCredit;
            // var otherGroup = _curriculumProvider.GetOtherCourseGroupRegistrations(student.Id, courses);

            requestViewModel.CurriculumCourseGroups = courseGroups;
            // requestViewModel.OtherCourseGroups = otherGroup;
            requestViewModel.Student = student;
            var model = new GradingReportByCurriculumViewModel
                        {
                            Criteria = criteria,
                            StudentRegistrationCoursesViewModels  = new StudentRegistrationCoursesViewModel
                                                                    {
                                                                        TranscriptGrade = _studentProvider.GetStudentRegistrationCourseViewModel(student.Id, null),
                                                                        TransferCourse = _studentProvider.GetStudentRegistrationCourseTranferViewModel(student.Id, null),
                                                                        TransferCourseWithGrade = _studentProvider.GetStudentRegistrationCourseTranferWithGradeViewModel(student.Id, null),
                                                                    },
                            GraduatingRequest = requestViewModel
                        };

            return View(model);
        }

        public IActionResult ExportExcel(string code)
        {
            var student = _studentProvider.GetStudentByCode(code);
            var requestViewModel = new GraduatingRequestViewModel();
            long curriculumVersionId = student.CurriculumInformations.FirstOrDefault()?.CurriculumVersionId ?? 0;
            int totalCourseGroup = 0;
            var courseGroups = _curriculumProvider.GetCourseGroupWithRegistrationCourses(student.Id
                                                                                         , curriculumVersionId
                                                                                         , out totalCourseGroup);

            requestViewModel.TotalCourseGroup = totalCourseGroup;  
            // var otherGroup = _curriculumProvider.GetOtherCourseGroupRegistrations(student.Id, courses);
            requestViewModel.CurriculumCourseGroups = courseGroups;
            // requestViewModel.OtherCourseGroups = otherGroup;
            requestViewModel.Student = student;
            return View(requestViewModel);
        }
    }
}