using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.MasterTables
{
    [PermissionAuthorize("QuestionnaireMember", "")]
    public class QuestionnaireMemberController : BaseController
    {
        public QuestionnaireMemberController(ApplicationDbContext db,
                                             IFlashMessage flashMessage,
                                             ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider) { }

        public ActionResult Index(Criteria criteria, int page)
        {
            CreateSelectList();
            var members = _db.QuestionnaireMembers.Include(x => x.Instructor)
                                                      .ThenInclude(x => x.Title)
                                                  .Where(x => string.IsNullOrEmpty(criteria.CodeAndName)
                                                              || x.Instructor.Code.StartsWith(criteria.CodeAndName)
                                                              || x.Instructor.FirstNameEn.StartsWith(criteria.CodeAndName)
                                                              || x.Instructor.LastNameEn.StartsWith(criteria.CodeAndName))
                                                  .IgnoreQueryFilters()
                                                  .GetPaged(criteria ,page);
            return View(members);
        }

        [PermissionAuthorize("QuestionnaireMember", PolicyGenerator.Write)]
        public ActionResult Create(QuestionnaireMemberViewModel model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList(model.AcademicLevelId, model.FacultyId);
            
            var instructors = _db.Instructors.Where(x => (model.AcademicLevelId == 0
                                                          || x.InstructorWorkStatus.AcademicLevelId == model.AcademicLevelId)
                                                          && (model.FacultyId == 0
                                                              || x.InstructorWorkStatus.FacultyId == model.FacultyId)
                                                          && (model.DepartmentId == 0
                                                              || x.InstructorWorkStatus.DepartmentId == model.DepartmentId)
                                                          && (model.InstructorTypeId == 0
                                                              || x.InstructorWorkStatus.InstructorTypeId == model.InstructorTypeId)
                                                          && (string.IsNullOrEmpty(model.CodeAndName)
                                                              || x.Code.StartsWith(model.CodeAndName)
                                                              || x.FirstNameEn.StartsWith(model.CodeAndName)
                                                              || x.LastNameEn.StartsWith(model.CodeAndName)))
                                             .Select(x => new QuestionnaireMemberInstructor
                                                          {
                                                              InstructorId = x.Id,
                                                              Code = x.Code,
                                                              Title = x.Title.NameEn,
                                                              FirstName = x.FirstNameEn,
                                                              LastName = x.LastNameEn,
                                                              Email = x.Email
                                                          })
                                             .ToList();

            var questionnaireMembers = _db.QuestionnaireMembers.Where(x => instructors.Any(y => y.InstructorId == x.InstructorId)).ToList();
            foreach (var item in instructors)
            {
                item.IsChecked = questionnaireMembers.Any(x => x.InstructorId == item.InstructorId) ? "on" : null;
            }

            model.Instructors = instructors;
            return View(model);
        }

        [PermissionAuthorize("QuestionnaireMember", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestFormLimits(ValueCountLimit = Int32.MaxValue)]
        public ActionResult SaveCreate(QuestionnaireMemberViewModel model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            try
            {
                var duplicateIds = _db.QuestionnaireMembers.Where(x => model.Instructors.Any(y => y.InstructorId == x.InstructorId))
                                                           .Select(x => x.InstructorId)
                                                           .ToList();

                var removeUnCheckedIds = model.Instructors.Where(x => x.IsChecked == null)
                                                          .Select(x => x.InstructorId)
                                                          .ToList();

                var removeQuestionnaireMember = _db.QuestionnaireMembers.Where(x => removeUnCheckedIds.Contains(x.InstructorId))
                                                                        .ToList();

                model.Instructors = model.Instructors.Where(x => x.IsChecked == "on"
                                                                 && !duplicateIds.Contains(x.InstructorId))
                                                     .ToList();
                                                     
                _db.QuestionnaireMembers.RemoveRange(removeQuestionnaireMember);
                var questionnireMembers = new List<QuestionnaireMember>();
                foreach (var item in model.Instructors)
                {
                    questionnireMembers.Add(new QuestionnaireMember
                                            {
                                                InstructorId = item.InstructorId
                                            });
                }

                _db.QuestionnaireMembers.AddRange(questionnireMembers);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
                return Redirect(returnUrl);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToCreate);
                CreateSelectList();
                return RedirectToAction(nameof(Create), model);
            }
        }

        [PermissionAuthorize("QuestionnaireMember", PolicyGenerator.Write)]
        public ActionResult Delete(long id, string returnUrl)
        {
            var model = Find(id);
            try
            {
                _db.QuestionnaireMembers.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }

            return Redirect(returnUrl);
        }

        private QuestionnaireMember Find(long? id)
        {
            var model = _db.QuestionnaireMembers.IgnoreQueryFilters()
                                                .SingleOrDefault(x => x.Id == id);
            return model;
        }

        private void CreateSelectList(long academicLevelId = 0, long facultyId = 0)
        {
            ViewBag.Instructors = _selectListProvider.GetInstructors();
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
            ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);
            ViewBag.InstructorTypes = _selectListProvider.GetInstructorTypes();
        }
    }
}