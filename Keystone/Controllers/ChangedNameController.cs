using AutoMapper;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.Logs;
using KeystoneLibrary.Models.DataModels.Profile;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Vereyon.Web;

namespace Keystone.Controllers
{
    public class ChangedNameController : BaseController
    {
        protected readonly ICacheProvider _cacheProvider;
        protected readonly IStudentProvider _studentProvider;
        protected readonly IDateTimeProvider _dateTimeProvider;
        protected readonly IPetitionProvider _petitionProvider;
        protected readonly IFileProvider _fileProvider;
        private UserManager<ApplicationUser> _userManager { get; }

        public ChangedNameController(ApplicationDbContext db,
                                     IFlashMessage flashMessage,
                                     IMapper mapper,
                                     ISelectListProvider selectListProvider,
                                     ICacheProvider cacheProvider,
                                     IStudentProvider studentProvider,
                                     IDateTimeProvider dateTimeProvider,
                                     IPetitionProvider petitionProvider,
                                     IFileProvider fileProvider,
                                     UserManager<ApplicationUser> user) : base(db, flashMessage, mapper, selectListProvider)
        {
            _cacheProvider = cacheProvider;
            _studentProvider = studentProvider;
            _dateTimeProvider = dateTimeProvider;
            _petitionProvider = petitionProvider;
            _fileProvider = fileProvider;
            _userManager = user;
        }

        public IActionResult Index(int page, Criteria criteria)
        {
            CreateSelectList();
            var startedAt = _dateTimeProvider.ConvertStringToDateTime(criteria.StartedAt);
            var endedAt = _dateTimeProvider.ConvertStringToDateTime(criteria.EndedAt);
            
            var changedNameLog = (from changeName in _db.ChangedNameLogs
                                  join student in _db.Students on changeName.StudentId equals student.Id
                                  where (string.IsNullOrEmpty(criteria.Code)
                                         || criteria.Code == student.Code)
                                         && (changeName.ChangedAt >= startedAt
                                             || startedAt == null)
                                         && (endedAt == null
                                             || changeName.ChangedAt <= endedAt)
                                         && (criteria.AcademicYear == null
                                             || changeName.RunningNumber == criteria.AcademicYear)
                                         && (criteria.StartYear == null
                                             || changeName.Year == criteria.StartYear)
                                         && (string.IsNullOrEmpty(criteria.Status) 
                                             || criteria.Status == changeName.Status)
                                  select new ChangedNameLogViewModel
                                         {
                                             StudentCode = student.Code,
                                             ChangedNameDetail = changeName
                                         })
                                  .OrderByDescending(x => x.ChangedNameDetail.RequestedAt)
                                  .ToList();     

            var modelPageResult = changedNameLog.AsQueryable().GetPaged(criteria, page);
            return View(modelPageResult);
        }

        public IActionResult Details(long id, string returnUrl)
        {
            CreateSelectList();
            ViewBag.ReturnUrl = returnUrl;
            var changedNameLog = _petitionProvider.GetChangedNameLog(id);
            var student = _studentProvider.GetStudentInformationById(changedNameLog.StudentId);
            if (student.GraduationInformation != null)
            {
                _flashMessage.Danger(Message.GraduateStudent);
            }

            var model = new ChangedNameViewModel();
            model = _mapper.Map<Student, ChangedNameViewModel>(student);
            model = _mapper.Map<ChangedNameLog, ChangedNameViewModel>(changedNameLog, model);
            model.StudentChangedNameLogs = _db.ChangedNameLogs.Where(x => x.StudentId == changedNameLog.StudentId
                                                                          && x.Status == "a")
                                                              .ToList();

            return View(model);
        }
        
        public async Task<ActionResult> Reject(long id)
        {
            var model = _petitionProvider.GetChangedNameLog(id);
            var user = await _userManager.GetUserAsync(User);
            var studentCode = _studentProvider.GetStudentCodeById(model.StudentId);

            try
            {
                model.Status = "r";
                model.ChangedBy = user == null ? "" : user.NormalizedUserName;
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToEdit);
            }

            CreateSelectList();
            return RedirectToAction(nameof(Index), new Criteria { Code = studentCode });
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(ChangedNameViewModel model)
        {
            CreateSelectList();
            if (string.IsNullOrEmpty(model.StudentCode) || string.IsNullOrEmpty(model.DistrictRegistrationAtText))
            {
                _flashMessage.Warning(Message.RequiredData);
                return RedirectToAction(nameof(Index), new Criteria { Code = model.StudentCode });
            }

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    DateTime? districtRegistrationAt = _dateTimeProvider.ConvertStringToDateTime(model.DistrictRegistrationAtText);
                    DateTime? changedAt = _dateTimeProvider.ConvertStringToDateTime(model.ChangedAtText);
                    var student = _studentProvider.GetStudentInformationByCode(model.StudentCode);
                    var user = await _userManager.GetUserAsync(User);

                    var changedNameLog = _petitionProvider.GetChangedNameLog(model.Id);
                    changedNameLog.ChangedFirstNameEn = model.NewFirstNameEn;
                    changedNameLog.ChangedLastNameEn = model.NewLastNameEn;
                    changedNameLog.ChangedFirstNameTh = model.NewFirstNameTh;
                    changedNameLog.ChangedLastNameTh = model.NewLastNameTh;
                    changedNameLog.ReferenceNumber = model.ReferenceNumber;
                    changedNameLog.DistrictRegistrationOffice = model.DistrictRegistrationOffice;
                    changedNameLog.DistrictRegistrationAt = districtRegistrationAt.HasValue ? districtRegistrationAt.Value.Date : default(DateTime);
                    changedNameLog.ChangedType = model.ChangedType;
                    changedNameLog.NameType = model.ChangedFlag;
                    changedNameLog.ChangedAt = changedAt;
                    changedNameLog.ChangedBy = user == null ? "" : user.NormalizedUserName;
                    changedNameLog.Remark = model.Remark;
                    changedNameLog.Status = "a";
                    if (model.UploadFile != null && model.UploadFile.Length > 0)
                    {
                        changedNameLog.DocumentUrl = _fileProvider.UploadFile(UploadSubDirectory.CHANGE_NAME, model.UploadFile, model.StudentCode + "_" + model.NewFirstNameEn);
                    }
                
                    student.FirstNameTh = model.NewFirstNameTh;
                    student.LastNameTh = model.NewLastNameTh;
                    student.FirstNameEn = model.NewFirstNameEn;
                    student.LastNameEn = model.NewLastNameEn;
                    
                    _db.StudentLogs.Add(new StudentLog
                                        {
                                            StudentId = student.Id,
                                            TermId = _cacheProvider.GetCurrentTerm(student.AcademicInformation.AcademicLevelId)?.Id ?? 0,
                                            Source = "Student Change Name",
                                            Log = JsonConvert.SerializeObject(changedNameLog),
                                            UpdatedAt = DateTime.UtcNow
                                        });
                    _db.SaveChanges();
                    transaction.Commit();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index), new Criteria { Code = model.StudentCode });
                }
                catch
                {
                    transaction.Rollback();
                    _flashMessage.Danger(Message.UnableToSave);
                    CreateSelectList();
                    return RedirectToAction(nameof(Index), new Criteria { Code = model.StudentCode });
                }
            }
        }

        private void CreateSelectList() 
        {
            ViewBag.FlagType = _selectListProvider.GetChangeFlagType();
            ViewBag.NameType = _selectListProvider.GetChangeNameType();
            ViewBag.Statuses = _selectListProvider.GetChangedNameLogStatuses();
        }
    }
}