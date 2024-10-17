using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{

    public class CoordinatorController : BaseController
    {
        protected readonly IInstructorProvider _instructorProvider;
        protected readonly IAcademicProvider _academicProvider;
        protected readonly IGradeProvider _gradeProvider;
        
        public CoordinatorController(ApplicationDbContext db,
                                     IFlashMessage flashMessage,
                                     ISelectListProvider selectListProvider,
                                     IInstructorProvider instructorProvider,
                                     IAcademicProvider academicProvider,
                                     IGradeProvider gradeProvider) : base(db, flashMessage, selectListProvider) 
        {
            _instructorProvider = instructorProvider;
            _academicProvider = academicProvider;
            _gradeProvider = gradeProvider;
        }

        public IActionResult Index(int page, Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.TermId);
            if (criteria.AcademicLevelId == 0 && criteria.TermId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var models = _db.Coordinators.Include(x => x.Course)
                                         .Include(x => x.Instructor)
                                         .Where(x => x.Course.AcademicLevelId == criteria.AcademicLevelId
                                                     && x.TermId == criteria.TermId
                                                     && (string.IsNullOrEmpty(criteria.CodeAndName)
                                                         || x.Course.CodeAndName.Contains(criteria.CodeAndName))
                                                     && (string.IsNullOrEmpty(criteria.FirstName)
                                                         || x.Instructor.FullNameEn.Contains(criteria.CodeAndName))
                                                     && criteria.AcademicLevelId != 0
                                                     && criteria.TermId != 0)
                                         .GroupBy(x => x.CourseId)
                                         .Select(x => new CoordinatorViewModel
                                                      {
                                                          AcademicLevelId = x.First().Course.AcademicLevelId,
                                                          TermId = x.First().TermId,
                                                          CourseId = x.Key,
                                                          CourseCode = x.First().Course.Code,
                                                          CourseName = x.First().Course.NameEn,
                                                          CoordinatorIds = x.Select(y => y.Id)
                                                                            .ToList(),
                                                          InstructorIds = x.Select(y => y.InstructorId)
                                                                           .ToList(),
                                                          Instructors = x.Select(y => y.Instructor)
                                                                         .ToList(),
                                                          TotalCoordinators = x.Count()
                                                      })
                                         .GetPaged(criteria, page);

            return View(models);
        }

        public IActionResult Manage(CoordinatorViewModel model)
        {
            model.InstructorIds = _gradeProvider.GetCoordinators(model.CourseId, model.TermId)
                                                .Select(x => x.InstructorId)
                                                .ToList();
            
            CreateInstructorsSelectList(model.TermId, model.CourseId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Manage")]
        public ActionResult ManagePost(CoordinatorViewModel coordinatorViewModel)
        {
            if (ModelState.IsValid)
            {
                var model = _gradeProvider.GetCoordinators(coordinatorViewModel.CourseId, coordinatorViewModel.TermId);
                var coordinators = new List<Coordinator>();
                try
                {
                    if (model.Any())
                    {
                        _db.Coordinators.RemoveRange(model);
                    }

                    foreach (var item in coordinatorViewModel.InstructorIds)
                    {
                        coordinators.Add(new Coordinator()
                                         {
                                             TermId = coordinatorViewModel.TermId,
                                             CourseId = coordinatorViewModel.CourseId,
                                             InstructorId = item   
                                         });
                    }

                    _db.Coordinators.AddRange(coordinators);
                    _db.SaveChanges();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index), new { AcademicLevelId = coordinatorViewModel.AcademicLevelId,
                                                                 TermId = coordinatorViewModel.TermId,
                                                                 CodeAndName = coordinatorViewModel.CourseCode });
                }
                catch
                {
                    CreateInstructorsSelectList(coordinatorViewModel.TermId, coordinatorViewModel.CourseId);
                    _flashMessage.Danger(Message.UnableToSave);
                    return View(coordinatorViewModel);
                }
            }

            CreateInstructorsSelectList(coordinatorViewModel.TermId, coordinatorViewModel.CourseId);
            _flashMessage.Danger(Message.UnableToCreate);
            return View(coordinatorViewModel);
        }

        public IActionResult Delete(long termId, long courseId)
        {
            var models = _gradeProvider.GetCoordinators(courseId, termId);                        
            _db.Coordinators.RemoveRange(models);
            _db.SaveChanges();
            _flashMessage.Confirmation(Message.SaveSucceed);
            return RedirectToAction(nameof(Index));
        }

        public void CreateSelectList(long academicLevelId = 0, long termId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();

            if (academicLevelId != 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            }

            if (termId != 0)
            {
                ViewBag.Courses = _selectListProvider.GetCoursesByTerm(termId);
            }
        }

        public JsonResult GetInstructorSearchByCourse(long termId, long courseId)
        {
            var existingInstructorsIds = _gradeProvider.GetCoordinators(courseId, termId)
                                                       .Select(x => x.InstructorId)
                                                       .ToList();

            var instructorsInCourse = GetInstructorsInCourse(termId, courseId);
            var instructorIdsInCourseSelectList = instructorsInCourse.Select(x => new SelectListItem
                                                                                  {
                                                                                       Text = x.CodeAndName,
                                                                                       Value = x.Id.ToString(),
                                                                                       Selected = existingInstructorsIds.Contains(x.Id)
                                                                                  });
            
            var instructorIdsInCourse = instructorsInCourse.Select(x => x.Id)
                                                           .ToList();                                                                      
            var exceptedInstructors = GetExceptedInstructors(instructorIdsInCourse);
            var exceptedInstructorSelectList = exceptedInstructors.Select(x => new SelectListItem
                                                                               {
                                                                                    Text = x.CodeAndName,
                                                                                    Value = x.Id.ToString(),
                                                                                    Selected = existingInstructorsIds.Contains(x.Id)
                                                                               });

            var result = new List<IEnumerable<SelectListItem>>();
            result.Add(instructorIdsInCourseSelectList);
            result.Add(exceptedInstructorSelectList);
                
            return Json(result);
        }

        public List<Instructor> GetInstructorsInCourse(long termId, long courseId)
        {
            var instructorsInCourse = _instructorProvider.GetTermInstructorsByCourseId(termId, courseId);
            return instructorsInCourse;
        }

        public List<Instructor> GetExceptedInstructors(List<long> instructorIds)
        {
            var exceptedInstructors = _db.Instructors.Where(x => !instructorIds.Contains(x.Id))
                                                     .ToList();
            return exceptedInstructors;
        }

        public void CreateInstructorsSelectList(long termId, long courseId)
        {
            var term = _academicProvider.GetTerm(termId);
            ViewBag.AcademicLevel = _academicProvider.GetAcademicLevel(term.AcademicLevelId)?.NameEn;
            ViewBag.Term = term.TermText;
            ViewBag.Courses = _selectListProvider.GetCoursesByTerm(termId);

            var instructorsInCourse = GetInstructorsInCourse(termId, courseId);
            ViewBag.InstructorsInCourse = instructorsInCourse.Select(x => new SelectListItem
                                                                          {
                                                                              Text = x.CodeAndName,
                                                                              Value = x.Id.ToString()
                                                                          });
            
            var instructorIdsInCourse = instructorsInCourse.Select(x => x.Id)
                                                           .ToList();
            ViewBag.ExceptedInstructors = GetExceptedInstructors(instructorIdsInCourse).Select(x => new SelectListItem
                                                                                                    {
                                                                                                        Text = x.CodeAndName,
                                                                                                        Value = x.Id.ToString()
                                                                                                    });
        }
    }
}