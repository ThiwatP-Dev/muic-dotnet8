using AutoMapper;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Profile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using Vereyon.Web;
using KeystoneLibrary.Enumeration;
using KeystoneLibrary.Helpers;
using Keystone.Permission;

namespace Keystone.Controllers
{
    [PermissionAuthorize("AdmissionStudent", "")]
    public class AdmissionStudentController : BaseController
    {
        protected readonly IStudentProvider _studentProvider;
        protected readonly IAdmissionProvider _admissionProvider;
        protected readonly ICacheProvider _cacheProvider;
        protected readonly IAcademicProvider _academicProvider;
        protected readonly IFileProvider _fileProvider;
        protected readonly IDateTimeProvider _dateTimeProvider;
        protected readonly ICardProvider _cardProvider;

        public AdmissionStudentController(ApplicationDbContext db, 
                                          IFlashMessage flashMessage, 
                                          IMapper mapper, 
                                          ISelectListProvider selectListProvider,
                                          IStudentProvider studentProvider,
                                          IAdmissionProvider admissionProvider,
                                          ICacheProvider cacheProvider,
                                          IAcademicProvider academicProvider,
                                          IFileProvider fileProvider,
                                          IDateTimeProvider dateTimeProvider,
                                          ICardProvider cardProvider) : base(db, flashMessage, mapper, selectListProvider)
        {
            _studentProvider = studentProvider;
            _admissionProvider = admissionProvider;
            _cacheProvider = cacheProvider;
            _academicProvider = academicProvider;
            _fileProvider = fileProvider;
            _dateTimeProvider = dateTimeProvider;
            _cardProvider = cardProvider;
        }

        public IActionResult Index(int page, Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId);
            if (criteria.StudentCodeFrom == null && criteria.StudentCodeTo == null && string.IsNullOrEmpty(criteria.FirstName)
                && string.IsNullOrEmpty(criteria.LastName) && string.IsNullOrEmpty(criteria.Code) && string.IsNullOrEmpty(criteria.CitizenAndPassport)
                && criteria.NationalityId == 0 && criteria.FacultyId == 0 && criteria.DepartmentId == 0 && string.IsNullOrEmpty(criteria.Status)
                && string.IsNullOrEmpty(criteria.EntranceExamResult) && criteria.StartStudentBatch == null && string.IsNullOrEmpty(criteria.Active)
                && string.IsNullOrEmpty(criteria.HaveCode) && string.IsNullOrEmpty(criteria.StartedAt) && string.IsNullOrEmpty(criteria.EndedAt)
                && criteria.EndStudentBatch == null && criteria.AcademicLevelId == 0 && criteria.AdmissionRoundId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            DateTime? startedAt = _dateTimeProvider.ConvertStringToDateTime(criteria.StartedAt);
            DateTime? endedAt = _dateTimeProvider.ConvertStringToDateTime(criteria.EndedAt);

            var students = _db.Students.Include(x => x.Nationality)
                                       .Include(x => x.AcademicInformation)
                                           .ThenInclude(x => x.Faculty)
                                       .Include(x => x.AcademicInformation)
                                           .ThenInclude(x => x.Department)
                                       .Include(x => x.AcademicInformation)
                                           .ThenInclude(x => x.AcademicLevel)
                                       .Include(x => x.AdmissionInformation)
                                           .ThenInclude(x => x.AdmissionRound)
                                       .Include(x => x.AdmissionInformation)
                                           .ThenInclude(x => x.AdmissionTerm)
                                       .Where(x => (string.IsNullOrEmpty(criteria.FirstName)
                                                    || x.FirstNameEn.StartsWith(criteria.FirstName)
                                                    || x.FirstNameTh.StartsWith(criteria.FirstName))
                                                    && (string.IsNullOrEmpty(criteria.LastName)
                                                        || x.LastNameEn.StartsWith(criteria.LastName)
                                                        || x.LastNameTh.StartsWith(criteria.LastName))
                                                    && (string.IsNullOrEmpty(criteria.Code)
                                                        || x.AdmissionInformation.ApplicationNumber.StartsWith(criteria.Code.ToUpper()))
                                                    && (criteria.FacultyId == 0
                                                        || x.AcademicInformation.FacultyId == criteria.FacultyId)
                                                    && (criteria.DepartmentId == 0
                                                        || x.AcademicInformation.DepartmentId == criteria.DepartmentId)
                                                    && (criteria.NationalityId == 0
                                                        || x.NationalityId == criteria.NationalityId)
                                                    && (string.IsNullOrEmpty(criteria.CitizenAndPassport) 
                                                        || x.CitizenNumber.Contains(criteria.CitizenAndPassport)
                                                        || x.Passport.Contains(criteria.CitizenAndPassport))
                                                    && (string.IsNullOrEmpty(criteria.Active) 
                                                        || x.IsActive == Convert.ToBoolean(criteria.Active))
                                                    && (string.IsNullOrEmpty(criteria.HaveCode) 
                                                        || !string.IsNullOrEmpty(x.Code) == Convert.ToBoolean(criteria.HaveCode))
                                                    && (criteria.AcademicLevelId == 0   
                                                        || x.AdmissionInformation.AdmissionTerm.AcademicLevelId == criteria.AcademicLevelId)
                                                    && (criteria.AdmissionRoundId == 0
                                                        || x.AdmissionInformation.AdmissionRound.Id == criteria.AdmissionRoundId)
                                                    && (string.IsNullOrEmpty(criteria.EntranceExamResult)
                                                        || x.AdmissionInformation.EntranceExamResult == criteria.EntranceExamResult)
                                                    && (criteria.StudentCodeFrom == null
                                                        || (criteria.StudentCodeTo == null ? x.CodeInt == criteria.StudentCodeFrom
                                                                                           : x.CodeInt >= criteria.StudentCodeFrom))
                                                    && (criteria.StudentCodeTo == null
                                                        || (criteria.StudentCodeFrom == null ? x.CodeInt == criteria.StudentCodeTo
                                                                                             : x.CodeInt <= criteria.StudentCodeTo))
                                                    && (criteria.StartStudentBatch == null
                                                        || x.AcademicInformation == null
                                                        || x.AcademicInformation.Batch >= criteria.StartStudentBatch)
                                                    && (criteria.EndStudentBatch == null
                                                        || x.AcademicInformation == null
                                                        || x.AcademicInformation.Batch <= criteria.EndStudentBatch)
                                                    && (startedAt == null
                                                        || (x.AdmissionInformation.AppliedAt != null
                                                            && x.AdmissionInformation.AppliedAt.Value.Date >= startedAt))
                                                    && (endedAt == null
                                                        || (x.AdmissionInformation.AppliedAt != null
                                                            && x.AdmissionInformation.AppliedAt.Value.Date <= endedAt))
                                                    && x.StudentStatus == "a")
                                       .Select(x => _mapper.Map<Student, SearchAdmissionStudentViewModel>(x))
                                       .ToList()
                                       .Where(x => string.IsNullOrEmpty(criteria.Status)
                                                   || x.AdmissionStatus == criteria.Status)
                                       .OrderBy(x => x.Code)
                                       .AsQueryable()
                                       .GetPaged(criteria, page);
            
            return View(students);
        }

        public IActionResult Details(string codeOrNumber, Guid studentId, string tabIndex, string returnUrl)
        {
            AdmissionStudentViewModel model = new AdmissionStudentViewModel();
            if (!String.IsNullOrEmpty(codeOrNumber) || studentId != null)
            {
                if (_admissionProvider.IsExistStudentId(studentId)
                    || (!string.IsNullOrEmpty(codeOrNumber) && _admissionProvider.IsExistCodeCitizenPassportApplicationNumber(codeOrNumber)))
                {
                    var student = string.IsNullOrEmpty(codeOrNumber) ? _admissionProvider.GetStudentInformationById(studentId)
                                                                     : _admissionProvider.GetStudentInformationByCode(codeOrNumber);
                    model.Student = student;
                    model.AdmissionStudentInformation = new AdmissionStudentInformation 
                                                        {
                                                            AdmissionInformation = student.AdmissionInformation,
                                                            StudentExemptedExamScores = _studentProvider.GetStudentExemptedExamScore(student.Id)
                                                        };

                    if (model.AdmissionStudentInformation.AdmissionInformation == null)
                    {
                        model.AdmissionStudentInformation.AdmissionInformation = new AdmissionInformation
                                                                                 {
                                                                                     StudentId = student.Id,
                                                                                     AdmissionDate = DateTime.UtcNow,
                                                                                 };
                    }

                    if (model.Student.AcademicInformation == null)
                    {
                        model.Student.AcademicInformation = new AcademicInformation
                                                            {
                                                                AcademicLevelId = model.AdmissionStudentInformation.AdmissionInformation.AcademicLevelId ?? 0,
                                                                CurriculumVersion = model.AdmissionStudentInformation.AdmissionInformation.CurriculumVersion
                                                            };
                    }

                    if (model.Student.AcademicInformation == null && model.Student.AdmissionInformation != null)
                    {
                        model.Student.AcademicInformation = new AcademicInformation
                                                            {
                                                                AcademicLevelId = model.AdmissionStudentInformation.AdmissionInformation?.AcademicLevelId ?? 0,
                                                                FacultyId = model.AdmissionStudentInformation.AdmissionInformation.FacultyId ?? 0,
                                                                DepartmentId = model.AdmissionStudentInformation.AdmissionInformation.DepartmentId ?? 0,
                                                                CurriculumVersionId = model.AdmissionStudentInformation.AdmissionInformation.CurriculumVersionId ?? 0
                                                            };
                    }

                    model.StudentRequiredDocument = _studentProvider.GetStudentRequiredDocument(student);
                    CreateModelSelectList(model.Student);
                }
                else
                {
                    CreateSelectList();
                    CreateModelSelectList(null);
                    ModelState.AddModelError(string.Empty, Message.StudentNotFound);
                }
            }
            else
            {
                // Create Student
                CreateSelectList();
                CreateModelSelectList(null);
                model.Student = new Student();
            }
            
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        [PermissionAuthorize("AdmissionStudent", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SaveGeneral(Student model)
        {
            var studentCode = model.Code;
            if (string.IsNullOrEmpty(studentCode))
            {
                studentCode = _admissionProvider.GetApplicationNumber(model.Id);
            }

            if (ModelState.IsValid)
            {
                if (_db.Students.Any(x => x.Code == model.Code) || _db.Students.Any(x => x.Id == model.Id))
                {
                    try
                    {
                        _db.Entry(model).State = EntityState.Modified;
                        await _db.SaveChangesAsync();
                        _flashMessage.Confirmation(Message.SaveSucceed);
                    }
                    catch 
                    { 
                        _flashMessage.Danger(Message.UnableToEdit);
                    }
                    
                    return RedirectToAction(nameof(Details), new { codeOrNumber = model.Code, tabIndex = "0" });
                }
                else if (model.Id == Guid.Empty)
                {
                    if (_admissionProvider.IsStudnetBlacklisted(model.CitizenNumber, model.Passport, model.FirstNameEn, model.LastNameEn, model.FirstNameTh, model.LastNameTh, model.BirthDate, model.Gender))
                    {
                        _flashMessage.Danger(Message.BlackListedStudent);
                    }
                    else
                    {
                        try
                        {
                            model.StudentStatus = "a";
                            model.IsActive = KeystoneLibrary.Providers.StudentProvider.IsActiveFromStudentStatus(model.StudentStatus);
                            _db.Students.Add(model);
                            _db.SaveChanges();
                            _flashMessage.Confirmation(Message.SaveSucceed);
                        }
                        catch
                        {
                            _flashMessage.Danger(Message.UnableToSave);
                        }
                    }
                    
                    return RedirectToAction(nameof(Details), new { codeOrNumber = studentCode, tabIndex = "0" });
                }
            }
            
            _flashMessage.Danger(Message.UnableToEdit);
            return RedirectToAction(nameof(Details), new { codeOrNumber = studentCode, tabIndex = "0" });
        }

        [PermissionAuthorize("AdmissionStudent", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveAdmissionInformation(AdmissionStudentInformation model)
        {
            var studentCode = _studentProvider.GetStudentCodeById(model.AdmissionInformation.StudentId);
            if (string.IsNullOrEmpty(studentCode))
            {
                studentCode = _admissionProvider.GetApplicationNumber(model.AdmissionInformation.StudentId);
            }

            if(model.AdmissionInformation.Id != 0) // update
            {
                try
                {
                    _db.AdmissionInformations.Attach(model.AdmissionInformation);
                    _db.Entry(model.AdmissionInformation).State = EntityState.Modified;
                    _db.Entry(model.AdmissionInformation).Property(x => x.CreatedAt).IsModified = false;
                    _db.Entry(model.AdmissionInformation).Property(x => x.CreatedBy).IsModified = false;
                    _db.SaveChanges();

                    var update = _studentProvider.UpdateStudentExemptedExamScore(model.StudentExemptedExamScores, model.AdmissionInformation.StudentId);
                    if (!update)
                    {
                        _flashMessage.Danger(Message.UnableToEditExemptedExamScore);
                        return RedirectToAction(nameof(Details), new { codeOrNumber = studentCode, tabIndex = "1" });
                    }

                    _flashMessage.Confirmation(Message.SaveSucceed);
                }
                catch
                { 
                    _flashMessage.Danger(Message.UnableToEdit);
                }
            }
            else // add new
            {
                try
                {
                    _db.AdmissionInformations.Add(model.AdmissionInformation);

                    if (model.AdmissionInformation.AdmissionRoundId != null )
                    {
                        var student = _studentProvider.GetStudentById(model.AdmissionInformation.StudentId);
                        student.IdCardCreatedDate = model.AdmissionInformation.AdmissionDate;

                        if (model.AdmissionInformation.AcademicLevelId != 0)
                        {
                            var expiredDate = _cardProvider.GetCardExpiration(model.AdmissionInformation.StudentId,
                                                                              model.AdmissionInformation.AcademicLevelId ?? 0,
                                                                              model.AdmissionInformation.FacultyId,
                                                                              model.AdmissionInformation.DepartmentId,
                                                                              student.IdCardCreatedDate);
                            student.IdCardExpiredDate = expiredDate;
                        }
                    }
                    
                    _db.SaveChanges();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                }
                catch
                {
                    _flashMessage.Danger(Message.UnableToSave);
                }
            }

            if (model.AdmissionInformation.Id != 0 && model.AdmissionInformation.AdmissionDocumentGroupId != null
                && model.AdmissionInformation.PreviousDocumentId != model.AdmissionInformation.AdmissionDocumentGroupId)
            {
                var success = _studentProvider.SaveDocumentStudentByDocumentGroup(model.AdmissionInformation.StudentId, model.AdmissionInformation.AdmissionDocumentGroupId ?? 0);
                if (!success)
                {
                    _flashMessage.Danger(Message.UnableToSaveDocument);
                }

                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            
            return RedirectToAction(nameof(Details), new { codeOrNumber = studentCode, tabIndex = "1" });        
        }

        [PermissionAuthorize("AdmissionStudent", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SaveAcademicInformation(AcademicInformation model)
        {
            var student = _db.Students.Include(x => x.AdmissionInformation)
                                      .Include(x => x.AcademicInformation)
                                      .SingleOrDefault(x => x.Id == model.StudentId);

            var academicInformation = _db.AcademicInformations.SingleOrDefault(x => x.Id == model.Id);
            var studentCode = student.Code;
            if (string.IsNullOrEmpty(studentCode))
            {
                studentCode = _admissionProvider.GetApplicationNumber(model.StudentId);
            }

            if (student.AcademicInformation != null)
            {
                if (ModelState.IsValid && await TryUpdateModelAsync<AcademicInformation>(academicInformation))
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
                }

                return RedirectToAction(nameof(Details), new { codeOrNumber = studentCode, tabIndex = "2" });
            }
            else if (model.Id == 0)
            {
                model.FacultyId = student.AdmissionInformation.FacultyId ?? 0;
                model.DepartmentId = student.AdmissionInformation.DepartmentId;
                model.CurriculumVersionId = student.AdmissionInformation.CurriculumVersionId;

                try
                {
                    _db.AcademicInformations.Add(model);
                    _db.SaveChanges();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                }
                catch
                {
                    _flashMessage.Danger(Message.UnableToEdit);
                }
                
                return RedirectToAction(nameof(Details), new { codeOrNumber = studentCode, tabIndex = "2" });
            }
            
            _flashMessage.Confirmation(Message.UnableToSave);
            return RedirectToAction(nameof(Details), new { codeOrNumber = studentCode, tabIndex = "2" });        
        }

        [PermissionAuthorize("AdmissionStudent", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveContactInformation(Student model)
        {
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

                return RedirectToAction(nameof(Details), new { codeOrNumber = model.Code, tabIndex = "5" });
            }
            
            _flashMessage.Danger(Message.UnableToEdit);
            return RedirectToAction(nameof(Details), new { codeOrNumber = model.Code, tabIndex = "5" });
        }

        [PermissionAuthorize("AdmissionStudent", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveDocument(StudentRequiredDocument model)
        {
            Regex reg = new Regex("[*'\"/,_&#^@]");
            var studentCode = _studentProvider.GetStudentCodeById(model.StudentId);
            if (string.IsNullOrEmpty(studentCode))
            {
                studentCode = _admissionProvider.GetApplicationNumber(model.StudentId);
            }

            if (model.StudentDocuments.Any(x => x.UploadFile != null)) 
            {
                foreach (var document in model.StudentDocuments.Where(x => x.UploadFile != null))
                {
                    var formFile = document.UploadFile;
                    if (formFile.Length > 0)
                    {
                        var documentType = document.DocumentId.HasValue ? _studentProvider.GetDocument(document.DocumentId.Value).NameEn : DateTime.Now.ToString("yyMMddHHmmss");
                        documentType = reg.Replace(documentType, string.Empty);
                        document.ImageUrl = _fileProvider.UploadFile(UploadSubDirectory.STUDENT_DOCUMENTS, formFile, studentCode + "_" + documentType);
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

            return RedirectToAction(nameof(Details), new { codeOrNumber = studentCode, tabIndex = "3" });
        }

        [PermissionAuthorize("AdmissionStudent", PolicyGenerator.Write)]
        public ActionResult ChangeStudentStatus(Guid studentId)
        {
            var student = _db.Students.Include(x => x.AdmissionInformation)
                                          .ThenInclude(x => x.AdmissionTerm)
                                      .SingleOrDefault(x => x.Id == studentId);

            var studentCode = student.Code;
            if (string.IsNullOrEmpty(studentCode) || !studentCode.Any())
            {
                _flashMessage.Warning(Message.RequiredData);
                return RedirectToAction(nameof(Details), new { studentId = student.Id, tabIndex = "0" });
            }

            if (string.IsNullOrEmpty(studentCode))
            {
                studentCode = _admissionProvider.GetApplicationNumber(studentId);
            }

            if (_admissionProvider.IsStudnetBlacklisted(student.CitizenNumber, student.Passport, student.FirstNameEn, student.LastNameEn, student.FirstNameTh, student.LastNameTh, student.BirthDate, student.Gender))
            {
                _flashMessage.Danger(Message.BlackListedStudent);
                return RedirectToAction(nameof(Details), new { codeOrNumber = studentCode });
            }
            else
            {
                var currentTerm = _cacheProvider.GetCurrentTerm(student.AdmissionInformation.AdmissionTerm.AcademicLevelId);
                using (var transaction = _db.Database.BeginTransaction())
                {
                    try
                    {
                        student.StudentStatus = "s";
                        student.IsActive = KeystoneLibrary.Providers.StudentProvider.IsActiveFromStudentStatus(student.StudentStatus);
                        var success = _studentProvider.SaveStudentStatusLog(student.Id
                                                                            , currentTerm.Id
                                                                            , SaveStatusSouces.ADMISSION.GetDisplayName()
                                                                            , "Confirm student from admission"
                                                                            , student.StudentStatus);
                        if (!success)
                        {
                            _flashMessage.Danger(Message.UnableToEdit);
                            transaction.Rollback();
                            ModelState.AddModelError(string.Empty, "Unauthorized");
                        }

                        _db.SaveChanges();
                        
                        transaction.Commit();
                        _flashMessage.Confirmation(Message.SaveSucceed);
                        return RedirectToAction(nameof(Index));
                    }
                    catch
                    {
                        _flashMessage.Danger(Message.UnableToSave);
                        transaction.Rollback();
                        ModelState.AddModelError(string.Empty, "Unauthorized");
                        return RedirectToAction(nameof(Details), new { codeOrNumber = studentCode });
                    }
                }
            }
        }
        public IActionResult ApplicationFormPreview(string studentCode, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var student = _admissionProvider.GetStudentInformationByCode(studentCode);
            var model =_admissionProvider.GetApplicationFormViewModel(student);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken] 
        public JsonResult GenerateStudentCode(long academicLevelId, long admissionRoundId)
        {
            var code = _admissionProvider.GenerateStudentCode(academicLevelId, admissionRoundId);
            return Json(code);
        }

        [HttpPost]
        [ValidateAntiForgeryToken] 
        public JsonResult IsExistStudentCode(string code)
        {
            var isExist = _admissionProvider.IsExistStudentCode(code);
            return Json(isExist);
        }

        [HttpPost]
        [ValidateAntiForgeryToken] 
        public string GetAdmissionFirstClassDate(long admissionRoundId)
        {
            var admissionFirstClassDate = _admissionProvider.GetAdmissionFirstClassDate(admissionRoundId);
            return admissionFirstClassDate;
        }

        public void CreateSelectList(long academicLevelId = 0, long facultyId = 0)
        {
            ViewBag.Nationalities = _selectListProvider.GetNationalities();
            ViewBag.EntranceExamResults = _selectListProvider.GetEntranceExamResults();
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.ActiveStatuses = _selectListProvider.GetActiveStatuses();
            ViewBag.StudentGroups = _selectListProvider.GetStudentGroups();
            ViewBag.PaidStatuses = _selectListProvider.GetPaidStatuses();
            ViewBag.AllYesNoAnswer = _selectListProvider.GetAllYesNoAnswer();
            ViewBag.AdmissionStatuses = _selectListProvider.GetAdmissionStatuses();

            if (academicLevelId != 0)
            {
                ViewBag.AdmissionRounds = _selectListProvider.GetAdmissionRoundByAcademicLevelId(academicLevelId);
                ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);

                if (facultyId != 0)
                {
                    ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);
                }
            }
        }

        public void CreateModelSelectList(Student model)
        {
            var academicLevelId = model?.AcademicInformation?.AcademicLevelId ?? 0;
            var titleId = model?.TitleId ?? 0;
            var facultyId = model?.AdmissionInformation?.FacultyId ?? 0;
            var curriculumId = model?.AdmissionInformation?.CurriculumVersion?.CurriculumId ?? 0;
            var departmentId = model?.AdmissionInformation?.DepartmentId ?? 0;
            var admissionTermId = model?.AdmissionInformation?.AdmissionTermId ?? 0;
            var previousSchoolCountryId = model?.AdmissionInformation?.PreviousSchool?.CountryId ?? 0;
            var previousSchoolId = model?.AdmissionInformation?.PreviousSchoolId ?? 0;
            var admissionRoundId = model?.AdmissionInformation?.AdmissionRoundId ?? 0;
            var curriculumVersionId = model?.AdmissionInformation?.CurriculumVersionId ?? 0;
            var nationalityId = model?.NationalityId ?? 0;
            var batch = model?.AcademicInformation?.Batch ?? 0;
            var studentGroupId = model?.AcademicInformation?.StudentGroupId ?? 0;
            var studentFeeTypeId = model?.StudentFeeTypeId ?? 0;

            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.TitlesEn = _selectListProvider.GetTitlesEn();
            ViewBag.TitlesTh = _selectListProvider.GetTitleThByTitleEn(titleId);
            ViewBag.Races = _selectListProvider.GetRaces();
            ViewBag.Nationalities = _selectListProvider.GetNationalities();
            ViewBag.Religions = _selectListProvider.GetReligions();
            ViewBag.Countries = _selectListProvider.GetCountries();
            ViewBag.Cities = _selectListProvider.GetCities();
            ViewBag.States = _selectListProvider.GetStates();
            ViewBag.Provinces = _selectListProvider.GetProvinces();
            ViewBag.MaritalStatuses = _selectListProvider.GetMaritalStatuses();
            ViewBag.LivingStatuses = _selectListProvider.GetLivingStatuses();
            ViewBag.Deformations = _selectListProvider.GetDeformations();
            ViewBag.AdmissionTypes = _selectListProvider.GetAdmissionTypes();
            ViewBag.PreviousSchools = _selectListProvider.GetPreviousSchools();
            ViewBag.EducationBackgrounds = _selectListProvider.GetEducationBackground();
            ViewBag.ExemptedExaminations = _selectListProvider.GetExemptedAdmissionExaminations();
            ViewBag.EntranceExamResults = _selectListProvider.GetEntranceExamResults();
            ViewBag.Agencies = _selectListProvider.GetAgencies();
            ViewBag.AdmissionChannels = _selectListProvider.GetAdmissionChannels();
            ViewBag.AdmissionPlaces = _selectListProvider.GetAdmissionPlaces();
            ViewBag.YesNoAnswer = _selectListProvider.GetYesNoAnswer();
            ViewBag.Documents = _selectListProvider.GetDocuments();
            ViewBag.StudentFeeTypes = _selectListProvider.GetStudentFeeTypes();
            ViewBag.ResidentTypes = _selectListProvider.GetResidentTypes();
            ViewBag.StudentStatuses = _selectListProvider.GetStudentStatuses();
            ViewBag.NativeLanguages = _selectListProvider.GetNativeLanguages();
            ViewBag.Relationships = _selectListProvider.GetRelationships();
            ViewBag.StudentGroups = _selectListProvider.GetStudentGroups();
            ViewBag.DocumentGroups = _selectListProvider.GetStudentDocumentGroups();

            if (academicLevelId != 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
                ViewBag.AdmissionRounds = admissionTermId == 0 ? _selectListProvider.GetAdmissionRoundByAcademicLevelId(academicLevelId)
                                                               : _selectListProvider.GetAdmissionRoundByTermId(admissionTermId);
                ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
                ViewBag.StudentFeeGroups = _selectListProvider.GetStudentFeeGroups(academicLevelId, facultyId, departmentId, curriculumId,
                                                                                   curriculumVersionId, nationalityId, batch, studentGroupId,
                                                                                   studentFeeTypeId);
                ViewBag.AcademicPrograms = _selectListProvider.GetAcademicProgramsByAcademicLevelId(academicLevelId);

                if (facultyId != 0)
                {
                    ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);
                }
            }

            if (academicLevelId != 0 && facultyId != 0 && departmentId != 0)
            {
                if (admissionTermId != 0)
                {
                    ViewBag.AdmissionCurriculumVersions = _selectListProvider.GetCurriculumVersionForAdmissionCurriculum(admissionTermId, admissionRoundId, facultyId, departmentId);
                }
            }

            if (curriculumVersionId != 0)
            {
                ViewBag.StudyPlans = _selectListProvider.GetStudyPlanByCurriculumVersion(curriculumVersionId);
            }
        }
    }
}