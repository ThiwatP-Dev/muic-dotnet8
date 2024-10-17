using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.Curriculums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    [PermissionAuthorize("Curriculum", "")]
    public class CurriculumVersionController : BaseController
    {
        protected readonly ICurriculumProvider _curriculumProvider;
        protected readonly IPrerequisiteProvider _prerequisiteProvider;

        public CurriculumVersionController(ApplicationDbContext db,
                                           IFlashMessage flashMessage,
                                           ISelectListProvider selectListProvider,
                                           ICurriculumProvider curriculumProvider,
                                           IPrerequisiteProvider prerequisiteProvider) : base(db, flashMessage, selectListProvider)
        {
            _curriculumProvider = curriculumProvider;
            _prerequisiteProvider = prerequisiteProvider;
        }

        public IActionResult Details(long id, long minorId, long concentrationId, string returnUrl, string tabIndex)
        {
            ViewBag.ReturnUrl = returnUrl;
            var model = new CurriculumVersionViewModel();
            model.MinorId = minorId;
            model.ConcentrationId = concentrationId;
            model.CurriculumVersionId = id;
            model.CurriculumVersion = _curriculumProvider.GetCurriculumVersion(id);
            CreateSelectList(model.CurriculumVersion.Curriculum.FacultyId, model.CurriculumVersion.Curriculum.AcademicLevelId, model.CurriculumVersion.Id);
            CreateSelectListDependency();
            model.CurriculumVersion.CourseGroups = _curriculumProvider.GetCourseGroups(id, model.MinorId, model.ConcentrationId);
            model.CurriculumVersion.PlanGroups = _curriculumProvider.GetStudyPlansByCurriculumVersion(id);
            model.CurriculumVersion.CurriculumSpecializationGroups = _curriculumProvider.GetSpecializationGroupsByCurriculumVersion(id);
            model.Corequisites = _curriculumProvider.GetCurriculumCorequisites(model.CurriculumVersion.Id);
            model.CourseEquivalents = _curriculumProvider.GetCurriculumCourseEquivalents(model.CurriculumVersion.Id);

            return View("~/Views/Curriculum/Version/Details.cshtml", model);
        }

        [PermissionAuthorize("Curriculum", PolicyGenerator.Write)]
        public IActionResult CreateCopyVersion(long academicLevelId, long masterCurriculumId, long curriculumId, long curriculumVersionId, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.Curriculums = _selectListProvider.GetCurriculumByAcademicLevelId(academicLevelId);
            ViewBag.CurriculumVersions = _selectListProvider.GetCurriculumVersion(masterCurriculumId);
            CopyCurriculumViewModel model = new CopyCurriculumViewModel
                                            {
                                                AcademicLevelId = academicLevelId,
                                                CurriculumId = curriculumId,
                                                CurriculumVersionId = curriculumVersionId,
                                                MasterCurriculumId = masterCurriculumId
                                            };

            if (curriculumId == 0 && curriculumVersionId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View("~/Views/Curriculum/Version/CreateCopyVersion.cshtml", model);
            }

            var curriculum = _curriculumProvider.GetCurriculum(masterCurriculumId);
            CreateSelectList(curriculum.FacultyId, academicLevelId);
            model.AcademicLevelId = curriculum.AcademicLevelId;
            model.Version = _curriculumProvider.GetCurriculumVersion(curriculumVersionId);
            if (model.Version != null)
            {
                model.Version.ApprovedDate = DateTime.Now;
            }
            model.CourseGroup = _curriculumProvider.GetParentCourseGroupsByVersionId(curriculumVersionId);

            CreateSelectList(0, curriculum.AcademicLevelId);
            return View("~/Views/Curriculum/Version/CreateCopyVersion.cshtml", model);
        }

        [PermissionAuthorize("Curriculum", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateCopyVersion(CopyCurriculumViewModel model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var curriculumVersion = model.Version;
            curriculumVersion.CurriculumId = model.CurriculumId;
            if (model.Version.StartBatch.HasValue && model.Version.EndBatch.HasValue)
            {
                if(model.Version.StartBatch > model.Version.EndBatch)
                {
                    _flashMessage.Danger(Message.InvalidBatch);
                    return View("~/Views/Curriculum/Version/CreateCopyVersion.cshtml", model);
                }
            }
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    _db.CurriculumVersions.Add(curriculumVersion);
                    _db.SaveChanges();

                    _curriculumProvider.CopyCurriculumVersion(curriculumVersion.Id, model.CurriculumVersionId, model.CourseGroup, 
                        model.IsCopyPrerequisite, model.IsCopySpecializeGroup, model.IsCopyBlacklistCourses, model.IsCopyCoRequisiteAndCourseEquivalent);

                    _db.CurriculumInstructor.AddRange(_curriculumProvider.SetCurriculumInstructor(curriculumVersion.CurriculumInstructorIds, "c", curriculumVersion.Id));
                    _db.CurriculumInstructor.AddRange(_curriculumProvider.SetCurriculumInstructor(curriculumVersion.ThesisInstructorIds, "t", curriculumVersion.Id));
                    _db.CurriculumInstructor.AddRange(_curriculumProvider.SetCurriculumInstructor(curriculumVersion.InstructorIds, "i", curriculumVersion.Id));
                    _db.SaveChanges();

                    transaction.Commit();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(CurriculumController.Details),
                                            nameof(Curriculum), new { id = model.CurriculumId, returnUrl = returnUrl });
                }
                catch 
                {
                    transaction.Rollback();
                    _flashMessage.Danger(Message.UnableToCreate);
                    CreateSelectList(0, model.AcademicLevelId);
                    return View("~/Views/Curriculum/Version/CreateCopyVersion.cshtml", model);
                }
            }
        }

        [PermissionAuthorize("Curriculum", PolicyGenerator.Write)]
        public ActionResult Create(long curriculumId, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var curriculum = _curriculumProvider.GetCurriculum(curriculumId);
            CreateSelectList(curriculum.FacultyId, curriculum.AcademicLevelId);
            var version = new CurriculumVersion()
            {
                CurriculumId = curriculumId,
                ApprovedDate = DateTime.Now,
                InstructorIds = new List<long>(),
                CurriculumInstructorIds = new List<long>(),
                ThesisInstructorIds = new List<long>()
            };

            return View("~/Views/Curriculum/Version/Create.cshtml", version);
        }

        [PermissionAuthorize("Curriculum", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CurriculumVersion model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList(model.Curriculum.FacultyId, model.Curriculum.AcademicLevelId);
            var curriculum = _curriculumProvider.GetCurriculum(model.CurriculumId);
            if (ModelState.IsValid)
            {
                if (!String.IsNullOrEmpty(model.Code) && _curriculumProvider.IsExistCurriculumCode(model.Code))
                {
                    _flashMessage.Danger(Message.IDUniqueConstraintError);
                    return View("~/Views/Curriculum/Version/Create.cshtml", model);
                }

                if (model.StartBatch.HasValue && model.EndBatch.HasValue)
                {
                    if(model.StartBatch > model.EndBatch)
                    {
                        _flashMessage.Danger(Message.InvalidBatch);
                        return View("~/Views/Curriculum/Version/Create.cshtml", model);
                    }
                }

                // if (model.CurriculumInstructorIds == null)
                // {
                //     _flashMessage.Danger(Message.AddInstructor);
                //     return View("~/Views/Curriculum/Version/Edit.cshtml", model);
                // }

                using (var transaction = _db.Database.BeginTransaction())
                {
                    try
                    {
                        _db.CurriculumVersions.Add(model);
                        _db.SaveChanges();

                        _db.CurriculumInstructor.AddRange(_curriculumProvider.SetCurriculumInstructor(model.CurriculumInstructorIds, "c", model.Id));
                        _db.CurriculumInstructor.AddRange(_curriculumProvider.SetCurriculumInstructor(model.ThesisInstructorIds, "t", model.Id));
                        _db.CurriculumInstructor.AddRange(_curriculumProvider.SetCurriculumInstructor(model.InstructorIds, "i", model.Id));
                        _db.SaveChanges();

                        transaction.Commit();
                        _flashMessage.Confirmation(Message.SaveSucceed);
                        return RedirectToAction(nameof(CurriculumController.Details),
                                                nameof(Curriculum), new { id = model.CurriculumId, returnUrl = returnUrl });
                    }
                    catch
                    {
                        transaction.Rollback();
                        _flashMessage.Danger(Message.UnableToCreate);
                        return View("~/Views/Curriculum/Version/Create.cshtml", model);
                    }
                }
            }

            _flashMessage.Danger(Message.UnableToCreate);
            return View("~/Views/Curriculum/Version/Create.cshtml", model);
        }

        public ActionResult Edit(long id, string returnUrl)
        {
            var version = _curriculumProvider.GetCurriculumVersion(id);
            version.CurriculumInstructorIds = version.CurriculumInstructors.Where(x => x.Type == "c")
                                                                           .Select(x => x.InstructorId)
                                                                           .ToList();

            version.ThesisInstructorIds = version.CurriculumInstructors.Where(x => x.Type == "t")
                                                                       .Select(x => x.InstructorId)
                                                                       .ToList();

            version.InstructorIds = version.CurriculumInstructors.Where(x => x.Type == "i")
                                                                 .Select(x => x.InstructorId)
                                                                 .ToList();

            CreateSelectList(version.Curriculum.FacultyId, version.Curriculum.AcademicLevelId, version.Id);
            ViewBag.ReturnUrl = returnUrl;
            return View("~/Views/Curriculum/Version/Edit.cshtml", version);
        }

        [PermissionAuthorize("Curriculum", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(CurriculumVersion model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var modelToUpdate = _curriculumProvider.GetCurriculumVersion(model.Id);
            CreateSelectList(modelToUpdate.Curriculum.FacultyId, modelToUpdate.Curriculum.AcademicLevelId);

            if (ModelState.IsValid)
            {
                // if (!String.IsNullOrEmpty(model.Code) && model.Code != modelToUpdate.Code
                //     && _curriculumProvider.IsExistCurriculumCode(model.Code))
                // {
                //     _flashMessage.Danger(Message.IDUniqueConstraintError);
                //     return View("~/Views/Curriculum/Version/Edit.cshtml", model);
                // }
                if (model.StartBatch.HasValue && model.EndBatch.HasValue)
                {
                    if(model.StartBatch > model.EndBatch)
                    {
                        _flashMessage.Danger(Message.InvalidBatch);
                        return View("~/Views/Curriculum/Version/Edit.cshtml", model);
                    }
                }
                using (var transaction = _db.Database.BeginTransaction())
                {
                    try
                    {
                        await TryUpdateModelAsync<CurriculumVersion>(modelToUpdate);

                        // if (model.CurriculumInstructorIds == null)
                        // {
                        //     _flashMessage.Danger(Message.AddInstructor);
                        //     return View("~/Views/Curriculum/Version/Edit.cshtml", model);
                        // }

                        var previousInstructors = _curriculumProvider.GetCurriculumInstructors(model.Id);
                        var instructorToUpdate = UnionInstructors(model);
                        var removeInstructor = previousInstructors.Select(x => new { x.InstructorId, x.Type })
                                                                  .Except(instructorToUpdate.Select(y => new { y.InstructorId, y.Type }))
                                                                  .Select(x => new CurriculumInstructor()
                                                                  {
                                                                      CurriculumVersionId = model.Id,
                                                                      InstructorId = x.InstructorId,
                                                                      Type = x.Type
                                                                  })
                                                                  .ToList();

                        var insertInstructor = instructorToUpdate.Select(x => new { x.InstructorId, x.Type })
                                                                 .Except(previousInstructors.Select(y => new { y.InstructorId, y.Type }))
                                                                 .Select(x => new CurriculumInstructor()
                                                                 {
                                                                     CurriculumVersionId = model.Id,
                                                                     InstructorId = x.InstructorId,
                                                                     Type = x.Type
                                                                 })
                                                                 .ToList();

                        foreach (var item in removeInstructor)
                        {
                            var remove = previousInstructors.SingleOrDefault(x => x.CurriculumVersionId == item.CurriculumVersionId
                                                                                  && x.Type == item.Type
                                                                                  && x.InstructorId == item.InstructorId);

                            _db.CurriculumInstructor.Remove(remove);
                        }

                        _db.CurriculumInstructor.AddRange(insertInstructor);
                        await _db.SaveChangesAsync();

                        transaction.Commit();
                        _flashMessage.Confirmation(Message.SaveSucceed);
                        return RedirectToAction(nameof(CurriculumController.Details),
                                                nameof(Curriculum), new { id = model.CurriculumId, returnUrl = returnUrl });
                    }
                    catch
                    {
                        transaction.Rollback();
                        _flashMessage.Danger(Message.UnableToEdit);
                        return View("~/Views/Curriculum/Version/Edit.cshtml", model);
                    }
                }
            }

            _flashMessage.Danger(Message.UnableToEdit);
            return View("~/Views/Curriculum/Version/Edit.cshtml", model);
        }

        [PermissionAuthorize("Curriculum", PolicyGenerator.Write)]
        public ActionResult Delete(long id, string returnUrl)
        {
            CurriculumVersion model = Find(id);
            var curriculumInstructors = _db.CurriculumInstructor.Where(x => x.CurriculumVersionId == id).ToList();
            var curriculumCourses = _db.CurriculumCourses.Where(x => x.CourseGroup.CurriculumVersion.Curriculum.Id == id).ToList();
            var courseGroups = _db.CourseGroups.Where(x => x.CurriculumVersionId == id).ToList();
            var studyPlans = _db.StudyPlans.Where(x => x.CurriculumVersionId == id).ToList();
            var studyCourses = _db.StudyCourses.Where(x => x.StudyPlan.CurriculumVersion.Curriculum.Id == id).ToList();
            if (model == null)
            {
                _flashMessage.Danger(Message.UnableToDelete);
                return RedirectToAction("Details", "Curriculum", new { id = model.CurriculumId, returnUrl = returnUrl });
            }

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    _db.CurriculumInstructor.RemoveRange(curriculumInstructors);
                    _db.CurriculumCourses.RemoveRange(curriculumCourses);
                    _db.CourseGroups.RemoveRange(courseGroups);
                    _db.StudyCourses.RemoveRange(studyCourses);
                    _db.StudyPlans.RemoveRange(studyPlans);
                    _db.CurriculumVersions.Remove(model);
                    _db.SaveChanges();

                    transaction.Commit();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction("Details", "Curriculum", new { id = model.CurriculumId, returnUrl = returnUrl });
                }
                catch
                {
                    transaction.Rollback();
                    _flashMessage.Danger(Message.UnableToDelete);
                    return RedirectToAction("Details", "Curriculum", new { id = model.CurriculumId, returnUrl = returnUrl });
                }
            }
        }

        [PermissionAuthorize("Curriculum", PolicyGenerator.Write)]
        public ActionResult SaveBlacklistCourses(CurriculumVersion model, string returnUrl, string type = "")
        {
            if(type == "export")
            {
                if (model != null)
                {
                    var curriculumVersion = _curriculumProvider.GetCurriculumVersion(model.Id);
                    using (var wb = GenerateWorkBook(curriculumVersion))
                    {
                        return wb.Deliver($"{ curriculumVersion.CodeAndName } Blacklist Courses Report.xlsx");
                    }
                }

                return Redirect(returnUrl);
            }
            else
            {
                try
                {
                    var deletedCourses = _db.CurriculumBlacklistCourses.Where(x => x.CurriculumVersionId == model.Id)
                                                                    .IgnoreQueryFilters()
                                                                    .ToList();

                    _db.CurriculumBlacklistCourses.RemoveRange(deletedCourses);

                    if (model.CurriculumBlacklistCourses != null)
                    {
                        var curriculumBlacklistCourses = model.CurriculumBlacklistCourses.Where(x => x.CourseId != 0)
                                                                                        .GroupBy(x => x.CourseId)
                                                                                        .Select(x => new CurriculumBlacklistCourse
                                                                                                    {
                                                                                                        CourseId = x.First().CourseId,
                                                                                                        CurriculumVersionId = model.Id
                                                                                                    })
                                                                                        .ToList();
                        
                        foreach (var item in curriculumBlacklistCourses)
                        {
                            _db.CurriculumBlacklistCourses.Add(item);
                        }
                    }

                    _db.SaveChanges();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                return RedirectToAction("Details", "CurriculumVersion", new { id = model.Id, returnUrl = returnUrl, tabIndex = "4" });
                }
                catch
                {
                    _flashMessage.Danger(Message.UnableToSave);
                return RedirectToAction("Details", "CurriculumVersion", new { id = model.Id, returnUrl = returnUrl, tabIndex = "4" });
                }
            }
        }

        private XLWorkbook GenerateWorkBook(CurriculumVersion model)
        {
            var wb = new XLWorkbook();
            var ws = wb.AddWorksheet();
            int row = 1;
            var column = 1;

            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column).Style.Font.Bold = true;
            ws.Cell(row, column).Style.Fill.BackgroundColor = XLColor.FromArgb(184, 204, 228);
            ws.Cell(row, column).SetValue<string>($"Blacklist Courses");
            ws.Range(ws.Cell(row, column), ws.Cell(row, 4)).Merge();
            row++;
            column = 1;

            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column).Style.Font.Bold = true;
            ws.Cell(row, column).Style.Fill.BackgroundColor = XLColor.FromArgb(184, 204, 228);
            ws.Cell(row, column).SetValue<string>($"Curriculum : { model.Curriculum.NameEn }");
            ws.Range(ws.Cell(row, column), ws.Cell(row, 4)).Merge();
            row++;
            column = 1;
            
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column).Style.Font.Bold = true;
            ws.Cell(row, column).Style.Fill.BackgroundColor = XLColor.FromArgb(184, 204, 228);
            ws.Cell(row, column).SetValue<string>($"Curriculum Version : { model.CodeAndName }");
            ws.Range(ws.Cell(row, column), ws.Cell(row, 4)).Merge();
            row++;
            column = 1;

            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column).Style.Font.Bold = true;
            ws.Cell(row, column).Style.Fill.BackgroundColor = XLColor.FromArgb(184, 204, 228);
            ws.Cell(row, column++).Value = "Course Code".ToUpper();

            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Cell(row, column).Style.Font.Bold = true;
            ws.Cell(row, column).Style.Fill.BackgroundColor = XLColor.FromArgb(184, 204, 228);
            ws.Cell(row, column++).Value = "Course Name En".ToUpper();

            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Cell(row, column).Style.Font.Bold = true;
            ws.Cell(row, column).Style.Fill.BackgroundColor = XLColor.FromArgb(184, 204, 228);
            ws.Cell(row, column++).Value = "Course Name Th".ToUpper();

            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column).Style.Font.Bold = true;
            ws.Cell(row, column).Style.Fill.BackgroundColor = XLColor.FromArgb(184, 204, 228);
            ws.Cell(row, column++).Value = "Course Credit".ToUpper();


            foreach (var item in model.CurriculumBlacklistCourses)
            {
                    row++;
                    column = 1;
                    ws.Cell(row, column).SetValue<string>(item.Course.Code);
                    ws.Cell(row, column++).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;


                    ws.Cell(row, column).Value = item.Course.NameEn;
                    ws.Cell(row, column++).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                    ws.Cell(row, column).Value = item.Course.NameTh;
                    ws.Cell(row, column++).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                    ws.Cell(row, column).Value = item.Course.CreditText;
                    ws.Cell(row, column++).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }
            ws.Columns().AdjustToContents();            
            ws.Rows().AdjustToContents();
            return wb;
        }

        private CurriculumVersion Find(long? id)
        {
            var curriculumVersion = _db.CurriculumVersions.IgnoreQueryFilters()
                                                          .SingleOrDefault(x => x.Id == id);
            return curriculumVersion;
        }

        [PermissionAuthorize("Curriculum", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveSpecializationGroup(CurriculumVersion model, string returnUrl) 
        {
            var curriculumSpecializationGroups = model.CurriculumSpecializationGroups.Where(x => x.SpecializationGroupId != 0)
                                                             .GroupBy(x => x.SpecializationGroupId)
                                                             .Select(x => x.First())
                                                             .ToList();

            var removeSpecializationgroup = _db.CurriculumSpecializationGroups.Where(x => x.CurriculumVersionId == model.Id)
                                                                              .ToList();

            var result = new List<CurriculumSpecializationGroup>();
            try
            {
                foreach (var item in curriculumSpecializationGroups)
                {
                    result.Add(new CurriculumSpecializationGroup
                               {
                                   SpecializationGroupId = item.SpecializationGroupId,
                                   CurriculumVersionId = model.Id
                               });
                }
                _db.CurriculumSpecializationGroups.RemoveRange(removeSpecializationgroup);
                _db.CurriculumSpecializationGroups.AddRange(result);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToEdit);
            }

            return RedirectToAction("Details", "CurriculumVersion", new { id = model.Id, returnUrl = returnUrl, tabIndex = "3" });
        }

        [PermissionAuthorize("Curriculum", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateCorequisite(CurriculumVersionViewModel model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectListDependency();
            var curriculumVersion = _curriculumProvider.GetCurriculumVersion(model.CurriculumVersionId);
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var deletedCourses = _db.CurriculumDependencies.Where(x => x.CurriculumVersionId == model.CurriculumVersionId
                                                                               && x.DependencyType == "Corequisite")
                                                                   .IgnoreQueryFilters()
                                                                   .ToList();

                    _db.CurriculumDependencies.RemoveRange(deletedCourses);
                    foreach (var item in model.Corequisites)
                    {
                        _db.CurriculumDependencies.Add(new CurriculumDependency
                                                       {
                                                           CurriculumVersionId = model.CurriculumVersionId,
                                                           DependencyType = "Corequisite",
                                                           DependencyId = item.CorequisiteId
                                                       });
                    }

                    _db.SaveChanges();
                    transaction.Commit();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction("Details", "CurriculumVersion", new { id = curriculumVersion.Id, returnUrl = returnUrl, tabIndex = "5" });
                }
                catch
                {
                    CreateSelectListDependency();
                    transaction.Rollback();
                    _flashMessage.Danger(Message.UnableToCreate);
                    return RedirectToAction("Details", "CurriculumVersion", new { id = curriculumVersion.Id, returnUrl = returnUrl, tabIndex = "5" });
                }
            }
        }

        [PermissionAuthorize("Curriculum", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateCourseEquivalent(CurriculumVersionViewModel model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectListDependency();
            var curriculumVersion = _curriculumProvider.GetCurriculumVersion(model.CurriculumVersionId);
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var deletedCourses = _db.CurriculumDependencies.Where(x => x.CurriculumVersionId == model.CurriculumVersionId
                                                                               && x.DependencyType == "Equivalence")
                                                                   .IgnoreQueryFilters()
                                                                   .ToList();

                    _db.CurriculumDependencies.RemoveRange(deletedCourses);
                    foreach (var item in model.CourseEquivalents)
                    {
                        _db.CurriculumDependencies.Add(new CurriculumDependency
                                                       {
                                                           CurriculumVersionId = model.CurriculumVersionId,
                                                           DependencyType = "Equivalence",
                                                           DependencyId = item.CourseEquivalentId
                                                       });
                    }

                    _db.SaveChanges();
                    transaction.Commit();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction("Details", "CurriculumVersion", new { id = curriculumVersion.Id, returnUrl = returnUrl, tabIndex = "6" });
                }
                catch
                {
                    CreateSelectListDependency();
                    transaction.Rollback();
                    _flashMessage.Danger(Message.UnableToCreate);
                    return RedirectToAction("Details", "CurriculumVersion", new { id = curriculumVersion.Id, returnUrl = returnUrl, tabIndex = "6" });
                }
            }
        }

        private void CreateSelectList(long facultyId, long academicLevelId, long curriculumVersionId = 0)
        {
            ViewBag.AcademicPrograms = _selectListProvider.GetAcademicProgramsByAcademicLevelId(academicLevelId);
            ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            ViewBag.Instructors = _selectListProvider.GetInstructorsByFacultyId(facultyId);
            ViewBag.Minors = _selectListProvider.GetMinorsByCurriculumVersionId(curriculumVersionId);
            ViewBag.Concentrations = _selectListProvider.GetConcentrationsByCurriculumVersionId(curriculumVersionId);
            ViewBag.Courses = _selectListProvider.GetCourses();
            ViewBag.SpecializationGroups = _selectListProvider.GetSpecializationGroups();
        }

        private void CreateSelectListDependency()
        {
            ViewBag.DependencyTypes = _selectListProvider.GetDependencyTypes();
            ViewBag.Corequisites = _selectListProvider.GetCorequisites();
            ViewBag.CourseEquivalents = _selectListProvider.GetCourseEquivalents();
        }

        private List<CurriculumInstructor> UnionInstructors(CurriculumVersion version)
        {
            var instructors = _curriculumProvider.SetCurriculumInstructor(version.CurriculumInstructorIds, "c", version.Id);
            instructors = instructors.Union(_curriculumProvider.SetCurriculumInstructor(version.ThesisInstructorIds, "t", version.Id))
                                     .ToList();
            instructors = instructors.Union(_curriculumProvider.SetCurriculumInstructor(version.InstructorIds, "i", version.Id))
                                     .ToList();

            return instructors;
        }

        [HttpGet]
        public string ExportCurriculumByFacultyAndDepartment(long facultyId, long departmentId)
        {
            return _curriculumProvider.ExportCurriculum(facultyId, departmentId);
        }
    }
}