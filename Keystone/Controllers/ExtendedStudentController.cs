using AutoMapper;
using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Profile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.MasterTables
{
    [PermissionAuthorize("ExtendedStudent", "")]
    public class ExtendedStudentController : BaseController
    {
        protected readonly IMasterProvider _masterProvider;
        protected readonly ICacheProvider _cacheProvider;

        public ExtendedStudentController(ApplicationDbContext db,
                                         IFlashMessage flashMessage,
                                         IMapper mapper,
                                         ISelectListProvider selectListProvider,
                                         IMasterProvider masterProvider,
                                         ICacheProvider cacheProvider) : base(db, flashMessage, mapper, selectListProvider)
        {
            _masterProvider = masterProvider;
            _cacheProvider = cacheProvider;
        }

        public IActionResult Index(int page, Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId);
            if (criteria.AcademicLevelId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var model = _db.ExtendedStudents.AsNoTracking()
                                            .Include(x => x.Student)
                                                .ThenInclude(x => x.AcademicInformation)
                                                .ThenInclude(x => x.Department)
                                                .ThenInclude(x => x.Faculty)
                                            .Include(x => x.Term)
                                            .Where(x => (criteria.AcademicLevelId == 0
                                                         || x.Student.AcademicInformation.AcademicLevelId == criteria.AcademicLevelId)
                                                        && (criteria.FacultyId == 0
                                                            || x.Student.AcademicInformation.FacultyId == criteria.FacultyId)
                                                        && (criteria.DepartmentId == 0
                                                            || x.Student.AcademicInformation.DepartmentId == criteria.DepartmentId)
                                                        && (String.IsNullOrEmpty(criteria.Code)
                                                            || x.Student.Code.StartsWith(criteria.Code))
                                                        && (string.IsNullOrEmpty(criteria.CodeAndName)
                                                            || x.Student.FirstNameEn.StartsWith(criteria.CodeAndName)
                                                            || x.Student.FirstNameTh.StartsWith(criteria.CodeAndName)
                                                            || x.Student.MidNameEn.StartsWith(criteria.CodeAndName)
                                                            || x.Student.MidNameTh.StartsWith(criteria.CodeAndName)
                                                            || x.Student.LastNameEn.StartsWith(criteria.CodeAndName)
                                                            || x.Student.LastNameTh.StartsWith(criteria.CodeAndName)))
                                            .IgnoreQueryFilters()
                                            .OrderBy(x => x.Student.Code)
                                            .GetPaged(criteria, page);

            return View(model);
        }

        [PermissionAuthorize("ExtendedStudent", PolicyGenerator.Write)]
        public ActionResult Create(Criteria criteria, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.AcademicLevelId = criteria.AcademicLevelId;
            var model = new ExtendedStudentViewModel
                        {
                            Criteria = criteria
                        };

            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId);
            if (criteria.ExtendedYear == 0 || criteria.AcademicLevelId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            model.Students = _db.Students.AsNoTracking()
                                         .Where(x => x.StudentStatus == "s"
                                                     && (x.ExtendedStudents == null
                                                         || !x.ExtendedStudents.Any())
                                                     && (x.GraduationInformations == null
                                                         || !x.GraduationInformations.Any())
                                                     && x.AcademicInformation.AcademicLevelId == criteria.AcademicLevelId
                                                     && (criteria.TermId == 0
                                                         || x.AdmissionInformation.AdmissionTermId == criteria.TermId)
                                                     && (criteria.FacultyId == 0
                                                         || x.AcademicInformation.FacultyId == criteria.FacultyId)
                                                     && (criteria.DepartmentId == 0
                                                         || x.AcademicInformation.DepartmentId == criteria.DepartmentId))
                                         .Select(x => new ExtendedStudentDetail
                                                      {
                                                          StudentId = x.Id,
                                                          StudentCode = x.Code,
                                                          StudentTitle = x.Title.NameEn,
                                                          StudentFirstName = x.FirstNameEn,
                                                          StudentMidName = x.MidNameEn,
                                                          StudentLastName = x.LastNameEn,
                                                          Department = x.AcademicInformation.Department.Code,
                                                          AdmissionTerm = x.AdmissionInformation.AdmissionTerm.TermText,
                                                          CreditComp = x.AcademicInformation.CreditComp,
                                                          StudentEmail = x.Email
                                                      })
                                         .OrderBy(x => x.StudentCode)
                                         .ToList();

            var currentTerm = _cacheProvider.GetCurrentTerm(criteria.AcademicLevelId);
            model.Students.Select(x => {
                                           var admissionTerm = x.AdmissionTerm != null ? x.AdmissionTerm.Split('/') : new string[] {};
                                           if (admissionTerm.Any())
                                           {
                                               var admissionSemester = int.Parse(admissionTerm[0]);
                                               var admissionYear = int.Parse(admissionTerm[1]);
                                               
                                               x.StudiedYear = (currentTerm.AcademicYear - admissionYear) + 1;
                                               if (currentTerm.AcademicTerm < admissionSemester)
                                               {
                                                   x.StudiedYear += (decimal)0.5;
                                               }
                                           }
                                           
                                           x.ExtendedYear = criteria.ExtendedYear;
                                           return x;
                                       })
                          .ToList();

            model.Students = model.Students.Where(x => criteria.ExtendedYear == 0
                                                       || x.StudiedYear >= criteria.ExtendedYear)
                                           .ToList();

            return View(model);
        }

        [PermissionAuthorize("ExtendedStudent", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(List<ExtendedStudentDetail> model, long academicLevelId, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (model == null)
            {
                CreateSelectList();
                _flashMessage.Warning(Message.RequiredData);
                return View(model);
            }

            var termId = _cacheProvider.GetCurrentTerm(academicLevelId)?.Id ?? 0;
            var students = model.Where(x => x.IsCheck)
                                .Select(x => new ExtendedStudent
                                             {
                                                 StudentId = x.StudentId,
                                                 TermId = termId,
                                                 StudiedYear = x.StudiedYear,
                                                 Credit = x.CreditComp,
                                                 SendEmail = false
                                             })
                                .ToList();
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    _db.ExtendedStudents.AddRange(students);
                    _db.SaveChanges();

                    var extendedStudents = _db.Students.Where(x => students.Select(y => y.StudentId)
                                                                        .Contains(x.Id))
                                                    .ToList();

                    var logs = new List<StudentLog>();
                    foreach(var extendedStudent in extendedStudents)
                    {
                        logs.Add(new StudentLog
                                {
                                    StudentId = extendedStudent.Id,
                                    TermId = termId,
                                    Source = "ExtendedStudent",
                                    Log = "Add Extended Student"
                                });
                    }

                    _db.StudentLogs.AddRange(logs);
                    _db.SaveChanges();
                    transaction.Commit();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return Json(new { newUrl = $"/ExtendedStudent/Index/?AcademicLevelId={ academicLevelId }&ExtendedYear={ model.First().ExtendedYear }" });
                }
                catch
                {
                    CreateSelectList(academicLevelId);
                    transaction.Rollback();
                    _flashMessage.Danger(Message.UnableToCreate);
                    return View(model);
                }
            }
        }
      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SendEmail(List<Guid> studentIds)
        {
            return RedirectToAction(nameof(Create));
        }

        [PermissionAuthorize("ExtendedStudent", PolicyGenerator.Write)]
        public ActionResult Delete(long id, string returnUrl)
        {
            var model = _db.ExtendedStudents.SingleOrDefault(x => x.Id == id);
            try
            {
                _db.ExtendedStudents.Remove(model);
                _db.StudentLogs.Add(new StudentLog
                                    {
                                        StudentId = model.StudentId,
                                        TermId = model.TermId,
                                        Source = "Extended Student",
                                        Log = "Remove Extended Student"
                                    });
                
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }

            return Redirect(returnUrl);
        }

        private void CreateSelectList(long academicLevelId = 0, long facultyId = 0) 
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            if (academicLevelId != 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
                ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
            }

            if (facultyId != 0)
            {
                ViewBag.Departments = _selectListProvider.GetDepartments(facultyId);
            }
        }
    }
}