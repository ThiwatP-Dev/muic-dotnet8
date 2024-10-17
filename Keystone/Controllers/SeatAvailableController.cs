using AutoMapper;
using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Net;
using Vereyon.Web;

namespace Keystone.Controllers
{
    [PermissionAuthorize("SeatAvailable", "")]
    public class SeatAvailableController : BaseController 
    {
        protected readonly ICacheProvider _cacheProvider;
        protected readonly IRegistrationProvider _registrationProvider;
        protected readonly ISectionProvider _sectionProvider;

        public SeatAvailableController(ApplicationDbContext db,
                                       IMapper mapper,
                                       IMemoryCache memoryCache,
                                       IFlashMessage flashMessage,
                                       ICacheProvider cacheProvider,
                                       IRegistrationProvider registrationProvider,
                                       ISectionProvider sectionProvider,
                                       ISelectListProvider selectListProvider) : base(db, flashMessage, mapper, memoryCache, selectListProvider)
        { 
            _cacheProvider = cacheProvider;
            _registrationProvider = registrationProvider;
            _sectionProvider = sectionProvider;
        }

        public IActionResult Index(SeatAvailableViewModel model)
        {
            CreateSelectList();
            if (model.Criteria == null || (model.Criteria.AcademicLevelId == 0 && model.Criteria.TermId == 0))
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            CreateSelectList(model.Criteria);

            var sections = _db.Sections.Where(x => x.Course.AcademicLevelId == model.Criteria.AcademicLevelId
                                                   && x.TermId == model.Criteria.TermId
                                                   && (string.IsNullOrEmpty(model.Criteria.CodeAndName)
                                                       || x.Course.Code.Contains(model.Criteria.CodeAndName)
                                                       || x.Course.NameEn.Contains(model.Criteria.CodeAndName))
                                                    && (string.IsNullOrEmpty(model.Criteria.SectionNumber)
                                                      || x.Number.Contains(model.Criteria.SectionNumber))
                                                   && (model.Criteria.FacultyId == 0
                                                       || model.Criteria.FacultyId == x.Course.FacultyId)
                                                   && (model.Criteria.DepartmentId == 0
                                                       || model.Criteria.DepartmentId == x.Course.DepartmentId))
                                       .Select(x => new CoursesSeatAvailable
                                                    {
                                                        SectionId = x.Id,
                                                        TitleNameEn = x.MainInstructor.Title.NameEn,
                                                        FirstNameEn = x.MainInstructor.FirstNameEn,
                                                        LastNameEn = x.MainInstructor.LastNameEn,
                                                        CourseCode = x.Course.Code,
                                                        CourseName = x.Course.NameEn,
                                                        CourseCredit = x.Course.Credit,
                                                        CourseLab = x.Course.Lab,
                                                        CourseLecture = x.Course.Lecture,
                                                        CourseOther = x.Course.Other,
                                                        SectionNumber = x.Number,
                                                        SeatAvailable = x.SeatAvailable,
                                                        SeatLimit = x.SeatLimit,
                                                        SeatUsed = x.SeatUsed,
                                                        SeatPayment = x.RegistrationCourses.Count(y => y.IsPaid),
                                                        ParentSectionId = x.ParentSectionId,
                                                        IsOutbound = x.IsOutbound,
                                                        IsSpecialCase = x.IsSpecialCase,
                                                        Remark = x.Remark
                                                    })
                                       .ToList();

            var sectionIds = sections.Select(x => (long?)x.SectionId).ToList();
            var registrationCourses = new List<RegistrationCourseWithDrawals>();
            if(model.Criteria.IsWithdrawal)
            {
                registrationCourses = _db.RegistrationCourses.Where(x => sectionIds.Contains(x.SectionId))
                                                            .Select(x => new RegistrationCourseWithDrawals
                                                                        {
                                                                            SectionId = x.SectionId,
                                                                            IsWithDrawal = x.Withdrawals.Any(y => y.Status == "a")
                                                                        })
                                                            .ToList();
            }

            var sectionDetails = _db.SectionDetails.Where(x => sectionIds.Contains(x.SectionId))
                                                   .Select(x => new
                                                                {
                                                                    SectionId = x.SectionId,
                                                                    CodeAndName = x.Instructor == null ? "" : x.Instructor.Title.NameEn + " " + x.Instructor.FirstNameEn + " " + x.Instructor.LastNameEn
                                                                })
                                                   .Distinct()
                                                   .ToList();
                                                   
            sections.Select(x => { 
                                     x.Instructors.AddRange(sectionDetails.Where(y => x.SectionId == y.SectionId
                                                                                      && !string.IsNullOrEmpty(y.CodeAndName))
                                                                          .Select(y => y.CodeAndName)  
                                                                          .ToList());   

                                     x.SeatWithdraw = registrationCourses.Count(y => y.SectionId == x.SectionId && y.IsWithDrawal);

                                     return x;
                                 }).ToList();

            model.Courses = sections.OrderBy(x => x.CourseCode)
                                    .ThenBy(x => x.SectionNumber)
                                    .ToList();

            return View(model);
        }

        [PermissionAuthorize("SeatAvailable", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditSeat(long id, int seatLimit, string remark, int seatAvailable = 0) 
        {
            var section = _db.Sections.Include(x => x.ParentSection).SingleOrDefault(x => x.Id == id);
            var jointSections = new List<Section>();
            if(section.ParentSection != null)
            {
                if(seatLimit > section.ParentSection.SeatLimit)
                {
                    return Ok(new {
                                    IsInvalidSeat = true,
                                    Section = section
                                  });
                }
            }
            else
            {
                jointSections =  _db.Sections.Where(x => x.ParentSectionId == id).ToList();
            }

            using(var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    section.SeatLimit = seatLimit;
                    section.Remark = remark;
                    _db.Entry(section).State = EntityState.Modified;
                    _db.SaveChanges();

                    _sectionProvider.ReCalculateSeatAvailable(section.Id);
                   
                    //update available seat
                    _registrationProvider.CallUSparkAPIUpdateSeat(section.Id);

                    if(jointSections.Any())
                    {
                        foreach(var item in jointSections)
                        {
                            item.SeatLimit = seatLimit;

                            _sectionProvider.ReCalculateSeatAvailable(item.Id);
                            _registrationProvider.CallUSparkAPIUpdateSeat(item.Id);
                        }
                        _db.SaveChanges();
                    }
                    
                    transaction.Commit();
                    return Ok(new {
                                    IsInvalidSeat = false,
                                    Section = section
                                  });
                } 
                catch
                {
                    transaction.Rollback();
                    return StatusCode((int)HttpStatusCode.Forbidden, Message.UnableToEdit);
                }
            }
        }

        private void CreateSelectList()
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
        }

        private void CreateSelectList(Criteria criteria)
        {
            ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(criteria.AcademicLevelId);
            ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(criteria.AcademicLevelId, criteria.FacultyId);
            ViewBag.Courses = _selectListProvider.GetCoursesBySectionGroup(criteria.TermId, criteria.FacultyId, criteria.DepartmentId);
            ViewBag.Sections = _selectListProvider.GetSections(criteria.TermId, criteria.CourseIds);
            ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(criteria.AcademicLevelId);
        }
    }
}