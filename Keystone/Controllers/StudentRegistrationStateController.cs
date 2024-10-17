using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Profile;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;

namespace Keystone.Controllers
{
    [PermissionAuthorize("StudentRegistrationState", "")]
    public class StudentRegistrationStateController : BaseController
    {
        protected readonly IRegistrationProvider _registrationProvider;

        public StudentRegistrationStateController(ApplicationDbContext db, 
                                                  IFlashMessage flashMessage,
                                                  ISelectListProvider selectListProvider,
                                                  IRegistrationProvider registrationProvider) : base(db, flashMessage, selectListProvider)
        {
            _registrationProvider = registrationProvider;
        }

        public IActionResult Index(Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId);
            if (criteria.AcademicLevelId == 0 || criteria.TermId == 0 || string.IsNullOrEmpty(criteria.Code))
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var studentId = _db.Students.FirstOrDefault(x => x.Code == criteria.Code)?.Id;
            if (studentId == null)
            {
                _flashMessage.Warning(Message.StudentNotFound);
                return View();
            }

            var studentState = _db.StudentStates.FirstOrDefault(x => x.StudentId == studentId
                                                                     && x.TermId == criteria.TermId);
            if (studentState == null)
            {
                studentState = new StudentState
                               {
                                   StudentId = studentId.Value,
                                   TermId = criteria.TermId
                               };
            }

            var model = new StudentStateViewModel
                        {
                            Criteria = criteria,
                            StudentState = studentState
                        };

            return View(model);
        }

        [PermissionAuthorize("StudentRegistrationState", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(Guid studentId, long termId, string state)
        {
            var term = _db.Terms.SingleOrDefault(x => x.Id == termId);
            var studentCode = _db.Students.SingleOrDefault(x => x.Id == studentId).Code;
            var criteria = new 
                           { 
                               AcademicLevelId = term.AcademicLevelId, 
                               TermId = termId, 
                               Code = studentCode 
                           };

            if (string.IsNullOrEmpty(state))
            {
                _flashMessage.Warning(Message.RequiredData);
                return RedirectToAction(nameof(Index), criteria);
            }

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    if (!_registrationProvider.UpdateStudentState(studentId, studentCode, termId, state, "KS", out string errorMsg))
                    {
                        transaction.Rollback();
                        _flashMessage.Danger(errorMsg);
                        return RedirectToAction(nameof(Index), criteria);
                    }

                    transaction.Commit();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                }
                catch
                {
                    transaction.Rollback();
                    _flashMessage.Danger(Message.UnableToEdit);
                }
            }
            
            return RedirectToAction(nameof(Index), criteria);
        }

        public void CreateSelectList(long academicLevelId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.StudentStates = _selectListProvider.GetStudentStates();
            if (academicLevelId > 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            }
        }
    }
}