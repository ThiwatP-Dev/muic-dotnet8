using AutoMapper;
using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{


    public class CloseSectionController : BaseController
    {
        protected readonly IRegistrationProvider _registrationProvider;
        protected readonly ISectionProvider _sectionProvider;
        
        public CloseSectionController(ApplicationDbContext db,
                                      IFlashMessage flashMessage,
                                      IMapper mapper,
                                      ISelectListProvider selectListProvider,
                                      IRegistrationProvider registrationProvider,
                                      ISectionProvider sectionProvider) : base(db, flashMessage, mapper, selectListProvider) 
        { 
            _registrationProvider = registrationProvider;
            _sectionProvider = sectionProvider;
        }

        [PermissionAuthorize("CloseSection", "")]
        public IActionResult Index(Criteria criteria, int page = 1)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.TermId);
            var sections = _db.Sections.Include(x => x.Course)
                                       .Include(x => x.RegistrationCourses)
                                       .Where(x => !x.IsClosed
                                                   && x.Course.AcademicLevelId == criteria.AcademicLevelId
                                                   && x.TermId == criteria.TermId
                                                   && x.SeatUsed < criteria.Amount
                                                   && (criteria.CourseId == 0
                                                       || x.CourseId == criteria.CourseId))
                                       .ToList();
            
            var model = sections.OrderBy(x => x.Course.CodeAndName)
                                    .ThenBy(x => x.Number)
                                    .ThenBy(x => x.SeatUsed)
                                .AsQueryable()
                                .GetPaged(criteria, page, true);

            return View(model);
        }

        [PermissionAuthorize("CloseSection", PolicyGenerator.Write)]
        public ActionResult Close(long sectionId, int page, int amount)
        {
            var section = _sectionProvider.GetSection(sectionId);
            if (!_sectionProvider.HaveStudentsInSection(sectionId))
            {
                _flashMessage.Warning(Message.StudentsInSection);
                return RedirectToAction(nameof(Index), new 
                                                       { 
                                                           AcademicLevelId = section.Course.AcademicLevelId,
                                                           TermId = section.TermId,
                                                           CourseId = section.CourseId,
                                                           page = page,
                                                           Amount = amount
                                                       });
            }
            
            var errorMessage = "";
            using (var transaction = _db.Database.BeginTransaction())
            {
                var isClosed = _sectionProvider.CloseSection(section, out errorMessage, transaction);
                if (isClosed)
                {
                    try
                    {
                        _registrationProvider.CallUSparkAPICloseSection(sectionId);
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
                                                       CourseId = section.CourseId,
                                                       page = page,
                                                       Amount = amount
                                                   });
        }
        [PermissionAuthorize("CloseSection", PolicyGenerator.Write)]
        public ActionResult Transfer(long courseId, long sectionId, long termId)
        {
            TransferViewModel model = new TransferViewModel();
            var previousSection = _db.Sections.Include(x => x.Course)
                                              .Include(x => x.Term)
                                              .Include(x => x.RegistrationCourses)
                                                  .ThenInclude(x => x.Student)
                                              .Include(x => x.SectionDetails)
                                              .SingleOrDefault(x => x.Id == sectionId);
            
            if (previousSection != null)
            {
                model = _mapper.Map<Section, TransferViewModel>(previousSection);
                var sectionTime = previousSection.SectionDetails.OrderBy(x => x.Day)
                                                                .ThenBy(x => x.StartTime)
                                                                .ToList(); 
    
                model.PreviousSectionTime = String.Join(", ", sectionTime.Select(x => x.DayofweekAndTime));
                var sections = _db.Sections.Include(x => x.SectionDetails)
                                           .Where(x => !x.IsClosed
                                                       && !x.IsParent
                                                       && x.Id != sectionId
                                                       && x.CourseId == courseId
                                                       && x.TermId == termId
                                                       && x.SeatAvailable > 0
                                                       && x.SectionDetails.Count == sectionTime.Count)
                                           .ToList();
    
                List<Section> equalSections = new List<Section>();
                foreach (var section in sections)
                {
                    var equalSection = section.SectionDetails.Select(x => new 
                                                                          {
                                                                              x.Day,
                                                                              x.StartTime,
                                                                              x.EndTime
                                                                          }).Intersect(sectionTime.Select(x => new 
                                                                                                               {
                                                                                                                   x.Day,
                                                                                                                   x.StartTime,
                                                                                                                   x.EndTime
                                                                                                               }))
                                                             .OrderBy(x => x.Day)
                                                             .ThenBy(x => x.StartTime)
                                                             .ToList();
                        
                    if (equalSection.Any())
                    {
                        equalSections.Add(section);  
                    }         
                }
    
                ViewBag.Sections = equalSections.OrderBy(x => x.NumberValue);
            }

            return View(model);
        }

        [PermissionAuthorize("CloseSection", PolicyGenerator.Write)]
        public ActionResult ResetSeatAvailable(long sectionId, int page, int amount)
        {
            var model = _db.Sections.Include(x => x.Course)
                                    .SingleOrDefault(x => x.Id == sectionId);
            
            try
            {
                model.SeatAvailable = 0;
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToEdit);
            }
            
            
            return RedirectToAction(nameof(Index), new 
                                                   { 
                                                       AcademicLevelId = model.Course.AcademicLevelId,
                                                       TermId = model.TermId,
                                                       CourseId = model.CourseId,
                                                       page = page,
                                                       Amount = amount
                                                   });
        }

        [PermissionAuthorize("CourseToBeOffered", "")]
        public ActionResult StudentList(long sectionId, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var model = new List<CloseSectionStudentList>();
            var students = _registrationProvider.CallUSparkAPIGetStudentsBySectionId(sectionId);
            model.AddRange(_registrationProvider.GetCloseSectionStudentListsByStudents(students, sectionId));

            var jointSectionIds = _db.Sections.Where(x => x.ParentSectionId == sectionId).Select(x => x.Id).ToList();
            if(jointSectionIds != null)
            {
                var studentLists = new List<CloseSectionStudentList>();
                foreach(var jointSectionId in jointSectionIds)
                {
                    students = _registrationProvider.CallUSparkAPIGetStudentsBySectionId(jointSectionId);
                    studentLists = _registrationProvider.GetCloseSectionStudentListsByStudents(students, jointSectionId);

                    if(studentLists.Any())
                    {
                        model.AddRange(studentLists);
                    }
                }
            }
            return View(model);
        }

        private void CreateSelectList(long academicLevelId, long termId) 
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            if (academicLevelId != 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
                ViewBag.Courses = _selectListProvider.GetCoursesByTerm(termId);
            }
        }
    }
}