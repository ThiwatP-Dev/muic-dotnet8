using AutoMapper;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.USpark;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Vereyon.Web;

namespace Keystone.Controllers
{
    public class USparkCourseToBeOfferedController : BaseController
    {
        protected readonly IAcademicProvider _academicProvider;
        protected readonly IRegistrationProvider _registrationProvider;
        protected readonly ISectionProvider _sectionProvider;

        public USparkCourseToBeOfferedController(ApplicationDbContext db,
                                           IFlashMessage flashMessage,
                                           IMapper mapper,
                                           IAcademicProvider academicProvider,
                                           IRegistrationProvider registrationProvider,
                                           ISelectListProvider selectListProvider,
                                           ISectionProvider sectionProvider) : base(db, flashMessage, mapper, selectListProvider) 
        {
            _academicProvider = academicProvider;
            _registrationProvider = registrationProvider;
            _sectionProvider = sectionProvider;
        }

        public IActionResult Index(int page, Criteria criteria)
        {
            CreateIndexSelectList(criteria);
            if (criteria.CourseId == 0 || criteria.AcademicLevelId == 0 || criteria.TermId == 0)
            {
                 _flashMessage.Warning(Message.RequiredData);
                return View(new PagedResult<USparkSection>()
                            {
                                Criteria = criteria
                            });
            }

            // MIGRATE RECHECK
            var sections = _db.USparkSections.FromSqlRaw("EXEC [dbo].[spSearchSections] @CourseId, @SemesterId", 1261, 67)
                                             .GetPaged(criteria, page);
            
            // var section = _db.Sections.Include(x => x.SectionDetails)
            //                               .ThenInclude(x => x.InstructorSections)
            //                           .Include(x => x.ParentSection)
            //                           .Include(x => x.Course)
            //                           .Where(x => x.CourseId == criteria.CourseId
            //                                       && (criteria.SectionId == 0
            //                                           || x.Id == criteria.SectionId)
            //                                       && (criteria.TermId == 0
            //                                           || x.TermId == criteria.TermId)
            //                                       && (string.IsNullOrEmpty(criteria.SeatAvailable)
            //                                           || (Convert.ToBoolean(criteria.SeatAvailable) ? x.SeatAvailable > 0
            //                                                                                         : x.SeatAvailable == 0))
            //                                       && (String.IsNullOrEmpty(criteria.IsClosed)
            //                                           || x.IsClosed == Convert.ToBoolean(criteria.IsClosed)
            //                                       && (string.IsNullOrEmpty(criteria.Status)
            //                                           || x.Status == criteria.Status)))
            //                           .OrderBy(x => x.NumberValue)
            //                           .GetPaged(criteria, page);

            return View(sections);
        }
        
        public ActionResult Details(long id)
        {    
            var section = Find(id);
            return PartialView("_DetailsInfo", section);  
        }  
    
        public ActionResult Create(long academicLevelId, long courseId, long termId, string returnUrl)
        {
            var course = _registrationProvider.GetCourse(courseId);
            var examination = _registrationProvider.GetExamDate(courseId, termId);
            var term = _academicProvider.GetTerm(termId);
            Section section = new Section();
            _mapper.Map<Course, Section>(course, section);
            _mapper.Map<Term, Section>(term, section);
            if (examination != null)
            {
                section.MidtermDate = examination.MidtermDate;
                section.MidtermStart = examination.MidtermStart;
                section.MidtermEnd = examination.MidtermEnd;
                section.FinalDate = examination.FinalDate;
                section.FinalStart = examination.FinalStart;
                section.FinalEnd = examination.FinalEnd;
            }

            CreateSelectList(academicLevelId);
            CreateParentSelectList(courseId, termId);
            ViewBag.ReturnUrl = returnUrl;
            return View(section);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Section model, string returnUrl)
        {
            var term = _academicProvider.GetTerm(model.TermId);
            model.AcademicLevelId = term.AcademicLevelId;
            var errorMessage = "";
            if (!_sectionProvider.UpdateOpenedOrClosedAtSection(model, out errorMessage))
            {
                _flashMessage.Warning(errorMessage);
            }

            ViewBag.ReturnUrl = returnUrl;
            if(ModelState.IsValid)
            {
                if (_registrationProvider.IsExistSection(model.TermId, model.CourseId, model.Number))
                {
                    model.Course = _registrationProvider.GetCourse(model.CourseId);

                    CreateSelectList(term.AcademicLevelId);
                    CreateParentSelectList(model.CourseId, model.TermId);
                    _flashMessage.Danger(Message.ExistedSection);
                    return View(model);
                } 
                else 
                {
                    if (model.ParentSectionId != null && model.ParentSectionId != 0)
                    {
                        var parent = _registrationProvider.GetSection(model.ParentSectionId ?? 0);
                        var childSeat = _registrationProvider.GetChildSection(parent.Id).Sum(x => x.SeatAvailable);

                        childSeat += model.SeatAvailable;
                        if (childSeat > parent.SeatLimit)
                        {
                            model.IsSeatOver = true;
                            ViewData["AlertText"] = GetOverSeatMessage(model.Number, parent.Number, parent.SeatLimit);
                            model.Course = _registrationProvider.GetCourse(model.CourseId);
                            CreateSelectList(term.AcademicLevelId);
                            CreateParentSelectList(model.CourseId, model.TermId);
                            return View(model);
                        }
                    }

                    using (var transaction = _db.Database.BeginTransaction())
                    {
                        try
                        {
                            if (model.SharedSections.Any(x => x.CourseId == 0))
                            {
                                model.SharedSections = model.SharedSections.Where(x => x.CourseId != 0)
                                                                           .ToList();
                            }

                            model.StudentIds = JsonConvert.SerializeObject(model.Students) ?? "[]";
                            model.FacultyIds = JsonConvert.SerializeObject(model.Faculties) ?? "[]";
                            model.DepartmentIds = JsonConvert.SerializeObject(model.Departments) ?? "[]";
                            model.CurriculumIds = JsonConvert.SerializeObject(model.Curriculums) ?? "[]";
                            model.CurriculumVersionIds = JsonConvert.SerializeObject(model.CurriculumVersions) ?? "[]";
                            model.Batches = JsonConvert.SerializeObject(model.BatchesInt) ?? "[]";

                            model.SectionDetails = model.SectionDetails.Where(x => x.Day != -1
                                                                                   && x.StartTime != TimeSpan.Zero
                                                                                   && x.EndTime != TimeSpan.Zero)
                                                                       .ToList();
                                                                       
                            SetInstructorIds(model);
                            _db.Sections.Add(model);
                            _db.SaveChanges();

                            var instructorSections = GetInstructorSections(model);
                            _db.InstructorSections.AddRange(instructorSections);
                            _db.SaveChanges();

                            transaction.Commit();
                            _flashMessage.Confirmation(Message.SaveSucceed);
                            return RedirectToAction("Create", "SectionSlot", new 
                                                                             { 
                                                                                 id = model.Id, 
                                                                                 totalWeeks = model.TotalWeeks, 
                                                                                 returnUrl = returnUrl 
                                                                             });
                        }
                        catch
                        {
                            transaction.Rollback();
                            CreateSelectList(term.AcademicLevelId);
                            CreateParentSelectList(model.CourseId, model.TermId);
                            model.Course = _registrationProvider.GetCourse(model.CourseId);
                            _flashMessage.Danger(Message.UnableToCreate);
                            return View(model);
                        }
                    }
                } 
            }
            
            CreateSelectList(term.AcademicLevelId);
            CreateParentSelectList(model.CourseId, model.TermId);
            model.Course = _registrationProvider.GetCourse(model.CourseId);
            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public ActionResult CreateLoads()
        {
            return View();
        }

        public ActionResult Edit(long id, string returnUrl)
        {
            var section = Find(id);
            ViewBag.ReturnUrl = returnUrl;
            CreateParentSelectList(section.CourseId, section.TermId);
            if (section != null)
            {
                var term = _academicProvider.GetTerm(section.TermId);
                section.AcademicLevelId = term.AcademicLevelId;
                section.Students = string.IsNullOrEmpty(section.StudentIds) ? new List<string>()
                                                                            : JsonConvert.DeserializeObject<List<string>>(section.StudentIds);
                section.Faculties = string.IsNullOrEmpty(section.FacultyIds) ? new List<long>()
                                                                             : _registrationProvider.Deserialize(section.FacultyIds);
                section.Departments = string.IsNullOrEmpty(section.DepartmentIds) ? new List<long>()
                                                                                  : _registrationProvider.Deserialize(section.DepartmentIds);
                section.Curriculums = string.IsNullOrEmpty(section.CurriculumIds) ? new List<long>()
                                                                                  : _registrationProvider.Deserialize(section.CurriculumIds);
                section.CurriculumVersions = string.IsNullOrEmpty(section.CurriculumVersionIds) ? new List<long>()
                                                                                                : _registrationProvider.Deserialize(section.CurriculumVersionIds);
                section.BatchesInt = string.IsNullOrEmpty(section.Batches) ? new List<int>()
                                                                           : JsonConvert.DeserializeObject<List<int>>(section.Batches);
                section.SectionDetails.Select(x => {
                                                       x.Instructors = string.IsNullOrEmpty(x.InstructorIds) ? new List<long>()
                                                                                                             : _registrationProvider.Deserialize(x.InstructorIds);
                                                       return x;
                                                   })
                                      .ToList();

                CreateSelectList(term.AcademicLevelId, section.Faculties, section.Departments, section.Curriculums);
                return View(section);
            }
            else
            {
                return View(new Section());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Section section, string returnUrl)
        {
            var originalSection = Find(section.Id);
            var term = _academicProvider.GetTerm(section.TermId);
            section.AcademicLevelId = term.AcademicLevelId;
            ViewBag.ReturnUrl = returnUrl;

            if (ModelState.IsValid )
            {
                if (_registrationProvider.SectionExists(originalSection, section.TermId, section.CourseId, section.Number))
                {
                    CreateSelectList(term.AcademicLevelId);
                    CreateParentSelectList(section.CourseId, section.TermId);
                    section.Course = _registrationProvider.GetCourse(section.CourseId);
                    _flashMessage.Danger(Message.ExistedSection);
                    return View(section);
                }
                else
                {
                    if (section.ParentSectionId != null && section.ParentSectionId > 0)
                    {
                        var parent = _registrationProvider.GetSection(section.ParentSectionId ?? 0);
                        var childSeat = _registrationProvider.GetChildSection(parent.Id).Where(x => x.Id != originalSection.Id).Sum(x => x.SeatAvailable);

                        childSeat += section.SeatAvailable;
                        if (childSeat > parent.SeatLimit)
                        {
                            section.IsSeatOver = true;
                            ViewData["AlertText"] = GetOverSeatMessage(section.Number, parent.Number, parent.SeatLimit);
                            section.Course = _registrationProvider.GetCourse(section.CourseId);
                            CreateSelectList(term.AcademicLevelId);
                            CreateParentSelectList(section.CourseId, section.TermId);
                            return View(section);
                        }
                    }

                    using (var transaction = _db.Database.BeginTransaction())
                    {
                        try
                        {
                            if (section.SharedSections.Any(x => x.CourseId == 0))
                            {
                                section.SharedSections = section.SharedSections.Where(x => x.CourseId != 0)
                                                                                .ToList();
                            }

                            var sectionDetailIds = originalSection.SectionDetails.Select(x => x.Id);
                            var instructorSection = _db.InstructorSections.Where(x => sectionDetailIds.Contains(x.SectionDetailId))
                                                                          .ToList();
                            var sectionDetail = _db.SectionDetails.Where(x => x.SectionId == originalSection.Id)
                                                                  .ToList();

                            if (section.SharedSections != null)
                            {
                                var sharedSectionIds = originalSection.SharedSections.Select(x => x.Id);
                                var sharedSections = _db.SharedSections.Where(x => sharedSectionIds.Contains(x.Id))
                                                                       .ToList();

                                _db.SharedSections.RemoveRange(sharedSections);
                                _db.SharedSections.AddRange(section.SharedSections);
                            }

                            section.StudentIds = JsonConvert.SerializeObject(section.Students) ?? "[]";
                            section.FacultyIds = JsonConvert.SerializeObject(section.Faculties) ?? "[]";
                            section.DepartmentIds = JsonConvert.SerializeObject(section.Departments) ?? "[]";
                            section.CurriculumIds = JsonConvert.SerializeObject(section.Curriculums) ?? "[]";
                            section.CurriculumVersionIds = JsonConvert.SerializeObject(section.CurriculumVersions) ?? "[]";
                            section.Batches = JsonConvert.SerializeObject(section.BatchesInt) ?? "[]";

                            _db.InstructorSections.RemoveRange(instructorSection);
                            _db.SectionDetails.RemoveRange(sectionDetail);
                            _db.SaveChanges();

                            _db.Entry(section).State = EntityState.Modified;
                            _db.Entry(section).Property(x => x.CreatedBy).IsModified = false;
                            _db.Entry(section).Property(x => x.CreatedAt).IsModified = false;
                            _db.Entry(section).Property(x => x.IsActive).IsModified = false;
                            _db.SaveChanges();

                            var sectionDetails = section.SectionDetails.Where(x => x.Day != -1
                                                                                   && x.StartTime != TimeSpan.Zero
                                                                                   && x.EndTime != TimeSpan.Zero)
                                                                       .Select(x => { 
                                                                                        x.Id = 0;
                                                                                        x.InstructorIds = JsonConvert.SerializeObject(x.Instructors) ?? "[]";
                                                                                        return x;
                                                                                    })
                                                                       .ToList();
                            _db.SectionDetails.AddRange(sectionDetails);
                            _db.SaveChanges();

                            var instructorSections = GetInstructorSections(section);
                            _db.InstructorSections.AddRange(instructorSections);

                            var errorMessage = "";
                            if (!_sectionProvider.UpdateOpenedOrClosedAtSection(section, out errorMessage))
                            {
                                _flashMessage.Warning(errorMessage);
                            }

                            _db.SaveChanges();
                            transaction.Commit();
                            _flashMessage.Confirmation(Message.SaveSucceed);
                            return RedirectToAction("Edit", "SectionSlot", new { id = section.Id, returnUrl = returnUrl });
                        }
                        catch
                        {
                            transaction.Rollback();
                            CreateSelectList(term.AcademicLevelId);
                            CreateParentSelectList(section.CourseId, section.TermId);
                            section.Course = _registrationProvider.GetCourse(section.CourseId);
                            _flashMessage.Danger(Message.UnableToEdit);
                            return View(section);
                        }
                    }
                }
            }

            CreateSelectList(term.AcademicLevelId);
            CreateParentSelectList(section.CourseId, section.TermId);
            section.Course = _registrationProvider.GetCourse(section.CourseId);
            _flashMessage.Danger(Message.UnableToEdit);
            return View(section);
        }

        public ActionResult Delete(long id)
        {
            var section = Find(id);
            var term = _academicProvider.GetTerm(section.TermId);
            section.AcademicLevelId = term.AcademicLevelId;
            try
            {
                _db.Sections.Attach(section);
                _db.Sections.Remove(section);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }
            
            return RedirectToAction(nameof(Index), new Criteria 
                                                   {
                                                       CourseId = section.CourseId,
                                                       AcademicLevelId = section.AcademicLevelId, 
                                                       TermId = section.TermId 
                                                   });
        }

        private void CreateSelectList(long academicLevelId = 0, List<long> facultyIds = null, List<long> departmentIds = null, List<long> curriculumIds = null) 
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            if (academicLevelId > 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
                ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
                ViewBag.StudentIds = _selectListProvider.GetStudentsByAcademicLevelId(academicLevelId);
                if (facultyIds != null)
                {
                    ViewBag.Departments = _selectListProvider.GetDepartmentsByFacultyIds(academicLevelId, facultyIds);
                }

                if (departmentIds != null)
                {
                    ViewBag.Curriculums = _selectListProvider.GetCurriculumsByDepartmentIds(academicLevelId, facultyIds, departmentIds);
                }

                if (curriculumIds != null)
                {
                    ViewBag.CurriculumVersions = _selectListProvider.GetCurriculumVersionsByCurriculumIds(academicLevelId, curriculumIds);
                }
            }

            ViewBag.Courses = _selectListProvider.GetCourses();
            ViewBag.Sections = _selectListProvider.GetSections();
            ViewBag.Rooms = _selectListProvider.GetRooms();
            ViewBag.Instructors = _selectListProvider.GetInstructors();
            ViewBag.Dayofweeks = _selectListProvider.GetDayOfWeek(); 
            ViewBag.TeachingTypes = _selectListProvider.GetTeachingTypes();
            ViewBag.OpenCloseStatuses = _selectListProvider.GetOpenCloseStatuses();
            ViewBag.SectionStatuses = _selectListProvider.GetSectionStatuses();
            ViewBag.Batches = _selectListProvider.GetBatches();
            ViewBag.RoomJs = JsonConvert.SerializeObject(_selectListProvider.GetRooms());
            ViewBag.InstructorJs = JsonConvert.SerializeObject(_selectListProvider.GetInstructors());
            ViewBag.DayofweekJs = JsonConvert.SerializeObject(_selectListProvider.GetDayOfWeek()); 
            ViewBag.TeachingTypeJs = JsonConvert.SerializeObject(_selectListProvider.GetTeachingTypes());
            ViewBag.Campuses = _selectListProvider.GetCampuses();
        }

        private void CreateIndexSelectList(Criteria criteria) 
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            if (criteria.AcademicLevelId > 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(criteria.AcademicLevelId);
                ViewBag.Courses = _selectListProvider.GetCoursesBySectionStatus(criteria.TermId, criteria.SectionStatus);
            }

            if (criteria.CourseId > 0)
            {
                ViewBag.Sections = _selectListProvider.GetSections(criteria.TermId, criteria.CourseId);
            }

            ViewBag.OpenCloseStatuses = _selectListProvider.GetOpenCloseStatuses();
            ViewBag.SeatAvailableStatuses = _selectListProvider.GetSeatAvailableStatuses();
            ViewBag.AllYesNoAnswer = _selectListProvider.GetAllYesNoAnswer();
            ViewBag.SectionStatuses = _selectListProvider.GetSectionStatuses();
        }

        private Section Find(long? id)
        {
            var section = _db.Sections.Include(x => x.Course)
                                          .ThenInclude(x => x.Faculty)
                                      .Include(x => x.SectionDetails)
                                          .ThenInclude(x => x.Room)
                                      .Include(x => x.SectionDetails)
                                          .ThenInclude(x => x.InstructorSections)
                                          .ThenInclude(x => x.Instructor)
                                      .Include(x => x.SharedSections)
                                      .AsNoTracking()
                                      .SingleOrDefault(x => x.Id == id);
                                      
            section.SectionDetails = section.SectionDetails.OrderBy(x => x.Day)
                                                           .ThenBy(x => x.StartTime)
                                                           .ToList();
            return section;
        }

        private void CreateParentSelectList(long courseId, long termId)
        {
            ViewBag.ParentSections = _selectListProvider.GetParentSections(courseId, termId);
        }

        private void SetInstructorIds(Section section)
        {
            foreach(var item in section.SectionDetails)
            {
                item.InstructorIds = item.Instructors == null ? "[]" : JsonConvert.SerializeObject(item.Instructors);
            }
        }

        private string GetOverSeatMessage(string sectionNumber, string parentNumber, int seatLimit)
        {
            return $"Seat available of section { sectionNumber } is over than seat limit ({ seatLimit }) of Parent section { parentNumber }.";
        }

        private List<InstructorSection> GetInstructorSections(Section model)
        {
            var instructorSections = new List<InstructorSection>();
            foreach (var sectionDetail in model.SectionDetails)
            {
                if (sectionDetail.InstructorId != null || sectionDetail.InstructorId != 0)
                {
                    instructorSections.Add(new InstructorSection()
                                           {
                                               SectionDetailId = sectionDetail.Id,
                                               InstructorId = sectionDetail.InstructorId ?? 0,
                                               StartedAt = model.OpenedAt,
                                               EndedAt = model.ClosedAt,
                                               Hours = model.TotalWeeks
                                           });
                }
            }

            return instructorSections;
        }

        public ActionResult Close(long id)
        {
            var section = _sectionProvider.GetSection(id);
            var errorMessage = "";
            using (var transaction = _db.Database.BeginTransaction())
            {
                var isClosed = _sectionProvider.CloseSection(section, out errorMessage, transaction);
                if (isClosed)
                {
                    try
                    {
                        _registrationProvider.CallUSparkAPICloseSection(id);
                        transaction.Commit();
                        _flashMessage.Confirmation(Message.SaveSucceed);
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                        _flashMessage.Danger(e.Message);
                    }
                }
                else
                {
                    transaction.Rollback();
                    _flashMessage.Danger(errorMessage);
                }
            }

            return RedirectToAction(nameof(Index), new 
                                                   { 
                                                       AcademicLevelId = section.Course.AcademicLevelId,
                                                       TermId = section.TermId,
                                                       CourseId = section.CourseId
                                                   });
        }

        public ActionResult Open(long id)
        {
            var section = _sectionProvider.GetSection(id);
            var isOpened = _sectionProvider.OpenSection(section);
            if (isOpened)
            {
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            else
            {
                _flashMessage.Danger(Message.UnableToSave);
            }

            return RedirectToAction(nameof(Index), new 
                                                   { 
                                                       AcademicLevelId = section.Course.AcademicLevelId,
                                                       TermId = section.TermId,
                                                       CourseId = section.CourseId
                                                   });
        }

        public decimal MaxSeatLimitBySectionSeatLimit(long seatLimit)
        {
            return seatLimit;
        }
    }
}