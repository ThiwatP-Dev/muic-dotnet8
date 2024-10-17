using AutoMapper;
using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.Graduation;
using KeystoneLibrary.Models.Enums;
using KeystoneLibrary.Models.Report;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    [PermissionAuthorize("GraduatingRequest", "")]
    public class GraduatingRequestController : BaseController
    {
        protected readonly IGraduationProvider _graduationProvider;
        protected readonly IDateTimeProvider _dateTimeProvider;
        protected readonly ICurriculumProvider _curriculumProvider;
        protected readonly ICacheProvider _cacheProvider;

        public GraduatingRequestController(ApplicationDbContext db,
                                            IGraduationProvider graduationProvider,
                                            IDateTimeProvider datetimeProvider,
                                            ICurriculumProvider curriculumProvider,
                                            IFlashMessage flashMessage,
                                            IMapper mapper,
                                            ICacheProvider cacheProvider,
                                            ISelectListProvider selectListProvider) : base(db, flashMessage, mapper, selectListProvider)
        {
            _graduationProvider = graduationProvider;
            _dateTimeProvider = datetimeProvider;
            _curriculumProvider = curriculumProvider;
            _cacheProvider = cacheProvider;
        }

        public IActionResult Index(Criteria criteria, int page = 1)
        {
            CreateSelectList(criteria);
            if (criteria.AcademicLevelId == 0)
            {
                criteria.AcademicLevelId = _db.AcademicLevels.SingleOrDefault(x => x.NameEn.ToLower().Contains("bachelor")).Id;
                criteria.TermId = _cacheProvider.GetCurrentTerm(criteria.AcademicLevelId).Id;
                criteria.StudentStatus = "s";
                criteria.RequestType = "true";
                CreateSelectList(criteria);
                return View(new PagedResult<GraduatingRequestExcelViewModel>()
                            {
                                Criteria = criteria,
                                Results = null
                            });
            }
            
            var model = _graduationProvider.GetGraduatingRequest(criteria)
                                           .AsQueryable()
                                           .GetPaged(criteria, page, true);
            return View(model);
        }

        [PermissionAuthorize("GraduatingRequest", PolicyGenerator.Write)]
        public IActionResult Create(Guid studentId, string returnUrl)
        {
            var request = _db.GraduatingRequests.FirstOrDefault(x => x.StudentId == studentId);
            if (request == null)
            {
                try
                {
                    request = _graduationProvider.CreateGraduatingRequest(studentId);
                }
                catch
                {
                    return Redirect(returnUrl);
                }
            }

            return RedirectToAction(nameof(GroupingCourseLog), new { id = request?.Id, returnUrl = returnUrl });
        }

        public async Task<IActionResult> Details(long id, Guid studentId, string returnUrl, string tabIndex)
        {
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.Index = 1;
            id = _graduationProvider.GetGraduatingRequestByStaff(studentId);
            if (id > 0)
            {
                var model = await _graduationProvider.GetGraduatingRequestDetail(id);
                CreateSelectList(model);
                return View(model);
            }
            else 
            {
                return RedirectToAction(nameof(Index), new { returnUrl = returnUrl, tabIndex = tabIndex });
            }
        }

        public IActionResult GroupingCourseLog(long id, string returnUrl)
        {
            if (returnUrl != null)
            {
                ViewBag.ReturnUrl = returnUrl;
            }
            else 
            {
                ViewBag.ReturnUrl = "/GraduatingRequest";
            }
            var model = _graduationProvider.GetCourseGroupingLogs(id);
            return View(model);
        }

        public IActionResult GroupingCourseCreate(long graduatingrequestId, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var model = _graduationProvider.GetCourseGroupingDetail(graduatingrequestId);
            ViewBag.EqualCourses = _selectListProvider.GetCoursesByAcademicLevelId(model.Student.AcademicInformation.AcademicLevelId);
            return View(model);
        }

        public IActionResult GroupingCourseDetails(long id, string returnUrl, string returnMainUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.ReturnMainUrl = returnMainUrl;
            var model = _graduationProvider.GetCourseGroupingLogDetail(id);
            ViewBag.EqualCourses = _selectListProvider.GetCoursesByAcademicLevelId(model.Student.AcademicInformation.AcademicLevelId);
            return View(model);
        }

        [PermissionAuthorize("GraduatingRequest", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveEqualCourses(GraduatingRequestViewModel model, string returnUrl, string retrunMainUrl)
        {
            var logId =  _graduationProvider.SaveEqualCourses(model);
            return RedirectToAction(nameof(GroupingCourseMoveGroup), new { id = logId,
                                                                           returnUrl = returnUrl,
                                                                           returnMainUrl = retrunMainUrl });
        }

        public IActionResult GroupingCourseMoveGroup(long id, string returnUrl, string returnMainUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.ReturnMainUrl = returnMainUrl;
            var model = _graduationProvider.GetCourseGroupingLogMoveGroup(id);
            return View(model);
        }

        [PermissionAuthorize("GraduatingRequest", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveGroupingCourseMoveGroup(GraduatingRequestViewModel model, string returnUrl, string returnMainUrl)
        {
            _graduationProvider.SaveGroupingCourseMoveGroup(model);
            if (model.IsPrint)
            {
                return RedirectToAction(nameof(Print), new { id = model.CourseGroupingLogId,
                                                             returnUrl = returnUrl,
                                                             returnMainUrl = returnMainUrl });
            }
            else
            {
                return RedirectToAction(nameof(GroupingCourseLog), new { id = model.GraduatingRequestId, returnUrl = returnMainUrl });
            }
        }

        public IActionResult Print(long id, string returnUrl, string returnMainUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.ReturnMainUrl = returnMainUrl;
            var model = _graduationProvider.GetGroupingCourseMoveGroupForPrint(id);
            return View(model);
        }

        public IActionResult ChangeStatus(long graduatingRequestId, string returnUrl)
        {
            CreateSelectList();
            ViewBag.ReturnUrl = returnUrl;
            var graduatingRequest = _db.GraduatingRequests.SingleOrDefault(x => x.Id == graduatingRequestId);
            var model = new GraduatingRequestViewModel
                        {
                            GraduatingRequestId = graduatingRequestId,
                            IsPublish = graduatingRequest.IsPublished,
                            Status = graduatingRequest.Status,
                        };

            return PartialView("_ChangeStatusForm", model);
        }

        [PermissionAuthorize("GraduatingRequest", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveChangeStatus(long graduatingRequestId, string status, string remark, bool isPublish)
        {
            var graduatingRequest = _db.GraduatingRequests.SingleOrDefault(x => x.Id == graduatingRequestId);
            if (graduatingRequest == null)
            {
                return RedirectToAction(nameof(Index));
            }
            var studentId = graduatingRequest.StudentId;
            var testStatus = new GraduatingRequest { Status = status };
            if (testStatus.StatusText == "N/A")
            {
                _flashMessage.Danger(Message.RequiredData + " Status Must be selected");
                return RedirectToAction(nameof(Details), new
                {
                    id = graduatingRequestId,
                    studentId,
                    returnUrl = (TempData["returnUrl"])?.ToString()
                });
            }
            _graduationProvider.ChangeStatus(graduatingRequestId, status, remark, isPublish);
            return RedirectToAction(nameof(Details), new { id = graduatingRequestId, studentId,
                                                           returnUrl = (TempData["returnUrl"])?.ToString() });
        }

        [PermissionAuthorize("GraduatingRequest", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveCoursePredictions(GraduatingRequestViewModel model)
        {
            _graduationProvider.SaveCoursePredictions(model.GraduatingRequestId, model.CoursePredictions);
            return RedirectToAction(nameof(Details), new { id = model.GraduatingRequestId, model.Student.Id,
                                                           returnUrl = (TempData["returnUrl"])?.ToString() });
        }

        [PermissionAuthorize("GraduatingRequest", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveGraduation(long graduatingRequestId, Guid studentId, string studentStatus, DateTime? graduatedAt, long? termId
                                            , long? honorId, string remark, List<GraduationHonor> honors, string returnUrl)
        {
            if (string.IsNullOrEmpty(studentStatus) || graduatedAt == null || termId == null)
            {
                _flashMessage.Warning(Message.RequiredData);
                return RedirectToAction(nameof(Details), new { id = graduatingRequestId,
                                                               studentId = studentId,
                                                               returnUrl = returnUrl });
            }
            
            if (_graduationProvider.SaveGraduation(studentId, studentStatus, graduatedAt, termId, honorId, remark, honors))
            {
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            else
            {
                _flashMessage.Danger(Message.UnableToEdit);
            }

            return RedirectToAction(nameof(Details), new { id = graduatingRequestId,
                                                           studentId = studentId,
                                                           returnUrl = returnUrl });
        }

        [PermissionAuthorize("GraduatingRequest", PolicyGenerator.Write)]
        public IActionResult DeleteCourseGroupingLog(long id)
        {
            var requestId =  _graduationProvider.DeleteCourseGroupingLog(id);
            return RedirectToAction(nameof(GroupingCourseLog), new { id = requestId, returnUrl = (TempData["returnUrl"])?.ToString() });
        }

        [PermissionAuthorize("GraduatingRequest", PolicyGenerator.Write)]
        public IActionResult UpdateCourseGroupingLogUnpublish(long id)
        {
            var requestId =  _graduationProvider.UpdateCourseGroupingLogTogglePublish(id, false);
            return RedirectToAction(nameof(GroupingCourseLog), new { id = requestId, returnUrl = (TempData["returnUrl"])?.ToString() });
        }

        [PermissionAuthorize("GraduatingRequest", PolicyGenerator.Write)]
        public IActionResult UpdateCourseGroupingLogPublish(long id)
        {
            var requestId =  _graduationProvider.UpdateCourseGroupingLogTogglePublish(id, true);
            return RedirectToAction(nameof(GroupingCourseLog), new { id = requestId, returnUrl = (TempData["returnUrl"])?.ToString() });
        }

        [PermissionAuthorize("GraduatingRequest", PolicyGenerator.Write)]
        public IActionResult UpdateCourseGroupingLogUnapprove(long id)
        {
            var requestId =  _graduationProvider.UpdateCourseGroupingLogToggleApprove(id, false);
            return RedirectToAction(nameof(GroupingCourseLog), new { id = requestId, returnUrl = (TempData["returnUrl"])?.ToString() });
        }

        [PermissionAuthorize("GraduatingRequest", PolicyGenerator.Write)]
        public IActionResult UpdateCourseGroupingLogApprove(long id)
        {
            var requestId =  _graduationProvider.UpdateCourseGroupingLogToggleApprove(id, true);
            return RedirectToAction(nameof(GroupingCourseLog), new { id = requestId, returnUrl = (TempData["returnUrl"])?.ToString() });
        }

        public ActionResult GetSpecializationGroups(Guid studentId, string type, long? specializationGroupId)
        {
            var model = _curriculumProvider.GetCourseSpecializationGroupRegistrations(studentId, type, specializationGroupId);
            if (model == null)
            {
                return null;
            }
            
            return PartialView("~/Views/GraduatingRequest/_SpecializationCurriculum.cshtml", model);
        }

        public ActionResult AddGroupingCourse(long courseGroupId, long graduatingRequestId, string returnUrl)
        {
            CreateSelectList();
            ViewBag.ReturnUrl = returnUrl;
            var model = new CourseGroupingViewModel
                        {
                            CourseGroupId = courseGroupId,
                            GraduatingRequestId = graduatingRequestId
                        };

            return PartialView("_AddGroupingCourseForm", model);
        }

        [PermissionAuthorize("GraduatingRequest", PolicyGenerator.Write)]
        public JsonResult SubmitAddGroupingCourse(long courseGroupId, long graduatingRequestId, long courseId, long gradeId)
        {
            var graduatingRequest = _db.GraduatingRequests.FirstOrDefault(x => x.Id == graduatingRequestId);
            var model = new CourseGroupCourseViewModel();
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    bool IsCourseExist = _db.CourseGroupModifications.Any(x => x.StudentId == graduatingRequest.StudentId
                                                                               && x.CourseGroupId == courseGroupId
                                                                               && x.CourseId == courseId);
                    if (IsCourseExist)
                    {
                        return Json("duplicate");
                    }

                    var modification = _graduationProvider.AddCourseToGraduationCourseGroup(graduatingRequest.StudentId, courseId, courseGroupId, gradeId);
                    var course = _db.Courses.SingleOrDefault(x => x.Id == modification.CourseId);
                    var requiredGrade = _db.Grades.SingleOrDefault(x => x.Id == modification.RequiredGradeId);
                    model = new CourseGroupCourseViewModel
                            {
                                CourseCode = course.Code,
                                CourseNameEn = course.NameEn,
                                CreditText = course.CreditText,
                                RequiredGradeName = requiredGrade.Name,
                                CourseModificationId = modification.Id,
                                GraduatingRequestId = graduatingRequestId
                            };
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    return Json("error");
                }
            }

            return Json(model);
        }

        public ActionResult ChangeCourseGroup(long courseGroupId, long courseId, long curriculumVersionId, long graduatingRequestId, long moveCourseGroupId, string remark, string returnUrl)
        {
            CreateSelectList(curriculumVersionId);
            ViewBag.ReturnUrl = returnUrl;
            var model = new CourseGroupingViewModel
                        {
                            CourseGroupId = courseGroupId,
                            CourseId = courseId,
                            GraduatingRequestId = graduatingRequestId,
                            MoveCourseGroupId = moveCourseGroupId,
                            Remark = remark
                        };

            return PartialView("_ChangeCourseGroupForm", model);
        }

        [PermissionAuthorize("GraduatingRequest", PolicyGenerator.Write)]
        public JsonResult ChangeCourseGroupSubmit(long courseGroupId, long courseId, long graduatingRequestId, long moveCourseGroupId, string remark)
        {
            var result = new ResponseChangeCourseGroup();
            var graduatingRequest = _db.GraduatingRequests.SingleOrDefault(x => x.Id == graduatingRequestId);
            var updateCourseGroup = _db.CourseGroupModifications.SingleOrDefault(x => x.CourseGroupId == courseGroupId
                                                                                      && x.CourseId == courseId
                                                                                      && x.StudentId == graduatingRequest.StudentId);
            var curriculumCourse = _db.CurriculumCourses.FirstOrDefault(x => x.CourseGroupId == courseGroupId
                                                                             && x.CourseId == courseId);
            var newCourseGroupModification = new CourseGroupModification();
            if (updateCourseGroup != null && updateCourseGroup.CourseGroupId == moveCourseGroupId)
            {
                result.Status = "duplicate";
                return Json(result);
            }

            try
            {
                if (updateCourseGroup == null)
                {
                    newCourseGroupModification = new CourseGroupModification
                                                 {
                                                     StudentId = graduatingRequest.StudentId,
                                                     CurriculumCourseId = curriculumCourse?.Id,
                                                     CourseId = courseId,
                                                     MoveCourseGroupId = moveCourseGroupId == 0 ? null : moveCourseGroupId,
                                                     CourseGroupId = courseGroupId,
                                                     RequiredGradeId = curriculumCourse.RequiredGradeId,
                                                     IsAddManually = false,
                                                     IsDisabled = false,
                                                     Remark = remark
                                                 };

                    _db.CourseGroupModifications.Add(newCourseGroupModification);
                }
                else
                {
                    if (moveCourseGroupId == 0 && string.IsNullOrEmpty(remark))
                    {
                        _db.CourseGroupModifications.Remove(updateCourseGroup);
                    }
                    else if(moveCourseGroupId == 0 && !string.IsNullOrEmpty(remark))
                    {
                        updateCourseGroup.MoveCourseGroupId = null;
                        updateCourseGroup.Remark = remark;
                    }
                    else
                    {
                        updateCourseGroup.MoveCourseGroupId = moveCourseGroupId;
                        updateCourseGroup.Remark = remark;
                    }
                }
                
                _db.SaveChanges();
            }
            catch
            {
                result.Status = "error";
                return Json(result);
            }

            result.MoveCourseGroup = moveCourseGroupId != 0 ? _db.CourseGroups.SingleOrDefault(x => x.Id == moveCourseGroupId).NameEn : "";
            result.Remark = remark;
            result.MoveCourseGroupId = moveCourseGroupId;
            result.Status = "success";
            return Json(result);
        }

        public ActionResult MappingCurriculum(long graduatingRequestId, string returnUrl)
        {
            long courseGroupingLogId = 0;
            courseGroupingLogId = _db.CourseGroupingLogs.Where(x => x.GraduatingRequestId == graduatingRequestId).Select(x => x.Id).FirstOrDefault();
            var curriculumVersionId = _db.GraduatingRequests.Include(x => x.Student)
                                                            .ThenInclude(x => x.AcademicInformation)
                                                            .SingleOrDefault(x => x.Id == graduatingRequestId).Student.AcademicInformation.CurriculumVersionId;
            var courseGroups = _db.CourseGroups.Where(x => x.CurriculumVersionId == curriculumVersionId).ToList();
            var coursegroupIds = courseGroups.Select(x => x.Id).ToList();
            var curriculumCourses = _db.CurriculumCourses.Where(x => coursegroupIds.Contains(x.CourseGroupId));
            var curriculumCourseCourseIds = curriculumCourses.Select(x => x.CourseId);
            var courseGroupingDetails = _db.CourseGroupingDetails.Where(x => x.CourseGroupingLogId == courseGroupingLogId)
                                                                 .ToList();
            var addGroupingCourseDetails = new List<CourseGroupingDetail>();
            var newGroupingLog = new CourseGroupingLog();

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    _db.CourseGroupingDetails.RemoveRange(courseGroupingDetails);
                    _db.SaveChanges();

                    if (courseGroupingLogId == 0)
                    {
                        newGroupingLog = new CourseGroupingLog
                                        {
                                            GraduatingRequestId = graduatingRequestId,
                                            IsPublished = false,
                                            IsApproved = false
                                        };

                        _db.CourseGroupingLogs.Add(newGroupingLog);
                        _db.SaveChanges();
                    }

                    foreach (var item in curriculumCourses)
                    {
                        var courseGroupName = courseGroups.SingleOrDefault(x => x.Id == item.CourseGroupId).NameEn;
                        var parentCourseGroup = courseGroups.SingleOrDefault(x => x.Id == item.CourseGroup.CourseGroupId);
                        var courseGroupingDetail = new CourseGroupingDetail
                                                   {
                                                       CourseGroupingLogId = courseGroupingLogId != 0 ? courseGroupingLogId : newGroupingLog.Id,
                                                       CourseGroupId = item.CourseGroupId,
                                                       CourseGroupName = courseGroupName,
                                                       CourseId = item.CourseId,
                                                       ParentCourseGroupId = parentCourseGroup?.Id,
                                                       ParentCourseGroupName = parentCourseGroup?.NameEn
                                                   };

                        addGroupingCourseDetails.Add(courseGroupingDetail);
                    }

                    _db.CourseGroupingDetails.AddRange(addGroupingCourseDetails);
                    _db.SaveChanges();
                    transaction.Commit();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                }
                catch
                {
                    transaction.Rollback();
                    _flashMessage.Danger(Message.UnableToCreate);
                }
            }

            return RedirectToAction(nameof(Details), new { id = graduatingRequestId, returnUrl = returnUrl, tabIndex = "5" });
        }

        [PermissionAuthorize("GraduatingRequest", PolicyGenerator.Write)]
        public string DisableCourse(long graduatingRequestId, long courseGroupId, long courseId, long? moveCourseGroupId)
        {
            string isDisabled = "";
            try
            {
                var graduatingRequest = _db.GraduatingRequests.SingleOrDefault(x => x.Id == graduatingRequestId);
                var courseGroupModification = _db.CourseGroupModifications.SingleOrDefault(x => x.StudentId == graduatingRequest.StudentId
                                                                                                && x.CourseGroupId == courseGroupId
                                                                                                && x.CourseId == courseId);
                var curriculumCourse = _db.CurriculumCourses.FirstOrDefault(x => x.CourseGroupId == courseGroupId && x.CourseId == courseId);
                if (courseGroupModification == null)
                {
                    var newCourseGroupModification = new CourseGroupModification
                                                     {
                                                         StudentId = graduatingRequest.StudentId,
                                                         CurriculumCourseId = curriculumCourse?.Id,
                                                         CourseId = courseId,
                                                         CourseGroupId = courseGroupId,
                                                         RequiredGradeId = curriculumCourse.RequiredGradeId,
                                                         MoveCourseGroupId = moveCourseGroupId != null ? moveCourseGroupId : null,
                                                         IsAddManually = false,
                                                         IsDisabled = true
                                                     };

                    _db.CourseGroupModifications.Add(newCourseGroupModification);
                    isDisabled = "true";
                }
                else if (courseGroupModification != null && !courseGroupModification.IsDisabled)
                {
                    courseGroupModification.IsDisabled = true;
                    isDisabled = "true";
                }
                else
                {
                    _db.CourseGroupModifications.Remove(courseGroupModification);
                    isDisabled = "false";
                }
                
                _db.SaveChanges();
            }
            catch
            {
                return "error";
            }

            return isDisabled;
        }

        [PermissionAuthorize("GraduatingRequest", PolicyGenerator.Write)]
        public bool DeleteManuallyCourse(long modificationId, long graduatingRequestId)
        {
            var studentId = _db.GraduatingRequests.Single(x => x.Id == graduatingRequestId).StudentId;
            try
            {
                var courseGroupModification = _db.CourseGroupModifications.SingleOrDefault(x => x.Id == modificationId);
                _db.CourseGroupModifications.Remove(courseGroupModification);
                _db.SaveChanges();
            }
            catch
            {
                return false;
            }

            return true;
        }

        [HttpPost]
        [RequestFormLimits(ValueCountLimit = Int32.MaxValue)]
        public IActionResult ExportExcel(Criteria criteria, string returnUrl)
        {
            var results = _graduationProvider.GetGraduatingRequest(criteria).ToList();
            if (results != null && results.Any())
            {
                using (var wb = GenerateWorkBook(results))
                {
                    return wb.Deliver($"Graduating Request Report.xlsx");
                }
            }

            return Redirect(returnUrl);
        }

        private XLWorkbook GenerateWorkBook(List<GraduatingRequestExcelViewModel> results)
        {
            var wb = new XLWorkbook();
            var ws = wb.AddWorksheet();
            int row = 1;
            var column = 1;
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column).Style.Font.Bold = true;
            ws.Cell(row, column).Style.Fill.BackgroundColor = XLColor.FromArgb(184, 204, 228);
            ws.Cell(row, column++).Value = "STUDENT CODE";

            ws.Cell(row, column).Style.Font.Bold = true;
            ws.Cell(row, column).Style.Fill.BackgroundColor = XLColor.FromArgb(184, 204, 228);
            ws.Cell(row, column++).Value = "NAME EN";

            ws.Cell(row, column).Style.Font.Bold = true;
            ws.Cell(row, column).Style.Fill.BackgroundColor = XLColor.FromArgb(184, 204, 228);
            ws.Cell(row, column++).Value = "NAME TH";

            ws.Cell(row, column).Style.Font.Bold = true;
            ws.Cell(row, column).Style.Fill.BackgroundColor = XLColor.FromArgb(184, 204, 228);
            ws.Cell(row, column++).Value = "DIVISION";

            ws.Cell(row, column).Style.Font.Bold = true;
            ws.Cell(row, column).Style.Fill.BackgroundColor = XLColor.FromArgb(184, 204, 228);
            ws.Cell(row, column++).Value = "MAJOR";

            ws.Cell(row, column).Style.Font.Bold = true;
            ws.Cell(row, column).Style.Fill.BackgroundColor = XLColor.FromArgb(184, 204, 228);
            ws.Cell(row, column++).Value = "CURRICULUM";

            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column).Style.Font.Bold = true;
            ws.Cell(row, column).Style.Fill.BackgroundColor = XLColor.FromArgb(184, 204, 228);
            ws.Cell(row, column++).Value = "TOTAL CREDIT";

            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column).Style.Font.Bold = true;
            ws.Cell(row, column).Style.Fill.BackgroundColor = XLColor.FromArgb(184, 204, 228);
            ws.Cell(row, column++).Value = "CREDIT EARN";

            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column).Style.Font.Bold = true;
            ws.Cell(row, column).Style.Fill.BackgroundColor = XLColor.FromArgb(184, 204, 228);
            ws.Cell(row, column++).Value = "CREDIT REGISTRATION";

            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column).Style.Font.Bold = true;
            ws.Cell(row, column).Style.Fill.BackgroundColor = XLColor.FromArgb(184, 204, 228);
            ws.Cell(row, column++).Value = "GPA";

            ws.Cell(row, column).Style.Font.Bold = true;
            ws.Cell(row, column).Style.Fill.BackgroundColor = XLColor.FromArgb(184, 204, 228);
            ws.Cell(row, column++).Value = "TELEPHONE";

            ws.Cell(row, column).Style.Font.Bold = true;
            ws.Cell(row, column).Style.Fill.BackgroundColor = XLColor.FromArgb(184, 204, 228);
            ws.Cell(row, column++).Value = "EMAIL";

            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column).Style.Font.Bold = true;
            ws.Cell(row, column).Style.Fill.BackgroundColor = XLColor.FromArgb(184, 204, 228);
            ws.Cell(row, column++).Value = "EXPECTED TERM";

            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column).Style.Font.Bold = true;
            ws.Cell(row, column).Style.Fill.BackgroundColor = XLColor.FromArgb(184, 204, 228);
            ws.Cell(row, column++).Value = "REQUEST DATE";

            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column).Style.Font.Bold = true;
            ws.Cell(row, column).Style.Fill.BackgroundColor = XLColor.FromArgb(184, 204, 228);
            ws.Cell(row, column++).Value = "REQUEST STATUS";

            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column).Style.Font.Bold = true;
            ws.Cell(row, column).Style.Fill.BackgroundColor = XLColor.FromArgb(184, 204, 228);
            ws.Cell(row++, column++).Value = "STUDENT STATUS";

            foreach (var item in results)
            {
                column = 1;
                ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(row, column++).SetValue<string>(item.StudentCode);

                ws.Cell(row, column++).Value = item.FullNameEn;
                ws.Cell(row, column++).Value = item.FullNameTh;
                ws.Cell(row, column++).Value = item.FacultyCodeAndName;
                ws.Cell(row, column++).Value = item.DepartmentCode;
                ws.Cell(row, column++).Value = item.CurriculumVersionCodeAndName;

                ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(row, column++).Value = item.TotalCredit;

                ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(row, column++).Value = item.CreditComp;

                ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(row, column++).Value = item.CreditEarn;

                ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(row, column++).Value = item.GPAString;

                ws.Cell(row, column++).SetValue<string>(item.TelephoneNumber);
                ws.Cell(row, column++).Value = item.Email;
                
                ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(row, column++).SetValue<string>(item.ExpectedTermText);

                ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(row, column++).SetValue<string>(item.RequestDateText);

                ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(row, column++).Value = item.RequestStatusText;

                ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(row++, column++).Value = item.StudentStatusText;
            }
            
            ws.Columns().AdjustToContents();
            ws.Rows().AdjustToContents();
            return wb;
        }
        
        public void CreateSelectList(long curriculumVersionId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Faculties = _selectListProvider.GetFaculties();
            ViewBag.Departments = _selectListProvider.GetDepartments();
            ViewBag.RequestFromStudents = _selectListProvider.GetYesNoAnswer();
            ViewBag.Statuses = _selectListProvider.GetGraduatingStatuses();
            ViewBag.CourseGroups = _selectListProvider.GetCurriculumCourseGroupsAndChildren(curriculumVersionId);
            ViewBag.Courses = _selectListProvider.GetCourses();
            ViewBag.Grades = _selectListProvider.GetGrades();
        }

        public void CreateSelectList(Criteria criteria)
        {
            if (criteria.AcademicLevelId > 0)
            {
                ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
                ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(criteria.AcademicLevelId);
                ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(criteria.AcademicLevelId, criteria.FacultyId);
                ViewBag.Curriculums = _selectListProvider.GetCurriculumByAcademicLevelId(criteria.AcademicLevelId);
                ViewBag.CurriculumVersions = _selectListProvider.GetCurriculumVersionsByCurriculumIds(criteria.AcademicLevelId, new List<long>() { criteria.CurriculumId });
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(criteria.AcademicLevelId);
                ViewBag.ExpectedGraduationTerms = _selectListProvider.GetExpectedGraduationTerms(criteria.AcademicLevelId);
                ViewBag.ExpectedGraduationYears = _selectListProvider.GetExpectedGraduationYears(criteria.AcademicLevelId);
            }
            else 
            {
                CreateSelectList();
            }
            ViewBag.RequestFromStudents = _selectListProvider.GetYesNoAnswer();
            ViewBag.Statuses = _selectListProvider.GetGraduatingStatuses();
            ViewBag.StudentStatuses = _selectListProvider.GetStudentStatuses();
            ViewBag.YesNoAnswers = _selectListProvider.GetAllYesNoAnswer();
        }

        public void CreateSelectList(GraduatingRequestViewModel model)
        {
            CreateSelectList();
            ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(model.Student.AcademicInformation.AcademicLevelId);
            ViewBag.Courses = _selectListProvider.GetCoursesByCurriculumVersion(model.Student.AcademicInformation.AcademicLevelId, model.Student.AcademicInformation.CurriculumVersionId.Value);
            ViewBag.SpecializationTypes = _selectListProvider.GetSpecializationGroupTypes();
            ViewBag.AcademicHonors = _selectListProvider.GetAcademicHonors();
            ViewBag.StudentStatuses = _selectListProvider.GetStudentStatuses(GetStudentStatusesEnum.GraduationRequest);
        }

        public ActionResult GetAcademicHonors(Guid studentId)
        {
            var model = _graduationProvider.GetGraduationHonors(studentId);
            if (model == null)
            {
                return null;
            }
            
            return PartialView("~/Views/GraduatingRequest/_GraduationHonor.cshtml", model);
        }
    }
}