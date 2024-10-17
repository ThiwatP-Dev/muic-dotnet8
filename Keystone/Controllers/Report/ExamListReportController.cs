using KeystoneLibrary.Data;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Keystone.Permission;

namespace Keystone.Controllers
{
    [PermissionAuthorize("ExamListReport", "")]
    public class ExamListReportController : BaseController
    {
        protected readonly IDateTimeProvider _dateTimeProvider;
        protected readonly ISectionProvider _sectionProvider;
        protected readonly IReservationProvider _reservationProvider;
        protected readonly IRoomProvider _roomProvider;        
        protected readonly ICacheProvider _cacheProvider;

        public ExamListReportController(ApplicationDbContext db,
                                                          ISelectListProvider selectListProvider,
                                                          IFlashMessage flashMessage,
                                                          IDateTimeProvider dateTimeProvider,
                                                          ISectionProvider sectionProvider,
                                                          IRoomProvider roomProvider,
                                                          ICacheProvider cacheProvider,
                                                          IReservationProvider reservationProvider) : base(db, flashMessage, selectListProvider)
        {
            _dateTimeProvider = dateTimeProvider;
            _sectionProvider = sectionProvider;
            _roomProvider = roomProvider;
            _reservationProvider = reservationProvider;
            _cacheProvider = cacheProvider;
        }

        public IActionResult Index(Criteria criteria, int page)
        {
            CreateSelectList("defaultWaiting", criteria.AcademicLevelId, criteria.TermId, criteria.CampusId);
            if(criteria.AcademicLevelId == 0 && criteria.TermId == 0)
            {
                criteria.AcademicLevelId = _db.AcademicLevels.SingleOrDefault(x => x.NameEn.ToLower().Contains("bachelor")).Id;
                criteria.TermId = _cacheProvider.GetCurrentTerm(criteria.AcademicLevelId).Id;
                CreateSelectList("defaultWaiting", criteria.AcademicLevelId, criteria.TermId, criteria.CourseId,criteria.CampusId);
                return View(new PagedResult<ExportListReportViewModel>()
                    {
                        Criteria = criteria
                    });

            }
            if (criteria.ExaminationTypeId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View(new PagedResult<ExportListReportViewModel>()
                {
                    Criteria = criteria
                });
            }
            try
            {
                List<ExportListReportViewModel> examinationList = _reservationProvider.GetExaminationListReport(criteria);
                var paged = examinationList.GetPaged(criteria, 0, true);
                return View(paged);
            }
            catch (Exception ex)
            {
                _flashMessage.Danger(Message.UnableToSearch + " " + ex.Message);
                return View(new PagedResult<ExportListReportViewModel>()
                {
                    Criteria = criteria
                });
            }
        }

       
        [HttpPost]
        [RequestFormLimits(ValueCountLimit = Int32.MaxValue)]
        public IActionResult ExportExcel(Criteria criteria, string returnUrl)
        {
            List<ExportListReportViewModel> examinationList = _reservationProvider.GetExaminationListReport(criteria);
            if (examinationList != null && examinationList.Any())
            {
                DateTime? starteAt = _dateTimeProvider.ConvertStringToDateTime(criteria.StartedAt);
                DateTime? endedAt = _dateTimeProvider.ConvertStringToDateTime(criteria.EndedAt);

                var examType = criteria.ExaminationTypeId == 1 ? "Midterm" : "Final";
                var sampleDate = examinationList.First();
                var dateString = $"Term {sampleDate.TermText} {(starteAt.HasValue ? "from " + starteAt.Value.ToString("d MMMM yyyy") : "" ) + " - " + (endedAt.HasValue ? endedAt.Value.ToString("d MMMM yyyy") : "")}";

                var fileName = $"{examType} Exam of Term {sampleDate.TermText}.xlsx";
                foreach (var c in System.IO.Path.GetInvalidFileNameChars())
                {
                    fileName = fileName.Replace(c, '_');
                }

                using (var wb = GenerateWorkBook(examType, dateString, examinationList))
                {
                    return wb.Deliver(fileName);
                }
            }

            return Redirect(returnUrl);
        }

        private XLWorkbook GenerateWorkBook(string examType, string dateString, List<ExportListReportViewModel> results)
        {
            var wb = new XLWorkbook();
            var ws = wb.AddWorksheet();
            int row = 1;
            var column = 1;

            //Report Header
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column).Style.Font.Bold = true;
            ws.Cell(row, column).Style.Font.FontSize = 14;
            ws.Cell(row, column).Value = "Mahidol University International College";
            ws.Range(ws.Cell(row, column), ws.Cell(row, column + 20)).Merge();
            row++; 
            column = 1;
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column).Style.Font.Bold = true;
            ws.Cell(row, column).Style.Font.FontSize = 14;
            ws.Cell(row, column).Value = $"{examType} examination Schedule";
            ws.Range(ws.Cell(row, column), ws.Cell(row, column + 20)).Merge();
            row++;
            column = 1;
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column).Style.Font.Bold = true;
            ws.Cell(row, column).Style.Font.FontSize = 14;
            ws.Cell(row, column).Value = $"{dateString}";
            ws.Range(ws.Cell(row, column), ws.Cell(row, column + 20)).Merge();
            row++;
            column = 1;

            //real table header
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column++).Value = "Date";
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column++).Value = "Time";
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column).Style.Alignment.SetTextRotation(90);
            ws.Cell(row, column++).Value = "Division";
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column++).Value = "Subject";
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column).Style.Alignment.SetTextRotation(90);
            ws.Cell(row, column++).Value = "Section";
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column).Style.Alignment.SetTextRotation(90);
            ws.Cell(row, column++).Value = "";
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column).Style.Alignment.SetTextRotation(90);
            ws.Cell(row, column++).Value = "Co-section";
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column++).Value = "Instructor";
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column).Style.Alignment.SetTextRotation(90);
            ws.Cell(row, column++).Value = "No. of student";
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column).Style.Alignment.SetTextRotation(90);
            ws.Cell(row, column++).Value = "Student Total";
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;            
            ws.Cell(row, column++).Value = "Room";
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column).Style.Alignment.SetTextRotation(90);
            ws.Cell(row, column++).Value = "Proctor Request";
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column).Style.Alignment.SetTextRotation(90);
            ws.Cell(row, column++).Value = "No. of proctor";
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column++).Value = "Proctor's Name";
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column).Style.Alignment.SetTextRotation(90);
            ws.Cell(row, column++).Value = "Absent Instructor";
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column).Style.Alignment.SetTextRotation(90);
            ws.Cell(row, column++).Value = "Open book";
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column).Style.Alignment.SetTextRotation(90);
            ws.Cell(row, column++).Value = "Calculator";
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column).Style.Alignment.SetTextRotation(90);
            ws.Cell(row, column++).Value = "Formula Sheet";
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column).Style.Alignment.SetTextRotation(90);
            ws.Cell(row, column++).Value = "Appendix";
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column).Style.Alignment.SetTextRotation(90);
            ws.Cell(row, column++).Value = "Exam book";
            ws.Cell(row, column++).Value = "Remark";
            ws.Cell(row++, column).Value = "Status";

            var groups = from item in results
                         group item by item.DateString into resultGroup
                         select resultGroup;
            foreach (var group in groups)
            {
                column = 1;
                ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(row, column).Style.Font.Bold = true;
                ws.Cell(row, column).Style.NumberFormat.Format = "dddd, mmmm d, yyyy";
                ws.Cell(row, column).Value = $"{group.Key}";
                ws.Range(ws.Cell(row, column), ws.Cell(row, column + 20)).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                row++;
                
                foreach (var item in group)
                {
                    column = 1;
                    ws.Cell(row, column).Style.NumberFormat.Format = "dddd, mmmm d, yyyy";
                    ws.Cell(row, column++).Value = item.DateString;
                    ws.Cell(row, column++).Value = item.Time;
                    ws.Cell(row, column++).Value = item.Division;
                    ws.Cell(row, column++).Value = item.CourseCodeAndNameAndCredit;
                    ws.Cell(row, column++).Value = item.SectionNumber;
                    ws.Cell(row, column++).Value = item.SectionTypes;
                    if(item.ParentSectionId == 0 || item.ParentSectionId == null)
                    {
                        ws.Cell(row, column++).Value = item.JointSectionsString;
                    }
                    else
                    {
                        ws.Cell(row, column++).Value = $"{item.ParentSectionCourseCode} ({item.ParentSectionNumber})";
                    }
                    ws.Cell(row, column++).Value = item.InstructorFullNameEn;
                    ws.Cell(row, column++).Value = item.TotalStudent;
                    ws.Cell(row, column++).Value = "";//totaltotal student
                    ws.Cell(row, column++).Value = item.Room;
                    ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell(row, column++).Value = item.UseProctor ? "Y" : "N";
                    ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell(row, column++).Value = item.UseProctor ? item.TotalProctor.ToString() : "-";
                    ws.Cell(row, column++).Value = item.ProctorName;
                    ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell(row, column++).Value = item.AbsentInstructor ? "✓" : "";
                    ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell(row, column++).Value = item.AllowOpenbook ? "✓" : "";
                    ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell(row, column++).Value = item.AllowCalculator ? "✓" : "";
                    ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell(row, column++).Value = item.AllowFomulaSheet ? "✓" : "";
                    ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell(row, column++).Value = item.AllowAppendix ? "✓" : "";
                    ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell(row, column++).Value = item.AllowBooklet ? "✓" : "";

                    ws.Cell(row, column++).Value = item.StudentRemark;
                    ws.Cell(row++, column).Value = item.StatusText;
                }
            }
            ws.Columns(1,3).AdjustToContents();
            ws.Columns(4, 4).Width = 70;
            ws.Columns(5, 6).AdjustToContents();
            ws.Columns(8, 22).AdjustToContents();
            return wb;
        }

        private void CreateSelectList(string currentStatus = "", long academicLevelId = 0, long termId = 0, long courseId = 0, long campusId = 0)
        {
            ViewBag.Campuses = _selectListProvider.GetCampuses();
            ViewBag.Instructors = _selectListProvider.GetInstructors();
            ViewBag.ReservationStatuses = _selectListProvider.GetReservationStatuses(currentStatus);
            ViewBag.SenderTypes = _selectListProvider.GetSenderTypes();
            ViewBag.ExaminationTypes = _selectListProvider.GetExaminationTypes();
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            if (campusId != 0)
            {
                ViewBag.Buildings = _selectListProvider.GetBuildings(campusId);
            }

            if (academicLevelId != 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);

                if (termId != 0)
                {
                    ViewBag.Courses = _selectListProvider.GetCoursesByAcademicLevelAndTerm(academicLevelId, termId);

                    if (courseId != 0)
                    {
                        ViewBag.Sections = _selectListProvider.GetSectionByCourseId(termId, courseId);
                    }
                }
            }

            // TimeSpan? startedAt = _dateTimeProvider.ConvertStringToTime(start);
            // TimeSpan? endedAt = _dateTimeProvider.ConvertStringToTime(end);
            // if (date != null && startedAt != null && endedAt != null)
            // {
            //     ViewBag.Rooms = _selectListProvider.GetAvailableRoom(date.Value, startedAt.Value, endedAt.Value, "i", roomId);
            // }
            ViewBag.Rooms = _selectListProvider.GetRooms();
        }
    }
}