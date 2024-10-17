using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    [PermissionAuthorize("DeleteAllCourse", "")]
    public class DeleteAllCourseController : BaseController
    {
        protected readonly IReceiptProvider _receiptProvider;

        public DeleteAllCourseController(ApplicationDbContext db,
                                         IFlashMessage flashMessage,
                                         ISelectListProvider selectListProvider,
                                         IReceiptProvider registrationProvider) : base(db, flashMessage, selectListProvider)
        {
            this._receiptProvider = registrationProvider;
        }

        public IActionResult Index(int page, Criteria criteria)
        {
            CreateSelectList(criteria);
            if (criteria.AcademicLevelId == 0 || criteria.TermId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var model = _db.Students.Include(x => x.RegistrationCourses)
                                        .ThenInclude(x => x.Section)
                                        .ThenInclude(x => x.Course)
                                    .Include(x => x.AcademicInformation)
                                        .ThenInclude(x => x.Department)
                                    .Include(x => x.AcademicInformation)
                                        .ThenInclude(x => x.Faculty)
                                    .Include(x => x.LatePayments)
                                    .Include(x => x.Title)
                                    .Where(x => x.AcademicInformation.AcademicLevelId == criteria.AcademicLevelId
                                                && x.RegistrationCourses.Any(y => y.TermId == criteria.TermId && !y.IsPaid && y.Status != "d")
                                                && (criteria.FacultyId == 0
                                                    || x.AcademicInformation.FacultyId == criteria.FacultyId)
                                                && (criteria.DepartmentId == 0
                                                    || x.AcademicInformation.DepartmentId == criteria.DepartmentId)
                                                && (string.IsNullOrEmpty(criteria.Code)
                                                    || x.Code.Contains(criteria.Code))
                                                && (criteria.IsLatePayment == "all"
                                                    || (Convert.ToBoolean(criteria.IsLatePayment) ? x.LatePayments.Any(y => y.TermId == criteria.TermId)
                                                                                                  : !x.LatePayments.Any(y => y.TermId == criteria.TermId))))
                                    .OrderBy(x => x.Code)
                                    .GetPaged(criteria, page);
            return View(model);
        }

        [PermissionAuthorize("DeleteAllCourse", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteCourses(Criteria criteria, List<Guid> deleteItem)
        {
            if (deleteItem != null && deleteItem.Any())
            {
                try
                {
                    _receiptProvider.CancelInvoicesWithUpdateRegistrationCourse(deleteItem, criteria.TermId);
                    _flashMessage.Confirmation(Message.SaveSucceed);
                }
                catch
                {
                    _flashMessage.Danger(Message.UnableToDelete);
                }
            }
            else
            {
                _flashMessage.Danger("Please select at least one record");
            }
            return RedirectToAction(nameof(Index), criteria);
        }

        public ActionResult GetRegistrationByStudentAndTerm(Guid id, long termId, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var student = _db.Students.Include(x => x.Title).SingleOrDefault(x => x.Id == id);
            var term = _db.Terms.SingleOrDefault(x => x.Id == termId);
            student.RegistrationTermText = term.TermText;
            student.RegistrationCourses = _db.RegistrationCourses.Include(x => x.Section)
                                                                     .ThenInclude(x => x.MainInstructor)
                                                                     .ThenInclude(x => x.Title)
                                                                 .Include(x => x.Course)
                                                                 .Where(x => x.StudentId == student.Id
                                                                             && x.TermId == termId
                                                                             && x.Status != "d")
                                                                 .ToList();

            return PartialView("~/Views/DeleteAllCourse/_DetailsInfo.cshtml", student);
        }

        private void CreateSelectList(Criteria criteria)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.LatePaymentStatuses = _selectListProvider.GetLatePaymentStatuses();
            if (criteria.AcademicLevelId != 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(criteria.AcademicLevelId);
                ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(criteria.AcademicLevelId);
            }

            if (criteria.FacultyId != 0)
            {
                ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(criteria.AcademicLevelId, criteria.FacultyId);
            }
        }
    }
}