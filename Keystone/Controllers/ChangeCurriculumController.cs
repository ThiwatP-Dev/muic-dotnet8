using AutoMapper;
using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Vereyon.Web;

namespace Keystone.Controllers
{
    [PermissionAuthorize("ChangeCurriculum", PolicyGenerator.Write)]
    public class ChangeCurriculumController : BaseController
    {
        protected readonly ICacheProvider _cacheProvider;
        protected readonly IRegistrationProvider _registrationProvider;
        protected readonly IStudentProvider _studentProvider;
        protected readonly ICurriculumProvider _curriculumProvider;
        protected readonly IGradeProvider _gradeProvider;
        
        public ChangeCurriculumController(ApplicationDbContext db, 
                                          IFlashMessage flashMessage, 
                                          IMapper mapper, 
                                          ISelectListProvider selectListProvider,
                                          ICacheProvider cacheProvider,
                                          IStudentProvider studentProvider,
                                          IRegistrationProvider registrationProvider,
                                          ICurriculumProvider curriculumProvider,
                                          IGradeProvider gradeProvider) : base(db, flashMessage, mapper, selectListProvider)
        {
            _cacheProvider = cacheProvider;
            _registrationProvider = registrationProvider;
            _studentProvider = studentProvider;
            _curriculumProvider = curriculumProvider;
            _gradeProvider = gradeProvider;
        }

        [RequestFormLimits(ValueCountLimit = Int32.MaxValue)]
        public IActionResult Index(StudentTransferViewModel model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var isExist = _studentProvider.IsExistStudent(model.StudentCode);
            if (isExist) 
            {
                var student = _studentProvider.GetStudentInformationByCode(model.StudentCode);
                model.StudentId = student.Id;
                model.StudentCode = student.Code;
                model.StudentFirstName = student.FirstNameEn;
                model.StudentLastName = student.LastNameEn;
                model.AcademicLevel = student.AcademicInformation.AcademicLevel.NameEn;
                model.FacultyName = student.AcademicInformation.Faculty.NameEn;
                model.DepartmentName = student.AcademicInformation.Department?.Code ?? "N/A";
                model.Credit = student.AcademicInformation.CreditComp;
                model.GPA = student.AcademicInformation.GPA;
                model.CurriculumName = student.AcademicInformation.CurriculumVersion?.Curriculum?.NameEn ?? "N/A";
                model.CurriculumVersionName = student.AcademicInformation.CurriculumVersion?.NameEn ?? "N/A";
                model.CurriculumId = model.CurriculumId;
                model.CurriculumVersionId = model.CurriculumVersionId;
                model.ChangedMajorCount = student.AcademicInformation.ChangedMajorCount;
                if (!model.StudentCourses.Any())
                {
                    model.StudentCourses = _registrationProvider.GetRegistrationCourses(student.Id);
                }
                else
                {
                    var courseTermIds = model.StudentCourses.Select(x => x.TermId).ToList();
                    var sectionIds = model.StudentCourses.Select(x => x.SectionId).ToList();
                    var term = _db.Terms.Where(x => courseTermIds.Contains(x.Id)).ToList();
                    var section = _db.Sections.Where(x => sectionIds.Contains(x.Id)).ToList();
                    foreach (var item in model.StudentCourses)
                    {
                        item.TermText = term.SingleOrDefault(x => x.Id == item.TermId).TermText;
                        item.SectionNumber = section.SingleOrDefault(x => x.Id == item.SectionId)?.Number;
                        ViewBag.NewCourses = new List<SelectList>();
                        ViewBag.NewCourses.Add(_selectListProvider.GetCoursesByTerm(item.TermId));
                    }
                }
                //model.StudentCourseEquivalents = _registrationProvider.GetRegistrationEquivalentCourses(model.StudentCourses);
                
                CreateSelectList(model.StudentCode, student.AcademicInformation.CurriculumVersion?.Curriculum?.Id ?? 0,
                                 student.AcademicInformation.AcademicLevelId, model.CurriculumVersionId);
                return View(model);
            }
            else 
            {
                _flashMessage.Warning(Message.StudentNotFound);
                model.StudentCourses = new List<StudentTransferCourseViewModel>();
                //model.StudentCourseEquivalents = new List<StudentCourseEquivalentViewModel>();
                return View(model);
            }
        }

        [RequestFormLimits(ValueCountLimit = Int32.MaxValue)]
        public IActionResult MatchCourses(StudentTransferViewModel model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (model.CurriculumVersionId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return RedirectToAction(nameof(Index), new { studentCode = model.StudentCode });
            }

            if (model.CurriculumId != 0) 
            {
                var curriculum = _curriculumProvider.GetCurriculum(model.CurriculumId);
                model.CurriculumName = curriculum.NameEn;
            }
            if (model.CurriculumVersionId != 0) 
            {
                var curriculumVersion = _curriculumProvider.GetCurriculumVersion(model.CurriculumVersionId);
                model.CurriculumVersionName = curriculumVersion.NameEn;
            }
            
            model.StudentCourseEquivalents = _registrationProvider.GetRegistrationEquivalentCourses(model.StudentCourses, model.CurriculumVersionId);
            model.StudentCourses = model.StudentCourses.Where(x => x.CourseId > 0).ToList();
            
            var student = _studentProvider.GetStudentInformationByCode(model.StudentCode);
            CreateSelectList(model.StudentCode, student.AcademicInformation.CurriculumVersion.Curriculum.Id, 
                             student.AcademicInformation.AcademicLevelId, model.CurriculumVersionId);

            return View("~/Views/ChangeCurriculum/MatchCourses.cshtml", model);
        }

        [HttpPost]
        [RequestFormLimits(ValueCountLimit = Int32.MaxValue)]
        public IActionResult Summary(StudentTransferViewModel model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.DisplayQuestion = "Course is Empty, Are you sure to save this form?";
            model.StudentCourseEquivalents = model.StudentCourseEquivalents.Where(x => x.IsChecked != null)
                                                                           .Select(x => {
                                                                                            x.CurrentCourseName = _db.Courses.SingleOrDefault(y => y.Id == x.CurrentCourseId)?.CourseAndCredit;
                                                                                            x.CurrentCourseGrade = _db.Grades.SingleOrDefault(y => y.Id == x.GradeId).Name;
                                                                                            x.NewCourseName = x.NewCourseId != 0 
                                                                                                              ? _db.Courses.SingleOrDefault(y => y.Id == x.NewCourseId).CourseAndCredit
                                                                                                              : _db.Courses.SingleOrDefault(y => y.Id == x.CurrentCourseId).CourseAndCredit;
                                                                                            x.GradeName = x.NewGradeId != null
                                                                                                          ? _db.Grades.SingleOrDefault(y => y.Id == x.NewGradeId).Name
                                                                                                          : _db.Grades.SingleOrDefault(y => y.Id == x.GradeId).Name;
                                                                                            return x;
                                                                                        })
                                                                           .ToList();
            ModelState.Clear();
            return View("~/Views/ChangeCurriculum/Summary.cshtml", model);
        }

        [RequestFormLimits(ValueCountLimit = Int32.MaxValue)]
        public IActionResult Submit(StudentTransferViewModel model, string returnUrl)
        {
            //Save to database
            var student = _studentProvider.GetStudentInformationById(model.StudentId);
            var term = _cacheProvider.GetCurrentTerm(student.AcademicInformation.AcademicLevelId);

            try
            {
                if (_registrationProvider.ChangeCurriculum(model, student, term))
                {
                    _flashMessage.Confirmation(Message.SaveSucceed);
                }
                else
                {
                    _flashMessage.Danger(Message.UnableToEdit);
                }
            }
            catch (Exception ex)
            {
                _flashMessage.Danger(Message.UnableToEdit + " " + ex.Message);
            }

            var courseTermIds = model.StudentCourses.Select(x => x.TermId).ToList();
            var sectionIds = model.StudentCourses.Select(x => x.SectionId).ToList();
            var courseTerms = _db.Terms.Where(x => courseTermIds.Contains(x.Id)).ToList();
            var section = _db.Sections.Where(x => sectionIds.Contains(x.Id)).ToList();
            foreach (var item in model.StudentCourses)
            {
                item.TermText = courseTerms.SingleOrDefault(x => x.Id == item.TermId).TermText;
                item.SectionNumber = section.SingleOrDefault(x => x.Id == item.SectionId)?.Number;
                ViewBag.NewCourses = new List<SelectList>();
                ViewBag.NewCourses.Add(_selectListProvider.GetCoursesByTerm(item.TermId));
            }

            var studentTransfer = new StudentTransferViewModel
                                  {
                                      StudentId = student.Id,
                                      StudentFirstName = student.FirstNameEn,
                                      StudentLastName = student.LastNameEn,
                                      AcademicLevel = student.AcademicInformation.AcademicLevel.NameEn,
                                      FacultyName = student.AcademicInformation.Faculty.NameEn,
                                      DepartmentName = student.AcademicInformation.Department?.Code ?? "N/A",
                                      Credit = student.AcademicInformation.CreditComp,
                                      GPA = student.AcademicInformation.GPA,
                                      CurriculumName = student.AcademicInformation.CurriculumVersion?.Curriculum?.NameEn ?? "N/A",
                                      CurriculumVersionName = student.AcademicInformation.CurriculumVersion?.NameEn ?? "N/A",
                                      CurriculumId = model.CurriculumId,
                                      CurriculumVersionId = model.CurriculumVersionId,
                                      ChangedMajorCount = student.AcademicInformation.ChangedMajorCount,
                                      StudentCode = model.StudentCode,
                                      StudentCourses = model.StudentCourses
                                  };
            

            CreateSelectList(model.StudentCode, model.CurriculumId, student.AcademicInformation.AcademicLevelId, model.CurriculumVersionId);
            return View("~/Views/ChangeCurriculum/Index.cshtml", studentTransfer);
        }

        private void CreateSelectList(string studentCode, long curriculumId, long academicLevelId, long newCurriculumVersionId)
        {
            ViewBag.Curriculums = _selectListProvider.GetSelectableCurriculumList(studentCode);
            ViewBag.CurriculumVersions = _selectListProvider.GetSelectableCurriculumVersionList(curriculumId, studentCode);
            ViewBag.Grades = _selectListProvider.GetGrades();
            if (academicLevelId > 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
                ViewBag.Courses = _selectListProvider.GetCoursesByCurriculumVersion(academicLevelId, newCurriculumVersionId);
            }
            
            if (newCurriculumVersionId > 0) 
            {
                ViewBag.CourseGroups = _selectListProvider.GetCurriculumCourseGroupsForChangeCurriculum(newCurriculumVersionId);
            }
        }
    }
}