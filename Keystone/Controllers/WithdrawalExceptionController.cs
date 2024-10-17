using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Withdrawals;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;

namespace Keystone.Controllers
{
    public class WithdrawalExceptionController : BaseController
    {
        protected readonly IRegistrationProvider _registrationProvider;
        protected readonly IWithdrawalProvider _withdrawalProvider;

        public WithdrawalExceptionController(ApplicationDbContext db,
                                             IFlashMessage flashMessage,
                                             ISelectListProvider selectListProvider,
                                             IAcademicProvider academicProvider,
                                             IRegistrationProvider registrationProvider,
                                             IWithdrawalProvider withdrawalProvider) : base(db, flashMessage, selectListProvider) 
        {
            _registrationProvider = registrationProvider;
            _withdrawalProvider = withdrawalProvider;
        }

        public IActionResult Index(WithdrawalExceptionViewModel model, string tabIndex)
        {
            CreateSelectList(model.AcademicLevelId ,model.FacultyId);
            model.ExceptionalCourses = _withdrawalProvider.GetExceptionalCourses(model.CourseName);
            model.ExceptionalFaculty = _withdrawalProvider.GetExceptionalFaculties(model.FacultyId, model.DepartmentId);
            
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateExceptionByCourse([FromBody]WithdrawalException model)
        {
            WithdrawalExceptionViewModel exceptions = new WithdrawalExceptionViewModel();
            if (model.CourseId != null
                && _withdrawalProvider.IsExistExceptionCourse(model.CourseId ?? 0))
            {
                exceptions.RespondStatus = "DataDuplicate";
            }
            else
            {
                try
                {
                    _db.WithdrawalExceptions.Add(model);
                    _db.SaveChanges();
                    exceptions.RespondStatus = "SaveSuccess";
                }
                catch
                {
                    exceptions.RespondStatus = "InvalidInput";
                    _flashMessage.Danger(Message.UnableToCreate);
                }
            }

            var course = _registrationProvider.GetCourse(model.CourseId ?? 0);
            CreateSelectList();
            exceptions.CourseName = course.CodeAndName;
            exceptions.ExceptionalCourses = _withdrawalProvider.GetExceptionalCourses("");

            return PartialView("~/Views/WithdrawalException/_ExceptionCoursesContent.cshtml", exceptions);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateExceptionByFaculty([FromBody]WithdrawalException model)
        {
            WithdrawalExceptionViewModel exceptions = new WithdrawalExceptionViewModel();
            if (model.FacultyId != null && model.DepartmentId != null
                && _withdrawalProvider.IsExistExceptionDepartment(model.FacultyId ?? 0, model.DepartmentId ?? 0))
            {
                exceptions.RespondStatus = "DataDuplicate";
            }
            else
            {
                try
                {
                    _db.WithdrawalExceptions.Add(model);
                    _db.SaveChanges();
                    exceptions.RespondStatus = "SaveSuccess";
                }
                catch
                {
                    exceptions.RespondStatus = "InvalidInput";
                    _flashMessage.Danger(Message.UnableToCreate);
                }
            }

            CreateSelectList(model.FacultyId ?? 0);
            exceptions.ExceptionalFaculty = _withdrawalProvider.GetExceptionalFaculties(model.FacultyId ?? 0, 0);
            
            return PartialView("~/Views/WithdrawalException/_ExceptionFacultiesContent.cshtml", exceptions);
        }

        [HttpPost]
        public ActionResult CreateExceptionByDepartments([FromBody] List<WithdrawalException> models)
        {
            WithdrawalExceptionViewModel exceptions = new WithdrawalExceptionViewModel();   
            try
            {
                var departmentIds = models.Select(y => y.DepartmentId ?? 0)
                                          .ToList();
                var exceptionalDepartments = _withdrawalProvider.GetWithdrawalExceptionsByDepartmentIds(departmentIds);
                var updatedExceptionalDepartments = models.Where(x => !exceptionalDepartments.Select(y => y.DepartmentId)
                                                                                             .Contains(x.DepartmentId));

                _db.WithdrawalExceptions.AddRange(updatedExceptionalDepartments);
                _db.SaveChanges();
                exceptions.RespondStatus = "SaveSuccess";
            }
            catch
            {
                exceptions.RespondStatus = "InvalidInput";
                _flashMessage.Danger(Message.UnableToCreate);
            }
            
            var faculty = models.FirstOrDefault();
            CreateSelectList(faculty.FacultyId ?? 0);
            exceptions.ExceptionalFaculty = _withdrawalProvider.GetExceptionalFaculties(faculty.FacultyId ?? 0, 0);

            return PartialView("~/Views/WithdrawalException/_ExceptionFacultiesContent.cshtml", exceptions);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(long id, string tabIndex)
        {
            WithdrawalExceptionViewModel exceptions = new WithdrawalExceptionViewModel();
            var model = Find(id);
            try
            {
                _db.WithdrawalExceptions.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToCreate);
            }

            if (tabIndex == "0")
            {
                var course = _registrationProvider.GetCourse(model.CourseId ?? 0);
                CreateSelectList(course.FacultyId ?? 0);
                exceptions.ExceptionalCourses = _withdrawalProvider.GetExceptionalCourses("");
                return PartialView("~/Views/WithdrawalException/_ExceptionCoursesContent.cshtml", exceptions);
            }
            else
            {
                CreateSelectList(model.FacultyId ?? 0);
                exceptions.ExceptionalFaculty = _withdrawalProvider.GetExceptionalFaculties(model.FacultyId ?? 0);
                return PartialView("~/Views/WithdrawalException/_ExceptionFacultiesContent.cshtml", exceptions);
            }
        }

        private WithdrawalException Find(long id)
        {
            var withdrawalException = _db.WithdrawalExceptions.Find(id); 
            return withdrawalException;
        }

        private void CreateSelectList(long academicLevelId = 0, long facultyId = 0)
        {
            ViewBag.Courses = _selectListProvider.GetExceptionalCourses();
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            if (academicLevelId != 0)
            {
                ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
                if (facultyId != 0)
                {
                    ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);
                    ViewBag.ExceptionalDepartments = _selectListProvider.GetExceptionalDepartments(facultyId);
                }
            }
        }
    }
}