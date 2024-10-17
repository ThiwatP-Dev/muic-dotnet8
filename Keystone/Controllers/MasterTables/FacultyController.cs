using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.MasterTables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.MasterTables
{
    [PermissionAuthorize("Faculty", "")]
    public class FacultyController : BaseController
    {
        protected readonly IExceptionManager _exceptionManager;

        public FacultyController(ApplicationDbContext db,
                                 IFlashMessage flashMessage,
                                 IExceptionManager exceptionManager,
                                 ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider) 
        { 
            _exceptionManager = exceptionManager;
        }

        public IActionResult Index()
        {
            var models = _db.Faculties.IgnoreQueryFilters()
                                      .ToList();
            return View(models);
        }

        public IActionResult Details(long id, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var model = new FacultyViewModel();
            var faculty = _db.Faculties.SingleOrDefault(x => x.Id == id);
            model.FacultyId = id;
            model.Code = faculty.Code;
            model.NameEn = faculty.NameEn;
            model.NameTh = faculty.NameTh;
            model.Abbreviation = faculty.Abbreviation;
            model.Departments = _db.Departments.Where(x => x.FacultyId == id)
                                               .Select(x => new FacultyDepartment
                                                            {
                                                                Code = x.Code,
                                                                NameEn = x.NameEn,
                                                                NameTh = x.NameTh,
                                                                Abbreviation = x.Abbreviation
                                                            })
                                               .AsNoTracking()
                                               .ToList();
                                                   
            model.Curriculums = _db.CurriculumVersions.Where(x => x.Curriculum.FacultyId == id)
                                                      .Select(x => new FacultyCurriculum
                                                                   {
                                                                       Code = x.Code,
                                                                       NameEn = x.NameEn,
                                                                       NameTh = x.NameTh
                                                                   })
                                                      .AsNoTracking()
                                                      .ToList();

            model.Directors = _db.FacultyMembers.Where(x => x.FacultyId == id
                                                            && x.Type == "pd")
                                                .Select(x => new FacultyDirector
                                                             {
                                                                 FacultyMemberId = x.Id,
                                                                 Name = $"{ x.Instructor.Title.NameEn } { x.Instructor.FirstNameEn } { x.Instructor.LastNameEn }",
                                                                 Email = x.Instructor.Email,
                                                                 FilterCourseGroup = x.FilterCourseGroup.Name,
                                                                 FilterCurriculumVersionGroup = x.FilterCurriculumVersionGroup.Name
                                                             })
                                                .AsNoTracking()
                                                .ToList();

            model.ChairMen = _db.FacultyMembers.Where(x => x.FacultyId == id
                                                           && x.Type == "c")
                                               .Select(x => new FacultyDirector
                                                            {
                                                                FacultyMemberId = x.Id,
                                                                Name = $"{ x.Instructor.Title.NameEn } { x.Instructor.FirstNameEn } { x.Instructor.LastNameEn }",
                                                                Email = x.Instructor.Email,
                                                                FilterCourseGroup = x.FilterCourseGroup.Name,
                                                                FilterCurriculumVersionGroup = x.FilterCurriculumVersionGroup.Name
                                                            })
                                               .AsNoTracking()
                                               .ToList();
            return View(model);
        }

        [PermissionAuthorize("Faculty", PolicyGenerator.Write)]
        public ActionResult Create()
        {
            return View();
        }

        [PermissionAuthorize("Faculty", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Faculty model)
        {
            if (ModelState.IsValid)
            {
                _db.Faculties.Add(model);
                try
                {
                    _db.SaveChanges();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception e)
                {
                    if (_exceptionManager.IsDuplicatedEntityCode(e))
                    {
                        _flashMessage.Danger(Message.CodeUniqueConstraintError);
                    }
                    else
                    {
                        _flashMessage.Danger(Message.UnableToCreate);
                    }

                    return View(model);
                }
            }

            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public ActionResult Edit(long id)
        {
            Faculty model = Find(id);	
            return View(model);
        }

        [PermissionAuthorize("Faculty", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long? id)
        {
            var model = Find(id);

            if (ModelState.IsValid && await TryUpdateModelAsync<Faculty>(model))
            {
                try
                {
                    await _db.SaveChangesAsync();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception e)
                { 
                    if (_exceptionManager.IsDuplicatedEntityCode(e))
                    {
                        _flashMessage.Danger(Message.CodeUniqueConstraintError);
                    }
                    else
                    {
                        _flashMessage.Danger(Message.UnableToEdit);
                    }
                    
                    return View(model);
                }
            }

            _flashMessage.Danger(Message.UnableToEdit);
            return View(model);
        }

        public ActionResult AddFacultyMember(long id, string type, string returnUrl)
        {
            CreateSelectList();
            ViewBag.ReturnUrl = returnUrl;
            var model = new FacultyDirector
                        {
                            FacultyId = id,
                            Type = type
                        };
                        
            return PartialView("_FacultyMemberForm", model);
        }

        public ActionResult EditFacultyMember(long id, string returnUrl)
        {
            CreateSelectList();
            ViewBag.ReturnUrl = returnUrl;
            var facultyMember = _db.FacultyMembers.SingleOrDefault(x => x.Id == id);
            var model = new FacultyDirector
                        {
                            FacultyMemberId = facultyMember.Id,
                            InstructorId = facultyMember.InstructorId,
                            FacultyId = facultyMember.FacultyId,
                            FilterCourseGroupId = facultyMember.FilterCourseGroupId,
                            FilterCurriculumVersionGroupId = facultyMember.FilterCurriculumVersionGroupId,
                            Type = facultyMember.Type
                        };

            return PartialView("_FacultyMemberForm", model);
        }

        [PermissionAuthorize("Faculty", PolicyGenerator.Write)]
        [HttpPost]
        public ActionResult AddFacultyMember(FacultyDirector model, string returnUrl)
        {
            var tabIndex = model.Type == "pd" ? "3" : model.Type == "c" ? "2" : "0";
            ViewBag.ReturnUrl = returnUrl;
            // if (_db.QuestionnaireMembers.Any(x => x.InstructorId == model.InstructorId))
            // {
            //     _flashMessage.Danger(Message.UnableToSaveDataDuplicate);
            //     CreateSelectList();
            //     return RedirectToAction(nameof(Details), new { id = model.FacultyId, returnUrl = returnUrl, tabIndex = "2" });
            // }

            try
            {
                var facultyMember = new FacultyMember
                                    {
                                        InstructorId = model.InstructorId,
                                        FacultyId = model.FacultyId,
                                        FilterCourseGroupId = model.FilterCourseGroupId,
                                        FilterCurriculumVersionGroupId = model.FilterCurriculumVersionGroupId,
                                        Type = model.Type
                                    };

                _db.FacultyMembers.Add(facultyMember);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToCreate);
                return RedirectToAction(nameof(Details), new { id = model.FacultyId, returnUrl = returnUrl, tabIndex });
            }

            return RedirectToAction(nameof(Details), new { id = model.FacultyId, returnUrl = returnUrl, tabIndex });
        }

        [PermissionAuthorize("Faculty", PolicyGenerator.Write)]
        [HttpPost]
        public ActionResult EditFacultyMember(FacultyDirector model, string returnUrl)
        {
            var tabIndex = model.Type == "pd" ? "3" : model.Type == "c" ? "2" : "0";
            ViewBag.ReturnUrl = returnUrl;
            // if (_db.QuestionnaireMembers.Any(x => x.InstructorId == model.InstructorId))
            // {
            //     _flashMessage.Danger(Message.UnableToSaveDataDuplicate);
            //     CreateSelectList();
            //     return RedirectToAction(nameof(Details), new { id = model.FacultyId, returnUrl = returnUrl, tabIndex = "2" });
            // }

            try
            {
                var facultyMember = _db.FacultyMembers.SingleOrDefault(x => x.Id == model.FacultyMemberId);
                facultyMember.InstructorId = model.InstructorId;
                facultyMember.FilterCourseGroupId = model.FilterCourseGroupId;
                facultyMember.FilterCurriculumVersionGroupId = model.FilterCurriculumVersionGroupId;
                facultyMember.Type = model.Type;

                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToEdit);
                return RedirectToAction(nameof(Details), new { id = model.FacultyId, returnUrl = returnUrl, tabIndex });
            }

            return RedirectToAction(nameof(Details), new { id = model.FacultyId, returnUrl = returnUrl, tabIndex });
        }

        [PermissionAuthorize("Faculty", PolicyGenerator.Write)]
        public ActionResult DeleteProgramDirector(long id, string returnUrl)
        {
            var model = _db.FacultyMembers.SingleOrDefault(x => x.Id == id);
            var tabIndex = model.Type == "pd" ? "3" : model.Type == "c" ? "2" : "0";
            try
            {
                _db.FacultyMembers.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }

            return RedirectToAction(nameof(Details), new { id = model.FacultyId, returnUrl = returnUrl, tabIndex });
        }

        [PermissionAuthorize("Faculty", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            Faculty model = Find(id);
            try
            {
                _db.Faculties.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }

            return RedirectToAction(nameof(Index));
        }

        private Faculty Find(long? id) 
        {
            var model = _db.Faculties.IgnoreQueryFilters()
                                     .SingleOrDefault(x => x.Id == id);
            return model;
        }

        private void CreateSelectList()
        {
            ViewBag.Instructors = _selectListProvider.GetInstructors();
            ViewBag.FilterCourseGroups = _selectListProvider.GetFilterCourseGroups();
            ViewBag.FilterCurriculumVersionGroups = _selectListProvider.GetFilterCurriculumVersionGroups();
            ViewBag.ProgramDirectorTypes = _selectListProvider.GetProgramDirectorTypes();
        }
    }
}