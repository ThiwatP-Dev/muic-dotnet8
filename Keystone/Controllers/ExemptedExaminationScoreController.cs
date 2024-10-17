using AutoMapper;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Admission;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    public class ExemptedExaminationScoreController : BaseController
    {
        protected readonly IDateTimeProvider _dateTimeProvider;
        public ExemptedExaminationScoreController(ApplicationDbContext db,
                                                  IFlashMessage flashMessage,
                                                  IMapper mapper,
                                                  ISelectListProvider selectListProvider,
                                                  IDateTimeProvider dateTimeProvider) : base(db, flashMessage, mapper, selectListProvider)
        {
            _dateTimeProvider = dateTimeProvider;
        }

        public IActionResult Index(Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId);
            if (criteria.ExemptedAdmissionExaminationId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            DateTime? startedAt = _dateTimeProvider.ConvertStringToDateTime(criteria.StartedAt);
            var exemptedExaminationScores = _db.ExemptedExaminationScores.Include(x => x.ExemptedAdmissionExamination)
                                                                         .Include(x => x.AdmissionType)
                                                                         .Include(x => x.Faculty)
                                                                         .Include(x => x.Department)
                                                                         .Include(x => x.Course)
                                                                         .Where(x => (criteria.ExemptedAdmissionExaminationId == 0
                                                                                      || x.ExemptedAdmissionExaminationId == criteria.ExemptedAdmissionExaminationId)
                                                                                     && (startedAt == null
                                                                                         || x.StartedAt >= startedAt.Value.Date)
                                                                                     && (criteria.AdmissionTypeId == 0
                                                                                         || x.AdmissionTypeId == criteria.AdmissionTypeId)
                                                                                     && (criteria.CourseId == 0
                                                                                         || x.CourseId == criteria.CourseId)
                                                                                     && (criteria.FacultyId == 0
                                                                                         || x.FacultyId == criteria.FacultyId)
                                                                                     && (criteria.DepartmentId == 0
                                                                                         || x.DepartmentId == criteria.DepartmentId))
                                                                         .ToList();

            var scores = exemptedExaminationScores.GroupBy(x => new { x.ExemptedAdmissionExaminationId, x.AdmissionTypeId, x.StartedAt, x.EndedAt })
                                                  .Select(x => new ExemptedExaminationScoreDetail
                                                               {
                                                                   ExemptedExaminationId = x.Key.ExemptedAdmissionExaminationId,
                                                                   AdmissionTypeId = x.Key.AdmissionTypeId ?? 0,
                                                                   ExemptedExaminationName = x.FirstOrDefault().ExemptedAdmissionExamination.NameEn,
                                                                   AdmissionTypeName = x.FirstOrDefault().AdmissionType.NameEn,
                                                                   IssuedDate = x.Key.StartedAt,
                                                                   ExpiredDate = x.Key.EndedAt,
                                                                   FacultyName = string.Join(", ", x.Where(y => y.Faculty != null)
                                                                                                    .Select(y => y.Faculty.NameEn)
                                                                                                    .Distinct()),
                                                                   DepartmentName = string.Join(", ", x.Where(y => y.Department != null)
                                                                                                       .Select(y => y.Department.NameEn)
                                                                                                       .Distinct()),
                                                                   IsActive = x.FirstOrDefault().IsActive,
                                                                   PreferredCourses = x.GroupBy(y => y.Course)
                                                                                                    .Select(z => new PreferredCourse
                                                                                                                 {
                                                                                                                     MinScore = z.FirstOrDefault().MinimumScore,
                                                                                                                     MaxScore = z.FirstOrDefault().MaximumScore,
                                                                                                                     CourseName = z.FirstOrDefault().Course.CodeAndName
                                                                                                                 })
                                                                                                    .ToList()
                                                               })
                                                  .ToList();

            var model = new ExemptedExaminationScoreViewModel
            {
                Criteria = criteria,
                ExemptedExaminationScoreDetails = scores
            };

            return View(model);
        }

        public IActionResult Create(string returnUrl)
        {
            CreateSelectList();
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ExemptedExaminationScoreDetail model)
        {
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    model.IssuedDate = _dateTimeProvider.ConvertStringToDateTime(model.IssuedDateInput);
                    model.ExpiredDate = _dateTimeProvider.ConvertStringToDateTime(model.ExpiredDateInput);

                    var exemptedExaminationScores = MapExemptedExaminationScores(model);
                    _db.ExemptedExaminationScores.AddRange(exemptedExaminationScores);
                    _db.SaveChanges();
                    transaction.Commit();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index), new Criteria
                    {
                        ExemptedAdmissionExaminationId = model.ExemptedExaminationId
                    });
                }
                catch
                {
                    transaction.Rollback();
                    CreateSelectList();
                    _flashMessage.Danger(Message.UnableToCreate);
                    return View(model);
                }
            }
        }

        public IActionResult Edit(long exemptedExaminationId, long admissionTypeId, DateTime? issuedDate, DateTime? expiredDate, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var exemptedExaminationScores = _db.ExemptedExaminationScores.Include(x => x.Course)
                                                                         .Where(x => x.ExemptedAdmissionExaminationId == exemptedExaminationId
                                                                                     && x.AdmissionTypeId == admissionTypeId
                                                                                     && ((issuedDate == null && x.StartedAt == null)
                                                                                         || (x.StartedAt != null
                                                                                             && issuedDate != null
                                                                                             && x.StartedAt.Value.Date == issuedDate.Value.Date))
                                                                                     && ((expiredDate == null && x.EndedAt == null)
                                                                                         || (x.EndedAt != null
                                                                                             && expiredDate != null
                                                                                             && x.EndedAt.Value.Date == expiredDate.Value.Date)))
                                                                         .ToList();

            var model = exemptedExaminationScores.GroupBy(x => new { x.ExemptedAdmissionExaminationId, x.AdmissionTypeId, x.StartedAt, x.EndedAt })
                                                 .Select(x => new ExemptedExaminationScoreDetail
                                                 {
                                                     ExemptedExaminationId = x.Key.ExemptedAdmissionExaminationId,
                                                     AcademicLevelId = x.FirstOrDefault()?.Course?.AcademicLevelId ?? 0,
                                                     AdmissionTypeId = x.Key.AdmissionTypeId ?? 0,
                                                     IssuedDate = x.Key.StartedAt,
                                                     ExpiredDate = x.Key.EndedAt,
                                                     IssuedDateInput = x.Key.StartedAt?.ToString(StringFormat.ShortDate),
                                                     ExpiredDateInput = x.Key.EndedAt?.ToString(StringFormat.ShortDate),
                                                     PreferredCourses = x.GroupBy(y => y.Course)
                                                                         .Select(z => new PreferredCourse
                                                                         {
                                                                             MinScore = z.FirstOrDefault().MinimumScore,
                                                                             MaxScore = z.FirstOrDefault().MaximumScore,
                                                                             CourseId = z.FirstOrDefault().CourseId,
                                                                             Remark = z.FirstOrDefault().Remark
                                                                         })
                                                                         .ToList(),
                                                     AffectedFacultyDepartments = exemptedExaminationScores.GroupBy(y => y.FacultyId)
                                                                                                                        .Select(y => new AffectedFacultyDepartment
                                                                                                                        {
                                                                                                                            FacultyId = y.Key.GetValueOrDefault(0),
                                                                                                                            DepartmentIds = y.Where(z => z.FacultyId == y.Key.GetValueOrDefault(0)
                                                                                                                                                         && z.DepartmentId.HasValue)
                                                                                                                                                          .Select(z => z.DepartmentId.Value)
                                                                                                                                                          .Distinct()
                                                                                                                                                          .ToList(),
                                                                                                                            FacultyDepartments = _selectListProvider.GetDepartments(y.Key.GetValueOrDefault(0))
                                                                                                                        })
                                                                                                                        .ToList()
                                                 })
                                                 .SingleOrDefault();

            CreateSelectList(model?.AcademicLevelId ?? 0);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ExemptedExaminationScoreDetail model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var removeCourses = _db.ExemptedExaminationScores.Where(x => model.ExemptedExaminationId == x.ExemptedAdmissionExaminationId
                                                                         && model.PreferredCourses.Select(y => y.CourseId).Contains(x.CourseId)
                                                                         && model.PreferredCourses.Select(y => y.MinScore).Contains(x.MinimumScore)
                                                                         && model.PreferredCourses.Select(y => y.MaxScore).Contains(x.MaximumScore));
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    model.IssuedDate = _dateTimeProvider.ConvertStringToDateTime(model.IssuedDateInput);
                    model.ExpiredDate = _dateTimeProvider.ConvertStringToDateTime(model.ExpiredDateInput);

                    var exemptedExaminationScores = MapExemptedExaminationScores(model);
                    _db.ExemptedExaminationScores.RemoveRange(removeCourses);
                    _db.SaveChanges();

                    _db.ExemptedExaminationScores.AddRange(exemptedExaminationScores);
                    _db.SaveChanges();

                    transaction.Commit();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index), new Criteria
                    {
                        ExemptedAdmissionExaminationId = model.ExemptedExaminationId
                    });
                }
                catch
                {
                    transaction.Rollback();
                    CreateSelectList(model.AcademicLevelId);
                    _flashMessage.Danger(Message.UnableToCreate);
                    return View(model);
                }
            }
        }

        public IActionResult Delete(long exemptedExaminationId, long admissionTypeId, DateTime issuedDate, DateTime expiredDate)
        {
            var model = _db.ExemptedExaminationScores.Where(x => x.ExemptedAdmissionExaminationId == exemptedExaminationId
                                                                 && x.AdmissionTypeId == admissionTypeId
                                                                 && x.StartedAt == issuedDate
                                                                 && x.EndedAt == expiredDate)
                                                     .ToList();
            try
            {
                _db.ExemptedExaminationScores.RemoveRange(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }

            return RedirectToAction(nameof(Index), new Criteria
            {
                ExemptedAdmissionExaminationId = exemptedExaminationId
            });
        }

        private void CreateSelectList(long academicLevelId = 0)
        {
            ViewBag.ExemptedAdmissionExaminations = _selectListProvider.GetExemptedAdmissionExaminations();
            ViewBag.AdmissionTypes = _selectListProvider.GetAdmissionTypes();
            ViewBag.Courses = _selectListProvider.GetCourses();
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Faculties = academicLevelId == 0 ? _selectListProvider.GetFaculties()
                                                     : _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
        }

        private List<ExemptedExaminationScore> MapExemptedExaminationScores(ExemptedExaminationScoreDetail model)
        {
            var exemptedExaminationScores = new List<ExemptedExaminationScore>();
            // all faculty, all department
            if (model.AffectedFacultyDepartments == null || !model.AffectedFacultyDepartments.Any())
            {
                foreach (var preferredCourse in model.PreferredCourses)
                {
                    var exemptedExaminationScore = new ExemptedExaminationScore
                                                   {
                                                       ExemptedAdmissionExaminationId = model.ExemptedExaminationId,
                                                       FacultyId = null,
                                                       Department = null,
                                                       AdmissionTypeId = model.AdmissionTypeId,
                                                       StartedAt = model.IssuedDate,
                                                       EndedAt = model.ExpiredDate,
                                                       CourseId = preferredCourse.CourseId,
                                                       MinimumScore = preferredCourse.MinScore,
                                                       MaximumScore = preferredCourse.MaxScore,
                                                       Remark = preferredCourse.Remark
                                                   };

                    exemptedExaminationScores.Add(exemptedExaminationScore);
                }

                return exemptedExaminationScores;
            }

            foreach (var affectedFacultyDepartment in model.AffectedFacultyDepartments)
            {
                if (affectedFacultyDepartment.DepartmentIds != null && affectedFacultyDepartment.DepartmentIds.Any())
                {
                    foreach (var departmentId in affectedFacultyDepartment.DepartmentIds)
                    {
                        foreach (var preferredCourse in model.PreferredCourses)
                        {
                            // specific Faculty and Department 
                            var exemptedExaminationScore = new ExemptedExaminationScore
                                                           {
                                                               ExemptedAdmissionExaminationId = model.ExemptedExaminationId,
                                                               FacultyId = affectedFacultyDepartment.FacultyId,
                                                               DepartmentId = departmentId,
                                                               AdmissionTypeId = model.AdmissionTypeId,
                                                               StartedAt = model.IssuedDate,
                                                               EndedAt = model.ExpiredDate,
                                                               CourseId = preferredCourse.CourseId,
                                                               MinimumScore = preferredCourse.MinScore,
                                                               MaximumScore = preferredCourse.MaxScore,
                                                               Remark = preferredCourse.Remark
                                                           };

                            exemptedExaminationScores.Add(exemptedExaminationScore);
                        }
                    }
                }
                else
                {
                    foreach (var preferredCourse in model.PreferredCourses)
                    {
                        // specific faculty, all department
                        var exemptedExaminationScore = new ExemptedExaminationScore
                                                       {
                                                           ExemptedAdmissionExaminationId = model.ExemptedExaminationId,
                                                           FacultyId = affectedFacultyDepartment.FacultyId,
                                                           DepartmentId = null,
                                                           AdmissionTypeId = model.AdmissionTypeId,
                                                           StartedAt = model.IssuedDate,
                                                           EndedAt = model.ExpiredDate,
                                                           CourseId = preferredCourse.CourseId,
                                                           MinimumScore = preferredCourse.MinScore,
                                                           MaximumScore = preferredCourse.MaxScore,
                                                           Remark = preferredCourse.Remark
                                                       };

                        exemptedExaminationScores.Add(exemptedExaminationScore);
                    }
                }
            }

            return exemptedExaminationScores;
        }
    }
}