using AutoMapper;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Admission;
using KeystoneLibrary.Models.DataModels.Curriculums;
using KeystoneLibrary.Models.DataModels.Profile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;
using KeystoneLibrary.Enumeration;
using KeystoneLibrary.Helpers;
using KeystoneLibrary.Models.Enums;
using KeystoneLibrary.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using KeystoneLibrary.Models.DataModels;
using Keystone.Permission;

namespace Keystone.Controllers
{
    public class StudentController : BaseController
    {
        protected readonly IStudentProvider _studentProvider;
        protected readonly ICacheProvider _cacheProvider;
        protected readonly IAdmissionProvider _admissionProvider;
        protected readonly IAcademicProvider _academicProvider;
        protected readonly IScholarshipProvider _scholarshipProvider;
        protected readonly IFileProvider _fileProvider;
        protected readonly IStudentPhotoProvider _studentPhotoProvider;
        protected readonly IRegistrationProvider _registrationProvider;
        protected readonly IUserProvider _userProvider;
        private readonly UserManager<ApplicationUser> _userManager;
        public StudentController(ApplicationDbContext db,
                                 IFlashMessage flashMessage,
                                 ISelectListProvider selectListProvider,
                                 IMapper mapper,
                                 IStudentProvider studentProvider,
                                 ICacheProvider cacheProvider,
                                 IAdmissionProvider admissionProvider,
                                 IAcademicProvider academicProvider,
                                 IScholarshipProvider scholarshipProvider,
                                 IRegistrationProvider registrationProvider,
                                 IUserProvider userProvider,
                                 IFileProvider fileProvider,
                                 IStudentPhotoProvider studentPhotoProvider,
                                 UserManager<ApplicationUser> userManager) : base(db, flashMessage, mapper, selectListProvider)
        {
            _studentProvider = studentProvider;
            _cacheProvider = cacheProvider;
            _admissionProvider = admissionProvider;
            _academicProvider = academicProvider;
            _scholarshipProvider = scholarshipProvider;
            _fileProvider = fileProvider;
            _studentPhotoProvider = studentPhotoProvider;
            _registrationProvider = registrationProvider;
            _userProvider = userProvider;
            _userManager = userManager;
        }

        [PermissionAuthorize("StudentDetails", "")]
        [Route("Student/Details", Name = "FindStudentByCode")]
        public async Task<IActionResult> Details(string code, string tabIndex, string returnUrl)
        {
            //_studentProvider.UpdateGradeComp();

            Student model = new Student();
            model.AcademicInformation = new AcademicInformation();
            model.StudentAddresses = new List<StudentAddress>();
            model.MaintenanceStatuses = new List<MaintenanceStatus>();
            model.StudentRequiredDocument = new StudentRequiredDocument();
            model.StudentExemptedExamScores = new List<StudentExemptedExamScore>();

            if (!String.IsNullOrEmpty(code))
            {
                if (_studentProvider.IsExistStudentExceptAdmission(code))
                {
                    model = _studentProvider.GetStudentInformationByCode(code);

                    var userTabPermissions = await CheckTabPermissions();
                    model.TabPermissions = userTabPermissions;

                    model.GraduationInformation = model.GraduationInformations.FirstOrDefault();
                    model.StudentRequiredDocument = _studentProvider.GetStudentRequiredDocument(model);
                    model.StudentRegistrationCourseViewModels = _studentProvider.GetStudentRegistrationCourseViewModel(null, code);
                    model.StudentRegistrationCoursesViewModels.TransferCourse = _studentProvider.GetStudentRegistrationCourseTranferViewModel(null, code);
                    model.StudentRegistrationCoursesViewModels.TransferCourseWithGrade = _studentProvider.GetStudentRegistrationCourseTranferWithGradeViewModel(null, code);
                    model.StudentRegistrationCoursesViewModels.TranscriptGrade = model.StudentRegistrationCourseViewModels;
                    model.IsStudentExtended = _studentProvider.IsStudentExtended(null, code);
                    model.ScholarshipStudent = _scholarshipProvider.GetCurrentScholarshipStudent(model.Id);
                    model.StudentIncidentLogs = _studentProvider.GetStudentIncidentLogsByStudentId(model.Id);

                    var currentTerm = _cacheProvider.GetCurrentTerm(model.AcademicInformation?.AcademicLevelId ?? 0);
                    var probations = _studentProvider.GetCurrentStudentProbation(model.Id, currentTerm.Id);
                    model.IsCurrentStudentProbation = probations != null && probations.Any();
                    if (model != null && model.AdmissionInformation != null)
                    {
                        model.AdmissionInformation.StudentExemptedExamScores = _studentProvider.GetStudentExemptedExamScore(model.Id);
                        model.AdmissionInformation.AcademicLevelId = model.AdmissionInformation.AdmissionTerm?.AcademicLevelId ?? 0;
                    }

                    if (model.StudentIncidents.Any())
                    {
                        var user = GetUser();
                        var userIds = model.StudentIncidents.Select(x => x.CreatedBy).ToList();
                        var userNames = _userProvider.GetCreatedFullNameByIds(userIds);
                        foreach (var item in model.StudentIncidents)
                        {
                            item.UserId = user.Id;
                            item.CreatedByFullNameEn = userNames.Where(x => x.CreatedBy == item.CreatedBy).FirstOrDefault().CreatedByFullNameEn;
                        }
                        model.IsRegistrationLock = model.StudentIncidents.Any(x => x.IsActive && x.LockedRegistration);
                        model.IsPaymentLock =  model.StudentIncidents.Any(x => x.IsActive && x.LockedPayment);
                        model.IsSignInLock = model.StudentIncidents.Any(x => x.IsActive && x.LockedSignIn);
                    }

                    try
                    {
                        model.ProfileImageURL = await _studentPhotoProvider.GetStudentImg(model.Code);
                    }
                    catch (Exception) { }

                    _studentProvider.UpdateCGPA(model.Id);
                    CreateSelectList(model);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, Message.StudentNotFound);
                }
            }

            CreateSelectList(model.BirthCountryId ?? 0, model.BirthStateId ?? 0);
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        [PermissionAuthorize("StudentDetails", "")]
        [Route("Student/Details/{id:Guid}")]
        public async Task<IActionResult> Details(Guid id, string tabIndex, string returnUrl)
        {
            Student model = new Student();
            model.StudentAddresses = new List<StudentAddress>();
            model.MaintenanceStatuses = new List<MaintenanceStatus>();
            model.StudentRequiredDocument = new StudentRequiredDocument();
            model.StudentExemptedExamScores = new List<StudentExemptedExamScore>();

            if (id != Guid.Empty)
            {
                if (_studentProvider.IsExistStudentExceptAdmission(id))
                {
                    model = _studentProvider.GetStudentInformationById(id);

                    var userTabPermissions = await CheckTabPermissions();
                    model.TabPermissions = userTabPermissions;

                    model.StudentRequiredDocument = _studentProvider.GetStudentRequiredDocument(model);
                    model.StudentRegistrationCourseViewModels = _studentProvider.GetStudentRegistrationCourseViewModel(id, null);
                    model.StudentRegistrationCoursesViewModels.TransferCourse = _studentProvider.GetStudentRegistrationCourseTranferViewModel(id, null);
                    model.StudentRegistrationCoursesViewModels.TransferCourseWithGrade = _studentProvider.GetStudentRegistrationCourseTranferWithGradeViewModel(id, null);
                    model.StudentRegistrationCoursesViewModels.TranscriptGrade = model.StudentRegistrationCourseViewModels;
                    model.IsStudentExtended = _studentProvider.IsStudentExtended(id, null);
                    model.ScholarshipStudent = _scholarshipProvider.GetCurrentScholarshipStudent(model.Id);
                    model.StudentIncidentLogs = _studentProvider.GetStudentIncidentLogsByStudentId(model.Id);

                    var currentTerm = _cacheProvider.GetCurrentTerm(model.AcademicInformation?.AcademicLevelId ?? 0);
                    var probations = _studentProvider.GetCurrentStudentProbation(model.Id, currentTerm.Id);
                    model.IsCurrentStudentProbation = probations != null && probations.Any();
                    if (model.AdmissionInformation != null)
                    {
                        model.AdmissionInformation.AcademicLevelId = model.AcademicInformation.AcademicLevelId;
                        model.AdmissionInformation.StudentExemptedExamScores = _studentProvider.GetStudentExemptedExamScore(model.Id);
                    }
                    if (model.StudentIncidents.Any())
                    {
                        var user = GetUser();
                        var userIds = model.StudentIncidents.Select(x => x.CreatedBy).ToList();
                        var userNames = _userProvider.GetCreatedFullNameByIds(userIds);
                        foreach (var item in model.StudentIncidents)
                        {
                            item.UserId = user.Id;
                            item.CreatedByFullNameEn = userNames.Where(x => x.CreatedBy == item.CreatedBy).FirstOrDefault().CreatedByFullNameEn;
                        }
                        model.IsRegistrationLock = model.StudentIncidents.Any(x => x.IsActive && x.LockedRegistration);
                        model.IsPaymentLock =  model.StudentIncidents.Any(x => x.IsActive && x.LockedPayment);
                        model.IsSignInLock = model.StudentIncidents.Any(x => x.IsActive && x.LockedSignIn);
                    }

                    try
                    {
                        model.ProfileImageURL = await _studentPhotoProvider.GetStudentImg(model.Code);
                    }
                    catch (Exception) { }

                    _studentProvider.UpdateCGPA(model.Id);
                    CreateSelectList(model);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, Message.StudentNotFound);
                }
            }

            CreateSelectList(model.BirthCountryId ?? 0, model.BirthStateId ?? 0);
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        private async Task<List<UserTabPermissionViewModel>> CheckTabPermissions()
        {
            var user = GetUser();
            var userRoles = await _userManager.GetRolesAsync(user);
            var roleIds = _db.Roles.AsNoTracking()
                                   .Where(x => userRoles.Contains(x.Name))
                                   .Select(x => x.Id)
                                   .ToList();

            bool IsAdmin = userRoles.Any(x => x.Contains("Admin"));
            List<UserTabPermissionViewModel> userTabPermissions = new List<UserTabPermissionViewModel>();

            var studentProfileMenu = _db.Menus.AsNoTracking()
                                              .SingleOrDefault(x => x.Url.Equals("/Student/Details", StringComparison.OrdinalIgnoreCase));
            if (studentProfileMenu != null)
            {
                var userPermissions = _db.TabPermissions.AsNoTracking()
                                                       .Where(x => x.Tab.MenuId == studentProfileMenu.Id
                                                                   && (x.UserId == user.Id
                                                                       //|| roleIds.Contains(x.RoleId) //TODO: Role Permission of Tab still not work
                                                                       )
                                                                   && (x.IsReadable || x.IsWritable))
                                                       .ToList();
                var menuPermission = _db.MenuPermissions.AsNoTracking()
                                                        .FirstOrDefault(x => x.MenuId == studentProfileMenu.Id
                                                                                 && roleIds.Contains(x.RoleId));
                var tabs = _db.Tabs.AsNoTracking()
                                   .Where(x => x.MenuId == studentProfileMenu.Id)
                                   .ToList();

                if (tabs != null && tabs.Any())
                {
                    foreach (var tab in tabs)
                    {
                        var usertab = new UserTabPermissionViewModel();
                        usertab.Tab = tab.TitleEn;

                        if (IsAdmin)
                        {
                            usertab.IsReadable = true;
                            usertab.IsWritable = true;
                            userTabPermissions.Add(usertab);
                        }
                        else
                        {
                            var permission = userPermissions.FirstOrDefault(x => x.TabId == tab.Id
                                                                                && x.IsReadable);
                            if (permission != null)
                            {
                                usertab.IsReadable = permission.IsReadable;
                                usertab.IsWritable = permission.IsWritable;
                                userTabPermissions.Add(usertab);
                            }
                            else if (menuPermission != null)
                            {
                                usertab.IsReadable = menuPermission.IsReadable;
                                usertab.IsWritable = menuPermission.IsWritable;
                                userTabPermissions.Add(usertab);
                            }
                        }
                    }
                }
            }
            return userTabPermissions;
        }

        private async Task<bool> IsTabWritable(string tabName)
        {
            var user = GetUser();
            var userRoles = await _userManager.GetRolesAsync(user);
            var roleIds = _db.Roles.AsNoTracking()
                                   .Where(x => userRoles.Contains(x.Name))
                                   .Select(x => x.Id)
                                   .ToList();

            bool IsAdmin = userRoles.Any(x => x.Contains("Admin"));

            var studentProfileMenu = _db.Menus.AsNoTracking()
                                              .SingleOrDefault(x => x.Url.Equals("/Student/Details", StringComparison.OrdinalIgnoreCase));
            if (studentProfileMenu != null)
            {
                if (IsAdmin)
                {
                    return true;
                }
                else
                {
                    var tabPermission = _db.TabPermissions.AsNoTracking()
                                                        .Where(x => x.Tab.MenuId == studentProfileMenu.Id
                                                                    && (x.UserId == user.Id
                                                                        //|| roleIds.Contains(x.RoleId)
                                                                        )
                                                                    && x.IsWritable
                                                                    && x.Tab.TitleEn.Equals(tabName, StringComparison.OrdinalIgnoreCase))
                                                        .SingleOrDefault();
                    if (tabPermission != null)
                    {
                        return tabPermission.IsWritable;
                    }

                    var menuPermission = _db.MenuPermissions.AsNoTracking()
                                                  .FirstOrDefault(x => x.MenuId == studentProfileMenu.Id
                                                                           && roleIds.Contains(x.RoleId));
                    if (menuPermission != null)
                    {
                        return menuPermission.IsWritable;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return false;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SaveGeneral(Student model, string currentStudentStatus)
        {
            if (!await IsTabWritable("GENERAL"))
            {
                _flashMessage.Danger(Message.UnableToEditInvalidPermission);
                return RedirectToAction(nameof(Details), new { code = model.Code, tabIndex = "0" });
            }

            if (ModelState.IsValid)
            {
                if (_db.Students.Any(x => x.Code == model.Code))
                {
                    var modelToUpdate = _studentProvider.GetStudentById(model.Id);
                    var currentTerm = _cacheProvider.GetCurrentTerm(modelToUpdate.AcademicInformation.AcademicLevelId);
                    if (await TryUpdateModelAsync<Student>(modelToUpdate))
                    {
                        using (var transaction = _db.Database.BeginTransaction())
                        {
                            try
                            {
                                if (model.StudentStatus != currentStudentStatus)
                                {
                                    var success = _studentProvider.SaveStudentStatusLog(modelToUpdate.Id
                                                                                        , currentTerm.Id
                                                                                        , SaveStatusSouces.GENERAL.GetDisplayName()
                                                                                        , modelToUpdate.StudentRemark
                                                                                        , modelToUpdate.StudentStatus);
                                    if (!success)
                                    {
                                        _flashMessage.Danger(Message.UnableToEdit);
                                        transaction.Rollback();
                                        ModelState.AddModelError(string.Empty, "Unauthorized");
                                    }
                                }

                                await _db.SaveChangesAsync();
                                _flashMessage.Confirmation(Message.SaveSucceed);
                                transaction.Commit();

                                if (model.StudentStatus == "a")
                                {
                                    return RedirectToAction(nameof(Details), "AdmissionStudent", new { codeOrNumber = model.Code });
                                }
                            }
                            catch (Exception e)
                            {
                                _flashMessage.Danger(Message.DatabaseProblem + e.Message);
                                transaction.Rollback();
                                ModelState.AddModelError(string.Empty, "Unauthorized");
                            }
                        }

                        return RedirectToAction(nameof(Details), new { code = model.Code, tabIndex = "0" });
                    }
                }
                else
                {
                    _db.Students.Add(model);
                    await _db.SaveChangesAsync();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Details), new { code = model.Code, tabIndex = "0" });
                }
            }

            //use for debug on deployed program
            var message = string.Join(" | ", ModelState.Values
                                                       .SelectMany(v => v.Errors)
                                                       .Select(e => e.ErrorMessage));

            _flashMessage.Danger($"{ Message.UnableToEdit } { message }");
            ModelState.AddModelError(string.Empty, "Unauthorized");
            return RedirectToAction(nameof(Details), new { code = model.Code, tabIndex = "0" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SaveContactInformation(Student model)
        {
            if (!await IsTabWritable("CONTACT"))
            {
                _flashMessage.Danger(Message.UnableToEditInvalidPermission);
                return RedirectToAction(nameof(Details), new { code = model.Code, tabIndex = "7" });
            }

            if (_db.Students.Any(x => x.Code == model.Code))
            {
                var result = _studentProvider.SaveStudentContact(model);
                if (string.IsNullOrEmpty(result))
                {
                    _flashMessage.Confirmation(Message.SaveSucceed);
                }
                else
                {
                    _flashMessage.Danger(Message.DatabaseProblem + result);
                    ModelState.AddModelError(string.Empty, "Unauthorized");
                }

                return RedirectToAction(nameof(Details), new { code = model.Code, tabIndex = "7" });
            }

            //use for debug on deployed program
            var message = string.Join(" | ", ModelState.Values
                                                       .SelectMany(v => v.Errors)
                                                       .Select(e => e.ErrorMessage));

            _flashMessage.Danger($"{ Message.UnableToEdit } { message }");
            ModelState.AddModelError(string.Empty, "Unauthorized");
            return RedirectToAction(nameof(Details), new { code = model.Code, tabIndex = "7" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SaveAcademicInformation(AcademicInformation model)
        {
            if (! await IsTabWritable("ACADEMIC"))
            {
                _flashMessage.Danger(Message.UnableToEditInvalidPermission);
                return RedirectToAction(nameof(Details), new { id = model.StudentId, tabIndex = "1" });
            }

            if (ModelState.IsValid)
            {
                if (_db.AcademicInformations.Any(x => x.StudentId == model.StudentId))
                {
                    var modelToUpdate = _db.AcademicInformations.SingleOrDefault(x => x.StudentId == model.StudentId);
                    if (await TryUpdateModelAsync<AcademicInformation>(modelToUpdate))
                    {
                        try
                        {
                            await _db.SaveChangesAsync();
                            _flashMessage.Confirmation(Message.SaveSucceed);
                        }
                        catch
                        {
                            _flashMessage.Danger(Message.UnableToEdit);
                        }

                        return RedirectToAction(nameof(Details), new { id = model.StudentId, tabIndex = "1" });
                    }
                }
                else
                {
                    _db.AcademicInformations.Add(model);
                    await _db.SaveChangesAsync();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Details), new { id = model.StudentId, tabIndex = "1" });
                }
            }

            _flashMessage.Danger(Message.UnableToEdit);
            return RedirectToAction(nameof(Details), new { id = model.StudentId, tabIndex = "1" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SaveAdmissionInformation(AdmissionInformation model)
        {
            if (!await IsTabWritable("ADMISSION"))
            {
                _flashMessage.Danger(Message.UnableToEditInvalidPermission);
                return RedirectToAction(nameof(Details), new { id = model.StudentId, tabIndex = "3" });
            }

            if (model.Id != 0) // update same data
            {
                try
                {
                    // add model back to entry
                    _db.AdmissionInformations.Attach(model);

                    // get data from database before modified (https://stackoverflow.com/questions/55784921/get-original-value-of-detached-entity)
                    // var previousDocumentId = _db.Entry(model).GetDatabaseValues().GetValue<long>("AdmissionDocumentGroupId");
                    _db.Entry(model).State = EntityState.Modified;
                    _db.Entry(model).Property(x => x.CreatedAt).IsModified = false;
                    _db.Entry(model).Property(x => x.CreatedBy).IsModified = false;
                    _db.SaveChanges();

                    var update = _studentProvider.UpdateStudentExemptedExamScore(model.StudentExemptedExamScores, model.StudentId);
                    if (!update)
                    {
                        _flashMessage.Danger(Message.UnableToEditExemptedExamScore);
                        return RedirectToAction(nameof(Details), new { id = model.StudentId, tabIndex = "3" });
                    }

                }
                catch
                {
                    _flashMessage.Danger(Message.UnableToEdit);
                    return RedirectToAction(nameof(Details), new { id = model.StudentId, tabIndex = "3" });
                }
            }
            else // add new admission information
            {
                try
                {
                    _db.AdmissionInformations.Add(model);
                    _db.SaveChanges();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                }
                catch
                {
                    _flashMessage.Danger(Message.UnableToCreate);
                    return RedirectToAction(nameof(Details), new { id = model.StudentId, tabIndex = "3" });
                }
            }

            if (model.AdmissionDocumentGroupId != null && model.AdmissionDocumentGroupId != 0
                && model.PreviousDocumentId != model.AdmissionDocumentGroupId)
            {
                var success = _studentProvider.SaveDocumentStudentByDocumentGroup(model.StudentId, model.AdmissionDocumentGroupId ?? 0);
                if (!success)
                {
                    _flashMessage.Danger(Message.UnableToSaveDocument);
                }
            }

            return RedirectToAction(nameof(Details), new { id = model.StudentId, tabIndex = "3" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SaveGraduationInformation(GraduationInformation model)
        {
            if (!await IsTabWritable("GRADUATION"))
            {
                _flashMessage.Danger(Message.UnableToEditInvalidPermission);
                return RedirectToAction(nameof(Details), new { id = model.StudentId, tabIndex = "4" });
            }

            if (ModelState.IsValid)
            {
                if (_db.GraduationInformations.Any(x => x.StudentId == model.StudentId))
                {
                    var modelToUpdate = _db.GraduationInformations.SingleOrDefault(x => x.StudentId == model.StudentId);
                    if (await TryUpdateModelAsync<GraduationInformation>(modelToUpdate))
                    {
                        try
                        {
                            await _db.SaveChangesAsync();
                            _flashMessage.Confirmation(Message.SaveSucceed);
                        }
                        catch
                        {
                            _flashMessage.Danger(Message.UnableToEdit);
                        }

                        return RedirectToAction(nameof(Details), new { id = model.StudentId, tabIndex = "4" });
                    }
                }
                else
                {
                    _db.GraduationInformations.Add(model);
                    await _db.SaveChangesAsync();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Details), new { id = model.StudentId, tabIndex = "4" });
                }
            }

            _flashMessage.Danger(Message.UnableToEdit);
            return RedirectToAction(nameof(Details), new { id = model.StudentId, tabIndex = "4" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditIncidentStatuses(Student model)
        {
            if (!await IsTabWritable("INCIDENT"))
            {
                _flashMessage.Danger(Message.UnableToEditInvalidPermission);
                return RedirectToAction(nameof(Details), new { id = model.Id, tabIndex = "5" });
            }

            if (_db.StudentIncidents.Any(x => x.StudentId == model.Id))
            {
                using (var transaction = _db.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (var item in model.StudentIncidents)
                        {
                            var modelToUpdate = _db.StudentIncidents.SingleOrDefault(x => x.Id == item.Id);
                            _mapper.Map<StudentIncident, StudentIncident>(item, modelToUpdate);
                            _db.SaveChanges();

                            var student = _db.Students.IgnoreQueryFilters().SingleOrDefault(x => x.Id == model.Id);
                            var studentIncidents = _studentProvider.GetStudentIncidentsByStudentId(model.Id);
                            var registrationLock = false;
                            var paymentLock = false;
                            var signInLock = false;
                            if (studentIncidents.Any())
                            {
                                registrationLock = studentIncidents.Any(x => x.LockedRegistration);
                                paymentLock = studentIncidents.Any(x => x.LockedPayment);
                                signInLock = studentIncidents.Any(x => x.LockedSignIn);
                            }

                            if (await _registrationProvider.UpdateLockedStudentUspark(student.Code, registrationLock, paymentLock, signInLock))
                            {
                                transaction.Rollback();
                            }
                            else
                            {
                                transaction.Commit();
                            }
                        }
                        _flashMessage.Confirmation(Message.SaveSucceed);
                    }
                    catch
                    {
                        transaction.Rollback();
                        _flashMessage.Danger(Message.UnableToEdit);
                    }
                }
            }

            return RedirectToAction(nameof(Details), new { id = model.Id, tabIndex = "5" });
        }

        #region Change Curriculum
        public IActionResult ChangeCurriculum(Guid id, string returnUrl)
        {
            if (IsChangeFacultyLocked(id))
            {
                ModelState.AddModelError(string.Empty, Message.UnableToChangeCurriculum);
                return View(new ChangeCurriculumViewModel() { StudentId = id });
            }

            CreateSelectList();
            var student = _db.Students.Include(x => x.AcademicInformation)
                                         .ThenInclude(x => x.Faculty)
                                      .Include(x => x.AcademicInformation)
                                         .ThenInclude(x => x.Department)
                                      .Include(x => x.AcademicInformation)
                                         .ThenInclude(x => x.CurriculumVersion)
                                         .ThenInclude(x => x.Curriculum)
                                      .Include(x => x.AdmissionInformation)
                                         .ThenInclude(x => x.AdmissionType)
                                      .Include(x => x.AcademicInformation)
                                         .ThenInclude(x => x.AcademicProgram)
                                      .Include(x => x.GraduationInformations)
                                      .SingleOrDefault(x => x.Id == id);

            var curriculum = _mapper.Map<Student, ChangeCurriculumViewModel>(student);
            ViewBag.ReturnUrl = returnUrl;
            return View(curriculum);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangeCurriculum(ChangeCurriculumViewModel model, string returnUrl)
        {
            if (!await IsTabWritable("CURRICULUM"))
            {
                _flashMessage.Danger(Message.UnableToEditInvalidPermission);
                return RedirectToAction("Details", nameof(Student), new { id = model.StudentId, tabIndex = "1", returnUrl = returnUrl });
            }

            ViewBag.ReturnUrl = returnUrl;
            if (IsChangeFacultyLocked(model.StudentId))
            {
                ModelState.AddModelError(string.Empty, Message.UnableToChangeCurriculum);
                return View(model);
            }

            var curriculum = _db.CurriculumVersions.Include(x => x.Curriculum)
                                                   .SingleOrDefault(x => x.Id == model.CurriculumVersionId);
            var modelToUpdate = _db.AcademicInformations.SingleOrDefault(x => x.StudentId == model.StudentId);
            if (modelToUpdate != null && curriculum != null)
            {
                try
                {
                    modelToUpdate = _mapper.Map<CurriculumVersion, AcademicInformation>(curriculum, modelToUpdate);
                    _db.SaveChanges();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction("Details", nameof(Student), new { id = model.StudentId, tabIndex = "1", returnUrl = returnUrl });
                }
                catch
                {
                    ModelState.AddModelError(string.Empty, Message.UnableToEdit);
                    _flashMessage.Danger(Message.UnableToEdit);
                    return View(model);
                }
            }

            _flashMessage.Danger(Message.UnableToEdit);
            return View(model);
        }

        public bool IsChangeFacultyLocked(Guid id)
        {
            var isLocked = _db.StudentIncidents.Any(x => x.StudentId == id
                                                         && x.LockedChangeFaculty);
            return isLocked;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetFacultyByCurriculumId(long id)
        {
            var curriculum = _db.Curriculums.Include(x => x.Faculty)
                                            .Include(x => x.Department)
                                            .FirstOrDefault(x => x.Id == id);

            return Json($"{ curriculum.Faculty?.NameEn } - { curriculum.Department?.NameEn }");
        }
        #endregion

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SaveDocument(StudentRequiredDocument model)
        {
            if (!await IsTabWritable("DOCUMENT"))
            {
                _flashMessage.Danger(Message.UnableToEditInvalidPermission);
                return RedirectToAction(nameof(Details), new { id = model.StudentId, tabIndex = "11" });
            }

            var studentCode = _studentProvider.GetStudentCodeById(model.StudentId);
            if (model.StudentDocuments.Any(x => x.UploadFile != null))
            {
                foreach (var document in model.StudentDocuments.Where(x => x.UploadFile != null))
                {
                    var formFile = document.UploadFile;
                    if (formFile.Length > 0)
                    {
                        var documentTypeName = document.DocumentId.HasValue ? _studentProvider.GetDocument(document.DocumentId.Value).NameEn : DateTime.Now.ToString("yyMMddHHmmss");
                        document.ImageUrl = _fileProvider.UploadFile(UploadSubDirectory.STUDENT_DOCUMENTS, formFile, studentCode + "_" + documentTypeName);
                    }
                }
            }

            var success = _studentProvider.SaveStudentDocument(model);
            if (success)
            {
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            else
            {
                _flashMessage.Danger(Message.UnableToSave);
            }

            return RedirectToAction(nameof(Details), new { code = studentCode, tabIndex = "11" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UploadProfileImage(Student model)
        {
            if (model.ReturnController == "student" && !await IsTabWritable("GENERAL"))
            {
                _flashMessage.Danger(Message.UnableToEditInvalidPermission);
                return RedirectToAction(nameof(Details), new { id = model.Id, tabIndex = "0" });
            }

            // Webcam
            if (!String.IsNullOrEmpty(model.UploadFileBase64))
            {
                //string imageUrl = _fileProvider.UploadFile(UploadSubDirectory.STUDENT_PROFILE_IMAGE, model.UploadFileBase64, "png", model.StudentCode);

                var imageUrl = await _studentPhotoProvider.UploadFile(model.UploadFileBase64, model.StudentCode);
                var success = _studentProvider.SaveStudentProfileImage(model.StudentCode, imageUrl);
                if (success)
                {
                    _flashMessage.Confirmation(Message.SaveSucceed);
                }
                else
                {
                    _flashMessage.Danger(Message.UnableToSave);
                }
            }
            else
            {
                // Upload image
                if (model.UploadFile != null)
                {
                    var formFile = model.UploadFile;
                    if (formFile.Length > 0)
                    {
                        //var profileImageURL = _fileProvider.UploadFile(UploadSubDirectory.STUDENT_PROFILE_IMAGE, formFile, model.StudentCode);

                        var profileImageURL = await _studentPhotoProvider.UploadFile(formFile, model.StudentCode);
                        var success = _studentProvider.SaveStudentProfileImage(model.StudentCode, profileImageURL);
                        if (success)
                        {
                            _flashMessage.Confirmation(Message.SaveSucceed);
                        }
                        else
                        {
                            _flashMessage.Danger(Message.UnableToSave);
                        }
                    }
                }
            }

            if (model.ReturnController == "student")
            {
                return RedirectToAction(nameof(Details), new { code = model.StudentCode, tabIndex = "0" });
            }
            else
            {
                return RedirectToAction("Details", "AdmissionStudent", new { codeOrNumber = model.StudentCode, tabIndex = "0" });
            }
        }

        public ActionResult ChangeStudentStatus(Guid id, string studentStatus)
        {
            CreateSelectList();
            var model = new ChangeStudentStatusViewModel()
                        {
                            StudentId = id,
                            EffectiveAt = DateTime.Today,
                            StudentStatus = studentStatus
                        };

            return PartialView("_ChangeStudentStatus", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangeStudentStatus(ChangeStudentStatusViewModel model)
        {
            if (!await IsTabWritable("GENERAL"))
            {
                _flashMessage.Danger(Message.UnableToEditInvalidPermission);
                return RedirectToAction(nameof(Details), new { id = model.StudentId, tabIndex = "1" });
            }

            CreateSelectList();
            var student = _studentProvider.GetStudentById(model.StudentId);
            var currentTermId = _cacheProvider.GetCurrentTerm(student.AcademicInformation.AcademicLevelId).Id;
            student.StudentStatus = model.StudentStatus;
            student.IsActive = KeystoneLibrary.Providers.StudentProvider.IsActiveFromStudentStatus(student.StudentStatus);
            
            var success = _studentProvider.SaveStudentStatusLog(model.StudentId
                                                                , currentTermId
                                                                , SaveStatusSouces.CHANGESTUDENTSTATUS.GetDisplayName()
                                                                , model.Remark
                                                                , model.StudentStatus
                                                                , model.EffectiveAt);
            if (!success)
            {
                _flashMessage.Danger(Message.UnableToEdit);
                return View(model);
            }

            _db.SaveChanges();
            return RedirectToAction(nameof(Details), new { code = student.Code, tabIndex = "0" });
        }

        public ContentResult UpdateStudentCGPA(string password, string isUpdate = "", string forceStudentCode = "")
        {
            if (!"UpdateStudentCGPA".Equals(password))
            {
                return Content("");
            }
            if (string.IsNullOrEmpty(isUpdate))
            {
                var hasGradeUpdateCount = _db.AcademicInformations.Count(x => x.IsHasGradeUpdate);
                return new ContentResult
                {
                    Content =  $"<html>There are {hasGradeUpdateCount} to be updated<br/><a href=\"{Request.Path}?password={password}&isUpdate=true\">Click Here To Update</a><br/>DateTime: { DateTime.Now }<br/>DateTimeUtc:{DateTime.UtcNow}</html>",
                    ContentType = "text/html"
                };
            }

            List<Guid> listOfStudentId = new List<Guid>();
            if (string.IsNullOrEmpty(forceStudentCode))
            {
                listOfStudentId.AddRange(_db.AcademicInformations.Where(x => x.IsHasGradeUpdate).Select(x => x.StudentId).ToList());
            }
            else
            {
                var student = _db.Students.FirstOrDefault(x => x.Code == forceStudentCode);
                if (student != null)
                {
                    listOfStudentId.Add(student.Id);
                }
            }
            if (!listOfStudentId.Any())
            {
                return new ContentResult
                {
                    Content = $"<html>No CGPA To Update</html>",
                    ContentType = "text/html"
                };
            }
            int count = 0;
            foreach (var studentId in listOfStudentId)
            {
                _studentProvider.UpdateCGPA(studentId);
                count++;
            }

            return new ContentResult
            {
                Content = $"<html>Success: {count} ty</html>",
                ContentType = "text/html"
            };
        }

        private void CreateSelectList(long countryId = 0, long stateId = 0, long titleId = 0)
        {
            ViewBag.TitlesEn = _selectListProvider.GetTitlesEn();
            ViewBag.TitleTh = _selectListProvider.GetTitlesTh();
            ViewBag.Races = _selectListProvider.GetRaces();
            ViewBag.Nationalities = _selectListProvider.GetNationalities();
            ViewBag.Religions = _selectListProvider.GetReligions();
            ViewBag.Countries = _selectListProvider.GetCountries();
            if (countryId != 0)
            {
                ViewBag.States = _selectListProvider.GetStates(countryId);
                ViewBag.Provinces = _selectListProvider.GetProvinces(countryId);
            }

            if (stateId != 0)
            {
                ViewBag.Cities = _selectListProvider.GetCitiesByStateId(stateId);
            }

            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.AdmissionTypes = _selectListProvider.GetAdmissionTypes();
            ViewBag.AcademicHonors = _selectListProvider.GetAcademicHonors();
            ViewBag.PreviousSchools = _selectListProvider.GetPreviousSchools();
            ViewBag.EducationBackgrounds = _selectListProvider.GetEducationBackground();
            ViewBag.ExemptedExaminations = _selectListProvider.GetExemptedAdmissionExaminations();
            ViewBag.Statuses = _selectListProvider.GetRegistrationStatuses();
            ViewBag.ExamType = _selectListProvider.GetExaminationTypes();
            ViewBag.Incidents = _selectListProvider.GetIncidents();
            ViewBag.StudentGroups = _selectListProvider.GetStudentGroups();
            ViewBag.BankBranches = _selectListProvider.GetBankBranches();
            ViewBag.Relationships = _selectListProvider.GetRelationships();
            ViewBag.MaritalStatuses = _selectListProvider.GetMaritalStatuses();
            ViewBag.LivingStatuses = _selectListProvider.GetLivingStatuses();
            ViewBag.Deformations = _selectListProvider.GetDeformations();
            ViewBag.EntranceExamResults = _selectListProvider.GetEntranceExamResults();
            ViewBag.Agencies = _selectListProvider.GetAgencies();
            ViewBag.Signatories = _selectListProvider.GetSignatories();
            ViewBag.AdmissionChannels = _selectListProvider.GetAdmissionChannels();
            ViewBag.AdmissionPlaces = _selectListProvider.GetAdmissionPlaces();
            ViewBag.InstructorForCheating = _selectListProvider.GetInstructorsForCheating();
            ViewBag.YesNoAnswer = _selectListProvider.GetYesNoAnswer();
            ViewBag.Documents = _selectListProvider.GetDocuments();
            ViewBag.StudentFeeTypes = _selectListProvider.GetStudentFeeTypes();
            ViewBag.ResidentTypes = _selectListProvider.GetResidentTypes();
            ViewBag.NativeLanguages = _selectListProvider.GetNativeLanguages();
            ViewBag.StudentStatuses = _selectListProvider.GetStudentStatuses(GetStudentStatusesEnum.StudentProfile);
        }

        private void CreateSelectList(Student model)
        {
            var titleId = model?.TitleId ?? 0;
            var academicLevelId = model?.AcademicInformation?.AcademicLevelId ?? 0;
            var facultyId = model?.AcademicInformation?.FacultyId ?? 0;
            var departmentId = model?.AcademicInformation?.DepartmentId ?? 0;
            var curriculumId = model?.AcademicInformation?.CurriculumVersion?.CurriculumId ?? 0;
            var curriculumVersionId = model?.AcademicInformation?.CurriculumVersionId ?? 0;

            var admissionFacultyId = model?.AdmissionInformation?.FacultyId ?? 0;
            var admissionDepartmentId = model?.AdmissionInformation?.DepartmentId ?? 0;
            var admissionCurriculumId = model?.AdmissionInformation?.CurriculumVersion?.CurriculumId ?? 0;
            var admissionCurriculumVersionId = model?.AdmissionInformation?.CurriculumVersionId ?? 0;
            var admissionTermId = model?.AdmissionInformation?.AdmissionTermId ?? 0;
            var admissionRoundId = model?.AdmissionInformation?.AdmissionRoundId ?? 0;

            var studentGroupId = model?.AcademicInformation?.StudentGroupId ?? 0;
            var nationalityId = model?.NationalityId ?? 0;
            var batch = model?.AcademicInformation?.Batch ?? 0;
            var studentFeeTypeId = model?.StudentFeeTypeId ?? 0;
            var previousSchoolCountryId = model?.AdmissionInformation?.PreviousSchool?.CountryId ?? 0;
            var previousSchoolId = model?.AdmissionInformation?.PreviousSchoolId ?? 0;

            ViewBag.TitleTh = _selectListProvider.GetTitleThByTitleEn(titleId);
            ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
            ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);
            ViewBag.Curriculums = _selectListProvider.GetCurriculumByDepartment(academicLevelId, facultyId, departmentId);
            ViewBag.CurriculumVersions = _selectListProvider.GetCurriculumVersion(curriculumId);

            ViewBag.AdmissionFaculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
            ViewBag.AdmissionDepartments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, admissionFacultyId);
            ViewBag.AdmissionCurriculumVersions = _selectListProvider.GetCurriculumVersionForAdmissionCurriculum(admissionTermId, admissionRoundId, facultyId, departmentId);

            ViewBag.AcademicPrograms = _selectListProvider.GetAcademicProgramsByAcademicLevelId(academicLevelId);
            ViewBag.MaintenanceFees = _selectListProvider.GetMaintenanceFees(facultyId, departmentId, academicLevelId, studentGroupId);
            ViewBag.Minors = _selectListProvider.GetMinorsByCurriculumVersionId(curriculumVersionId);
            ViewBag.Concentrations = _selectListProvider.GetConcentrationsByCurriculumVersionId(curriculumVersionId);
            ViewBag.YearWithTerm = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            ViewBag.AdmissionRounds = _selectListProvider.GetAdmissionRounds(academicLevelId, admissionTermId);
            ViewBag.DocumentGroups = _selectListProvider.GetStudentDocumentGroups();
            ViewBag.StudentFeeGroups = _selectListProvider.GetStudentFeeGroups();
            ViewBag.CurriculumVersions = _selectListProvider.GetImplementedCurriculumVersionsByStudentId(model.Id);
        }
    }
}