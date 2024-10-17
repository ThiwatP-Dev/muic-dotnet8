using AutoMapper;
using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.MasterTables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Vereyon.Web;

namespace Keystone.Controllers
{
    [PermissionAuthorize("CourseToBeOffered", "")]
    public class CourseToBeOfferedController : BaseController
    {
        protected readonly IAcademicProvider _academicProvider;
        protected readonly IRegistrationProvider _registrationProvider;
        protected readonly ISectionProvider _sectionProvider;
        protected readonly ICacheProvider _cacheProvider;
        protected readonly IRoomProvider _roomProvider;
        protected readonly IReservationProvider _reservationProvider;

        public CourseToBeOfferedController(ApplicationDbContext db,
                                           IFlashMessage flashMessage,
                                           IMapper mapper,
                                           IAcademicProvider academicProvider,
                                           IRegistrationProvider registrationProvider,
                                           ISelectListProvider selectListProvider,
                                           ISectionProvider sectionProvider,
                                           IRoomProvider roomProvider,
                                           IReservationProvider reservationProvider,
                                           ICacheProvider cacheProvider) : base(db, flashMessage, mapper, selectListProvider) 
        {
            _academicProvider = academicProvider;
            _registrationProvider = registrationProvider;
            _sectionProvider = sectionProvider;
            _cacheProvider = cacheProvider;
            _roomProvider = roomProvider;
            _reservationProvider = reservationProvider;
        }

        public IActionResult Index(int page, Criteria criteria)
        {
            CreateIndexSelectList(criteria);
            if (criteria.AcademicLevelId == 0 || criteria.TermId == 0)
            {
                criteria.AcademicLevelId = _db.AcademicLevels.SingleOrDefault(x => x.NameEn.ToLower().Contains("bachelor")).Id;
                criteria.TermId = _cacheProvider.GetRegistrationTerm(criteria.AcademicLevelId).Id;
                CreateIndexSelectList(criteria);
                return View(new PagedResult<SectionCourseToBeOfferedViewModel>()
                            {
                                Criteria = criteria
                            });
            }

            var sections = _db.Sections.AsNoTracking()
                                       .Where(x => x.TermId == criteria.TermId
                                                   && (string.IsNullOrEmpty(criteria.CodeAndName)
                                                       || x.Course.Code.Contains(criteria.CodeAndName)
                                                       || x.Course.NameEn.Contains(criteria.CodeAndName)
                                                       || (criteria.CodeAndName.Contains("*")
                                                           && x.Course.CourseRateId == 2))    
                                                   && (criteria.FacultyId == 0
                                                       || x.Course.FacultyId == criteria.FacultyId)
                                                   && (criteria.InstructorId == 0
                                                       || x.MainInstructorId == criteria.InstructorId)
                                                   && (string.IsNullOrEmpty(criteria.SeatAvailable)
                                                       || (Convert.ToBoolean(criteria.SeatAvailable) ? x.SeatAvailable > 0
                                                                                                     : x.SeatAvailable == 0))
                                                   && (criteria.IsClosed == null
                                                       || x.IsClosed == Convert.ToBoolean(criteria.IsClosed))
                                                   && (string.IsNullOrEmpty(criteria.Status)
                                                       || x.Status == criteria.Status)
                                                   && (string.IsNullOrEmpty(criteria.CreatedBy)
                                                       || x.CreatedBy == criteria.CreatedBy)
                                                   && (string.IsNullOrEmpty(criteria.SectionType)
                                                       || (criteria.SectionType == "o" ? x.IsOutbound
                                                                                       : criteria.SectionType == "g"
                                                                                       ? x.IsSpecialCase
                                                                                       : criteria.SectionType == "j" 
                                                                                       ? x.ParentSectionId != null
                                                                                       : x.ParentSectionId == null)))
                                       .Select(x => new SectionCourseToBeOfferedViewModel
                                                    {
                                                        Id = x.Id,
                                                        CourseCode = x.Course.CodeAndSpecialChar,
                                                        CourseName = x.Course.NameEn,
                                                        CourseCredit = x.Course.Credit,
                                                        CourseLab = x.Course.Lab,
                                                        CourseLecture = x.Course.Lecture,
                                                        CourseOther = x.Course.Other,
                                                        Title = x.MainInstructor.Title.NameEn,
                                                        FirstNameEn = x.MainInstructor.FirstNameEn,
                                                        LastNameEn = x.MainInstructor.LastNameEn,
                                                        Number = x.Number,
                                                        MidtermDate = x.MidtermDate,
                                                        MidtermStart = x.MidtermStart,
                                                        MidtermEnd = x.MidtermEnd,
                                                        FinalDate = x.FinalDate,
                                                        FinalStart = x.FinalStart,
                                                        FinalEnd = x.FinalEnd,
                                                        SeatLimit = x.SeatLimit,
                                                        SeatUsed = x.SeatUsed,
                                                        PlanningSeat = x.PlanningSeat,
                                                        Status = x.Status,
                                                        IsClosed = x.IsClosed,
                                                        IsOutbound = x.IsOutbound,
                                                        IsSpecialCase = x.IsSpecialCase,
                                                        ApprovedBy = x.ApprovedBy,
                                                        ApprovedAtText = x.ApprovedAtText,
                                                        Remark = x.Remark,
                                                        ParentSectionId = x.ParentSectionId,
                                                        ParentSectionSeatUsed = x.ParentSection == null ? 0 : x.ParentSection.SeatUsed,
                                                        ParentSectionCourseCode = x.ParentSection.Course.Code,
                                                        ParentCourseRateId = x.ParentSection.Course.CourseRateId,
                                                        ParentSectionNumber = x.ParentSection.Number,
                                                        CreatedBy = x.CreatedBy
                                                    })
                                       .OrderBy(x => x.CourseCode)
                                          .ThenBy(x => x.NumberValue)
                                       .GetPaged(criteria, page);

            var sectionIds = sections.Results.Select(x => x.Id).ToList();
            var sectionIdsNullable = sections.Results.Select(x => (long?)x.Id).ToList();

            var parentSectionIds = sections.Results.Where(x => x.ParentSectionId != null).Select(x => x.ParentSectionId);
            var jointsFromParentSectionIds = _db.Sections.Where(x => parentSectionIds.Contains(x.ParentSectionId));

            var jointSections = _db.Sections.AsNoTracking()
                                            .Where(x => sectionIdsNullable.Contains(x.ParentSectionId))
                                            .Select(x => new JointSectionCourseToBeOfferedViewModel
                                                         {
                                                             Id = x.Id,
                                                             ParentSectionId = x.ParentSectionId,
                                                             Number = x.Number,
                                                             CourseRateId = x.Course.CourseRateId,
                                                             CourseCode = x.Course.Code,
                                                             SeatUsed = x.SeatUsed
                                                         })
                                            .ToList();

            var sectionDetails = _db.SectionDetails.AsNoTracking()
                                                   .Where(x => sectionIds.Contains(x.SectionId))
                                                   .Select(x => new SectionDetailCourseToBeOfferedViewModel
                                                                {
                                                                    SectionId = x.SectionId,
                                                                    Day = x.Day,
                                                                    StartTime = x.StartTime,
                                                                    EndTime = x.EndTime,
                                                                })
                                                   .OrderBy(x => x.Day)
                                                      .ThenBy(x => x.StartTime)
                                                   .ToList();

            var users = _db.Users.AsNoTracking()
                                 .Where(x => sections.Results.Select(y => y.CreatedBy).Contains(x.Id))
                                 .ToList();

            var instructors = _db.Instructors.AsNoTracking()
                                             .Include(x => x.Title)
                                             .Where(x => users.Select(y => y.InstructorId).Contains(x.Id))
                                             .ToList();

            foreach (var item in sections.Results)
            {
                item.JointSections = jointSections.Where(x => x.ParentSectionId == item.Id)
                                                  .ToList();

                if (item.ParentSectionId == null)
                {
                    item.TotalSeatUsed = item.SeatUsed + item.JointSections.Sum(x => x.SeatUsed);
                }
                else
                {
                    item.TotalSeatUsed = item.ParentSectionSeatUsed + jointsFromParentSectionIds.Where(x => x.ParentSectionId == item.ParentSectionId)
                                                                                                .Sum(x => x.SeatUsed);
                }
                item.SectionDetails = sectionDetails.Where(x => x.SectionId == item.Id)
                                                    .OrderBy(x => x.Day)
                                                       .ThenBy(x => x.StartTime)
                                                    .ToList();

                var user = users.SingleOrDefault(x => x.Id == item.CreatedBy);
                var instructor = user != null ? instructors.SingleOrDefault(x => x.Id == user.InstructorId) : null;
                if (instructor != null)
                {
                    item.CreatedByText = $"{ instructor.Title.NameEn } { instructor.FirstNameEn } { instructor.LastNameEn }";
                }
                else if (user != null && !string.IsNullOrEmpty(user.FirstnameEN))
                {
                    item.CreatedByText = $"{ user.FirstnameEN } { user.LastnameEN }"; ;
                }
                else if (user != null)
                {
                    item.CreatedByText = user.UserName;
                }
                else
                {
                    item.CreatedByText = item.CreatedBy;
                }
            }

            return View(sections);
        }
        
        public ActionResult Details(long id)
        {    
            var section = Find(id);

            section.StudentCodes = string.IsNullOrEmpty(section.StudentIds) ? string.Empty 
                                                                            : section.StudentIds.Replace("[", "").Replace("]", "").Replace(",", ", ");
            section.Faculties = string.IsNullOrEmpty(section.FacultyIds) ? new List<long>()
                                                                         : _registrationProvider.Deserialize(section.FacultyIds);
            section.Departments = string.IsNullOrEmpty(section.DepartmentIds) ? new List<long>()
                                                                              : _registrationProvider.Deserialize(section.DepartmentIds);
            section.Curriculums = string.IsNullOrEmpty(section.CurriculumIds) ? new List<long>()
                                                                              : _registrationProvider.Deserialize(section.CurriculumIds);
            section.Minors = string.IsNullOrEmpty(section.MinorIds) ? new List<long>()
                                                                              : _registrationProvider.Deserialize(section.MinorIds);
            section.CurriculumVersions = string.IsNullOrEmpty(section.CurriculumVersionIds) ? new List<long>()
                                                                                            : _registrationProvider.Deserialize(section.CurriculumVersionIds);

            section.Batches = string.IsNullOrEmpty(section.Batches) ? string.Empty
                                                                    : section.Batches.Replace("[", "").Replace("]", "").Replace(",", ", ");;

            if(section.Faculties.Any())
            {
                var faculties = _db.Faculties.Where(x => section.Faculties.Contains(x.Id))
                                             .Select(x => x.Code + " - " + x.NameEn)
                                             .ToList();

                section.FacultiesCodeAndName = faculties;
            }

            if(section.Departments.Any())
            {
                var departments = _db.Departments.Where(x => section.Departments.Contains(x.Id))
                                                 .Select(x => x.Code + " - " + x.NameEn )
                                                 .ToList();

                section.DepartmentsCodeAndName = departments;
            }

            if(section.Curriculums.Any())
            {
                var curriculums = _db.Curriculums.Where(x => section.Curriculums.Contains(x.Id))
                                                 .Select(x => x.AbbreviationEn + " - " + x.NameEn)
                                                 .ToList();

                section.CurriculumsCodeAndName = curriculums;
            }   

            if(section.CurriculumVersions.Any())
            {
                var curriculumVersions = _db.CurriculumVersions.Where(x => section.CurriculumVersions.Contains(x.Id))
                                                               .Select(x => x.NameEn)
                                                               .ToList();

                section.CurriculumVersionsCodeAndName = curriculumVersions;
            }

            if(section.Minors.Any())
            {
                var minors = _db.SpecializationGroups.Where(x => section.Minors.Contains(x.Id) && x.Type == SpecializationGroup.TYPE_MINOR_CODE)
                                                     .Select(x => x.Code + " - " + x.NameEn)
                                                     .ToList();

                section.MinorsCodeAndName = minors;
            }

            return PartialView("_DetailsInfo", section);  
        }

        public ActionResult ChangeMainInstructor(long id, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            //var section = _db.Sections.SingleOrDefault(x => x.Id == id);
            var section = Find(id);
            if (section != null)
            {
                section.SectionDetails.Select(x => {
                    x.Instructors = string.IsNullOrEmpty(x.InstructorIds) ? new List<long>()
                                                                          : _registrationProvider.Deserialize(x.InstructorIds);

                    x.EndTimeNullAble = x.EndTime;
                    x.StartTimeNullAble = x.StartTime;

                    return x;
                })
                                      .OrderBy(x => x.Day)
                                          .ThenBy(x => x.StartTime)
                                      .ToList();
            }
            CreateSelectList(section?.CourseId ?? 0);
            return PartialView("_ChangeMainInstructor", section);
        }
        public ActionResult AddJointSection(long id, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
           
            var section = _db.Sections.AsNoTracking()
                                      .Where(x => x.Id == id)
                                      .Select(x => new AddJointSectionViewModel
                                                   {
                                                       Id = x.Id,
                                                       Number = x.Number,
                                                       CourseId = x.CourseId,
                                                       CourseCode = x.Course.Code,
                                                       CourseName = x.Course.NameEn,
                                                       CourseCredit = x.Course.Credit,
                                                       CourseLab = x.Course.Lab,
                                                       CourseLecture = x.Course.Lecture,
                                                       CourseOther = x.Course.Other,
                                                       CourseRateId = x.Course.CourseRateId,
                                                       SeatLimit = x.SeatLimit,
                                                       SeatUsed = x.SeatUsed,
                                                       PlanningSeat = x.PlanningSeat
                                                   })
                                      .FirstOrDefault();
            CreateSelectList(section?.CourseId ?? 0);

            section.OldJointSections = _db.Sections.AsNoTracking()
                                                  .Where(x => id == x.ParentSectionId)
                                                  .Select(x => new JointSectionCourseToBeOfferedViewModel
                                                               {
                                                                   Id = x.Id,
                                                                   ParentSectionId = x.ParentSectionId,
                                                                   Number = x.Number,
                                                                   CourseRateId = x.Course.CourseRateId,
                                                                   CourseCode = x.Course.Code,
                                                                   SeatLimit = x.SeatLimit,
                                                                   Remark = x.Remark
                                                               })
                                                  .ToList();

            return PartialView("_ContentJointSection", section);
        }

        [PermissionAuthorize("CourseToBeOffered", PolicyGenerator.Write)]
        [HttpPost]
        public ActionResult AddJointSection(AddJointSectionViewModel model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
         
            var masterSection = _db.Sections.AsNoTracking()
                                            .Include(x => x.SectionDetails)
                                            .SingleOrDefault(x => x.Id == model.Id);
            CreateSelectList(masterSection?.CourseId ?? 0);
            var newJointSection = new Section
                                  {
                                      ParentSectionId = masterSection.Id,
                                      Number = masterSection.Number,
                                      MainInstructorId = masterSection.MainInstructorId,
                                      FinalDate = masterSection.FinalDate,
                                      FinalStart = masterSection.FinalStart,
                                      FinalEnd = masterSection.FinalEnd,
                                      MidtermDate = masterSection.MidtermDate,
                                      MidtermEnd = masterSection.MidtermEnd,
                                      MidtermStart = masterSection.MidtermStart,
                                      TermId = masterSection.TermId,
                                      Status = masterSection.Status,
                                      IsClosed = masterSection.IsClosed,
                                      Remark = model.JointSectionRemark,
                                      CourseId = model.JointSectionCourseId,
                                      SeatLimit = model.JointSectionSeatLimit,
                                      SectionDetails = masterSection.SectionDetails?.Select(x => new SectionDetail
                                      {
                                          Day = x.Day,
                                          RoomId = x.RoomId,
                                          TeachingTypeId = x.TeachingTypeId,
                                          StartTime = x.StartTime,
                                          EndTime = x.EndTime,
                                          InstructorIds = x.InstructorIds,
                                          IsActive = x.IsActive,
                                          Remark = x.Remark,
                                          InstructorId = x.InstructorId
                                      }).ToList() ?? new List<SectionDetail>()
                                  };

            if(_registrationProvider.SectionExists(newJointSection, newJointSection.TermId, newJointSection.CourseId, newJointSection.Number))
            {
                var courseCode = _db.Courses.SingleOrDefault(x => x.Id == newJointSection.CourseId).Code;
                var message = $"course: { courseCode } section: { model.Number }, ";
                _flashMessage.Danger("This " + message + "already exist in database");
            }
            else
            {
                try
                {
                    _db.Sections.Add(newJointSection);
                    _db.SaveChanges();
                    _sectionProvider.ReCalculateSeatAvailable(masterSection.Id);
                    _flashMessage.Confirmation(Message.SaveSucceed);
                }
                catch
                {
                    _flashMessage.Danger(Message.UnableToDelete);
                }
            }

            return Redirect(returnUrl);
        }

        [PermissionAuthorize("CourseToBeOffered", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeMainInstructor(Section model, string returnUrl)
        {
            var section = _db.Sections.Include(x => x.SectionDetails).SingleOrDefault(x => x.Id == model.Id);
            var jointSections = _db.Sections.Include(x => x.SectionDetails).Where(x => x.ParentSectionId == section.Id).ToList();
            try
            {
                section.MainInstructorId = model.MainInstructorId;                
                if (jointSections != null)
                {
                    jointSections = jointSections.Select(x => {
                                                                  x.MainInstructorId = model.MainInstructorId;
                                                                  return x;
                                                              }).ToList();
                }

                if (model.SectionDetails != null)
                {
                    var jointDetailsMany = jointSections?.SelectMany(x => x.SectionDetails);
                    foreach (var detail in model.SectionDetails)
                    {
                        var sectionDetail = section.SectionDetails.FirstOrDefault(x => x.Id == detail.Id);
                        if (sectionDetail != null)
                        {
                            sectionDetail.InstructorId = detail.InstructorId;

                            if (jointDetailsMany != null && jointDetailsMany.Any()) {
                                var jointDetails = jointDetailsMany.Where(x => x.Day == sectionDetail.Day
                                                                                 && x.StartTime == sectionDetail.StartTime
                                                                                 && x.EndTime == sectionDetail.EndTime
                                                                                 )
                                                                   .ToList();
                                foreach (var jointDetail in jointDetails)
                                {
                                    jointDetail.InstructorId = detail.InstructorId;
                                }
                            }
                        }

                    }
                }

                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
                return Redirect(returnUrl);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToCreate);
                CreateSelectList(section?.CourseId ?? 0);
                return Redirect(returnUrl);
            }
        }

        [PermissionAuthorize("CourseToBeOffered", PolicyGenerator.Write)]
        public ActionResult Create(long academicLevelId, long courseId, long termId, string returnUrl)
        {
            var term = _academicProvider.GetTerm(termId);
            Section section = new Section();
            _mapper.Map<Term, Section>(term, section);
            section.OpenedAt = term.StartedAt ?? DateTime.Today;
            section.Term = term;
            section.IsMasterSection = true;
            section.Status = "c";
            section.MinimumSeat = 7;
            section.TotalWeeks = (int)(term?.TotalWeeksCount ?? 12);
            section.FirstTeachingTypeId = GetLectureTeachingTypeId("lecture");
            section.HaveSectionSlotRoom = false;
            CreateSelectList(0, academicLevelId);
            CreateParentSelectList(courseId, termId);
            ViewBag.ReturnUrl = returnUrl;
            return View(section);
        }

        [PermissionAuthorize("CourseToBeOffered", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Section model, string returnUrl)
        {
            var term = _academicProvider.GetTerm(model.TermId);
            model.AcademicLevelId = term.AcademicLevelId;
            model.Term = term;
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList(0, term.AcademicLevelId, model.Faculties, model.Departments, model.Curriculums);
            CreateParentSelectList(model.CourseId, model.TermId);

            if (string.IsNullOrEmpty(model.Number))
            {
                _flashMessage.Danger(Message.UnableToCreate);
                return View(model);
            }

            // check overlap course detail 
            if (model.SectionDetails != null)
            {
                var tempId = 0;
                model.SectionDetails = model.SectionDetails.Where(x => x.Day != -1
                                                                       && (x.StartTimeNullAble != TimeSpan.Zero && x.StartTimeNullAble != null)
                                                                       && (x.EndTimeNullAble != TimeSpan.Zero && x.EndTimeNullAble != null)
                                                                       && x.TeachingTypeId != 0)
                                                            .Select(x =>
                                                            {
                                                                x.TempId = tempId++;
                                                                return x;
                                                            })
                                                            .ToList();

                if (model.SectionDetails.Any(x => x.StartTimeNullAble > x.EndTimeNullAble))
                {
                    _flashMessage.Danger(Message.UnableToCreate + "Start time must less than or equal to end time");
                    return View(model);
                }

                var groupByDayOfWeek = (from section in model.SectionDetails
                                        group section by new { section.Day } into sections
                                        select sections
                                       ).ToList();
                foreach (var group in groupByDayOfWeek)
                {
                    foreach (var section in group)
                    {
                        if (group.Any(x => x.TempId != section.TempId 
                                              && section.Day == x.Day
                                              && section.StartTimeNullAble < x.EndTimeNullAble 
                                              && section.EndTimeNullAble > x.StartTimeNullAble))
                        {
                            _flashMessage.Danger(Message.UnableToCreate + "Overlap section detail");
                            return View(model);
                        }
                    }
                }
            }

            var course = _db.Courses.AsNoTracking().FirstOrDefault(x => x.Id == model.CourseId);
            if (course == null)
            {
                _flashMessage.Danger(Message.UnableToCreate);
                return View(model);
            }
            if (!course.IsAllowAddNewSection)
            {
                _flashMessage.Danger(Message.UnableToCreate + ": selected course is currently not allowed to create new section.");
                return View(model);
            }

            if (model.JointSections != null)
            {
                model.JointSections = model.JointSections.Where(x => x.CourseId != 0).ToList();
                if(model.JointSections.Any(x => x.CourseId == model.CourseId))
                {
                    _flashMessage.Danger(Message.SectionCourseDuplicateJoinCourses);
                    return View(model);
                }

                if(model.JointSections.Any(x => x.SeatLimit > model.SeatLimit))
                {
                    _flashMessage.Danger(Message.SeatLimitJointInvalid);
                    return View(model);
                }

                if(model.JointSections.GroupBy(x => x.CourseId).Any(x => x.Count() > 1))
                {
                    _flashMessage.Danger(Message.DuplicateJoinCourses);
                    return View(model);
                }

                string message = string.Empty;
                var removeCourseIds = new List<long>();
                foreach(var item in model.JointSections)
                {
                    if(_registrationProvider.SectionExists(item, model.TermId, item.CourseId, model.Number) && item.CourseId != 0)
                    {
                        var courseCode = _db.Courses.SingleOrDefault(x => x.Id == item.CourseId).Code;
                        removeCourseIds.Add(item.CourseId);
                        message += $"course: { courseCode } section: { model.Number }, ";
                    }
                }

                if(!string.IsNullOrEmpty(message))
                {
                    
                    model.JointSections = model.JointSections.Where(x => !removeCourseIds.Contains(x.CourseId)).ToList();
                    _flashMessage.Danger("This " + message + "already exist in database");
                    return View(model);
                }
            }
            
            if (_registrationProvider.IsExistSection(model.TermId, model.CourseId, model.Number))
            {
                _flashMessage.Danger(Message.ExistedSection);
                return View(model);
            } 

            if (model.SeatLimit - model.SeatUsed < 0)
            {
                _flashMessage.Danger(Message.UnableToEdit);
                return View(model);
            }
            else 
            {
                model.SeatAvailable = model.SeatLimit - model.SeatUsed;
                if(!string.IsNullOrEmpty(model.StudentCodes))
                {
                    var studentCodes = model.StudentCodes.Replace(" ","").Replace("\n","").Replace("\r","").Split(",").ToList();
                    var studentCodeNotfounds = new List<string>();

                    var students = _db.Students.Where(x => studentCodes.Contains(x.Code))
                                               .Select(x => x.Code)
                                               .Distinct()
                                               .OrderBy(x => x)
                                               .ToList();

                    studentCodeNotfounds = studentCodes.Except(students).ToList();

                    if(studentCodeNotfounds.Count != 0)
                    {
                        var studentString = String.Join(", ", studentCodeNotfounds.ToArray());
                        _flashMessage.Danger(Message.StudentNotFound + "(" + studentString +")");

                        return View(model);
                    }
                    if(students.Any())
                    {
                        model.Students = students;
                    }
                    else
                    {
                        model.Students = new List<string>();
                    }
                }

                using (var transaction = _db.Database.BeginTransaction())
                {
                    try
                    {
                        UpdateFieldIds(model);

                        model.SectionDetails.ForEach(x => 
                        {
                            x.InstructorIds = x.Instructors.SerializeMultiple();
                            if ((x.InstructorId ?? 0) > 0)
                            {
                                x.InstructorSections = new List<InstructorSection>()
                                                        {
                                                            new InstructorSection
                                                            {
                                                                InstructorId = x.InstructorId ?? 0,
                                                                StartedAt = model.OpenedAt,
                                                                EndedAt = model.ClosedAt,
                                                                Hours = model.TotalWeeks
                                                            }
                                                        };
                            };

                            x.StartTime = x.StartTimeNullAble ?? new TimeSpan();
                            x.EndTime = x.EndTimeNullAble ?? new TimeSpan();
                        });
                                                    
                        _db.Sections.Add(model);
                        _db.SaveChanges();
                        
                        if(model.IsDisabledMidterm)
                        {
                            model.MidtermStart = null;
                            model.MidtermEnd = null;
                            model.MidtermDate = null;
                            model.MidtermRoomId = null;
                        }
                        else if(model.MidtermDate != null && model.MidtermStart != null && model.MidtermEnd != null)
                        {
                            var examinationTypeMidtermId = _db.ExaminationTypes.SingleOrDefault(x => x.NameEn.ToLower() == "midterm").Id;
                            var result = _reservationProvider.UpdateExaminationReservation(new ExaminationReservation
                                                                                           {
                                                                                               ExaminationTypeId = examinationTypeMidtermId,
                                                                                               StartTime = model.MidtermStart??new TimeSpan(),
                                                                                               EndTime = model.MidtermEnd??new TimeSpan(),
                                                                                               Date = model.MidtermDate??new DateTime(),
                                                                                               SectionId = model.Id,
                                                                                               Status = "w",
                                                                                               InstructorId = model.MainInstructorId,
                                                                                               SenderType = "a",                                                                                                   
                                                                                               AcademicLevelId = model.AcademicLevelId,
                                                                                               TermId = model.TermId,
                                                                                               UseProctor = false,
                                                                                               AbsentInstructor = true,
                                                                                               AllowBooklet = false,
                                                                                               AllowCalculator = false,
                                                                                               AllowOpenbook = false
                                                                                           });

                            switch(result.Status)
                            {
                                case UpdateExamStatus.SaveExamSucceed : 
                                case UpdateExamStatus.UpdateExamSuccess : 
                                    break;

                                case UpdateExamStatus.ExaminationAlreadyApproved :  
                                    _flashMessage.Confirmation(Message.DataAlreadyExist);
                                    model.MidtermStart = null;
                                    model.MidtermEnd = null;
                                    model.MidtermDate = null;
                                    model.MidtermRoomId = null;
                                    break;

                                default :  
                                    model.MidtermStart = null;
                                    model.MidtermEnd = null;
                                    model.MidtermDate = null;
                                    model.MidtermRoomId = null;
                                    _flashMessage.Danger(Message.UnableToCreate + "midterm");
                                    break;
                            }
                        }

                        if(model.IsDisabledFinal)
                        {
                            model.FinalStart = null;
                            model.FinalEnd = null;
                            model.FinalDate = null;
                            model.FinalRoomId = null;
                        }
                        else if(model.FinalDate != null && model.FinalStart != null && model.FinalEnd != null)
                        {
                            var examinationTypeFinalId = _db.ExaminationTypes.SingleOrDefault(x => x.NameEn.ToLower() == "final").Id;
                            var result = _reservationProvider.UpdateExaminationReservation(new ExaminationReservation
                                                                                           {
                                                                                               ExaminationTypeId = examinationTypeFinalId,
                                                                                               StartTime = model.MidtermStart??new TimeSpan(),
                                                                                               EndTime = model.MidtermEnd??new TimeSpan(),
                                                                                               Date = model.MidtermDate??new DateTime(),
                                                                                               SectionId = model.Id,
                                                                                               Status = "w",
                                                                                               InstructorId = model.MainInstructorId,
                                                                                               SenderType = "a",                                                                                                   
                                                                                               AcademicLevelId = model.AcademicLevelId,
                                                                                               TermId = model.TermId,
                                                                                               UseProctor = false,
                                                                                               AbsentInstructor = true,
                                                                                               AllowBooklet = false,
                                                                                               AllowCalculator = false,
                                                                                               AllowOpenbook = false
                                                                                           });

                            switch(result.Status)
                            {
                                case UpdateExamStatus.SaveExamSucceed : 
                                case UpdateExamStatus.UpdateExamSuccess : 
                                    break;

                                case UpdateExamStatus.ExaminationAlreadyApproved :  
                                    _flashMessage.Confirmation(Message.DataAlreadyExist);
                                    model.FinalStart = null;
                                    model.FinalEnd = null;
                                    model.FinalDate = null;
                                    model.FinalRoomId = null;
                                    _db.SaveChanges();
                                    break;

                                default :  
                                    model.FinalStart = null;
                                    model.FinalEnd = null;
                                    model.FinalDate = null;
                                    model.FinalRoomId = null;
                                    _db.SaveChanges();
                                    _flashMessage.Danger(Message.UnableToCreate + "final");
                                    break;
                            }
                        }

                        // JOINT SECTION
                        if (model.JointSections != null)
                        {
                            model.JointSections = model.JointSections.Where(x => x.CourseId != 0)
                                                                     .ToList();
                            if (model.JointSections != null)
                            {
                                model.JointSections = GetJointSections(model);
                            }

                            _db.Sections.AddRange(model.JointSections);
                        }

                        // SECTION SLOT - ONLY PARENT SECTION
                        if (!model.ParentSectionId.HasValue 
                            && model.TotalWeeks > 0
                            && model.SectionDetails.Any())
                        {
                            SaveSectionSlot(model, model.TotalWeeks);
                        }

                        _db.SaveChanges();

                        transaction.Commit();
                        _flashMessage.Confirmation(Message.SaveSucceed);

                        return RedirectToAction("Index", "CourseToBeOffered", new  
                                                                              {
                                                                                CodeAndName = course.Code,
                                                                                AcademicLevelId = model.AcademicLevelId, 
                                                                                TermId = model.TermId 
                                                                              });
                    }
                    catch
                    {
                        transaction.Rollback();
                        _flashMessage.Danger(Message.UnableToCreate);
                        return View(model);
                    }
                }
            } 
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
                
                section.StudentCodes = string.IsNullOrEmpty(section.StudentIds) ? string.Empty 
                                                                                : section.StudentIds.Replace("[", "").Replace("]", "").Replace(",", ", ");
                
                section.Faculties = string.IsNullOrEmpty(section.FacultyIds) ? new List<long>()
                                                                             : _registrationProvider.Deserialize(section.FacultyIds);
                section.Departments = string.IsNullOrEmpty(section.DepartmentIds) ? new List<long>()
                                                                                  : _registrationProvider.Deserialize(section.DepartmentIds);
                section.Curriculums = string.IsNullOrEmpty(section.CurriculumIds) ? new List<long>()
                                                                                  : _registrationProvider.Deserialize(section.CurriculumIds);
                section.Minors = string.IsNullOrEmpty(section.MinorIds) ? new List<long>()
                                                                                  : _registrationProvider.Deserialize(section.MinorIds);
                section.CurriculumVersions = string.IsNullOrEmpty(section.CurriculumVersionIds) ? new List<long>()
                                                                                                : _registrationProvider.Deserialize(section.CurriculumVersionIds);
                section.BatchesInt = string.IsNullOrEmpty(section.Batches) ? new List<int>()
                                                                           : JsonConvert.DeserializeObject<List<int>>(section.Batches);

                section.SectionDetails.Select(x => {
                                                       x.Instructors = string.IsNullOrEmpty(x.InstructorIds) ? new List<long>()
                                                                                                             : _registrationProvider.Deserialize(x.InstructorIds);

                                                       x.EndTimeNullAble = x.EndTime;
                                                       x.StartTimeNullAble = x.StartTime;

                                                       return x;
                                                   })
                                      .OrderBy(x => x.Day)
                                          .ThenBy(x => x.StartTime)
                                      .ToList();
                section.JointSections = _sectionProvider.GetJointSections(id);
                section.JointSections = section.JointSections.Select(x => {
                                                                              x.IsOldJointSection = true;
                                                                              return x;
                                                                          }).ToList();
                section.HaveSectionSlotRoom = _roomProvider.IsHaveRoomInSectionSlot(section.Id);

                CreateSelectList(section.CourseId, term.AcademicLevelId, section.Faculties, section.Departments, section.Curriculums);
                return View(section);
            }
            else
            {
                return View(new Section());
            }
        }

        [PermissionAuthorize("CourseToBeOffered", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Section model, string returnUrl)
        {
            var section = _db.Sections.Include(x => x.Course).SingleOrDefault(x => x.Id == model.Id);
            var term = _academicProvider.GetTerm(model.TermId);
            model.Term = term;
            model.AcademicLevelId = term.AcademicLevelId;
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList(section.CourseId, term.AcademicLevelId, model.Faculties, model.Departments, model.Curriculums);
            CreateParentSelectList(model.CourseId, model.TermId);

            if (string.IsNullOrEmpty(model.Number))
            {
                _flashMessage.Danger(Message.UnableToCreate);
                return View(model);
            }

            // check overlap course detail 
            if (model.SectionDetails != null)
            {
                var tempId = 0;
                model.SectionDetails = model.SectionDetails.Where(x => x.Day != -1
                                                                       && (x.StartTimeNullAble != TimeSpan.Zero && x.StartTimeNullAble != null)
                                                                       && (x.EndTimeNullAble != TimeSpan.Zero && x.EndTimeNullAble != null)
                                                                       && x.TeachingTypeId != 0)
                                                            .Select(x =>
                                                            {
                                                                x.TempId = tempId++;
                                                                return x;
                                                            })
                                                            .ToList();

                if (model.SectionDetails.Any(x => x.StartTimeNullAble > x.EndTimeNullAble))
                {
                    _flashMessage.Danger(Message.UnableToCreate + "Start time must less than or equal to end time");
                    return View(model);
                }

                var groupByDayOfWeek = (from sectionDet in model.SectionDetails
                                        group sectionDet by new { sectionDet.Day } into sections
                                        select sections
                                       ).ToList();
                foreach (var group in groupByDayOfWeek)
                {
                    foreach (var sectionDet in group)
                    {
                        if (group.Any(x => x.TempId != sectionDet.TempId
                                              && sectionDet.Day == x.Day
                                              && sectionDet.StartTimeNullAble < x.EndTimeNullAble
                                              && sectionDet.EndTimeNullAble > x.StartTimeNullAble))
                        {
                            _flashMessage.Danger(Message.UnableToCreate + "Overlap section detail");
                            return View(model);
                        }
                    }
                }
            }

            if (model.JointSections != null)
            {
                model.JointSections = model.JointSections.Where(x => x.CourseId != 0).ToList();
                if(model.JointSections.Any(x => x.CourseId == model.CourseId))
                {
                    _flashMessage.Danger(Message.SectionCourseDuplicateJoinCourses);
                    return View(model);
                }

                if(model.JointSections.Any(x => x.SeatLimit > model.SeatLimit))
                {
                    _flashMessage.Danger(Message.SeatLimitJointInvalid);
                    return View(model);
                }

                if(model.JointSections.GroupBy(x => x.CourseId).Any(x => x.Count() > 1))
                {
                    _flashMessage.Danger(Message.DuplicateJoinCourses);
                    return View(model);
                }

                string message = string.Empty;
                var removeCourseIds = new List<long>();
                foreach(var item in model.JointSections)
                {
                    if(_registrationProvider.SectionExists(item, model.TermId, item.CourseId, model.Number) && item.CourseId != 0)
                    {
                        var courseCode = _db.Courses.SingleOrDefault(x => x.Id == item.CourseId).Code;
                        removeCourseIds.Add(item.CourseId);
                        message += $"course: { courseCode } section: { model.Number }, ";
                    }
                }

                if(!string.IsNullOrEmpty(message))
                {
                    
                    model.JointSections = model.JointSections.Where(x => !removeCourseIds.Contains(x.CourseId)).ToList();
                    _flashMessage.Danger("This " + message + "already exist in database");
                    return View(model);
                }

            }


            if (_registrationProvider.SectionExists(section, model.TermId, model.CourseId, model.Number))
            {
                _flashMessage.Danger(Message.ExistedSection);
                return View(model);
            }

            if (model.SeatLimit - model.SeatUsed < 0)
            {
                _flashMessage.Danger(Message.UnableToEdit);
                return View(model);
            }
            else
            {
                model.SeatAvailable = model.SeatLimit - model.SeatUsed;
                
                if(!string.IsNullOrEmpty(model.StudentCodes))
                {
                    var studentCodes = model.StudentCodes.Replace(" ","").Replace("\n","").Replace("\r","").Split(",").ToList();
                    var studentCodeNotfounds = new List<string>();

                    var students = _db.Students.Where(x => studentCodes.Contains(x.Code))
                                               .Select(x => x.Code)
                                               .Distinct()
                                               .OrderBy(x => x)
                                               .ToList();

                    studentCodeNotfounds = studentCodes.Except(students).ToList();

                    if(studentCodeNotfounds.Count != 0)
                    {
                        var studentString = String.Join(", ", studentCodeNotfounds.ToArray());
                        _flashMessage.Danger(Message.StudentNotFound + "(" + studentString +")");

                        return View(model);
                    }

                    model.Students = students;
                }

                using (var transaction = _db.Database.BeginTransaction())
                {
                    try
                    {
                        section.CourseId = model.CourseId;
                        section.Number = model.Number;                  
                        section.SeatLimit = model.SeatLimit;
                        section.PlanningSeat = model.PlanningSeat;
                        section.MinimumSeat = model.MinimumSeat;
                        section.SeatAvailable = model.SeatAvailable;

                        section.Batches = model.BatchesInt.SerializeMultiple();
                        section.StudentIds = model.Students.SerializeMultiple().Replace("\"","");
                        section.FacultyIds = model.Faculties.SerializeMultiple();
                        section.DepartmentIds = model.Departments.SerializeMultiple();
                        section.CurriculumIds = model.Curriculums.SerializeMultiple();
                        section.CurriculumVersionIds = model.CurriculumVersions.SerializeMultiple();                       
                        section.MinorIds = model.Minors.SerializeMultiple();
                        section.HaveSectionSlotRoom = model.HaveSectionSlotRoom;
                        section.Remark = model.Remark;
                        
                        if (section.ParentSectionId == null)
                        {
                            section.MainInstructorId = model.MainInstructorId;
                            section.IsMasterSection = model.IsMasterSection;
                            section.IsWithdrawable = model.IsWithdrawable;
                            section.IsSpecialCase = model.IsSpecialCase;
                            section.IsOutbound = model.IsOutbound;
                            section.IsClosed = model.IsClosed;

                            section.TotalWeeks = model.TotalWeeks;
                            section.IsDisabledFinal = model.IsDisabledFinal;
                            section.IsDisabledMidterm = model.IsDisabledMidterm;

                            // REMOVE EXAM INFO
                            if (model.MidtermDate == null 
                                || model.MidtermStart == null 
                                || model.MidtermEnd == null)
                            {
                                section.MidtermStart = null;
                                section.MidtermEnd = null;
                                section.MidtermDate = null;
                                section.MidtermRoomId = null;
                                var midtermId = _db.ExaminationTypes.SingleOrDefault(x => x.NameEn.ToLower() == "midterm").Id;
                                var examinationReservation = _db.ExaminationReservations.SingleOrDefault(x => x.SectionId == model.Id 
                                                                                                              && x.ExaminationTypeId == midtermId);
                                if(examinationReservation != null)
                                {
                                    examinationReservation.IsActive = false;
                                    examinationReservation.Status = "r";
                                    
                                    var roomSlot = _db.RoomSlots.FirstOrDefault(x => x.ExaminationReservationId == examinationReservation.Id
                                                                                     && x.IsActive);
                                    if (roomSlot != null)
                                    {
                                        roomSlot.IsActive = false;
                                        roomSlot.IsCancel = true;
                                    }
                                    _db.SaveChanges();
                                } 
                            }
                            else 
                            {
                                if (!model.IsMidtermApproval && model.IsDisabledMidterm)
                                {
                                    section.MidtermStart = null;
                                    section.MidtermEnd = null;
                                    section.MidtermDate = null;
                                    section.MidtermRoomId = null;
                                    var examinationTypeMidtermId = _db.ExaminationTypes.SingleOrDefault(x => x.NameEn.ToLower() == "midterm").Id;
                                    var examinationReservation = _db.ExaminationReservations.SingleOrDefault(x => x.SectionId == model.Id && x.ExaminationTypeId == examinationTypeMidtermId);
                                    if(examinationReservation != null)
                                    {
                                        examinationReservation.IsActive = false;
                                        examinationReservation.Status = "r";
                                        _db.SaveChanges();
                                    } 
                                }
                                else if(!model.IsMidtermApproval && model.MidtermDate != null && model.MidtermStart != null && model.MidtermEnd != null)
                                {
                                    var examinationTypeMidtermId = _db.ExaminationTypes.SingleOrDefault(x => x.NameEn.ToLower() == "midterm").Id;
                                    var result = _reservationProvider.UpdateExaminationReservation(new ExaminationReservation
                                                                                                   {
                                                                                                       ExaminationTypeId = examinationTypeMidtermId,
                                                                                                       StartTime = model.MidtermStart??new TimeSpan(),
                                                                                                       EndTime = model.MidtermEnd??new TimeSpan(),
                                                                                                       Date = model.MidtermDate??new DateTime(),
                                                                                                       SectionId = model.Id,
                                                                                                       Status = "w",
                                                                                                       InstructorId = model.MainInstructorId,
                                                                                                       SenderType = "a",
                                                                                                       AcademicLevelId = model.AcademicLevelId,
                                                                                                       TermId = section.TermId,
                                                                                                       UseProctor = false,
                                                                                                       AbsentInstructor = true,
                                                                                                       AllowBooklet = false,
                                                                                                       AllowCalculator = false,
                                                                                                       AllowOpenbook = false
                                                                                                   });

                                    switch(result.Status)
                                    {
                                        case UpdateExamStatus.SaveExamSucceed : 
                                        case UpdateExamStatus.UpdateExamSuccess :                                        
                                            break;

                                        case UpdateExamStatus.ExaminationAlreadyApproved :  
                                            _flashMessage.Confirmation(Message.DataAlreadyExist);
                                            section.MidtermStart = null;
                                            section.MidtermEnd = null;
                                            section.MidtermDate = null;
                                            section.MidtermRoomId = null;
                                            break;

                                        default :  
                                            section.MidtermStart = null;
                                            section.MidtermEnd = null;
                                            section.MidtermDate = null;
                                            section.MidtermRoomId = null;
                                            _flashMessage.Danger(Message.UnableToCreate + "midterm");
                                            break;
                                    }
                                }
                            }

                            if (model.FinalDate == null 
                                || model.FinalStart == null 
                                || model.FinalEnd == null)
                            {
                                section.FinalStart = null;
                                section.FinalEnd = null;
                                section.FinalDate = null;
                                section.FinalRoomId = null;
                                var finalId = _db.ExaminationTypes.SingleOrDefault(x => x.NameEn.ToLower() == "final").Id;
                                var examinationReservation = _db.ExaminationReservations.SingleOrDefault(x => x.SectionId == model.Id 
                                                                                                              && x.ExaminationTypeId == finalId);
                                if(examinationReservation != null)
                                {
                                    examinationReservation.IsActive = false;
                                    examinationReservation.Status = "r";
                                    
                                    var roomSlot = _db.RoomSlots.FirstOrDefault(x => x.ExaminationReservationId == examinationReservation.Id
                                                                                     && x.IsActive);
                                    if (roomSlot != null)
                                    {
                                        roomSlot.IsActive = false;
                                        roomSlot.IsCancel = true;
                                    }
                                    _db.SaveChanges();
                                } 
                            }
                            else 
                            {
                                if(!model.IsFinalApproval && model.IsDisabledFinal)
                                {
                                    section.FinalStart = null;
                                    section.FinalEnd = null;
                                    section.FinalDate = null;
                                    section.FinalRoomId = null;
                                    var examinationTypeFinalId = _db.ExaminationTypes.SingleOrDefault(x => x.NameEn.ToLower() == "final").Id;
                                    var examinationReservation = _db.ExaminationReservations.SingleOrDefault(x => x.SectionId == model.Id && x.ExaminationTypeId == examinationTypeFinalId);
                                    if(examinationReservation != null)
                                    {
                                        examinationReservation.IsActive = false;
                                        examinationReservation.Status = "r";
                                        _db.SaveChanges();
                                    } 
                                }
                                else if(!model.IsFinalApproval && model.FinalDate != null && model.FinalStart != null && model.FinalEnd != null)
                                {
                                    var examinationTypeFinalId = _db.ExaminationTypes.SingleOrDefault(x => x.NameEn.ToLower() == "final").Id;
                                    var result = _reservationProvider.UpdateExaminationReservation(new ExaminationReservation
                                                                                                   {
                                                                                                       ExaminationTypeId = examinationTypeFinalId,
                                                                                                       StartTime = model.FinalStart ?? new TimeSpan(),
                                                                                                       EndTime = model.FinalEnd ?? new TimeSpan(),
                                                                                                       Date = model.FinalDate ?? new DateTime(),
                                                                                                       SectionId = model.Id,
                                                                                                       Status = "w",
                                                                                                       InstructorId = model.MainInstructorId,
                                                                                                       SenderType = "a",
                                                                                                       AcademicLevelId = model.AcademicLevelId,
                                                                                                       TermId = section.TermId,
                                                                                                       UseProctor = false,
                                                                                                       AbsentInstructor = true,
                                                                                                       AllowBooklet = false,
                                                                                                       AllowCalculator = false,
                                                                                                       AllowOpenbook = false
                                                                                                   });

                                    switch(result.Status)
                                    {
                                        case UpdateExamStatus.SaveExamSucceed : 
                                        case UpdateExamStatus.UpdateExamSuccess : 
                                            break;

                                        case UpdateExamStatus.ExaminationAlreadyApproved :  
                                            _flashMessage.Confirmation(Message.DataAlreadyExist);
                                            section.FinalStart = null;
                                            section.FinalEnd = null;
                                            section.FinalDate = null;
                                            section.FinalRoomId = null;
                                            break;

                                        default :  
                                            section.FinalStart = null;
                                            section.FinalEnd = null;
                                            section.FinalDate = null;
                                            section.FinalRoomId = null;
                                            _flashMessage.Danger(Message.UnableToEdit + ";final");
                                            break;
                                    }
                                }
                            }

                            if(!section.HaveSectionSlotRoom)
                            {
                                DeleteSectionDetailsAndSlots(section.Id);
                                section.SectionDetails = new List<SectionDetail>();
                                if (model.SectionDetails != null)
                                {
                                    section.SectionDetails = model.SectionDetails.Select(x =>
                                    {
                                        var detail = _mapper.Map<SectionDetail, SectionDetail>(x);
                                        detail.InstructorIds = x.Instructors == null || !x.Instructors.Any() ? "[]" : JsonConvert.SerializeObject(x.Instructors);
                                        detail.StartTime = x.StartTimeNullAble ?? new TimeSpan();
                                        detail.EndTime = x.EndTimeNullAble ?? new TimeSpan();
                                        return detail;
                                    })
                                                                                 .ToList();
                                    foreach (var item in section.SectionDetails)
                                    {
                                        if ((item.InstructorId ?? 0) > 0)
                                        {
                                            item.InstructorSections = new List<InstructorSection>()
                                                                {
                                                                    new InstructorSection
                                                                    {
                                                                        InstructorId = item.InstructorId ?? 0,
                                                                        StartedAt = model.OpenedAt,
                                                                        EndedAt = model.ClosedAt,
                                                                        Hours = model.TotalWeeks
                                                                    }
                                                                };
                                        }
                                    }

                                    model.SectionDetails = section.SectionDetails;
                                }
                               
                                List<long> existedJointIds = new List<long>();
                                // joint sections
                                if (model.JointSections != null && model.JointSections.Any())
                                {
                                    // edit
                                    var editedJoints = model.JointSections.Where(x => x.Id > 0)
                                                                        .ToList();
                                    existedJointIds = editedJoints.Select(x => x.Id)
                                                                .ToList();
                                    foreach (var item in editedJoints)
                                    {
                                        var editedSection = _db.Sections.SingleOrDefault(x => x.Id == item.Id);
                                        
                                        editedSection.MainInstructorId = section.MainInstructorId;
                                        editedSection.FinalDate = section.FinalDate;
                                        editedSection.FinalStart = section.FinalStart;
                                        editedSection.FinalEnd = section.FinalEnd;
                                        editedSection.MidtermDate = section.MidtermDate;
                                        editedSection.MidtermEnd = section.MidtermEnd;
                                        editedSection.MidtermStart = section.MidtermStart;
                                        editedSection.Remark = item.Remark;
                                        
                                        DeleteSectionDetailsAndSlots(editedSection.Id);

                                        editedSection.SectionDetails = new List<SectionDetail>();
                                        foreach (var detail in section.SectionDetails)
                                        {
                                            var sectionDetail = _mapper.Map<SectionDetail>(detail);
                                            editedSection.SectionDetails.Add(sectionDetail);
                                        }
                                        if (item.SeatLimit - editedSection.SeatUsed > 0)
                                        {
                                            editedSection.SeatLimit = item.SeatLimit;
                                            // editedSection.SeatAvailable = editedSection.SeatLimit - editedSection.SeatUsed;
                                        }
                                    }

                                    // add
                                    model.JointSections = model.JointSections.Where(x => x.CourseId > 0 && x.Id == 0)
                                                                            .ToList();
                                    model.JointSections = GetJointSections(model);
                                    _db.Sections.AddRange(model.JointSections);
                                }

                                var deletedJoints = _db.Sections.Where(x => x.ParentSectionId == model.Id
                                                                            && !existedJointIds.Any(y => y == x.Id))
                                                                .ToList();
                                foreach (var item in deletedJoints)
                                {
                                    DeleteJointSection(item.Id);
                                } 
                            }
                            else
                            {
                                var sectionDetails =  _db.SectionDetails.Where(x => x.SectionId == model.Id).ToList();
                                if(sectionDetails.Any())
                                {
                                    foreach(var item in model.SectionDetails)
                                    {
                                        sectionDetails.Where(x => x.Id == item.Id)
                                                    .Select(x =>
                                                                {
                                                                    x.Remark = item.Remark;
                                                                    return x;
                                                                })
                                                    .ToList();
                                    }
                                }

                                //var jointSections = _db.Sections.Where(x => x.ParentSectionId == model.Id)
                                //                                .ToList();
                                //if(jointSections.Any())
                                //{
                                //    foreach(var item in model.JointSections)
                                //    {
                                //        jointSections.Where(x => x.Id == item.Id)
                                //                     .Select(x =>
                                //                                 {
                                //                                    x.Remark = item.Remark;
                                //                                    if (item.SeatLimit - x.SeatUsed > 0)
                                //                                    {
                                //                                        x.SeatLimit = item.SeatLimit;
                                //                                        // x.SeatAvailable = item.SeatLimit - x.SeatUsed;
                                //                                    }
                                //                                     return x;
                                //                                 })
                                //                     .ToList();
                                //    }
                                //}
                                List<long> existedJointIds = new List<long>();
                                // joint sections
                                if (model.JointSections != null && model.JointSections.Any())
                                {
                                    // edit
                                    var editedJoints = model.JointSections.Where(x => x.Id > 0)
                                                                        .ToList();
                                    existedJointIds = editedJoints.Select(x => x.Id)
                                                                .ToList();
                                    foreach (var item in editedJoints)
                                    {
                                        var editedSection = _db.Sections.SingleOrDefault(x => x.Id == item.Id);
                                        if (editedSection.SeatUsed > 0)
                                        {
                                            editedSection.Remark = item.Remark;
                                            if (item.SeatLimit - editedSection.SeatUsed > 0)
                                            {
                                                editedSection.SeatLimit = item.SeatLimit;
                                                // x.SeatAvailable = item.SeatLimit - x.SeatUsed;
                                            }
                                        }
                                        else
                                        {
                                            editedSection.MainInstructorId = section.MainInstructorId;
                                            editedSection.FinalDate = section.FinalDate;
                                            editedSection.FinalStart = section.FinalStart;
                                            editedSection.FinalEnd = section.FinalEnd;
                                            editedSection.MidtermDate = section.MidtermDate;
                                            editedSection.MidtermEnd = section.MidtermEnd;
                                            editedSection.MidtermStart = section.MidtermStart;
                                            editedSection.Remark = item.Remark;

                                            InactiveSectionDetailsAndSlots(editedSection.Id);

                                            editedSection.SectionDetails = new List<SectionDetail>();
                                            foreach (var detail in section.SectionDetails)
                                            {
                                                var sectionDetail = _mapper.Map<SectionDetail>(detail);
                                                editedSection.SectionDetails.Add(sectionDetail);
                                            }
                                            if (item.SeatLimit - editedSection.SeatUsed > 0)
                                            {
                                                editedSection.SeatLimit = item.SeatLimit;
                                                // editedSection.SeatAvailable = editedSection.SeatLimit - editedSection.SeatUsed;
                                            }
                                        }
                                    }

                                    // add
                                    model.JointSections = model.JointSections.Where(x => x.CourseId > 0 && x.Id == 0)
                                                                            .ToList();
                                    model.JointSections = GetJointSections(model);
                                    _db.Sections.AddRange(model.JointSections);
                                }

                                var deletedJoints = _db.Sections.Where(x => x.ParentSectionId == model.Id
                                                                            && !existedJointIds.Any(y => y == x.Id))
                                                                .ToList();
                                foreach (var item in deletedJoints)
                                {
                                    if (item.SeatUsed > 0)
                                    {
                                        _flashMessage.Danger(Message.UnableToEditJointSection);
                                        return View(model);
                                    }
                                    InactiveJointSection(item.Id);
                                }
                            }

                            _db.SaveChanges();
                        }

                        _db.Entry(section).State = EntityState.Modified;
                        _db.SaveChanges();
                        
                        if(section.Status == "a")
                        {
                            var updateWriteList = new UpdateWhiteListViewModel();
                            updateWriteList.Batches = section.Batches;
                            updateWriteList.StudentCodes = section.StudentIds;
                            updateWriteList.FacultyIds = section.FacultyIds;
                            updateWriteList.DepartmentIds = section.DepartmentIds;
                            updateWriteList.CurriculumCodes = section.CurriculumIds;
                            updateWriteList.CurriculumVersionIds = section.CurriculumVersionIds;                       
                            updateWriteList.MinorIds = section.MinorIds;
                            updateWriteList.MainInstructorId = section.MainInstructorId;
                            updateWriteList.Remark = section.Remark;

                            if(_registrationProvider.UpdateWriteList(updateWriteList, section.Id))
                            {
                                // transaction.Rollback();
                                // return View(model);
                            }
                        }

                        _sectionProvider.ReCalculateSeatAvailable(section.Id);

                        // SECTION SLOT - ONLY PARENT SECTION
                        if (!section.ParentSectionId.HasValue 
                            && section.TotalWeeks > 0
                            && section.SectionDetails.Any()
                            && !section.HaveSectionSlotRoom)
                        {
                            SaveSectionSlot(section, section.TotalWeeks);
                            _db.SaveChanges();
                        }

                        transaction.Commit();

                        //update available seat
                        _registrationProvider.CallUSparkAPIUpdateSeat(section.Id);

                        _flashMessage.Confirmation(Message.SaveSucceed);

                        return RedirectToAction("Index", "CourseToBeOffered", new  
                                                                              {
                                                                                CodeAndName = section.Course.Code,
                                                                                AcademicLevelId = model.AcademicLevelId, 
                                                                                TermId = model.TermId 
                                                                              });
                    }
                    catch
                    {
                        transaction.Rollback();
                        _flashMessage.Danger(Message.UnableToEdit);
                        return View(model);
                    }
                }
            }
        }

        [PermissionAuthorize("CourseToBeOffered", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            var section = Find(id);
            var term = _academicProvider.GetTerm(section.TermId);
            section.AcademicLevelId = term.AcademicLevelId;
            if(section.Status != "w" && section.Status != "c")
            {
                _flashMessage.Danger(Message.UnableToDelete);
                return RedirectToAction(nameof(Index), new Criteria 
                                                       {
                                                           CodeAndName = section.Course.Code,
                                                           AcademicLevelId = section.AcademicLevelId, 
                                                           TermId = section.TermId 
                                                       });
            }
            try
            {
                var jointSections = _db.Sections.Where(x => x.ParentSectionId == section.Id).ToList();
                if(jointSections != null && jointSections.Any())
                {
                    foreach(var item in jointSections)
                    {
                        DeleteSection(item.Id);
                    }
                }
                DeleteSectionSlots(id);
                DeleteSection(id);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }
            
            return RedirectToAction(nameof(Index), new Criteria 
                                                   {
                                                       CodeAndName = section.Course.Code,
                                                       AcademicLevelId = section.AcademicLevelId, 
                                                       TermId = section.TermId 
                                                   });
        }

        private void CreateSelectList(long courseId = 0, long academicLevelId = 0, List<long> facultyIds = null, List<long> departmentIds = null, List<long> curriculumIds = null) 
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

            ViewBag.Courses = _selectListProvider.GetAllowAddedCourses(courseId, true);
            ViewBag.Sections = _selectListProvider.GetSections();
            ViewBag.Rooms = _selectListProvider.GetRooms();
            ViewBag.Instructors = _selectListProvider.GetInstructors();
            ViewBag.Dayofweeks = _selectListProvider.GetDayOfWeek(); 
            ViewBag.TeachingTypes = _selectListProvider.GetTeachingTypes();
            ViewBag.OpenCloseStatuses = _selectListProvider.GetOpenCloseStatuses();
            ViewBag.SectionStatuses = _selectListProvider.GetCourseToBeOfferedSectionStatuses();
            ViewBag.Batches = _selectListProvider.GetBatches();
            ViewBag.RoomJs = JsonConvert.SerializeObject(_selectListProvider.GetRooms());
            ViewBag.InstructorJs = JsonConvert.SerializeObject(_selectListProvider.GetInstructors());
            ViewBag.DayofweekJs = JsonConvert.SerializeObject(_selectListProvider.GetDayOfWeek()); 
            ViewBag.TeachingTypeJs = JsonConvert.SerializeObject(_selectListProvider.GetTeachingTypes());
            ViewBag.Campuses = _selectListProvider.GetCampuses();
            ViewBag.Minors = _selectListProvider.GetSpecializationGroupByType("m");
            ViewBag.Instructors = _selectListProvider.GetInstructors();
        }

        private void CreateIndexSelectList(Criteria criteria) 
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            if (criteria.AcademicLevelId > 0)
            {
                ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(criteria.AcademicLevelId);
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(criteria.AcademicLevelId);
                ViewBag.Courses = _selectListProvider.GetCourses();
            }

            ViewBag.OpenCloseStatuses = _selectListProvider.GetOpenCloseStatuses();
            ViewBag.SeatAvailableStatuses = _selectListProvider.GetSeatAvailableStatuses();
            ViewBag.AllYesNoAnswer = _selectListProvider.GetAllYesNoValue();
            ViewBag.SectionStatuses = _selectListProvider.GetSectionStatuses();
            ViewBag.Instructors = _selectListProvider.GetInstructors();
            ViewBag.SectionTypes = _selectListProvider.GetSectionTypes();
            ViewBag.Users = _selectListProvider.GetUsersFullNameEn();
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
                                      .Include(x => x.SectionDetails)
                                          .ThenInclude(x => x.TeachingType)
                                      .Include(x => x.SectionSlots)
                                          .ThenInclude(x => x.TeachingType)
                                      .Include(x => x.SectionSlots)
                                          .ThenInclude(x => x.Room)
                                      .Include(x => x.SharedSections)
                                      .Include(x => x.Term)
                                          .ThenInclude(x => x.AcademicLevel)
                                      .Include(x => x.MainInstructor)                                    
                                      .AsNoTracking()
                                      .SingleOrDefault(x => x.Id == id);
            var examinationReservations = _db.ExaminationReservations.Where(x => x.SectionId == id).ToList();
            var examinationTypeMidtermId = _db.ExaminationTypes.SingleOrDefault(x => x.NameEn.ToLower() == "midterm").Id;
            var examinationTypeFinalId = _db.ExaminationTypes.SingleOrDefault(x => x.NameEn.ToLower() == "final").Id;
            var IsFinalApproval = examinationReservations.Any(x => x.Status == "a" && x.ExaminationTypeId == examinationTypeFinalId);
            var IsMidtermApproval = examinationReservations.Any(x => x.Status == "a" && x.ExaminationTypeId == examinationTypeMidtermId);

            section.ExaminationReservations = examinationReservations;
            section.IsFinalApproval = IsFinalApproval;
            section.IsMidtermApproval = IsMidtermApproval;

            if(section.ParentSectionId != null)
            {
                section.SectionDetails = _db.SectionDetails.Include(x => x.Room)
                                                           .Include(x => x.TeachingType)
                                                           .Include(x => x.InstructorSections)
                                                                .ThenInclude(x => x.Instructor)
                                                           .Where(x => x.SectionId == section.ParentSectionId)
                                                           .OrderBy(x => x.Day)
                                                           .ThenBy(x => x.StartTime)
                                                           .AsNoTracking()
                                                           .ToList();

                section.SectionSlots = _db.SectionSlots.Include(x => x.Room)
                                                       .Include(x => x.TeachingType)
                                                       .Where(x => x.SectionId == section.ParentSectionId)
                                                       .OrderBy(x => x.Date)
                                                            .ThenBy(x => x.StartTime)
                                                       .AsNoTracking()
                                                       .ToList();
            } 
            else 
            {
                section.SectionDetails = section.SectionDetails.OrderBy(x => x.Day)
                                                                    .ThenBy(x => x.StartTime)
                                                               .ToList();

                section.SectionSlots = section.SectionSlots.OrderBy(x => x.Date)
                                                                .ThenBy(x => x.StartTime)
                                                           .ToList();
            }
            
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

        private List<Section> GetJointSections(Section model)
        {
            var jointSections = new List<Section>();

            foreach (var item in model.JointSections)
            {
                if (!item.IsOldJointSection)
                {
                    var section = _mapper.Map<Section>(model);
                    section.CourseId = item.CourseId;
                    section.Number = model.Number;
                    section.SeatLimit = item.SeatLimit;
                    section.ExtraSeat = item.ExtraSeat;
                    section.SeatAvailable = item.SeatLimit;
                    section.MinimumSeat = 0;
                    section.PlanningSeat = 0;
                    section.SeatUsed = 0;
                    section.IsParent = false;
                    section.IsMasterSection = false;
                    section.ParentSectionId = model.Id;
                    section.Remark = item.Remark;
                    section.BatchFrom = null;
                    section.BatchTo = null;
                    section.Batches = null;
                    section.DepartmentIds = null;
                    section.FacultyIds = null;
                    section.CurriculumIds = null;
                    section.CurriculumVersionIds = null;
                    section.MainInstructorId = model.MainInstructorId;
                    section.FinalDate = model.FinalDate;
                    section.FinalStart = model.FinalStart;
                    section.FinalEnd = model.FinalEnd;
                    section.MidtermDate = model.MidtermDate;
                    section.MidtermStart = model.MidtermStart;
                    section.MidtermEnd = model.MidtermEnd;
                    section.SectionDetails = new List<SectionDetail>();
                    foreach (var detail in model.SectionDetails)
                    {
                        var sectionDetail = _mapper.Map<SectionDetail>(detail);
                        section.SectionDetails.Add(sectionDetail);
                    }

                    jointSections.Add(section);
                }
            }

            return jointSections;
        }

        [PermissionAuthorize("CourseToBeOffered", PolicyGenerator.Write)]
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

        [PermissionAuthorize("CourseToBeOffered", PolicyGenerator.Write)]
        public ActionResult Open(long id)
        {
            var section = _sectionProvider.GetSection(id);
            var isOpened = _sectionProvider.OpenSection(section);
            if (isOpened)
            {
                if (_registrationProvider.IsRegistrationPeriod(DateTime.Now, section.TermId))
                {
                    try
                    {
                        _registrationProvider.CallUSparkAPIOpenSection(section.Id);
                    }
                    catch (Exception e)
                    {
                        _flashMessage.Danger(e.Message);
                    }
                }

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

        public long GetLectureTeachingTypeId(string teachingType)
        {
            var teachingTypeId = _db.TeachingTypes.SingleOrDefault(x => x.NameEn.ToLower() == teachingType).Id;
            return teachingTypeId;
        } 

        public string GetNextSectionNumber(long courseId, long termId)
        {
            var sectionNumber = _sectionProvider.GetNextSectionNumber(courseId, termId);
            return sectionNumber;
        }

        private void UpdateFieldIds(Section section)
        {
            section.StudentIds = section.Students.SerializeMultiple().Replace("\"","");
            section.FacultyIds = section.Faculties != null ? (section.Faculties.SerializeMultiple()) : "[]";
            section.DepartmentIds = section.Departments != null ? (section.Departments.SerializeMultiple()) : "[]";
            section.CurriculumIds = section.Curriculums != null ? (section.Curriculums.SerializeMultiple()) : "[]";
            section.CurriculumVersionIds = section.CurriculumVersions != null ? (section.CurriculumVersions.SerializeMultiple()) : "[]";
            section.Batches = section.BatchesInt != null ? (section.BatchesInt.SerializeMultiple()) : "[]";
            section.MinorIds = section.Minors != null ? (section.Minors.SerializeMultiple()) : "[]";
        }

        private void DeleteSection(long id)
        {
            var section = _db.Sections.SingleOrDefault(x => x.Id == id);
            if (section.Status == "w" || section.Status == "c")
            {
                // Section
                foreach (var detail in _db.SectionDetails.Where(x => x.SectionId == id)
                                                         .ToList())
                {
                    var instructors = _db.InstructorSections.Where(x => x.SectionDetailId == detail.Id)
                                                            .ToList();
                    if (instructors != null && instructors.Any())
                    {
                        _db.InstructorSections.RemoveRange(instructors);
                    }

                    _db.SectionDetails.Remove(detail);
                }

                _db.Sections.Remove(section);
            }
        }

        private void DeleteJointSection(long id)
        {
            var section = _db.Sections.SingleOrDefault(x => x.Id == id);

            foreach (var detail in _db.SectionDetails.Where(x => x.SectionId == id)
                                                     .ToList())
            {
                var instructors = _db.InstructorSections.Where(x => x.SectionDetailId == detail.Id)
                                                        .ToList();
                if (instructors != null && instructors.Any())
                {
                    _db.InstructorSections.RemoveRange(instructors);
                }

                _db.SectionDetails.Remove(detail);
            }

            _db.Sections.Remove(section);
        }

        private void DeleteSectionDetailsAndSlots(long sectionId)
        {
            // Section detail
            var sectionDetails = _db.SectionDetails.Where(x => x.SectionId == sectionId)
                                                   .ToList();
            if (sectionDetails != null && sectionDetails.Any())
            {
                foreach (var item in sectionDetails)
                {
                    var instructors = _db.InstructorSections.Where(x => x.SectionDetailId == item.Id)
                                                            .ToList();
                    if (instructors != null && instructors.Any())
                    {
                        _db.InstructorSections.RemoveRange(instructors);
                    }

                    _db.SectionDetails.Remove(item);
                }
            }

            // Section slot 
            var sectionSlots = _db.SectionSlots.Where(x => x.SectionId == sectionId)
                                               .ToList();
            if (sectionSlots != null && sectionSlots.Any())
            {
                foreach (var item in sectionSlots)
                {
                    var roomSlots = _db.RoomSlots.Where(x => x.SectionSlotId == item.Id)
                                                 .IgnoreQueryFilters()
                                                 .ToList();
                    if (roomSlots != null && roomSlots.Any())
                    {
                        _db.RoomSlots.RemoveRange(roomSlots);
                    }

                    _db.SectionSlots.Remove(item);
                }
            }

            _db.SaveChanges();
        }

        private void InactiveJointSection(long id)
        {
            var section = _db.Sections.SingleOrDefault(x => x.Id == id);

            foreach (var detail in _db.SectionDetails.Where(x => x.SectionId == id)
                                                     .ToList())
            {
                var instructors = _db.InstructorSections.Where(x => x.SectionDetailId == detail.Id)
                                                        .ToList();
                if (instructors != null && instructors.Any())
                {
                    instructors.Select(x => { x.IsActive = false; return x; });
                }

                detail.IsActive = false;
            }

            section.IsActive = false;
            _db.SaveChanges();
        }

        private void InactiveSectionDetailsAndSlots(long sectionId)
        {
            // Section detail
            var sectionDetails = _db.SectionDetails.Where(x => x.SectionId == sectionId)
                                                   .ToList();
            if (sectionDetails != null && sectionDetails.Any())
            {
                foreach (var item in sectionDetails)
                {
                    var instructors = _db.InstructorSections.Where(x => x.SectionDetailId == item.Id)
                                                            .ToList();
                    if (instructors != null && instructors.Any())
                    {
                        //_db.InstructorSections.RemoveRange(instructors);
                        instructors.Select(x => { x.IsActive = false; return x; });
                    }

                    item.IsActive = false;
                }
            }

            // Section slot 
            var sectionSlots = _db.SectionSlots.Where(x => x.SectionId == sectionId)
                                               .ToList();
            if (sectionSlots != null && sectionSlots.Any())
            {
                foreach (var item in sectionSlots)
                {
                    var roomSlots = _db.RoomSlots.Where(x => x.SectionSlotId == item.Id)
                                                 .ToList();
                    if (roomSlots != null && roomSlots.Any())
                    {
                        _db.RoomSlots.RemoveRange(roomSlots);
                        roomSlots.Select(x => { x.IsActive = false; return x; });
                    }

                    item.IsActive = false;
                }
            }

            _db.SaveChanges();
        }

        private void DeleteSectionSlots(long sectionId)
        {
            // Section slot 
            var sectionSlots = _db.SectionSlots.Where(x => x.SectionId == sectionId)
                                               .ToList();
            if (sectionSlots != null && sectionSlots.Any())
            {
                foreach (var item in sectionSlots)
                {
                    var roomSlots = _db.RoomSlots.Where(x => x.SectionSlotId == item.Id)
                                                 .ToList();
                    if (roomSlots != null && roomSlots.Any())
                    {
                        _db.RoomSlots.RemoveRange(roomSlots);
                    }
                    _db.SectionSlots.Remove(item);
                }
            }

            _db.SaveChanges();
        }

        private void ReGenerateSectionDetailsAndSlots()
        {
            var term = _db.Terms.SingleOrDefault(x => x.Id == 140);
            var sections = _db.Sections.Include(x => x.SectionDetails)
                                       .Include(x => x.SectionSlots)
                                       .Where(x => x.TermId == 140 
                                                   && x.ParentSectionId == null
                                                   && (x.SectionDetails != null && x.SectionDetails.Count() > 0)
                                                   && (x.SectionSlots == null 
                                                       || x.SectionSlots.Where(y => !y.IsMakeUpClass).Count() == 0)).ToList();
            int total = sections.Count();
            int i = 1;
            List<SectionSlot> slots = new List<SectionSlot>();
            foreach (var section in sections)
            {
                // Master Section
                // DeleteSectionSlots(section.Id);
                foreach (var detail in section.SectionDetails)
                {
                    //var calendars = _db.Calendars.Where(x => x.Date >= term.StartedAt
                    //                                        && x.DayOfWeek == detail.Day)
                    //                             .Take(section.TotalWeeks)
                    //                             .ToList();

                    var holidays = _db.MuicHolidays.AsNoTracking()
                                               .Where(x => x.IsActive
                                                               && x.StartedAt <= section.OpenedAt.AddDays(section.TotalWeeks * 7)
                                                               && x.EndedAt >= section.OpenedAt)
                                               .Select(x => new
                                               {
                                                   x.StartedAt,
                                                   x.EndedAt
                                               })
                                               .Distinct()
                                               .ToList();

                    var holidaysEachDate = new List<DateTime>();
                    foreach (var holiday in holidays)
                    {
                        for (DateTime date = holiday.StartedAt.Date; date <= holiday.EndedAt.Date; date = date.AddDays(1))
                        {
                            holidaysEachDate.Add(date);
                        }
                    }
                    holidaysEachDate = holidaysEachDate.Distinct().ToList();

                    var calendars = _db.Calendars.Where(x => x.Date >= section.OpenedAt
                                                             && x.DayOfWeek == detail.Day
                                                             //&& !holidaysEachDate.Contains(x.Date)
                                                             )
                                                 .Take(section.TotalWeeks)
                                                 .ToList();

                    _db.SectionSlots.AddRange(calendars.Select(x => new SectionSlot
                                                                    {
                                                                        SectionId = section.Id,
                                                                        TeachingTypeId = detail.TeachingTypeId,
                                                                        Day = detail.Day,
                                                                        Date = x.Date,
                                                                        StartTime = detail.StartTime,
                                                                        EndTime = detail.EndTime,
                                                                        InstructorId = detail.InstructorId,
                                                                        RoomId = null,
                                                                        Status = holidaysEachDate.Contains(x.Date) ? "c" : "w",
                                                                        Remark = holidaysEachDate.Contains(x.Date) ? "Auto Cancel as University Holiday" : "",
                                                                        IsActive = true
                                                                    }).ToList());
                }

                // Joint Section
                var jointSections = _db.Sections.Where(x => x.ParentSectionId == section.Id).ToList();
                foreach (var jointSection in jointSections)
                {
                    DeleteSectionDetailsAndSlots(jointSection.Id);

                    jointSection.SectionDetails = new List<SectionDetail>();
                    foreach (var detail in section.SectionDetails)
                    {
                        var jointSectionDetail = _mapper.Map<SectionDetail>(detail);
                        jointSection.SectionDetails.Add(jointSectionDetail);
                    }
                }

                _db.SaveChanges();
                Console.WriteLine(i.ToString() + "/" + total.ToString());
                i++;
            }
        }

        private void SaveSectionSlot(Section section, int totalWeeks)
        {
            var sectionSlots = _sectionProvider.GenerateSectionSlots(totalWeeks, section);
            _db.SectionSlots.AddRange(sectionSlots);
        }
    }
}