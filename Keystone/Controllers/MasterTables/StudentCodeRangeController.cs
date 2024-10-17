using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Admission;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.MasterTable
{
    [PermissionAuthorize("StudentCodeRange", "")]
    public class StudentCodeRangeController : BaseController
    {
        protected readonly IAdmissionProvider _admissionProvider;

        public StudentCodeRangeController(ApplicationDbContext db,
                                          IFlashMessage flashMessage,
                                          ISelectListProvider selectListProvider,
                                          IAdmissionProvider admissionProvider) : base(db, flashMessage, selectListProvider) 
        {
            _admissionProvider = admissionProvider;
        }

        public IActionResult Index(Criteria criteria, int page = 1)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.TermId);
            if (criteria.AcademicLevelId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }
            
            var models = _db.StudentCodeRanges.Include(x => x.AcademicLevel)
                                              .Include(x => x.AdmissionRound)
                                                  .ThenInclude(x => x.AdmissionTerm)
                                              .Where(x => x.AcademicLevelId == criteria.AcademicLevelId
                                                          && (criteria.TermId == 0
                                                              || x.AdmissionRound.AdmissionTermId == criteria.TermId)
                                                          && (criteria.AdmissionRoundId == 0
                                                              || x.AdmissionRoundId == criteria.AdmissionRoundId))
                                              .OrderBy(x => x.AdmissionRound.AdmissionTerm.TermText)
                                                  .ThenBy(x => x.AdmissionRound.Round)
                                                  .ThenBy(x => x.StartedCode)
                                                  .ThenBy(x => x.EndedCode)
                                              .IgnoreQueryFilters()
                                              .GetPaged(criteria, page);
            return View(models);
        }

        [PermissionAuthorize("StudentCodeRange", PolicyGenerator.Write)]
        public ActionResult Create(string returnUrl)
        {
            CreateSelectList();
            ViewBag.ReturnUrl = returnUrl;
            return View(new StudentCodeRange());
        }

        [PermissionAuthorize("StudentCodeRange", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(StudentCodeRange model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (_admissionProvider.IsExistRangeInAdmissionRound(model.Id, model.AcademicLevelId, model.AdmissionRoundId))
            {
                CreateSelectList(model.AcademicLevelId);
                _flashMessage.Danger(Message.ExistStudentCodeRange);
                return View(model);
            }

            if (ModelState.IsValid)
            {
                _db.StudentCodeRanges.Add(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);

                var termId = _admissionProvider.GetTermByAdmissionRoundId(model.AdmissionRoundId)?.Id ?? 0;
                return RedirectToAction(nameof(Index), new Criteria
                                                        {
                                                            AcademicLevelId = model.AcademicLevelId,
                                                            TermId = termId,
                                                            AdmissionRoundId = model.AdmissionRoundId
                                                        });
            }

            CreateSelectList(model.AcademicLevelId);
            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public ActionResult Edit(long id, string returnUrl)
        {
            StudentCodeRange model = Find(id);
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList(model.AcademicLevelId);
            return View(model);
        }

        [PermissionAuthorize("StudentCodeRange", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(StudentCodeRange studentCodeRange)
        {
            var model = Find(studentCodeRange.Id);
            if (_admissionProvider.IsExistRangeInAdmissionRound(studentCodeRange.Id, studentCodeRange.AcademicLevelId, studentCodeRange.AdmissionRoundId))
            {
                CreateSelectList(studentCodeRange.AcademicLevelId);
                _flashMessage.Danger(Message.ExistStudentCodeRange);
                return View(model);
            }

            if (ModelState.IsValid && await TryUpdateModelAsync<StudentCodeRange>(model))
            {
                try
                {
                    await _db.SaveChangesAsync();
                    _flashMessage.Confirmation(Message.SaveSucceed);

                    var termId = _admissionProvider.GetTermByAdmissionRoundId(model.AdmissionRoundId)?.Id ?? 0;
                    return RedirectToAction(nameof(Index), new Criteria
                                                            {
                                                                AcademicLevelId = model.AcademicLevelId,
                                                                TermId = termId,
                                                                AdmissionRoundId = model.AdmissionRoundId
                                                            });
                }
                catch 
                { 
                    CreateSelectList(studentCodeRange.AcademicLevelId);
                    _flashMessage.Danger(Message.UnableToEdit);
                }
            }

            CreateSelectList(studentCodeRange.AcademicLevelId);
            _flashMessage.Danger(Message.UnableToEdit);
            return View(model);
        }

        [PermissionAuthorize("StudentCodeRange", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            StudentCodeRange model = Find(id);
            try
            {
                _db.StudentCodeRanges.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }
            
            var termId = _admissionProvider.GetTermByAdmissionRoundId(model.AdmissionRoundId)?.Id ?? 0;
            return RedirectToAction(nameof(Index), new Criteria
                                                   {
                                                       AcademicLevelId = model.AcademicLevelId,
                                                       TermId = termId,
                                                       AdmissionRoundId = model.AdmissionRoundId
                                                   });
        }

        private StudentCodeRange Find(long? id) 
        {
            var model = _db.StudentCodeRanges.Include(x => x.AdmissionRound)
                                             .IgnoreQueryFilters()
                                             .SingleOrDefault(x => x.Id == id);
            return model;
        }

        private void CreateSelectList(long academicLevelId = 0, long termId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            if (academicLevelId > 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
                ViewBag.AdmissionRounds = _selectListProvider.GetAdmissionRounds(academicLevelId, termId);
            }
        }

        public bool IsExistCodeRange(long id, long academicLevelId, int startedCode, int endedCode)
        {
            var IsDuplicate = _admissionProvider.IsDuplicateStudentCodeRange(id, academicLevelId, startedCode, endedCode);
            return IsDuplicate;
        }
    }
}