using KeystoneLibrary.Data;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.MasterTables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models.DataModels;
using Keystone.Permission;

namespace Keystone.Controllers.MasterTables
{
    [PermissionAuthorize("TransferUniversity", "")]
    public class TransferUniversityController : BaseController
    {
        protected readonly IMasterProvider _masterProvider;

        public TransferUniversityController(ApplicationDbContext db,
                                            IFlashMessage flashMessage,
                                            ISelectListProvider selectListProvider,
                                            IMasterProvider masterProvider) : base(db, flashMessage, selectListProvider)
        {
            _masterProvider = masterProvider;
        }

        public IActionResult Index(Criteria criteria, int page)
        {
            CreateSelectList();
            var models = _db.TransferUniversities.Include(x => x.Country)
                                                 .Where(x => (criteria.CountryId == 0
                                                              || x.CountryId == criteria.CountryId)
                                                              && (string.IsNullOrEmpty(criteria.CodeAndName)
                                                                  || x.NameEn.Contains(criteria.CodeAndName)
                                                                  || x.NameTh.Contains(criteria.CodeAndName)))
                                                 .IgnoreQueryFilters()
                                                 .GetPaged(criteria, page);
            return View(models);
        }

        public IActionResult Details(long id, string returnUrl)
        {
            var model = _masterProvider.FindTrasferUniversity(id);
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        [PermissionAuthorize("TransferUniversity", PolicyGenerator.Write)]
        public ActionResult Create(string returnUrl)
        {
            CreateSelectList();
            ViewBag.ReturnUrl = returnUrl;
            var model = new TransferUniversityViewModel
            {
                IsActive = true
            };

            return View(model);
        }

        [PermissionAuthorize("TransferUniversity", PolicyGenerator.Write)]
        [RequestFormLimits(ValueCountLimit = Int32.MaxValue)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(TransferUniversityViewModel model, string returnUrl)
        {
            try
            {
                ViewBag.ReturnUrl = returnUrl;
                if (IsValidTransferUniversitViewModel(model))
                {
                    CreateSelectList();
                    _flashMessage.Danger(Message.UnableToCreate);
                    return View(model);
                }

                var courses = new List<Course>();
                if (model.Courses != null)
                {
                    courses = model.Courses.Where(x => !string.IsNullOrEmpty(x.Code)
                                                       && !string.IsNullOrEmpty(x.NameEn)
                                                       && !string.IsNullOrEmpty(x.NameTh))
                                           .Select(x => new Course
                                                        {
                                                            AcademicLevelId = model.AcademicLevelId,
                                                            TransferUniversityId = model.Id,
                                                            Code = x.Code,
                                                            NameEn = x.NameEn,
                                                            NameTh = x.NameTh,
                                                            Credit = x.Credit ?? 0,
                                                            Lecture = x.Lecture ?? 0,
                                                            Lab = x.Lab ?? 0,
                                                            Other = x.Other ?? 0
                                                        })
                                           .ToList();
                }

                var transferUniversity = new TransferUniversity
                {
                    OHECCode = model.OHECCode,
                    NameEn = model.NameEn,
                    NameTh = model.NameTh,
                    CountryId = model.CountryId,
                    IsActive = model.IsActive,
                    Courses = courses
                };

                _db.TransferUniversities.Add(transferUniversity);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
                return RedirectToAction(nameof(Index), new
                {
                    CodeAndName = model.NameEn,
                    CountryId = model.CountryId
                });
            }
            catch
            {
                CreateSelectList();
                _flashMessage.Danger(Message.UnableToCreate);
                return View(model);
            }
        }

        public ActionResult Edit(long id, string returnUrl)
        {
            var transferUniversity = _masterProvider.FindTrasferUniversity(id);
            var courses = new List<TransferUniversityCourse>();
            foreach (var item in transferUniversity.Courses)
            {
                var course = new TransferUniversityCourse
                {
                    CourseId = item.Id,
                    AcademicLevelId = item.AcademicLevelId,
                    TransferUniversityId = item.TransferUniversityId,
                    Code = item.Code,
                    NameEn = item.NameEn,
                    NameTh = item.NameTh,
                    TranscriptNameEn1 = item.TranscriptNameEn1,
                    TranscriptNameTh1 = item.TranscriptNameTh1,
                    TranscriptNameEn2 = item.TranscriptNameEn2,
                    TranscriptNameTh2 = item.TranscriptNameTh2,
                    TranscriptNameEn3 = item.TranscriptNameEn3,
                    TranscriptNameTh3 = item.TranscriptNameTh3,
                    Credit = item.Credit,
                    Lecture = item.Lecture,
                    Lab = item.Lab,
                    Other = item.Other,
                    DescriptionEn = item.DescriptionEn,
                    DescriptionTh = item.DescriptionTh,
                    ShortNameEn = item.ShortNameEn,
                    ShortNameTh = item.ShortNameTh,
                    IsShowInTranscript = item.IsShowInTranscript,
                    IsCalculateCredit = item.IsCalculateCredit
                };

                courses.Add(course);
            }

            var model = new TransferUniversityViewModel
            {
                Id = id,
                OHECCode = transferUniversity.OHECCode,
                NameEn = transferUniversity.NameEn,
                NameTh = transferUniversity.NameTh,
                CountryId = transferUniversity.CountryId,
                IsActive = transferUniversity.IsActive,
            };

            if (courses.Any())
            {
                model.Courses = courses;
                model.AcademicLevelId = courses.FirstOrDefault().AcademicLevelId;
            }

            CreateSelectList();
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        [PermissionAuthorize("TransferUniversity", PolicyGenerator.Write)]
        [RequestFormLimits(ValueCountLimit = Int32.MaxValue)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(TransferUniversityViewModel model, string returnUrl)
        {
            try
            {
                if (IsValidTransferUniversitViewModel(model))
                {
                    CreateSelectList();
                    _flashMessage.Danger(Message.UnableToCreate);
                    return View(model);
                }

                model.Courses = model.Courses.Where(x => !string.IsNullOrEmpty(x.Code)
                                                         || !string.IsNullOrEmpty(x.NameEn)
                                                         || !string.IsNullOrEmpty(x.NameTh))
                                             .ToList();

                var courseIds = model.Courses.Select(x => x.CourseId);
                var transferUniversity = _db.TransferUniversities.SingleOrDefault(x => x.Id == model.Id);
                var updateCourses = _db.Courses.Where(x => x.TransferUniversityId == model.Id).ToList();
                var removeCourses = _db.Courses.Where(x => x.TransferUniversityId == model.Id
                                                           && !courseIds.Contains(x.Id))
                                               .ToList();
                if (model.Courses.Any())
                {
                    var newCourseList = new List<Course>();
                    for (var i = 0;i < model.Courses.Count(); i++)
                    {
                        if (updateCourses.Any(x => x.Id == model.Courses[i].CourseId))
                        {
                            updateCourses.Where(x => x.Id == model.Courses[i].CourseId)
                                         .Select(x => {
                                                          x.AcademicLevelId = model.AcademicLevelId;
                                                          x.TransferUniversityId = model.Id;
                                                          x.Code = model.Courses[i].Code;
                                                          x.NameEn = model.Courses[i].NameEn;
                                                          x.NameTh = model.Courses[i].NameTh;
                                                          x.TranscriptNameEn1 = model.Courses[i].TranscriptNameEn1;
                                                          x.TranscriptNameTh1 = model.Courses[i].TranscriptNameTh1;
                                                          x.TranscriptNameEn2 = model.Courses[i].TranscriptNameEn2;
                                                          x.TranscriptNameTh2 = model.Courses[i].TranscriptNameTh2;
                                                          x.TranscriptNameEn3 = model.Courses[i].TranscriptNameEn3;
                                                          x.TranscriptNameTh3 = model.Courses[i].TranscriptNameTh3;
                                                          x.Credit = model.Courses[i].Credit ?? 0;
                                                          x.Lecture = model.Courses[i].Lecture ?? 0;
                                                          x.Lab = model.Courses[i].Lab ?? 0;
                                                          x.Other = model.Courses[i].Other ?? 0;
                                                          x.DescriptionEn = model.Courses[i].DescriptionEn;
                                                          x.DescriptionTh = model.Courses[i].DescriptionTh;
                                                          x.ShortNameEn = model.Courses[i].ShortNameEn;
                                                          x.ShortNameTh = model.Courses[i].ShortNameTh;
                                                          x.IsShowInTranscript = model.Courses[i].IsShowInTranscript;
                                                          x.IsCalculateCredit = model.Courses[i].IsCalculateCredit;
                                                          return x;
                                                      })
                                         .ToList();
                        }
                        else
                        {
                            var newCourse = new Course
                                            {
                                                AcademicLevelId = model.AcademicLevelId,
                                                TransferUniversityId = model.Id,
                                                Code = model.Courses[i].Code,
                                                NameEn = model.Courses[i].NameEn,
                                                NameTh = model.Courses[i].NameTh,
                                                TranscriptNameEn1 = model.Courses[i].TranscriptNameEn1,
                                                TranscriptNameTh1 = model.Courses[i].TranscriptNameTh1,
                                                TranscriptNameEn2 = model.Courses[i].TranscriptNameEn2,
                                                TranscriptNameTh2 = model.Courses[i].TranscriptNameTh2,
                                                TranscriptNameEn3 = model.Courses[i].TranscriptNameEn3,
                                                TranscriptNameTh3 = model.Courses[i].TranscriptNameTh3,
                                                Credit = model.Courses[i].Credit ?? 0,
                                                Lecture = model.Courses[i].Lecture ?? 0,
                                                Lab = model.Courses[i].Lab ?? 0,
                                                Other = model.Courses[i].Other ?? 0,
                                                DescriptionEn = model.Courses[i].DescriptionEn,
                                                DescriptionTh = model.Courses[i].DescriptionTh,
                                                ShortNameEn = model.Courses[i].ShortNameEn,
                                                ShortNameTh = model.Courses[i].ShortNameTh,
                                                IsShowInTranscript = model.Courses[i].IsShowInTranscript,
                                                IsCalculateCredit = model.Courses[i].IsCalculateCredit
                                            };
                            
                            newCourseList.Add(newCourse);
                        }
                    }

                    _db.Courses.AddRange(newCourseList);
                }
                
                _db.Courses.RemoveRange(removeCourses);
                transferUniversity.OHECCode = model.OHECCode;
                transferUniversity.NameEn = model.NameEn;
                transferUniversity.NameTh = model.NameTh;
                transferUniversity.CountryId = model.CountryId;
                transferUniversity.IsActive = model.IsActive;

                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
                return RedirectToAction(nameof(Index), new
                                                       {
                                                           CodeAndName = model.NameEn,
                                                           CountryId = model.CountryId
                                                       });
            }
            catch
            {
                CreateSelectList();
                ViewBag.ReturnUrl = returnUrl;
                _flashMessage.Danger(Message.UnableToEdit);
                return View(model);
            }
        }

        [PermissionAuthorize("TransferUniversity", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            var model = _masterProvider.FindTrasferUniversity(id);
            try
            {
                _db.TransferUniversities.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }

            return RedirectToAction(nameof(Index));
        }

        private static bool IsValidTransferUniversitViewModel(TransferUniversityViewModel model)
        {
            return string.IsNullOrEmpty(model.NameEn)
                                || string.IsNullOrEmpty(model.NameTh)
                                || (model.Courses != null && model.Courses.Where(x => !string.IsNullOrEmpty(x.Code)
                                        || !string.IsNullOrEmpty(x.NameEn)
                                        || !string.IsNullOrEmpty(x.NameTh)
                                        || x.Lecture != null
                                        || x.Lab != null
                                        || x.Other != null
                                        )
                                    .Any(x => string.IsNullOrEmpty(x.Code)
                                        || string.IsNullOrEmpty(x.NameEn)
                                        || string.IsNullOrEmpty(x.NameTh)
                                        ));
        }

        private void CreateSelectList()
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Countries = _selectListProvider.GetCountries();
        }
    }
}