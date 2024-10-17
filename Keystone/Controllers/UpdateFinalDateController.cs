using KeystoneLibrary.Data;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models.DataModels;
using Microsoft.EntityFrameworkCore;
using Keystone.Permission;

namespace Keystone.Controllers
{
    [PermissionAuthorize("UpdateFinalDate", "")]
    public class UpdateFinalDateController : BaseController
    {
        protected readonly IReservationProvider _reservationProvider;
        protected readonly ISectionProvider _sectionProvider;

        public UpdateFinalDateController(ApplicationDbContext db,
                                         ISelectListProvider selectListProvider,
                                         IReservationProvider reservationProvider,
                                         ISectionProvider sectionProvider,
                                         IFlashMessage flashMessage) : base(db, flashMessage, selectListProvider)
        {
            _reservationProvider = reservationProvider;
            _sectionProvider = sectionProvider;
        }

        public IActionResult Index(Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId);
            if (criteria.AcademicLevelId == 0 || criteria.TermId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var results = _db.Sections.Where(x => x.TermId == criteria.TermId
                                                  && !x.IsDisabledFinal
                                                  && !x.IsClosed
                                                  && (string.IsNullOrEmpty(criteria.CodeAndName)
                                                      || x.Course.Code.Contains(criteria.CodeAndName)
                                                      || x.Course.NameEn.Contains(criteria.CodeAndName))
                                                  && ((criteria.SectionFrom ?? 0) == 0
                                                      || x.NumberValue >= criteria.SectionFrom)
                                                  && ((criteria.SectionTo ?? 0) == 0
                                                      || x.NumberValue <= criteria.SectionTo)
                                                  && (string.IsNullOrEmpty(criteria.SectionStatus)
                                                      || x.Status == criteria.SectionStatus)
                                                  && (string.IsNullOrEmpty(criteria.HaveFinal)
                                                      || (Convert.ToBoolean(criteria.HaveFinal) ? x.FinalDate.HasValue && x.FinalDate != new DateTime()
                                                                                               : !x.FinalDate.HasValue || x.FinalDate == new DateTime()))
                                                  &&  x.ParentSectionId == null
                                                //   (string.IsNullOrEmpty(criteria.SectionType)
                                                //       || (criteria.SectionType == "o" ? x.IsOutbound
                                                //                                       : criteria.SectionType == "g"
                                                //                                       ? x.IsSpecialCase
                                                //                                       : criteria.SectionType == "j" 
                                                //                                       ? x.ParentSectionId != null
                                                //                                       : x.ParentSectionId == null))
                                            )
                                      .Select(x => new UpdateFinalDate
                                                   {
                                                       SectionId = x.Id,
                                                       SectionNumber = x.Number,
                                                       CourseCode = x.Course.Code,
                                                       CourseName = x.Course.NameEn,
                                                       CourseCredit = x.Course.Credit,
                                                       CourseLab = x.Course.Lab,
                                                       CourseLecture = x.Course.Lecture,
                                                       CourseOther = x.Course.Other,
                                                       SeatUsedText = x.SeatUsedText,
                                                       FinalDate = x.FinalDate,
                                                       FinalDateTime = x.FinalDateTime,
                                                       StatusText = x.StatusText,
                                                       Status = x.Status,
                                                       SectionTypes = x.SectionTypes,
                                                       InstructorName = x.MainInstructor == null ? null : x.MainInstructor.Title.NameEn + " " + x.MainInstructor.FirstNameEn + " " + x.MainInstructor.LastNameEn
                                                   })
                                      .OrderBy(x => x.CourseCode)
                                         .ThenBy(x => x.SectionNumber)
                                      .ToList();

            UpdateFinalDateViewModel model = new UpdateFinalDateViewModel
                                             {
                                                 Criteria = criteria,
                                                 Results = results
                                             };

            return View(model);
        }

        public IActionResult Results(List<UpdateFinalDate> model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        [PermissionAuthorize("UpdateFinalDate", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Results(UpdateFinalDateViewModel model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (model.FinalDate == null 
                || model.FinalStart == null 
                || model.FinalEnd == null)
            {
                _flashMessage.Warning(Message.RequiredData);
            }
            else if(model.FinalStart > model.FinalEnd)
            {
                _flashMessage.Warning(Message.InvalidStartedTime);
            }
            else
            {
                try
                {
                    List<UpdateFinalDate> success = new List<UpdateFinalDate>();
                    List<UpdateFinalDate> fails = new List<UpdateFinalDate>();
                    var result = new UpdateFinalDateViewModel();
                    var finalType = _db.ExaminationTypes.FirstOrDefault(x => x.NameEn == "Final");
                    int successNumber = 0;
                    foreach (var item in model.Results.Where(x => x.IsSelected))
                    {
                        var section = _db.Sections.Include(x => x.Course)
                                                .SingleOrDefault(x => x.Id == item.SectionId);
                        if (section != null)
                        {
                            var examReserv = _db.ExaminationReservations.FirstOrDefault(x => x.IsActive
                                                                                            && x.TermId == section.TermId
                                                                                            && x.SectionId == item.SectionId
                                                                                            && x.ExaminationTypeId == finalType.Id
                                                                                        );
                            ExaminationReservation reservation;
                            if (examReserv != null)
                            {
                                reservation = examReserv;
                                reservation.Date = model.FinalDate.Value;
                                reservation.StartTime = model.FinalStart.Value;
                                reservation.EndTime = model.FinalEnd.Value;
                                reservation.Status = "w";
                            }
                            else 
                            { 
                                reservation = new ExaminationReservation
                                            {
                                                TermId = section.TermId,
                                                SectionId = section.Id,
                                                InstructorId = section.MainInstructorId,
                                                ExaminationTypeId = finalType.Id,
                                                Date = model.FinalDate.Value,
                                                StartTime = model.FinalStart.Value,
                                                EndTime = model.FinalEnd.Value,
                                                UseProctor = false,
                                                Status = "w",
                                                SenderType = "a",
                                                AbsentInstructor = true,
                                                AllowBooklet = false,
                                                AllowCalculator = false,
                                                AllowOpenbook = false
                                            };
                            }
                            var examResult = _reservationProvider.UpdateExaminationReservation(reservation);
                            switch (examResult.Status)
                            {
                                case UpdateExamStatus.SaveExamSucceed :
                                case UpdateExamStatus.UpdateExamSuccess :
                                    successNumber++;
                                    success.Add(new UpdateFinalDate
                                                {
                                                    CourseCode = section.Course.Code,
                                                    CourseName = section.Course.NameEn,
                                                    SectionNumber = section.Number,
                                                    Reason = examResult.Message
                                                });
                                    continue;
                                case UpdateExamStatus.ExaminationAlreadyApproved :
                                    fails.Add(new UpdateFinalDate
                                                {
                                                    CourseCode = section.Course.Code,
                                                    CourseName = section.Course.NameEn,
                                                    SectionNumber = section.Number,
                                                    Reason = Message.DataAlreadyExist
                                                });
                                    continue;
                                default:
                                    fails.Add(new UpdateFinalDate
                                                {
                                                    CourseCode = section.Course.Code,
                                                    CourseName = section.Course.NameEn,
                                                    SectionNumber = section.Number,
                                                    Reason = Message.UnableToSave
                                                });
                                    continue;
                            }
                        }
                    }

                    _db.SaveChanges();
                    _flashMessage.Confirmation($"Save succeed { successNumber } section(s).");
                    result.Fails = fails;
                    result.Success = success;
                    return View(result);
                }
                catch
                {
                    _flashMessage.Danger(Message.UnableToSave);
                }
            }

            return Redirect(returnUrl);
        }

        private void CreateSelectList(long academicLevelId)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.SectionStatuses = _selectListProvider.GetSectionStatuses();
            ViewBag.SectionTypes = _selectListProvider.GetSectionTypes();
            ViewBag.YesNoAnswer = _selectListProvider.GetYesNoAnswer();
            if (academicLevelId > 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            }
        }
    }
}