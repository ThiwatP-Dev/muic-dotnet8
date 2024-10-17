using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;
using Microsoft.AspNetCore.Authorization;
using KeystoneLibrary.Exceptions;
using KeystoneLibrary.Models.Api;
using System.Globalization;
using Swashbuckle.AspNetCore.Filters;
using KeystoneLibrary.Models.Api.ApiResponse;
using System.ComponentModel.DataAnnotations;
using Keystone.Extensions;

namespace Keystone.Controllers.MasterTables
{
    [AllowAnonymous]
    public class SectionAPIController : BaseController
    {
        protected readonly IRoomProvider _roomProvider;
        protected readonly IRegistrationProvider _registrationProvider;
        protected readonly ISectionProvider _sectionProvider;
        protected readonly IReservationProvider _reservationProvider;
        public SectionAPIController(ApplicationDbContext db,
                                    IFlashMessage flashMessage,
                                    KeystoneLibrary.Interfaces.IConfigurationProvider configurationProvider,
                                    IRoomProvider roomProvider,
                                    IRegistrationProvider registrationProvider,
                                    ISectionProvider sectionProvider,
                                    IReservationProvider reservationProvider,
                                    ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider, configurationProvider) 
        {
            _roomProvider = roomProvider;
            _registrationProvider = registrationProvider;
            _sectionProvider = sectionProvider;
            _reservationProvider = reservationProvider;
        }
        
        public IActionResult GetSections(string instructorCode, long academicYear, long academicTerm = 0)
        {
            if(ValidApiKey())
            {
                return Unauthorized(ApiException.InvalidKey());
            }

            var isInstructor = _db.Instructors.Any(x => x.Code == instructorCode);
            var term = _db.Terms.Where(x => x.AcademicYear == academicYear).ToList();

            if(!isInstructor)
            {
                return Error(SectionApiException.InstructorNotFound());
            }

            if(!term.Any())
            {
                return Error(SectionApiException.TermNotFound());
            }

            var sections = _db.Sections.AsNoTracking()
                                       .Where(x => x.MainInstructor.Code == instructorCode                                                    
                                                   && x.Term.AcademicYear == academicYear
                                                   && (x.Term.AcademicTerm == academicTerm
                                                       || academicTerm == 0 ))
                                       .Union(_db.SectionDetails.Where(x => x.Instructor.Code == instructorCode
                                                                            && x.Section.Term.AcademicYear == academicYear
                                                                            && (x.Section.Term.AcademicTerm == academicTerm
                                                                                || academicTerm == 0 ))
                                                                .Select(x => x.Section))
                                       .Union(_db.SectionSlots.Where(x => x.Instructor.Code == instructorCode
                                                                          && x.Section.Term.AcademicYear == academicYear
                                                                          && (x.Section.Term.AcademicTerm == academicTerm
                                                                              || academicTerm == 0 ))
                                                              .Select(x => x.Section))
                                       .Select(x => new SectionApiViewModel
                                                    {
                                                        SectionId = x.Id,
                                                        SectionNumber = x.Number,
                                                        TermText = x.Term.TermText,
                                                        SeatAvailable = x.SeatAvailable,
                                                        SeatLimit = x.SeatLimit,
                                                        SeatUsed = x.SeatUsed,
                                                        PlanningSeat = x.PlanningSeat,
                                                        ExtraSeat = x.ExtraSeat,
                                                        MinimumSeat = x.MinimumSeat,
                                                        Status = x.Status,
                                                        Remark = x.Remark,
                                                        MidtermStart = x.MidtermStart,
                                                        MidtermEnd = x.MidtermEnd,
                                                        MidtermDate = x.MidtermDate,
                                                        IsDisabledMidterm = x.IsDisabledMidterm,
                                                        FinalDate = x.FinalDate,
                                                        FinalStart = x.FinalStart,
                                                        FinalEnd = x.FinalEnd,
                                                        IsDisabledFinal = x.IsDisabledFinal,
                                                        FinalRoom = x.FinalRoom.NameEn,
                                                        MidtermRoom = x.MidtermRoom.NameEn,
                                                        Username = x.CreatedBy,
                                                        ParentSectionId = x.ParentSectionId,
                                                        IsOutbound = x.IsOutbound,
                                                        IsSpecialCase = x.IsSpecialCase,
                                                        MasterSection = x.ParentSectionId == null ? null
                                                                                                  : new MasterAndJointSectionViewModel 
                                                                                                    {
                                                                                                        SectionId = x.ParentSection.Id,
                                                                                                        SectionNumber = x.ParentSection.Number,
                                                                                                        CourseCode = x.ParentSection.Course.Code,
                                                                                                        CourseName = x.ParentSection.Course.NameEn,
                                                                                                        CourseCredit = x.ParentSection.Course.CreditText,
                                                                                                    },
                                                        Course = x.CourseId == 0 ? null
                                                                                    : new CourseApiViewModel
                                                                                      {
                                                                                          CourseId = x.Course.Id,
                                                                                          CourseCredit = x.Course.Credit,
                                                                                          CourseCode = x.Course.Code,
                                                                                          CourseNameEn = x.Course.NameEn,
                                                                                          Lecture = x.Course.Lecture,
                                                                                          Lab = x.Course.Lab,
                                                                                          Other = x.Course.Other
                                                                                      },
                                                        MainInstructor = new InstructorApiViewModel
                                                                         {
                                                                             InstructorCode = x.MainInstructor.Code,
                                                                             Title = x.MainInstructor.Title.NameEn,
                                                                             Name = x.MainInstructor.FirstNameEn,
                                                                             LastName = x.MainInstructor.LastNameEn
                                                                         },
                                                    })
                                             .ToList();

            var sectionids = sections.Select(x => x.SectionId).ToList();
            var sectionidsNullable = sections.Select(x => (long?)x.SectionId).ToList();

            var sectionslots = _db.SectionSlots.AsNoTracking()
                                               .Where(x => sectionids.Contains(x.SectionId))
                                               .Select(x => new SectionSlotApiViewModel
                                                               {
                                                                   SectionSlotId = x.Id,
                                                                   SectionId = x.SectionId,
                                                                   Room = x.Room.NameEn,
                                                                   Day = x.Dayofweek,
                                                                   StartTime = x.StartTime,
                                                                   EndTime = x.EndTime,
                                                                   Status = x.Status,
                                                                   Date = x.Date,
                                                                   TeachingType = x.TeachingType.NameEn,
                                                                   Remark = x.Remark,
                                                                   TotalTime = Convert.ToInt32(x.EndTime.TotalMinutes - x.StartTime.TotalMinutes),
                                                                   Instructor = x.InstructorId == null ? null
                                                                                                       : new InstructorApiViewModel
                                                                                                         {
                                                                                                             InstructorCode = x.Instructor.Code,
                                                                                                             Title = x.Instructor.Title.NameEn,
                                                                                                             Name = x.Instructor.FirstNameEn,
                                                                                                             LastName = x.Instructor.LastNameEn
                                                                                                         },
                                                               })
                                               .ToList();

            var coInstructors = _db.SectionSlots.AsNoTracking()
                                                .Where(x => sectionids.Contains(x.SectionId) && x.InstructorId != null)
                                                .Select(x => new 
                                                            {
                                                                SectionId = x.SectionId,
                                                                Instructor = x.InstructorId == null ? null
                                                                                                    : new InstructorApiViewModel
                                                                                                      {
                                                                                                          InstructorCode = x.Instructor.Code,
                                                                                                          Title = x.Instructor.Title.NameEn,
                                                                                                          Name = x.Instructor.FirstNameEn,
                                                                                                          LastName = x.Instructor.LastNameEn
                                                                                                      }
                                                            })
                                                .Distinct()
                                                .ToList();

            var sectiondetails = _db.SectionDetails.AsNoTracking()
                                                   .Where(x => sectionids.Contains(x.SectionId))
                                                   .Select(x => new SectionDetailApiViewModel
                                                                   {
                                                                       SectionDetailId = x.Id,
                                                                       SectionId = x.SectionId,
                                                                       RoomId = x.RoomId,
                                                                       Room = x.Room.NameEn,
                                                                       Day = x.Dayofweek,
                                                                       StartTime = x.StartTime,
                                                                       EndTime = x.EndTime,
                                                                       TeachingType = x.TeachingType.NameEn,
                                                                       Instructor = x.InstructorId == null ? null
                                                                                                           : new InstructorApiViewModel
                                                                                                             {
                                                                                                                 InstructorCode = x.Instructor.Code,
                                                                                                                 Title = x.Instructor.Title.NameEn,
                                                                                                                 Name = x.Instructor.FirstNameEn,
                                                                                                                 LastName = x.Instructor.LastNameEn
                                                                                                             },
                                                                       })
                                                   .ToList();

            var exams = _db.ExaminationReservations.AsNoTracking()
                                                   .Where(x => sectionids.Contains(x.SectionId))
                                                   .Select(x => new ExamTypeApiViewModel
                                                                {
                                                                    ExaminationId = x.Id,
                                                                    SectionId = x.SectionId,
                                                                    AbsentInstructor = x.AbsentInstructor,
                                                                    AllowBooklet = x.AllowBooklet,
                                                                    AllowCalculator = x.AllowCalculator,
                                                                    AllowOpenbook = x.AllowOpenbook,
                                                                    AllowAppendix = x.AllowAppendix,
                                                                    ExaminationType = x.ExaminationType == null ? string.Empty
                                                                                                                : x.ExaminationType.NameEn,
                                                                    Room = x.Room.NameEn,
                                                                    TotalProctor = x.TotalProctor,
                                                                    StudentRemark = x.StudentRemark,
                                                                    ProctorRemark = x.ProctorRemark,
                                                                    ExamStatus = x.ExamStatus,
                                                                    Status = x.Status,
                                                                    Instructor = x.Instructor == null ? null
                                                                                                      : new InstructorApiViewModel
                                                                                                        {
                                                                                                            InstructorCode = x.Instructor.Code,
                                                                                                            Title = x.Instructor.Title.NameEn,
                                                                                                            Name = x.Instructor.FirstNameEn,
                                                                                                            LastName = x.Instructor.LastNameEn
                                                                                                        },
                                                                    Date = x.Date,
                                                                    StartTime = x.StartTime,
                                                                    EndTime = x.EndTime,
                                                                })
                                                   .ToList();

            var sectionSlotExams = _db.ExaminationReservations.AsNoTracking()
                                                              .Where(x => sectionids.Contains(x.SectionId))
                                                              .Select(x => new SectionSlotApiViewModel
                                                                           {
                                                                               SectionId = x.SectionId,
                                                                               Status = String.IsNullOrEmpty(x.ExamStatus) ? (x.Status == "r" ? "r" : null) : x.ExamStatus,
                                                                               TeachingType = x.ExaminationType == null ? string.Empty
                                                                                                                        : x.ExaminationType.NameEn,
                                                                               Room = x.Room.NameEn,
                                                                               Instructor = x.Instructor == null ? null
                                                                                                                 : new InstructorApiViewModel
                                                                                                                   {
                                                                                                                       InstructorCode = x.Instructor.Code,
                                                                                                                       Title = x.Instructor.Title.NameEn,
                                                                                                                       Name = x.Instructor.FirstNameEn,
                                                                                                                       LastName = x.Instructor.LastNameEn
                                                                                                                   },
                                                                               Date = x.Date,
                                                                               Day = x.Dayofweek,
                                                                               StartTime = x.StartTime,
                                                                               EndTime = x.EndTime,
                                                                               AbsentInstructor = x.AbsentInstructor,
                                                                               ExaminationId = x.Id,
                                                                               TotalTime = Convert.ToInt32(x.EndTime.TotalMinutes - x.StartTime.TotalMinutes)
                                                                           })
                                                              .ToList();

            var jointSections = _db.Sections.AsNoTracking()
                                            .Where(x => sectionidsNullable.Contains(x.ParentSectionId))
                                            .Select(x => new MasterAndJointSectionViewModel
                                                         {
                                                             SectionId = x.Id,
                                                             SectionNumber = x.Number,
                                                             CourseCode = x.Course.Code,
                                                             CourseName = x.Course.NameEn,
                                                             CourseCredit = x.Course.CreditText,
                                                             ParentSectionId = (long)x.ParentSectionId,
                                                             SeatUsed = x.SeatUsed
                                                         })
                                            .ToList();

            foreach (var item in sections)
            {
                item.SectionSlots = sectionslots.Where(x => x.SectionId == item.SectionId).ToList();
                item.SectionSlots.AddRange(sectionSlotExams.Where(x => x.SectionId == item.SectionId).ToList());
                item.SectionDetails = sectiondetails.Where(x => x.SectionId == item.SectionId).ToList();
                item.ExaminationReservation = exams.Where(x => x.SectionId == item.SectionId).ToList();

                var students = _registrationProvider.CallUSparkAPIGetStudentsBySectionId(item.SectionId);
                item.StudentList = _registrationProvider.GetRegistrationCourseByStudentCodes(students, item.SectionId);

                item.CoInstructors = coInstructors.Where(x => x.SectionId == item.SectionId)
                                                  .Select(x => x.Instructor)
                                                  .GroupBy(x => x.InstructorCode)
                                                  .Select(x => x.FirstOrDefault())
                                                  .ToList();
                item.JointSections = jointSections.Where(x => x.ParentSectionId == item.SectionId).ToList();

                foreach(var jointSection in item.JointSections)
                {
                    students = _registrationProvider.CallUSparkAPIGetStudentsBySectionId(jointSection.SectionId);
                    item.StudentList.AddRange(_registrationProvider.GetRegistrationCourseByStudentCodes(students, jointSection.SectionId));
                }
                item.TotalSeatUsed = item.SeatUsed + jointSections.Where(x => x.ParentSectionId == item.SectionId).Sum(x => x.SeatUsed);
            };
  
            return Success(sections);
        }

        [HttpGet]
        public IActionResult GetRooms(DateTime date, TimeSpan startTime ,TimeSpan endTime) 
        {
            if(ValidApiKey())
            {
                return Unauthorized(ApiException.InvalidKey());
            }

            var rooms = _roomProvider.GetAvailableRoom(date, startTime, endTime, string.Empty);
            var roomIds = rooms.Select(x => x.Id).ToList();
            var result = _db.Rooms.Where(x => roomIds.Contains(x.Id))
                                  .Select(x => new RoomApiViewModel
                                               {
                                                   RoomId = x.Id,
                                                   NameEn = x.NameEn,
                                                   Capacity = x.Capacity,
                                                   ExaminationCapacity = x.ExaminationCapacity,
                                                   Floor = x.Floor,
                                                   RoomType = x.RoomType.Name,
                                                   Building = x.Building.NameEn,
                                                   IsOnline = x.IsOnline
                                               }).ToList();

            return Success(result);
        }

        [HttpGet]
        public IActionResult GetExaminationReservations(long sectionId) 
        {
            if(ValidApiKey())
            {
                return Unauthorized(ApiException.InvalidKey());
            }

            var result = _db.ExaminationReservations.Where(x => x.SectionId == sectionId)
                                                    .Select(x => new 
                                                                 {
                                                                     Id = x.Id,
                                                                     SectionId = x.SectionId,
                                                                     CourseCode = x.Section.Course.Code,
                                                                     CourseNameEn = x.Section.Course.NameEn,
                                                                     TermText = x.Term.TermText,
                                                                     ExaminationTypeId = x.ExaminationTypeId,
                                                                     ExaminationTypeNameEn = x.ExaminationType.NameEn,
                                                                     RoomId = x.RoomId,
                                                                     RoomNameEn = x.Room.NameEn,
                                                                     Date = x.Date,
                                                                     StartTime = x.StartTime,
                                                                     EndTime = x.EndTime,
                                                                     TotalProctor = x.TotalProctor,
                                                                     StudentRemark = x.StudentRemark,
                                                                     ProctorRemark = x.ProctorRemark,
                                                                     Status = x.Status,
                                                                     StatusText = x.StatusText,
                                                                     SenderType = x.SenderType,
                                                                     SenderTypeText = x.SenderTypeText,
                                                                     Instructor = x.Instructor == null ? null
                                                                                                       :  new InstructorApiViewModel
                                                                                                          {
                                                                                                              InstructorCode = x.Instructor.Code,
                                                                                                              Title = x.Instructor.Title.NameEn,
                                                                                                              Name = x.Instructor.FirstNameEn,
                                                                                                              LastName = x.Instructor.LastNameEn
                                                                                                          },
                                                                 })
                                                    .ToList();

            return Success(result);
        }

        [HttpGet]
        public IActionResult GetTeachingTypes() 
        {
            if(ValidApiKey())
            {
                return Unauthorized(ApiException.InvalidKey());
            }

            var result = _selectListProvider.GetTeachingTypes()
                                            .Select(x => new TeachingTypeApiViewModel
                                                         {
                                                             TeachingTypeId = long.Parse(x.Value),
                                                             NameEn = x.Text
                                                         })
                                            .ToList();

            return Success(result);
        }

        [HttpGet]
        public IActionResult GetInstructorOfTerm(long academicYear, long academicTerm, string instructorCode = "")
        {
            if(ValidApiKey())
            {
                return Unauthorized(ApiException.InvalidKey());
            }

            var term = _db.Terms.Where(x => x.AcademicYear == academicYear && x.AcademicTerm == academicTerm).ToList();

            if(!term.Any())
            {
                return Error(SectionApiException.TermNotFound());
            }

            if(!string.IsNullOrEmpty(instructorCode))
            {
                var isInstructor = _db.Instructors.Any(x => x.Code == instructorCode);
                if(!isInstructor)
                {
                    return Error(SectionApiException.InstructorNotFound());
                }
            }

            var sections = _db.Sections.AsNoTracking()
                                       .Where(x => (string.IsNullOrEmpty(instructorCode) || x.MainInstructor.Code == instructorCode)                                               
                                                   && x.Term.AcademicYear == academicYear
                                                   && (x.Term.AcademicTerm == academicTerm
                                                       || academicTerm == 0 ))
                                       .Union(_db.SectionDetails.Where(x => (string.IsNullOrEmpty(instructorCode) || x.Instructor.Code == instructorCode)
                                                                            && x.Section.Term.AcademicYear == academicYear
                                                                            && (x.Section.Term.AcademicTerm == academicTerm
                                                                                || academicTerm == 0 ))
                                                                .Select(x => x.Section))
                                       .Union(_db.SectionSlots.Where(x => (string.IsNullOrEmpty(instructorCode) || x.Instructor.Code == instructorCode)
                                                                          && x.Section.Term.AcademicYear == academicYear
                                                                          && (x.Section.Term.AcademicTerm == academicTerm
                                                                              || academicTerm == 0 ))
                                                              .Select(x => x.Section))
                                       .Select(x => new CourseSectionInstructorApiViewModel
                                                    {
                                                        SectionId = x.Id,
                                                        SectionNumber = x.Number,
                                                        MainInstructorId = x.MainInstructorId,
                                                        ParentSectionId = x.ParentSectionId,
                                                        IsSpecialCase = x.IsSpecialCase,
                                                        IsOutbound = x.IsOutbound,
                                                        MasterSection = x.ParentSectionId == null ? null
                                                                                                  : new MasterAndJointSectionViewModel 
                                                                                                    {
                                                                                                        SectionId = x.ParentSection.Id,
                                                                                                        SectionNumber = x.ParentSection.Number,
                                                                                                        CourseCode = x.ParentSection.Course.Code,
                                                                                                        CourseName = x.ParentSection.Course.NameEn,
                                                                                                        CourseCredit = x.ParentSection.Course.CreditText,
                                                                                                    },
                                                        Course = x.CourseId == 0 ? null
                                                                                    : new CourseApiViewModel
                                                                                      {
                                                                                          CourseId = x.Course.Id,
                                                                                          CourseCredit = x.Course.Credit,
                                                                                          CourseCode = x.Course.Code,
                                                                                          CourseNameEn = x.Course.NameEn,
                                                                                          Lecture = x.Course.Lecture,
                                                                                          Lab = x.Course.Lab,
                                                                                          Other = x.Course.Other
                                                                                      },
                                                        MainInstructor = new InstructorApiViewModel
                                                                         {
                                                                             InstructorCode = x.MainInstructor.Code,
                                                                             Title = x.MainInstructor.Title.NameEn,
                                                                             Name = x.MainInstructor.FirstNameEn,
                                                                             LastName = x.MainInstructor.LastNameEn
                                                                         },
                                                    })
                                             .ToList();

            var sectionids = sections.Select(x => x.SectionId).ToList();
            var sectionidsNullable = sections.Select(x => (long?)x.SectionId).ToList();

            var coInstructors = _db.SectionSlots.AsNoTracking()
                                                .Where(x => sectionids.Contains(x.SectionId) && x.InstructorId != null)
                                                .Distinct()
                                                .ToList();

            var jointSections = _db.Sections.AsNoTracking()
                                            .Where(x => sectionidsNullable.Contains(x.ParentSectionId))
                                            .Select(x => new MasterAndJointSectionViewModel
                                                         {
                                                             SectionId = x.Id,
                                                             SectionNumber = x.Number,
                                                             CourseCode = x.Course.Code,
                                                             CourseName = x.Course.NameEn,
                                                             CourseCredit = x.Course.CreditText,
                                                             ParentSectionId = (long)x.ParentSectionId,
                                                             SeatUsed = x.SeatUsed
                                                         })
                                            .ToList();

            foreach (var item in sections)
            {
                item.CoInstructors = coInstructors.Where(x => x.SectionId == item.SectionId)
                                                  .Select(x => x.InstructorId)
                                                  .Distinct()
                                                  .ToList();

                item.JointSections = jointSections.Where(x => x.ParentSectionId == item.SectionId).ToList();
            };
            var instructorIds = sections.Select(x => x.MainInstructorId).ToList();
            instructorIds = instructorIds.Union(coInstructors.Select(x => x.InstructorId))
                                         .Distinct()
                                         .ToList();
            var instructors =  _db.Instructors.Where(x => instructorIds.Contains(x.Id))
                                              .Select(x => new InstructorInfoApiViewModel 
                                                           {
                                                                Id = x.Id,
                                                                InstructorCode = x.Code,
                                                                FirstNameEn = x.FirstNameEn,
                                                                LastNameEn = x.LastNameEn,
                                                                TitleEn = x.Title.NameEn,
                                                                FirstNameTh = x.FirstNameTh,
                                                                LastNameTh = x.LastNameTh,
                                                                TitleTh = x.Title.NameTh,
                                                                AcademicLevel = x.InstructorWorkStatus.AcademicLevel.NameEn,
                                                                FacultyCode = x.InstructorWorkStatus.Faculty.Code,
                                                                Faculty = x.InstructorWorkStatus.Faculty.NameEn,
                                                                DepartmentCode = x.InstructorWorkStatus.Department.Code,
                                                                Department = x.InstructorWorkStatus.Department.NameEn,
                                                                Email = x.Email,
                                                                Type = x.InstructorWorkStatus.InstructorType.NameEn
                                                           })
                                              .IgnoreQueryFilters()
                                              .ToList();

            foreach(var item in instructors)
            {
                item.Courses = sections.Where(x => x.MainInstructorId == item.Id || x.CoInstructors.Any(y => y == item.Id))
                                       .Select(x => {
                                                        x.InstructorId = item.Id;
                                                        return x;
                                                    })
                                       .ToList();
            }

            return Success(instructors);
        }


        [HttpPost]
        public IActionResult CreateMakeUp([FromBody]CreateSectionSlotMakeUp model)
        {
            if(ValidApiKey())
            {
                return Unauthorized(ApiException.InvalidKey());
            }
            long instructorId = 0;

            if(!string.IsNullOrEmpty(model.InstructorCode))
            {
                var instructor = _db.Instructors.SingleOrDefault(x => x.Code == model.InstructorCode);
                if(instructor == null)
                {
                    return Error(SectionApiException.InstructorNotFound());
                }

                instructorId = instructor.Id;
            }

            if (model.StartTime > model.EndTime)
            {
                return Error(ApiException.InvalidParameter());
            }

            // UNIVERSITY HOLIDAY
            if(CheckUniversityHolidayAndLockReservationForMakeUp(model.Date))
            {
                return Error(SectionApiException.SectionSlotNotAllow());
            }

            if(CheckStatusSectionSlot(model.Status))
            {
                return Error(SectionApiException.TimeSlotStatusNotFound());
            }

            if(model.RoomId != 0)
            {
                var room =  _db.Rooms.SingleOrDefault(x => x.Id == model.RoomId);
                if(room == null)
                {
                    return Error(SectionApiException.RoomNotFound());
                } 
                else if (!_roomProvider.IsBuildingOpen(model.RoomId, model.StartTime, model.EndTime))
                {
                    return Error(SectionApiException.BuildingIsNotAvailable());
                }
            }

            var sectionSlot = new SectionSlot
                              {
                                  SectionId = model.SectionId,
                                  TeachingTypeId = model.TeachingTypeId,
                                  RoomId = model.RoomId,
                                  Date = model.Date,
                                  Day = (int)model.Date.DayOfWeek,
                                  StartTime = model.StartTime,
                                  EndTime = model.EndTime,
                                  Status = model.Status,
                                  IsActive = true,
                                  IsMakeUpClass = true,
                                  Remark = model.Remark
                              };
            var isHasExamOnSameDate = _reservationProvider.IsExamReservationExist(model.SectionId, 0, model.Date, model.StartTime, model.EndTime);
            if (isHasExamOnSameDate)
            {
                return Error(SectionApiException.ExaminationOverlap());
            }
            var isSectionOverlap = _sectionProvider.IsSectionSlotExisted(model.SectionId, 0, 0, model.Date, model.StartTime, model.EndTime);
            if (isSectionOverlap)
            {
                return Error(SectionApiException.SectionSlotOverlap());
            }

            if (instructorId != 0)
            {
                sectionSlot.InstructorId = instructorId;

                // check if instructor in exam reservation or not
                //API mode will check if instuctor are booking any others examreservation in the same time
                var isOtherExamInstructorAsProctor = _reservationProvider.IsOtherExamReservationExist(instructorId, model.Date,
                    model.StartTime, model.EndTime, 0);
                if (isOtherExamInstructorAsProctor)
                {
                    return Error(SectionApiException.ExaminationOverlap());
                }
            }

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    _db.SectionSlots.Add(sectionSlot);
                    _db.SaveChanges();

                    var result = _db.SectionSlots.Include(x => x.Instructor)
                                                     .ThenInclude(x => x.Title)
                                                 .Include(x => x.Room)
                                                 .Include(x => x.Section)
                                                 .Include(x => x.Section)
                                                    .ThenInclude(x => x.Term)
                                                 .Include(x => x.TeachingType)
                                                 .SingleOrDefault(x => x.Id == sectionSlot.Id);
                                                 
                    if(_roomProvider.IsExistedRoomSlot(result.RoomId ?? 0, result.Date, result.StartTime, result.EndTime) == null)
                    {
                        _roomProvider.CreateRoomSlotBySectionSlot(result);
                    }
                    else
                    {
                        transaction.Rollback();
                        return Error(ApiException.Forbidden());
                    }

                    transaction.Commit();

                    var resultViewModel = new SectionSlotApiViewModel
                                          {
                                              SectionSlotId = result.Id,
                                              SectionId = result.SectionId,
                                              Room = result.Room != null ? result.Room.NameEn : string.Empty,
                                              Status = result.Status,
                                              Day = result.Dayofweek,
                                              StartTime = result.StartTime,
                                              EndTime = result.EndTime,
                                              Date = result.Date,
                                              Remark = result.Remark,
                                              TotalTime = Convert.ToInt32(result.EndTime.TotalMinutes - result.StartTime.TotalMinutes),
                                              TeachingType = result.TeachingType == null ? string.Empty
                                                                                         : result.TeachingType.NameEn,
                                              Instructor = result.Instructor == null ? null
                                                                                     : new InstructorApiViewModel
                                                                                       {
                                                                                           InstructorCode = result.Instructor.Code,
                                                                                           Title = result.Instructor.Title.NameEn,
                                                                                           Name = result.Instructor.FirstNameEn,
                                                                                           LastName = result.Instructor.LastNameEn
                                                                                       },
                                          };
                    return Success(resultViewModel);
                }
                catch
                {
                    transaction.Rollback();
                    return Error(ApiException.Forbidden());
                }
            }            
        }

        [HttpPost]
        public IActionResult UpdateStatusSectionSlot(string status, long id)
        {
            if(ValidApiKey())
            {
                return Unauthorized(ApiException.InvalidKey());
            }

            if(CheckStatusSectionSlot(status))
            {
                return Error(SectionApiException.TimeSlotStatusNotFound());
            }

            var isSectionSlot = _db.SectionSlots.Any(x => x.Id == id);
            if(!isSectionSlot)
            {
                return Error(SectionApiException.SectionSlotNotFound());
            }

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var result = _db.SectionSlots.Include(x => x.Instructor)
                                                     .ThenInclude(x => x.Title)
                                                 .Include(x => x.Room)
                                                 .Include(x => x.TeachingType)
                                                 .SingleOrDefault(x => x.Id == id);
                    result.Status = status;

                    _db.SaveChanges();
                    if(status == "c")
                    {
                        _roomProvider.CancelRoomSlotBySectionSlotId(result.Id);
                    }

                    transaction.Commit();
                    var resultViewModel = new SectionSlotApiViewModel
                                          {
                                              SectionSlotId = result.Id,
                                              SectionId = result.SectionId,
                                              Room = result.Room != null ? result.Room.NameEn : string.Empty,
                                              Status = result.Status,
                                              Day = result.Dayofweek,
                                              StartTime = result.StartTime,
                                              EndTime = result.EndTime,
                                              Date = result.Date,
                                              Remark = result.Remark,
                                              TotalTime = Convert.ToInt32(result.EndTime.TotalMinutes - result.StartTime.TotalMinutes),
                                              TeachingType = result.TeachingType == null ? string.Empty
                                                                                         : result.TeachingType.NameEn,
                                              Instructor = result.Instructor == null ? null
                                                                                     : new InstructorApiViewModel
                                                                                       {
                                                                                           InstructorCode = result.Instructor.Code,
                                                                                           Title = result.Instructor.Title.NameEn,
                                                                                           Name = result.Instructor.FirstNameEn,
                                                                                           LastName = result.Instructor.LastNameEn
                                                                                       },
                                          };
                    return Success(resultViewModel);
                }
                catch
                {
                    transaction.Rollback();
                    return Error(SectionApiException.TimeSlotNotUpdate());
                }
            }            
        }

        [HttpPost]
        public IActionResult UpdateTeachingTypeSectionSlot(long teachingTypeId, long id)
        {
            if(ValidApiKey())
            {
                return Unauthorized(ApiException.InvalidKey());
            }

            var isTeachingType = _db.TeachingTypes.Any(x => x.Id == teachingTypeId);
            if(!isTeachingType)
            {
                return Error(SectionApiException.TeachingTypesNotFound());
            }

            var isSectionSlot = _db.SectionSlots.Any(x => x.Id == id);
            if(!isSectionSlot)
            {
                return Error(SectionApiException.SectionSlotNotFound());
            }
            

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var result = _db.SectionSlots.SingleOrDefault(x => x.Id == id);
                    result.TeachingTypeId = teachingTypeId;

                    _db.SaveChanges();

                    transaction.Commit();

                    result = _db.SectionSlots.Include(x => x.Instructor)
                                                     .ThenInclude(x => x.Title)
                                                 .Include(x => x.Room)
                                                 .Include(x => x.TeachingType)
                                                 .SingleOrDefault(x => x.Id == id);

                    var resultViewModel = new SectionSlotApiViewModel
                                          {
                                              SectionSlotId = result.Id,
                                              SectionId = result.SectionId,
                                              Room = result.Room != null ? result.Room.NameEn : string.Empty,
                                              Status = result.Status,
                                              Day = result.Dayofweek,
                                              StartTime = result.StartTime,
                                              EndTime = result.EndTime,
                                              Date = result.Date,
                                              Remark = result.Remark,
                                              TotalTime = Convert.ToInt32(result.EndTime.TotalMinutes - result.StartTime.TotalMinutes),
                                              TeachingType = result.TeachingType == null ? string.Empty
                                                                                         : result.TeachingType.NameEn,
                                              Instructor = result.Instructor == null ? null
                                                                                     : new InstructorApiViewModel
                                                                                       {
                                                                                           InstructorCode = result.Instructor.Code,
                                                                                           Title = result.Instructor.Title.NameEn,
                                                                                           Name = result.Instructor.FirstNameEn,
                                                                                           LastName = result.Instructor.LastNameEn
                                                                                       },
                                          };
                    return Success(resultViewModel);
                }
                catch
                {
                    transaction.Rollback();
                    return Error(SectionApiException.TimeSlotNotUpdate());
                }
            }            
        }

        [HttpPost]
        public IActionResult UpdateRemarkSectionSlot(long id, string remark)
        {
            if(ValidApiKey())
            {
                return Unauthorized(ApiException.InvalidKey());
            }

            var isSectionSlot = _db.SectionSlots.Any(x => x.Id == id);
            if(!isSectionSlot)
            {
                return Error(SectionApiException.SectionSlotNotFound());
            }
            

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var result = _db.SectionSlots.SingleOrDefault(x => x.Id == id);
                    result.Remark = remark;

                    _db.SaveChanges();

                    transaction.Commit();

                    result = _db.SectionSlots.Include(x => x.Instructor)
                                                     .ThenInclude(x => x.Title)
                                                 .Include(x => x.Room)
                                                 .Include(x => x.TeachingType)
                                                 .SingleOrDefault(x => x.Id == id);

                    var resultViewModel = new SectionSlotApiViewModel
                                          {
                                              SectionSlotId = result.Id,
                                              SectionId = result.SectionId,
                                              Room = result.Room != null ? result.Room.NameEn : string.Empty,
                                              Status = result.Status,
                                              Day = result.Dayofweek,
                                              StartTime = result.StartTime,
                                              EndTime = result.EndTime,
                                              Date = result.Date,
                                              Remark = result.Remark,
                                              TotalTime = Convert.ToInt32(result.EndTime.TotalMinutes - result.StartTime.TotalMinutes),
                                              TeachingType = result.TeachingType == null ? string.Empty
                                                                                         : result.TeachingType.NameEn,
                                              Instructor = result.Instructor == null ? null
                                                                                     : new InstructorApiViewModel
                                                                                       {
                                                                                           InstructorCode = result.Instructor.Code,
                                                                                           Title = result.Instructor.Title.NameEn,
                                                                                           Name = result.Instructor.FirstNameEn,
                                                                                           LastName = result.Instructor.LastNameEn
                                                                                       },
                                          };
                    return Success(resultViewModel);
                }
                catch
                {
                    transaction.Rollback();
                    return Error(SectionApiException.TimeSlotNotUpdate());
                }
            }            
        }

        [HttpPost]
        public IActionResult UpdateExam([FromBody] UpdateExamViewModel model)
        {            
            if(ValidApiKey())
            {
                return Unauthorized(ApiException.InvalidKey());
            }

            if(model == null)
            {
                return Error(ApiException.InvalidParameter());
            }

            if(CheckStatusExam(model.Status))
            {
                return Error(SectionApiException.ExaminationStatusNotFound());
            }

            var section = _db.Sections.SingleOrDefault(x => x.Id == model.SectionId);
            if(section == null)
            {
                return Error(SectionApiException.SectionNotFound());
            }

            if (model.Date <= new DateTime(1970,1,1))
            {
                return Error(ApiException.InvalidParameter());
            }

            if ((model.StartTime > model.EndTime) || (model.StartTime == TimeSpan.Zero)  || (model.EndTime == TimeSpan.Zero))
            {
                return Error(ApiException.InvalidParameter());
            }

            long examinationTypeId = 0;
            if(model.ExamType == "m")
            {
                examinationTypeId = _db.ExaminationTypes.FirstOrDefault(x => x.NameEn == "Midterm").Id;
            } 
            else if(model.ExamType == "f")
            {
                examinationTypeId = _db.ExaminationTypes.FirstOrDefault(x => x.NameEn == "Final").Id;
            } 
            else 
            {
                return Error(SectionApiException.ExaminationTypeNotFound());
            }
            
            if(section.IsDisabledFinal && model.ExamType == "f")
            {
                return Error(SectionApiException.ExamNotUpdateFinalDisabled());
            }
            else if(section.IsDisabledMidterm && model.ExamType == "m")
            {
                return Error(SectionApiException.ExamNotUpdateMidtermDisabled());
            }

            long instructorId = 0;
            if(!string.IsNullOrEmpty(model.InstructorCode))
            {
                var instructor = _db.Instructors.SingleOrDefault(x => x.Code == model.InstructorCode);
                if(instructor == null)
                {
                    return Error(SectionApiException.InstructorNotFound());
                }
                instructorId = instructor.Id;
            }

            long roomId = 0;
            if(model.RoomId != 0)
            {
                var room = _db.Rooms.FirstOrDefault(x => x.Id == model.RoomId);
                if(room == null)
                {
                    return Error(SectionApiException.RoomNotFound());
                }
                roomId = room.Id;
            }

            using (var transaction = _db.Database.BeginTransaction())
            {
                ExaminationReservation newExam = new ExaminationReservation()
                {
                    TermId = section.TermId,
                    SectionId = section.Id,
                    ExaminationTypeId = examinationTypeId,
                    Date = model.Date,
                    StartTime = model.StartTime,
                    EndTime = model.EndTime,
                    UseProctor = model.TotalProctor != 0,
                    StudentRemark = model.StudentRemark,
                    Status = model.Status,
                    SenderType = "api", // s = student, i = instructor, a = admin, api
                    IsActive = model.IsActive,
                    AbsentInstructor = model.AbsentInstructor,
                    AllowBooklet = model.AllowBooklet,
                    AllowCalculator = model.AllowCalculator,
                    AllowOpenbook = model.AllowOpenbook,
                    AllowAppendix = model.AllowAppendix,
                    TotalProctor = model.TotalProctor,                      
                    ProctorRemark = model.ProctorRemark,
                    InstructorId = instructorId,
                    RoomId = roomId
                };

                var result = _reservationProvider.UpdateExaminationReservation(newExam);
                switch(result.Status)
                {
                    case UpdateExamStatus.OverlapSectionSlot :
                        var sectionSlotOverlap = GetSectionSlotOverlap(instructorId, 0, model.Date, model.StartTime, model.EndTime, 0);
                        //if (sectionSlotOverlap != null)
                        //{
                        //    sectionSlotOverlap = sectionSlotOverlap.Where(x => !x.ExaminationId.HasValue
                        //                                                            || x.ExaminationId.Value < 1)
                        //                                           .ToList();
                        //}
                        transaction.Rollback();
                        return Error(SectionApiException.SectionSlotOverlap(), sectionSlotOverlap);

                    case UpdateExamStatus.OverlapExam :
                        transaction.Rollback();
                        var examOverlap = GetExanTypeOverlap(result.ExaminationReservation?.Id ?? 0, examinationTypeId, instructorId, roomId, model.Date, model.StartTime, model.EndTime);
                        return Error(SectionApiException.ExaminationOverlap(), examOverlap);

                    case UpdateExamStatus.ExaminationAlreadyApproved :
                        transaction.Rollback();
                        return Error(SectionApiException.ExaminationAlreadyApproved());

                    case UpdateExamStatus.SaveExamSucceed:
                    case UpdateExamStatus.UpdateExamSuccess:
                        transaction.Commit();
                        return Success(GenerateExamTypeApiViewModel(result.ExaminationReservation));
                    
                    default:
                        return Error(SectionApiException.ExaminationNotUpdate());

                } 
            }
        }

        [HttpPost]
        public IActionResult DisabledExam([FromBody] DisabledExamViewModel model)
        {            
            if(ValidApiKey())
            {
                return Unauthorized(ApiException.InvalidKey());
            }

            var section = _db.Sections.SingleOrDefault(x => x.Id == model.SectionId);
            if(section == null)
            {
                return Error(SectionApiException.SectionNotFound());
            }

            var exam = _db.ExaminationReservations.Include(x => x.ExaminationType)
                                                  .Include(x => x.Instructor)
                                                    .ThenInclude(x => x.Title)
                                                  .Include(x => x.Room)
                                                  .Where(x => x.SectionId == model.SectionId).ToList();

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {   
                       section.IsDisabledMidterm = model.IsDisabledMidterm;
                       section.IsDisabledFinal = model.IsDisabledFinal;
                       _db.SaveChanges();
                       if(exam.Any())
                       {
                            var jointSections = _db.Sections.Where(x => x.ParentSectionId == section.Id).ToList();
                            if(section.IsDisabledMidterm)
                            {   

                                var examinationTypeId = _db.ExaminationTypes.FirstOrDefault(x => x.NameEn.ToLower() == "midterm").Id;
                                section.MidtermDate = null;
                                section.MidtermStart = null;
                                section.MidtermEnd = null;
                                section.MidtermRoomId = null;

                                if(jointSections != null && jointSections.Any())
                                {
                                    foreach (var jointSection in jointSections)
                                    {
                                        jointSection.MidtermDate = null;
                                        jointSection.MidtermStart = null;
                                        jointSection.MidtermEnd = null;
                                        jointSection.MidtermRoomId = null;
                                    }
                                }
                                _db.SaveChanges();
                                
                                var midterms = exam.Where(x => x.ExaminationTypeId == examinationTypeId).ToList();
                                if(midterms.Any())
                                {
                                    foreach(var item in midterms)
                                    {
                                        var roomSlot = _db.RoomSlots.Where(x => x.ExaminationReservationId == item.Id).FirstOrDefault();
                                        if(roomSlot != null)
                                        {
                                            roomSlot.IsActive = false;
                                            roomSlot.IsCancel = true;
                                            _db.SaveChanges();
                                        }
                                        item.Status = "r";
                                        item.IsActive = false;
                                    }
                                }
                                _db.SaveChanges();
                            }

                            if(section.IsDisabledFinal)
                            {
                                var examinationTypeId = _db.ExaminationTypes.FirstOrDefault(x => x.NameEn.ToLower() == "final").Id;
                                section.FinalDate = null;
                                section.FinalStart = null;
                                section.FinalEnd = null;
                                section.FinalRoomId = null;

                                if(jointSections != null && jointSections.Any())
                                {
                                    foreach (var jointSection in jointSections)
                                    {
                                        jointSection.FinalDate = null;
                                        jointSection.FinalStart = null;
                                        jointSection.FinalEnd = null;
                                        jointSection.FinalRoomId = null;
                                    }
                                }
                                _db.SaveChanges();
                                var finals = exam.Where(x => x.ExaminationTypeId == examinationTypeId).ToList();
                                if(finals.Any())
                                {
                                    foreach(var item in finals)
                                    {
                                        var roomSlot = _db.RoomSlots.Where(x => x.ExaminationReservationId == item.Id).FirstOrDefault();
                                        if(roomSlot != null)
                                        {
                                            roomSlot.IsActive = false;
                                            roomSlot.IsCancel = true;
                                            _db.SaveChanges();
                                        }
                                        item.Status = "r";
                                        item.IsActive = false;
                                    }
                                }
                                _db.SaveChanges();
                            }
                       }
                        transaction.Commit();
                        return Success(null);
                }
                catch
                {
                    transaction.Rollback();
                    return Error(SectionApiException.ExaminationNotUpdate());
                }
            }
        }

        [HttpPost]
        public IActionResult UpdateStatusExam(string status, long id)
        {
            if(ValidApiKey())
            {
                return Unauthorized(ApiException.InvalidKey());
            }

            if(CheckStatusSectionSlot(status))
            {
                return Error(SectionApiException.ExaminationStatusNotFound());
            }

            var isExamination = _db.ExaminationReservations.Any(x => x.Id == id);
            if(!isExamination)
            {
                return Error(SectionApiException.ExaminationNotFound());
            }

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var result = _db.ExaminationReservations.SingleOrDefault(x => x.Id == id);
                    result.ExamStatus = status;

                    _db.SaveChanges();

                    transaction.Commit();

                    return Success(GenerateExamTypeApiViewModel(result));
                }
                catch
                {
                    transaction.Rollback();
                    return Error(SectionApiException.ExamNotUpdate());
                }
            }            
        }

        [HttpPatch]
        public IActionResult UpdateInstructorSectionslot([FromBody] SectionUpdateSlotInstructorViewModel request)
        {
            if (ValidApiKey())
            {
                return Unauthorized(ApiException.InvalidKey());
            }

            if (string.IsNullOrEmpty(request.NewLecturerUsername))
            {
                return Error(SectionApiException.InstructorNotFound());
            }

            var oldInstructor = _db.Instructors.Include(x => x.Title)
                                               .FirstOrDefault(x => x.Code.Equals(request.MainLecturerUsername));

            var newInstructor = _db.Instructors.Include(x => x.Title)
                                               .FirstOrDefault(x => x.Code.Equals(request.NewLecturerUsername));
            if (newInstructor == null)
            {
                return Error(SectionApiException.InstructorNotFound());
            }

            var sectionSlot = _db.SectionSlots//.Include(x => x.Instructor)
                                              //     .ThenInclude(x => x.Title)
                                               .Include(x => x.Room)
                                               .Include(x => x.TeachingType)
                                               .Include(x => x.Section)
                                               .SingleOrDefault(x => x.Id == request.SectionSlotId 
                                                                        && x.SectionId == request.SectionId
                                                                        // 7 Aug 24 : so this is a misunderstanding
                                                                        //&& ( ((x.InstructorId == null || x.InstructorId == 0) && oldInstructor == null ) 
                                                                        //         || ( x.InstructorId == oldInstructor.Id )
                                                                        //   )
                                                                        && (((x.Section.MainInstructorId == null || x.Section.MainInstructorId == 0) && oldInstructor == null)
                                                                                 || (x.Section.MainInstructorId == oldInstructor.Id)
                                                                           )
                                                                );
            if (sectionSlot == null)
            {
                return Error(SectionApiException.SectionSlotNotFound());
            }

            try
            {   
                sectionSlot.InstructorId = newInstructor.Id;
                sectionSlot.UpdatedAt = DateTime.UtcNow;
                sectionSlot.UpdatedBy = string.IsNullOrEmpty(request.UpdatedBy) ? "SectionAPI" : request.UpdatedBy;

                _db.SaveChangesNoAutoUserIdAndTimestamps();
               
                var resultViewModel = new SectionSlotApiViewModel
                {
                    SectionSlotId = sectionSlot.Id,
                    SectionId = sectionSlot.SectionId,
                    Room = sectionSlot.Room != null ? sectionSlot.Room.NameEn : string.Empty,
                    Status = sectionSlot.Status,
                    Day = sectionSlot.Dayofweek,
                    StartTime = sectionSlot.StartTime,
                    EndTime = sectionSlot.EndTime,
                    Date = sectionSlot.Date,
                    Remark = sectionSlot.Remark,
                    TotalTime = Convert.ToInt32(sectionSlot.EndTime.TotalMinutes - sectionSlot.StartTime.TotalMinutes),
                    TeachingType = sectionSlot.TeachingType == null ? string.Empty
                                                                                        : sectionSlot.TeachingType.NameEn,
                    Instructor =  new InstructorApiViewModel
                                  {
                                      InstructorCode = newInstructor.Code,
                                      Title = newInstructor.Title.NameEn,
                                      Name = newInstructor.FirstNameEn,
                                      LastName = newInstructor.LastNameEn
                                  },
                };
                return Success(resultViewModel);
            }
            catch
            {
                return Error(SectionApiException.TimeSlotNotUpdate());
            }
        }

        private ExamTypeApiViewModel GenerateExamTypeApiViewModel(ExaminationReservation model)
        {
            var exam = _db.ExaminationReservations.Include(x => x.ExaminationType)
                                                  .Include(x => x.Instructor)
                                                    .ThenInclude(x => x.Title)
                                                  .Include(x => x.Room)
                                                  .SingleOrDefault(x => x.Id == model.Id);

            return new ExamTypeApiViewModel 
                   {
                        ExaminationId = exam.Id,
                        SectionId = exam.SectionId,
                        ExaminationType = exam.ExaminationType.NameEn,
                        Room = exam.Room == null ? string.Empty : exam.Room.NameEn,
                        Date = exam.Date,
                        StartTime = exam.StartTime,
                        EndTime = exam.EndTime,
                        AbsentInstructor = exam.AbsentInstructor,
                        AllowBooklet = exam.AllowBooklet,
                        AllowCalculator = exam.AllowCalculator,
                        AllowOpenbook = exam.AllowOpenbook,
                        AllowAppendix = exam.AllowAppendix,
                        TotalProctor = exam.TotalProctor,
                        StudentRemark = exam.StudentRemark,
                        ProctorRemark = exam.ProctorRemark,
                        ExamStatus = exam.ExamStatus,
                        Status = exam.Status,
                        Instructor = exam.Instructor == null ? null 
                                                             : new InstructorApiViewModel()
                                                               {
                                                                 InstructorCode = exam.Instructor.Code,
                                                                 Title = exam.Instructor.Title.NameEn,
                                                                 Name = exam.Instructor.FirstNameEn,
                                                                 LastName = exam.Instructor.LastNameEn
                                                               }
                   };
        }

        private List<SectionSlotApiViewModel> GetSectionSlotOverlap(long InstructorId, long roomId, DateTime date, TimeSpan? start, TimeSpan? end, long sectionId = 0)
        {
            var sectionSlotOverlap = _db.SectionSlots.Where(x => x.Date == date
                                                                 && x.StartTime < end
                                                                 && x.EndTime > start
                                                                 && (InstructorId == 0 || x.InstructorId == InstructorId)
                                                                 && (roomId == 0 || x.RoomId == roomId)
                                                                 && (sectionId == 0 || x.SectionId == sectionId)
                                                                 && x.Status != "c")
                                                      .Select(x => new SectionSlotApiViewModel
                                                                      {
                                                                          SectionSlotId = x.Id,
                                                                          SectionId = x.SectionId,
                                                                          Room = x.Room.NameEn,
                                                                          Day = x.Dayofweek,
                                                                          StartTime = x.StartTime,
                                                                          EndTime = x.EndTime,
                                                                          Status = x.Status,
                                                                          Date = x.Date,
                                                                          TeachingType = x.TeachingType.NameEn,
                                                                          Remark = x.Remark,
                                                                          TotalTime = Convert.ToInt32(x.EndTime.TotalMinutes - x.StartTime.TotalMinutes),
                                                                          Instructor = x.InstructorId == null ? null
                                                                                                              : new InstructorApiViewModel
                                                                                                                {
                                                                                                                    InstructorCode = x.Instructor.Code,
                                                                                                                    Title = x.Instructor.Title.NameEn,
                                                                                                                    Name = x.Instructor.FirstNameEn,
                                                                                                                    LastName = x.Instructor.LastNameEn
                                                                                                                },
                                                                      })
                                                      .ToList();

            return sectionSlotOverlap;
        }

        private List<ExamTypeApiViewModel>GetExanTypeOverlap(long id, long examinationTypeId, long instructorId, long roomId, DateTime date, TimeSpan? start, TimeSpan? end)
        {
            var examOverlap = _db.ExaminationReservations.Where(x => x.Id != id
                                                                     && x.ExaminationTypeId == examinationTypeId
                                                                     && (roomId == 0 || x.RoomId == roomId)
                                                                     && (instructorId == 0 || x.InstructorId == instructorId)
                                                                     && x.Status != "r"
                                                                     && x.Date == date
                                                                     && x.StartTime < end
                                                                     && x.EndTime > start)
                                                         .Select(exam => new ExamTypeApiViewModel 
                                                                         {
                                                                                 ExaminationId = exam.Id,
                                                                                 SectionId = exam.SectionId,
                                                                                 ExaminationType = exam.ExaminationType.NameEn,
                                                                                 Room = exam.Room == null ? string.Empty : exam.Room.NameEn,
                                                                                 Date = exam.Date,
                                                                                 StartTime = exam.StartTime,
                                                                                 EndTime = exam.EndTime,
                                                                                 AbsentInstructor = exam.AbsentInstructor,
                                                                                 AllowBooklet = exam.AllowBooklet,
                                                                                 AllowCalculator = exam.AllowCalculator,
                                                                                 AllowOpenbook = exam.AllowOpenbook,
                                                                                 AllowAppendix = exam.AllowAppendix,
                                                                                 TotalProctor = exam.TotalProctor,
                                                                                 StudentRemark = exam.StudentRemark,
                                                                                 ProctorRemark = exam.ProctorRemark,
                                                                                 ExamStatus = exam.ExamStatus,
                                                                                 Status = exam.Status,
                                                                                 Instructor = exam.Instructor == null ? null 
                                                                                                                     : new InstructorApiViewModel()
                                                                                                                     {
                                                                                                                         InstructorCode = exam.Instructor.Code,
                                                                                                                         Title = exam.Instructor.Title.NameEn,
                                                                                                                         Name = exam.Instructor.FirstNameEn,
                                                                                                                         LastName = exam.Instructor.LastNameEn
                                                                                                                     }
                                                                         })
                                                         .ToList();

            return examOverlap;
        }



        private bool CheckUniversityHolidayAndLockReservationForMakeUp(DateTime datetime)
        {
            DateTime firstHoliday = new DateTime(2022,05,04);
            DateTime secondHoliday = new DateTime(2022,06,03);
            var dateNow = DateTime.UtcNow.AddHours(7).Date;
            if (firstHoliday.Date == datetime.Date || secondHoliday.Date == datetime.Date)
            {
                return true;
            }

            if ( _db.ReservationCalendars.Any(x => x.StartedAt <= dateNow
                                                     && x.EndedAt >= dateNow
                                                     && x.IsActive)
                )
            {
                return true;
            }
            return _db.MuicHolidays.Any(x => x.StartedAt <= datetime.Date
                                                && x.EndedAt >= datetime.Date
                                                && !x.IsMakeUpAble
                                                && x.IsActive);
        }

        private bool CheckStatusSectionSlot(string status)
        {
                switch (status)
                {
                    case "w":
                    case "p":
                    case "c":
                        return false;
                    default:
                        return true;
                }            
        }

        private bool CheckStatusExam(string status)
        {
                switch (status)
                {
                    case "w":
                    case "r":
                    case "c":
                        return false;
                    default:
                        return true;
                }
        }

        /// <summary>
        /// Get all Section Slot on specify date
        /// </summary>
        /// <param name="date">Search Date. String Format [dd/MM/yyyy HH:mm ] year is AD (2024)</param>
        /// <param name="isIncludeCancel">True = also include section slot that already canceled</param>
        /// <response code="200">List of section slots.</response>
        /// <response code="401">Specify api-key</response>
        /// <response code="400">Wrong input format or error see message detail</response>
        [Route("[controller]/GetSectionSlots")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(APIResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(APIResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(APIResponse))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(GetSectionSlotsDefault))]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnAuthorizeAPIResponseExample))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestWrongDateResponseExample))]
        public IActionResult GetSectionSlots([Required]string date, bool isIncludeCancel = false) 
        {
            if (ValidApiKey())
            {
                return Unauthorized(ApiException.InvalidKey());
            }

            if (string.IsNullOrEmpty(date))
            {
                return Error(ApiException.InvalidParameter());
            }

            CultureInfo enUS = new CultureInfo("en-US");
            if (!DateTime.TryParseExact(date, "dd/MM/yyyy", enUS, DateTimeStyles.None, out DateTime dateTime))
            {
                return Error(StudentAPIException.DateWrongFormat(date));
            }
            
            var sectionSlots = _db.SectionSlots.AsNoTracking()
                                               .Where(x => x.Date.Date == dateTime.Date
                                                              && (isIncludeCancel || (x.Status != "c"))
                                                     )
                                               .Select(x => new SectionSlotWithDetailApiViewModel
                                               {
                                                   SectionSlotId = x.Id,
                                                   SectionId = x.SectionId,
                                                   Room = x.Room.NameEn,
                                                   Day = x.Dayofweek,
                                                   StartTime = x.StartTime,
                                                   EndTime = x.EndTime,
                                                   Status = x.Status,
                                                   Date = x.Date,
                                                   TeachingType = x.TeachingType.NameEn,
                                                   Remark = x.Remark,
                                                   TotalTime = Convert.ToInt32(x.EndTime.TotalMinutes - x.StartTime.TotalMinutes),
                                                   Instructor = x.InstructorId == null ? null
                                                                                                       : new InstructorApiViewModel
                                                                                                       {
                                                                                                           InstructorCode = x.Instructor.Code,
                                                                                                           Title = x.Instructor.Title.NameEn,
                                                                                                           Name = x.Instructor.FirstNameEn,
                                                                                                           LastName = x.Instructor.LastNameEn
                                                                                                       },
                                                   CourseCode = x.Section.Course.Code,
                                                   CourseName = x.Section.Course.NameEn,
                                                   IsMakeUp = x.IsMakeUpClass,
                                                   SectionNumber = x.Section.Number
                                               })
                                               .ToList();
            return Success(sectionSlots);           
        }
        public class GetSectionSlotsDefault : IExamplesProvider<APIResponse>
        {
            public APIResponse GetExamples()
            {
                var data = new List<SectionSlotWithDetailApiViewModel>
                {
                    new SectionSlotWithDetailApiViewModel
                    {
                        SectionSlotId = 67890123,
                        SectionId = 123456,
                        Room = "Lab1301",
                        Day = "SAT",
                        StartTime = new TimeSpan(9,0,0),
                        EndTime = new TimeSpan(12,0,0),
                        Status = "w",
                        Date = new DateTime(2024,8,12),
                        TeachingType = "lecture",
                        Remark = "",
                        TotalTime = Convert.ToInt32((new TimeSpan(12,0,0)).TotalMinutes - (new TimeSpan(9,0,0)).TotalMinutes),
                        Instructor = new InstructorApiViewModel
                                    {
                                        InstructorCode = "teacherCode",
                                        Title = "Prof.",
                                        Name = "Somchai",
                                        LastName = "Chaidee"
                                    },
                        CourseCode = "ICBI221",
                        CourseName = "Animal Biology",
                        IsMakeUp = false,
                        SectionNumber = "1"
                    },
                    new SectionSlotWithDetailApiViewModel
                    {
                         SectionSlotId = 67890150,
                        SectionId = 123450,
                        Room = "Lab1302",
                        Day = "SAT",
                        StartTime = new TimeSpan(9,0,0),
                        EndTime = new TimeSpan(12,0,0),
                        Status = "w",
                        Date = new DateTime(2024,8,12),
                        TeachingType = "lab",
                        Remark = "",
                        TotalTime = Convert.ToInt32((new TimeSpan(12,0,0)).TotalMinutes - (new TimeSpan(9,0,0)).TotalMinutes),
                        Instructor = new InstructorApiViewModel
                                    {
                                        InstructorCode = "teacherCode",
                                        Title = "Asst. Prof.",
                                        Name = "Somsri",
                                        LastName = "Deechai"
                                    },
                        CourseCode = "ICGP104",
                        CourseName = "Body Fitness",
                        IsMakeUp = false,
                        SectionNumber = "1"
                    }
                };
                return new APIResponse
                {
                    Code = "200",
                    Message = "Success",
                    Data = data,
                };
            }
        }
    }
}