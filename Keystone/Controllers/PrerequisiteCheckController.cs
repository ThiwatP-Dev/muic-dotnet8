using AutoMapper;
using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;

namespace Keystone.Controllers.MasterTables
{
    [PermissionAuthorize("PrerequisiteCheck", "")]
    public class PrerequisiteCheckController : BaseController
    {
        private IPrerequisiteProvider _prerequisiteProvider;
        private ICurriculumProvider _curriculumProvider;

        public PrerequisiteCheckController(ApplicationDbContext db,
                              IFlashMessage flashMessage,
                              IMapper mapper,
                              ISelectListProvider selectListProvider,
                              IPrerequisiteProvider prerequisiteProvider,
                              ICurriculumProvider curriculumProvider,
                              ICacheProvider cacheProvider) : base(db, flashMessage, mapper, selectListProvider)
        {
            _prerequisiteProvider = prerequisiteProvider;
            _curriculumProvider = curriculumProvider;
        }

        public IActionResult Index()
        {
            CreateSelectList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(Criteria criteria)
        {
            CreateSelectList();
            if (criteria.CourseId == 0 || string.IsNullOrEmpty(criteria.StudentCode))
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var model = new PrerequisiteCheckViewModel();
            model.Criteria = criteria;
            var studentInfo = _prerequisiteProvider.GetStudentInfoForPrerequisite(criteria.StudentCode);
            if (studentInfo == null)
            {
                _flashMessage.Warning(Message.DataNotFound);
                return View();
            }
            if (studentInfo.AcademicInformation.CurriculumVersionId.HasValue) 
            {
                model.CurriculumInformations = studentInfo.Student.CurriculumInformations;
                if (criteria.Type == "major")
                {
                    //var courses = _curriculumProvider.GetCurriculumCourse(studentInfo.AcademicInformation.CurriculumVersionId.Value);
                    //if (!courses.Select(x => x.Id).Contains(criteria.CourseId))
                    //{
                    //    _flashMessage.Warning(Message.CourseNotFoundInCurriculum);
                    //    _flashMessage.Warning(Message.CannotTakeCourse);
                    //    model.RegistrationCourses = studentInfo.RegistrationCourses.OrderBy(x => x.Course.CourseAndCredit).ToList();
                    //    return View(model); 
                    //}

                    var message = "";
                    var canTakeCourse = _prerequisiteProvider.CheckPrerequisite(studentInfo, criteria.CourseId, studentInfo.AcademicInformation.CurriculumVersionId.Value, out message);
                    if (canTakeCourse) 
                    {
                        _flashMessage.Confirmation(Message.CanTakeCourse);
                    } 
                    else 
                    {
                        _flashMessage.Warning(Message.CannotTakeCourse);
                    }
                    var prerequisites = _prerequisiteProvider.CheckPrerequisite(studentInfo, criteria.CourseId, studentInfo.AcademicInformation.CurriculumVersionId.Value);
                    model.Prerequisites = prerequisites.OrderBy(x => x.Description).GroupBy(x => x.Description).Select(x => x.First()).ToList();
                    model.RegistrationCourses = studentInfo.RegistrationCourses.OrderBy(x => x.Course.CourseAndCredit).ToList();
                    return View(model); 
                }
                else
                {
                    var message = "";
                    var canTakeCourse = _prerequisiteProvider.CheckPrerequisite(studentInfo, criteria.CourseId, 0, out message);
                    if (canTakeCourse) 
                    {
                        _flashMessage.Confirmation(Message.CanTakeCourse);
                    } 
                    else 
                    {
                        _flashMessage.Warning(Message.CannotTakeCourse);
                    }
                    var prerequisites = _prerequisiteProvider.CheckPrerequisite(studentInfo, criteria.CourseId, 0);
                    model.Prerequisites = prerequisites.OrderBy(x => x.Description).GroupBy(x => x.Description).Select(x => x.First()).ToList();
                    model.RegistrationCourses = studentInfo.RegistrationCourses.OrderBy(x => x.Course.CourseAndCredit).ToList();
                    return View(model); 
                }
            }
            else 
            {
                _flashMessage.Warning(Message.DataNotFound);
                return View(); 
            }
        }

        public void CreateSelectList()
        {
            ViewBag.Courses = _selectListProvider.GetCourses(false);
        }
    }
}