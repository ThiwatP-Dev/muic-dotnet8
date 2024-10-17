using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Profile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Vereyon.Web;
using Keystone.Permission;

namespace Keystone.Controllers
{
    [PermissionAuthorize("StudentCourseByPass", "")]
    public class StudentCourseByPassController : BaseController
    {
        private readonly IConfiguration _config;
        private readonly IRegistrationProvider _registrationProvider;
        public StudentCourseByPassController(ApplicationDbContext db,
                                             IFlashMessage flashMessage,
                                             IConfiguration config,
                                             IRegistrationProvider registrationProvider,
                                             ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider)
        {
            _config = config;
            _registrationProvider = registrationProvider;
        }

        public ActionResult Index(Criteria criteria, string returnUrl, int page = 1)
        {
            CreateSelectList(criteria.AcademicLevelId);
            
            var studentCourseByPasses = _db.StudentCourseByPasses.AsNoTracking()
                                                                 .Where(x => (x.TermId == criteria.TermId || criteria.TermId == 0)
                                                                              && (x.Term.AcademicLevelId == criteria.AcademicLevelId || criteria.AcademicLevelId == 0 )
                                                                              && (x.Student.Code.Contains(criteria.CodeAndName)
                                                                                  || x.Student.FirstNameEn.Contains(criteria.CodeAndName)
                                                                                  || x.Student.LastNameEn.Contains(criteria.CodeAndName)
                                                                                  || x.Student.LastNameTh.Contains(criteria.CodeAndName)
                                                                                  || x.Student.FirstNameTh.Contains(criteria.CodeAndName)
                                                                                  || string.IsNullOrEmpty(criteria.CodeAndName)))
                                                                 .IgnoreQueryFilters()
                                                                 .Select(x => new StudentCourseByPassesViewModel
                                                                              {
                                                                                  Id = x.Id,
                                                                                  AcademicLevelNameEn = x.Term.AcademicLevel.NameEn,
                                                                                  StudentCode = x.Student.Code,
                                                                                  Title = x.Student.Title.NameEn,
                                                                                  FirstNameEn = x.Student.FirstNameEn,
                                                                                  MidNameEn = x.Student.MidNameEn,
                                                                                  LastNameEn = x.Student.LastNameEn,
                                                                                  CourseIdsString = x.CourseIds,
                                                                                  TermText = x.Term != null ? x.Term.AcademicTerm + "/" + x.Term.AcademicYear : string.Empty,
                                                                                  Remark = x.Remark,
                                                                                  IsActive = x.IsActive
                                                                              })
                                                                 .GetPaged(criteria, page, true);
            var course = _db.Courses.AsNoTracking()
                                    .ToList();
            foreach(var item in studentCourseByPasses.Results)
            {
                var courseCodes = course.Where(x => item.CourseIds.Contains(x.Id))
                                        .Select(x => $"{x.Code} {x.CreditText}")
                                        .ToList();
                                              
                item.CourseCodes = string.Join(", ", courseCodes);
            }

            return View(studentCourseByPasses);
        }

        [PermissionAuthorize("StudentCourseByPass", PolicyGenerator.Write)]
        public ActionResult Create(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList();
            return View(new StudentCourseByPassViewModel{ IsActive = true });
        }

        [PermissionAuthorize("StudentCourseByPass", PolicyGenerator.Write)]
        [HttpPost]
        public async Task<ActionResult> Create(StudentCourseByPassViewModel model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            if(model.TermId == 0 || string.IsNullOrEmpty(model.StudentId) || (model.CourseIds == null && model.CourseIds?.Count == 0))
            {
                CreateSelectList();
                _flashMessage.Warning(Message.RequiredData);
                return View(model);
            }

            var checkDuplicate = model.CourseIds.GroupBy(x => x).Any(y => y.Count() > 1);

            if(checkDuplicate)
            {
                CreateSelectList(model.AcademicLevelId);
                _flashMessage.Warning(Message.DuplicateCourses);

                return View(model); 
            }

            model.CourseIds = model.CourseIds.OrderBy(x => x).ToList();
            string courseIds = JsonConvert.SerializeObject(model.CourseIds);

            var studentCourseByPass = _db.StudentCourseByPasses.SingleOrDefault(x => x.StudentId == Guid.Parse(model.StudentId) && x.TermId == model.TermId && x.CourseIds == courseIds);

            if(studentCourseByPass != null)
            {
                CreateSelectList(model.AcademicLevelId);
                _flashMessage.Warning(Message.UnableToSaveDuplicate);
                return View(model);
            }

            studentCourseByPass = _db.StudentCourseByPasses.SingleOrDefault(x => x.StudentId == Guid.Parse(model.StudentId) && x.TermId == model.TermId);
            if(studentCourseByPass != null)
            {
                studentCourseByPass.CourseIds = JsonConvert.SerializeObject(model.CourseIds);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
                
                return RedirectToAction(nameof(Index));
            }

            using(var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    if(studentCourseByPass == null)
                    {
                        var result = new StudentCourseByPass
                                    {
                                        StudentId = Guid.Parse(model.StudentId),
                                        TermId = model.TermId,
                                        Remark = model.Remark,
                                        CourseIds = JsonConvert.SerializeObject(model.CourseIds),
                                        IsActive = model.IsActive,
                                    };

                        _db.StudentCourseByPasses.Add(result);
                        _db.SaveChanges();

                        result = _db.StudentCourseByPasses.Include(x => x.Student)
                                                          .Include(x => x.Term)
                                                          .SingleOrDefault(x => x.Id == result.Id);
                        
                        var body = new BodyUpdateStudentCourseByPass
                                   {
                                       StudentCode = result.Student.Code,
                                       Term = result.Term.AcademicTerm,
                                       AcademicYear = result.Term.AcademicYear,
                                       KSCourseIds = JsonConvert.DeserializeObject<List<long>>(result.CourseIds)
                                   };

                        if(await _registrationProvider.UpdateStudentCourseByPass(body))
                        {
                            CreateSelectList(model.AcademicLevelId);
                            transaction.Rollback();
                            _flashMessage.Danger(Message.DatabaseProblem);

                            return View(model);
                        }

                        transaction.Commit();
                        _flashMessage.Confirmation(Message.SaveSucceed);
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        studentCourseByPass.CourseIds = JsonConvert.SerializeObject(model.CourseIds);
                        _db.SaveChanges();

                        var body = new BodyUpdateStudentCourseByPass
                                   {
                                       StudentCode = studentCourseByPass.Student.Code,
                                       Term = studentCourseByPass.Term.AcademicTerm,
                                       AcademicYear = studentCourseByPass.Term.AcademicYear,
                                       KSCourseIds = JsonConvert.DeserializeObject<List<long>>(studentCourseByPass.CourseIds)
                                   };
                        if(await _registrationProvider.UpdateStudentCourseByPass(body))
                        {
                            CreateSelectList(model.AcademicLevelId);
                            transaction.Rollback();
                            _flashMessage.Danger(Message.DatabaseProblem);

                            return View(model);
                        }

                        _flashMessage.Confirmation(Message.SaveSucceed);

                        return RedirectToAction(nameof(Index));
                    }
                }
                catch
                {
                    CreateSelectList();
                    transaction.Rollback();
                    _flashMessage.Danger(Message.UnableToSave);

                    return View(model);
                }
            }
        }

        public ActionResult Edit(long id, string returnUrl)
        {   
            ViewBag.ReturnUrl = returnUrl;
            var studentCourseByPass = _db.StudentCourseByPasses.IgnoreQueryFilters()
                                                                .Include(x => x.Term)
                                                                .SingleOrDefault(x => x.Id == id);

            CreateSelectList(studentCourseByPass.Term.AcademicLevelId);

            var result = new StudentCourseByPassViewModel
                         {
                             Id = studentCourseByPass.Id,
                             AcademicLevelId = studentCourseByPass.Term.AcademicLevelId,
                             TermId = studentCourseByPass.TermId,
                             StudentId = studentCourseByPass.StudentId.ToString(),
                             IsActive = studentCourseByPass.IsActive,
                             Remark = studentCourseByPass.Remark,
                             CourseIds = string.IsNullOrEmpty(studentCourseByPass.CourseIds) ? new List<long>() : JsonConvert.DeserializeObject<List<long>>(studentCourseByPass.CourseIds ?? "")
                         };

            return View(result);
        }

        [PermissionAuthorize("StudentCourseByPass", PolicyGenerator.Write)]
        [HttpPost]
        public async Task<ActionResult> Edit(StudentCourseByPassViewModel model, string returnUrl)
        {   
            ViewBag.ReturnUrl = returnUrl;
            if(model.TermId == 0 || string.IsNullOrEmpty(model.StudentId) || (model.CourseIds == null && model.CourseIds?.Count == 0))
            {
                CreateSelectList();
                _flashMessage.Warning(Message.RequiredData);
                return View(model);
            }

            var checkDuplicate = model.CourseIds.GroupBy(x => x).Any(y => y.Count() > 1);
            if(checkDuplicate)
            {
                CreateSelectList(model.AcademicLevelId);
                _flashMessage.Warning(Message.DuplicateCourses);

                return View(model); 
            }

            var studentCourseByPass = _db.StudentCourseByPasses.IgnoreQueryFilters()
                                                               .Include(x => x.Term)
                                                               .SingleOrDefault(x => x.Id == model.Id);

            CreateSelectList(studentCourseByPass.Term.AcademicLevelId);

            using(var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    studentCourseByPass.TermId = model.TermId;
                    studentCourseByPass.StudentId = Guid.Parse(model.StudentId);
                    studentCourseByPass.Remark = model.Remark;
                    studentCourseByPass.IsActive = model.IsActive;
                    studentCourseByPass.CourseIds = JsonConvert.SerializeObject(model.CourseIds);

                    _db.SaveChanges();

                    var result = _db.StudentCourseByPasses.Include(x => x.Student)
                                                          .Include(x => x.Term)
                                                          .SingleOrDefault(x => x.Id == studentCourseByPass.Id);

                    var body = new BodyUpdateStudentCourseByPass
                               {
                                   StudentCode = result.Student.Code,
                                   Term = result.Term.AcademicTerm,
                                   AcademicYear = result.Term.AcademicYear,
                                   KSCourseIds = JsonConvert.DeserializeObject<List<long>>(result.CourseIds)
                               };

                    if(await _registrationProvider.UpdateStudentCourseByPass(body))
                    {
                        CreateSelectList();
                        transaction.Rollback();
                        _flashMessage.Danger(Message.DatabaseProblem);

                        return View(model);
                    }
                    transaction.Commit();

                    _flashMessage.Confirmation(Message.SaveSucceed);

                    return View(model);
                }
                catch
                {
                    transaction.Rollback();
                    _flashMessage.Danger(Message.UnableToEdit);

                    return View(model);
                }
            }
        }

        [PermissionAuthorize("StudentCourseByPass", PolicyGenerator.Write)]
        public async Task<ActionResult> Delete(long id)
        {
            var studentCourseByPass = _db.StudentCourseByPasses.Include(x => x.Term)
                                                               .Include(x => x.Student)
                                                               .IgnoreQueryFilters()
                                                               .SingleOrDefault(x => x.Id == id);


            using(var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var body = new BodyUpdateStudentCourseByPass
                               {
                                   StudentCode = studentCourseByPass.Student.Code,
                                   Term = studentCourseByPass.Term.AcademicTerm,
                                   AcademicYear = studentCourseByPass.Term.AcademicYear,
                                   KSCourseIds = new List<long>()
                               };

                    _db.StudentCourseByPasses.Remove(studentCourseByPass);
                    _db.SaveChanges();

                    if(await _registrationProvider.UpdateStudentCourseByPass(body))
                    {
                        transaction.Rollback();
                        _flashMessage.Danger(Message.DatabaseProblem);

                        return RedirectToAction(nameof(Index));
                    }
                    transaction.Commit();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    transaction.Rollback();
                    _flashMessage.Danger(Message.UnableToDelete);

                    return RedirectToAction(nameof(Index));
                }
            }
        }

        private void CreateSelectList(long academicLevelId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Courses = _selectListProvider.GetCourses();
            ViewBag.Students = _selectListProvider.GetStudents();
            if(academicLevelId != 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            }

        }
    }
}