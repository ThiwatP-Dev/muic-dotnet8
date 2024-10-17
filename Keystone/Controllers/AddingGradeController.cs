using AutoMapper;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    public class AddingGradeController : BaseController
    {
        private UserManager<ApplicationUser> _userManager { get; }
        protected readonly IStudentProvider _studentProvider;
        protected readonly IGradeProvider _gradeProvider;

        public AddingGradeController(ApplicationDbContext db,
                                     IFlashMessage flashMessage,
                                     ISelectListProvider selectListProvider,
                                     IMapper mapper,
                                     UserManager<ApplicationUser> user,
                                     IStudentProvider studentProvider,
                                     IGradeProvider gradeProvider) : base(db, flashMessage, mapper, selectListProvider)
        {
            _userManager = user;
            _studentProvider = studentProvider;
            _gradeProvider = gradeProvider;
        }
        
        public IActionResult Index(Criteria criteria, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (string.IsNullOrEmpty(criteria.Code) && !_studentProvider.IsExistStudent(criteria.Code))
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }
            
            var student = _studentProvider.GetStudentByCode(criteria.Code);
            CreateSelectList(student.AcademicInformation.AcademicLevelId);

            var model = _db.Students.Include(x => x.AcademicInformation)
                                        .ThenInclude(x => x.CurriculumVersion)
                                        .ThenInclude(x => x.Curriculum)
                                    .Include(x => x.AcademicInformation)
                                        .ThenInclude(x => x.Department)
                                        .ThenInclude(x => x.Faculty)
                                    .Include(x => x.RegistrationCourses)
                                        .ThenInclude(x => x.Term)
                                    .Include(x => x.RegistrationCourses)
                                        .ThenInclude(x => x.Course)
                                    .Include(x => x.RegistrationCourses)
                                        .ThenInclude(x => x.Grade)
                                    .Where(x => x.Code == criteria.Code)
                                    .Select(x => new AddingGradeViewModel
                                                 {
                                                     Criteria = criteria,
                                                     StudentId = x.Id,
                                                     Student = x,
                                                     RegistrationCourses = x.RegistrationCourses.OrderBy(y => y.Term.AcademicYear)
                                                                                                .ThenBy(y => y.Term.AcademicTerm)
                                                                                                .ThenBy(y => y.Course.Code) // sort by course not working
                                                                                                .ToList()
                                                 })
                                    .SingleOrDefault();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddRegistrationCourse(AddingGradeViewModel model)
        {
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    model.AddingCourses.Select(x => {
                                                        x.StudentId = model.StudentId;
                                                        x.GradeName = _gradeProvider.GetGradeById(x.GradeId ?? 0)?.Name ?? "";
                                                        x.GradePublishedAt = model.ApproveDate;
                                                        x.IsGradePublished = true;
                                                        x.IsSurveyed = true;
                                                        x.Status = "a";
                                                        x.IsPaid = true;
                                                        return x;
                                                    })
                                             .ToList();

                    _db.RegistrationCourses.AddRange(model.AddingCourses);
                    _db.SaveChanges();

                    var terms = model.AddingCourses.OrderBy(x => x.TermId).Select(x => x.TermId).Distinct().ToList();
                    foreach(var termId in terms)
                    {
                        _studentProvider.UpdateTermGrade(model.StudentId, termId);
                    }
                    _studentProvider.UpdateCGPA(model.StudentId);
                
                    var user = await _userManager.GetUserAsync(User);
                    var gradingLogs = model.AddingCourses.Select(x => new GradingLog
                                                                      {
                                                                          RegistrationCourseId = x.Id,
                                                                          PreviousGrade = "",
                                                                          CurrentGrade = x.GradeName,
                                                                          ApprovedAt = x.GradePublishedAt,
                                                                          ApprovedBy = user == null ? "" : user.NormalizedUserName,
                                                                          Type = "a",
                                                                          Remark = "Add Grade:" + x.Remark
                                                                      })
                                                               .ToList();
                    _db.GradingLogs.AddRange(gradingLogs);
                    _db.SaveChanges();
                    
                    transaction.Commit();
                    _flashMessage.Confirmation(Message.SaveSucceed);

                    return RedirectToAction(nameof(Index), new Criteria { Code = model.Criteria.Code });
                }
                catch
                {
                    transaction.Rollback();
                    _flashMessage.Danger(Message.UnableToCreate);

                    return View(nameof(Index), model);
                }
            }
        }

        public void CreateSelectList(long academicLevelId)
        {
            ViewBag.Signatories = _selectListProvider.GetSignatories();
            ViewBag.Grades = _selectListProvider.GetGrades();
            ViewBag.Courses = _selectListProvider.GetCourses();
            if (academicLevelId != 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            }
        }
    }
}