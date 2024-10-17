using AutoMapper;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Providers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Vereyon.Web;

namespace Keystone.Controllers
{
    public class GroupRegistrationController : BaseController
    {
        protected readonly IStudentProvider _studentProvider;
        protected readonly IScheduleProvider _scheduleProvider;
        protected readonly IRegistrationProvider _registrationProvider;

        public GroupRegistrationController(ApplicationDbContext db,
                                           IFlashMessage flashMessage,
                                           IMapper mapper,
                                           IStudentProvider studentProvider,
                                           IScheduleProvider scheduleProvider,
                                           IRegistrationProvider registrationProvider,
                                           ISelectListProvider selectListProvider) : base(db, flashMessage, mapper, selectListProvider) 
        {
            _studentProvider = studentProvider;
            _scheduleProvider = scheduleProvider;
            _registrationProvider = registrationProvider;
        }

        public IActionResult Index(GroupRegistrationViewModel model)
        {
            ViewBag.OpenedCourses = new List<Course>();

            CreateSelectList(model);
            if (model.TermId != 0)
            {
                var selectablePlans = new List<SelectableGroupRegistrationPlan>();
                var plans = _registrationProvider.GetPlans(model.TermId);
                foreach(var item in plans)
                {
                    var selectablePlan = new SelectableGroupRegistrationPlan();
                    var courseIds = _registrationProvider.Deserialize(item.CourseIds);

                    selectablePlan.Plan = item;
                    selectablePlan.Courses = courseIds.Select(x => _registrationProvider.GetCourse(x))
                                                      .ToList();

                    foreach(var schedule in item.PlanSchedules)
                    {
                        var sectionIds = _registrationProvider.Deserialize(schedule.SectionIds);
                        var sections = _db.Sections.Where(x => sectionIds.Any(y => y == x.Id))
                                                   .ToList();
                                                   
                        int minimumSeat = sections.Min(x => x.SeatAvailable);
                        selectablePlan.RegistrableStudentAmount += minimumSeat;
                    }
                    selectablePlans.Add(selectablePlan);
                }

                model.SelectablePlans = selectablePlans;
                ViewBag.OpenedCourses = _registrationProvider.GetOpenedCourses(model.TermId);
            }
            
            return View(model);
        }

        public ActionResult Delete()
        {
            return View();
        }

        public ActionResult Planner()
        {
            return View();
        }

        public ActionResult ChooseSections()
        {
            return PartialView("_SelectSectionsModal");
        }

        public ActionResult SelectPlan(GroupRegistrationViewModel model)
        {
            var plannedSchedules = _registrationProvider.GetPlanSchedules(model.SelectedPlanId);

            model.SelectablePlannedSchedules = new List<SelectablePlannedSchedule>();

            int currentScheduleIndex = 0;

            var desiredStudents = new List<Guid>();
            desiredStudents.AddRange(model.SelectedStudentIds);

            int desiredSeatAmount = desiredStudents.Count;

            while (desiredSeatAmount > 0 && currentScheduleIndex < plannedSchedules.Count)
            {
                var schedule = plannedSchedules[currentScheduleIndex];
                var sectionIds = JsonConvert.DeserializeObject<List<long>>(schedule.SectionIds);
                var sections = _scheduleProvider.GetSchedule(sectionIds);
                int registrableAmount = sections.Min(x => x.SeatAvailable);
                
                model.SelectablePlannedSchedules.Add(new SelectablePlannedSchedule
                                                     {
                                                         PlanSchedule = schedule,
                                                         ScheduleSections = sections,
                                                         RegistrableStudentIds = desiredStudents.Take(registrableAmount)
                                                                                                .ToList(),
                                                         TotalRegistrableStudentAmount = registrableAmount
                                                     });
                
                if (registrableAmount > desiredSeatAmount)
                {
                    desiredStudents.RemoveRange(0, desiredSeatAmount);
                }
                else
                {
                    desiredStudents.RemoveRange(0, registrableAmount);
                }

                desiredSeatAmount -= registrableAmount;
                currentScheduleIndex++;
            }

            model.RegistrableStudentAmount = model.SelectablePlannedSchedules.Sum(x => x.RegistrableStudentIds.Count);

            return PartialView("_SelectSchedule", model);
        }

        public ActionResult SearchStudents(GroupRegistrationViewModel model)
        {
            Int32.TryParse(model.BatchCodeStart, out var batchCodeStart);
            Int32.TryParse(model.BatchCodeEnd, out var batchCodeEnd);

            if (model.SelectedCourseGrades == null)
            {
                model.SelectedCourseGrades = new List<CourseGrade>();
            }

            model.SelectableStudents = _db.Students.Include(x => x.AcademicInformation)
                                                   .Include(x => x.AdmissionInformation)
                                                   .Include(x => x.RegistrationCourses)
                                                   .Where(x => (model.AcademicLevelId == 0
                                                                   || x.AcademicInformation.AcademicLevelId == model.AcademicLevelId)
                                                               && (batchCodeStart == 0 
                                                                   || x.AcademicInformation.Batch >= batchCodeStart)
                                                               && (batchCodeEnd == 0 
                                                                   || x.AcademicInformation.Batch <= batchCodeEnd)
                                                               && (model.FacultyId == 0 
                                                                   || x.AcademicInformation.FacultyId == model.FacultyId)
                                                               && (model.DepartmentId == 0 
                                                                   || x.AcademicInformation.DepartmentId == model.DepartmentId)
                                                               && (model.AdmissionStartedAt == null
                                                                   || model.AdmissionStartedAt <= x.AdmissionInformation.AdmissionDate)
                                                               && (model.AdmissionEndedAt == null
                                                                   || model.AdmissionEndedAt >= x.AdmissionInformation.AdmissionDate)
                                                               && (!model.SelectedCourseGrades.Any()
                                                                   || (model.SelectedCourseGrades.All(y => x.RegistrationCourses.Any(z => z.CourseId == y.CourseId
                                                                                                                                          && (model.GradePublishStartedAt == null
                                                                                                                                              || model.GradePublishStartedAt <= z.GradePublishedAt)
                                                                                                                                          && (model.GradePublishEndedAt == null
                                                                                                                                              || model.GradePublishEndedAt >= z.GradePublishedAt)
                                                                                                                                          && (y.Grade == GradeProvider.GradeAll
                                                                                                                                              || z.GradeName == y.Grade)
                                                                                                                                          || (y.Grade == GradeProvider.GradeNotReleased
                                                                                                                                              && z.Grade == null))))))
                                                   .OrderBy(x => x.Code)
                                                   .Select(x => new SelectableGroupRegistrationStudent
                                                                {
                                                                    Student = x,
                                                                    IsSelected = model.SelectedStudentIds == null ? false : model.SelectedStudentIds.Contains(x.Id)
                                                                })
                                                   .ToList();

            return PartialView("_StudentList", model);
        }

        public ActionResult ListScheduleStudents(GroupRegistrationViewModel model)
        {
            model.RegistrableScheduleStudents = _db.Students.Where(x => model.RegistrableScheduleStudentIds.Any(y => y == x.Id))
                                                            .ToList();

            return PartialView("_ScheduleStudents", model);
        }

        public ActionResult SelectPlannedSchedule(GroupRegistrationViewModel model)
        {
            return View();
        }

        public ActionResult SubmitRegistration(GroupRegistrationViewModel model)
        {
            // Only selected schedules
            var selectedSchedules = model.SelectablePlannedSchedules.Where(x => x.PlanSchedule.Id > 0)
                                                                    .ToList();
            
            var result = _registrationProvider.ModifyRegistrationCourse(selectedSchedules, model.TermId);
            if (result)
            {
                var returnUrl = Url.Action(nameof(Index));
                return Ok(returnUrl);
            }
            else
            {
                return Forbid();
            }
        }

        private void CreateSelectList(GroupRegistrationViewModel model) 
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            if (model.AcademicLevelId != 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(model.AcademicLevelId);
            }

            ViewBag.Grades = _selectListProvider.GetGradeOptions();
            model.Faculties = _selectListProvider.GetFaculties();
            model.Departments = _selectListProvider.GetDepartments(model.FacultyId);
        }
    }
}