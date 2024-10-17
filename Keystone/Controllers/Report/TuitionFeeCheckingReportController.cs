using Keystone.BackgroundTask;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.Report;
using KeystoneLibrary.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Diagnostics;
using Vereyon.Web;
using Keystone.Permission;
using ClosedXML.Excel;

namespace Keystone.Controllers.Report
{
    [PermissionAuthorize("TuitionFeeCheckingReport", "")]
    public class TuitionFeeCheckingReportController : BaseController
    {
        protected readonly IFileProvider _fileProvider;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly BackgroundWorkerQueue _backgroundWorkerQueue;
        private readonly IHostEnvironment _hostingEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private static string TuitionFeeJobConfigKey = "TuitionFeeCheckingReport";

        public TuitionFeeCheckingReportController(ApplicationDbContext db,
                                                  ISelectListProvider selectListProvider,
                                                  IFlashMessage flashMessage,
                                                  IFileProvider fileProvider,
                                                  IServiceScopeFactory serviceScopeFactory,
                                                  BackgroundWorkerQueue backgroundWorkerQueue,
                                                  IHostEnvironment hostingEnvironment,
                                                  IHttpContextAccessor httpContextAccessor) : base(db, flashMessage, selectListProvider)
        {
            _fileProvider = fileProvider;
            _serviceScopeFactory = serviceScopeFactory;
            _backgroundWorkerQueue = backgroundWorkerQueue;
            _hostingEnvironment = hostingEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }

        public IActionResult Index(Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId);
            if (criteria.AcademicLevelId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            //Get Report and Process it manually
            TuitionFeeCheckingReportJobConfiguration configure = null;
            var conf = _db.Configurations.FirstOrDefault(x => x.Key == TuitionFeeJobConfigKey);
            if (conf != null)
            {
                configure = JsonConvert.DeserializeObject<TuitionFeeCheckingReportJobConfiguration>(conf.Value);
            }
            else
            {
                configure = new TuitionFeeCheckingReportJobConfiguration();
            }
            var availableReport = configure.TuitionFeeCheckingReportJobs.Where(x => x.AcademicLevelId == criteria.AcademicLevelId).OrderBy(x => x.CreatedAtUTC).ToList();
            //Clean up maximum store report
            if (availableReport.Count() > 5)
            {
                var deleteReport = availableReport.First();
                if (_fileProvider.DeleteFile(UploadSubDirectory.TUITION_FEE_REPORT, deleteReport.ResultFilePath))
                {
                    configure.TuitionFeeCheckingReportJobs.Remove(deleteReport);
                    deleteReport = null;
                }

                //Save report
                if (conf != null)
                {
                    conf.Value = JsonConvert.SerializeObject(configure);
                    _db.SaveChanges();
                }
                else
                {
                    KeystoneLibrary.Models.DataModels.Configuration configdb = new KeystoneLibrary.Models.DataModels.Configuration()
                    {
                        Key = TuitionFeeJobConfigKey,
                        Value = JsonConvert.SerializeObject(configure),
                    };
                    _db.Configurations.Add(configdb);
                    _db.SaveChanges();
                }
            }

            TuitionFeeCheckingViewModel model = new TuitionFeeCheckingViewModel()
            {
                Criteria = criteria,
                Headers = null,
                Results = null,
                TuitionFeeCheckingReportJobs = availableReport,
            };
            return View(model);

            //TuitionFeeCheckingViewModel model = GetTutionFeeCheckingReport(criteria);

            //return View(model);
        }

        public IActionResult GenerateReport(Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId);
            if (criteria.AcademicLevelId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return RedirectToAction("Index", criteria);
            }

            var academicLevel = _db.AcademicLevels.FirstOrDefault(x => x.Id == criteria.AcademicLevelId);
            if (academicLevel == null)
            {
                _flashMessage.Warning(Message.RequiredData);
                return RedirectToAction("Index", criteria);
            }
            var academicLevelText = academicLevel.NameEn;

            TuitionFeeCheckingReportJobConfiguration configure = null;
            var conf = _db.Configurations.FirstOrDefault(x => x.Key == TuitionFeeJobConfigKey);
            if (conf != null)
            {
                configure = JsonConvert.DeserializeObject<TuitionFeeCheckingReportJobConfiguration>(conf.Value);
            }
            else
            {
                configure = new TuitionFeeCheckingReportJobConfiguration();
            }
            var availableReport = configure.TuitionFeeCheckingReportJobs.Where(x => x.AcademicLevelId == criteria.AcademicLevelId).OrderBy(x => x.CreatedAtUTC).ToList();
            if (availableReport.Any(x => x.CreatedAtUTC.AddHours(7).Date == DateTime.UtcNow.AddHours(7).Date && !x.EndTimeUTC.HasValue))
            {
                _flashMessage.Warning("Already has as of today report running. Please wait till the process is finish before generating new one or contact administrator");
                return RedirectToAction("Index", criteria);
            }
            TuitionFeeCheckingReportJobConfiguration.TuitionFeeCheckingReportJob job = new TuitionFeeCheckingReportJobConfiguration.TuitionFeeCheckingReportJob();
            job.AcademicLevelId = criteria.AcademicLevelId;
            job.CreatedAtUTC = DateTime.UtcNow;
            job.CreatedBy = "system test";
            configure.TuitionFeeCheckingReportJobs.Add(job);
            //Save report
            if (conf != null)
            {                
                conf.Value = JsonConvert.SerializeObject(configure);
                _db.SaveChanges();
            }
            else
            {
                KeystoneLibrary.Models.DataModels.Configuration configdb = new KeystoneLibrary.Models.DataModels.Configuration()
                {
                    Key = TuitionFeeJobConfigKey,
                    Value = JsonConvert.SerializeObject(configure),
                };
                _db.Configurations.Add(configdb);
                _db.SaveChanges();
            }

            var fileNameWithoutExtension = $"TuitionFeeReportFor{academicLevelText}_{DateTime.UtcNow.AddHours(7):yyyyMMdd_HHmm}";
            var supposeFilePath = "";
            var supposeFileUrl = "";
            try
            {
                fileNameWithoutExtension = fileNameWithoutExtension.Replace(" ", "_");
                var destinationFileName = fileNameWithoutExtension + ".xlsx";
                var folder = "/uploaded/" + UploadSubDirectory.TUITION_FEE_REPORT.GetStringValue() + "/";
                // MIGRATE RECHECK
                string folderPath = _hostingEnvironment.ContentRootPath + folder;
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                //var urlPath = _httpContextAccessor.HttpContext.Request.Scheme.ToString() + "://" + _httpContextAccessor.HttpContext.Request.Host.ToString() + folder + destinationFileName;
                var urlPath ="https://" + _httpContextAccessor.HttpContext.Request.Host.ToString() + folder + destinationFileName;
                supposeFilePath = Path.Combine(folderPath, destinationFileName);
                supposeFileUrl = urlPath;
            }
            catch
            {
                supposeFilePath = null;
                supposeFileUrl = null;
            }

            _backgroundWorkerQueue.QueueBackgroundWorkItem(async token =>
            {
                try
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {                        
                        var realdb = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        var feeProvider = scope.ServiceProvider.GetRequiredService<IFeeProvider>();
                        Stopwatch timer = new Stopwatch();
                        timer.Start();

                        var remark = "";
                        var fileUrl = "";
                        var filePath = "";

                        try
                        {
                            var result = GetTutionFeeCheckingReport(criteria, feeProvider, realdb);

                            using (var wb = GenerateWorkBook(result))
                            {
                                wb.SaveAs(supposeFilePath);

                                fileUrl = supposeFileUrl;
                                filePath = supposeFilePath;
                            }
                        }
                        catch (Exception ex2)
                        {
                            remark = "Error: " + ex2.Message + " ";
                        }

                        TuitionFeeCheckingReportJobConfiguration configureAgain = null;
                        var confAgain = realdb.Configurations.FirstOrDefault(x => x.Key == TuitionFeeJobConfigKey);
                        if (confAgain != null)
                        {
                            configureAgain = JsonConvert.DeserializeObject<TuitionFeeCheckingReportJobConfiguration>(confAgain.Value);
                            var jobAgain = configureAgain.TuitionFeeCheckingReportJobs.FirstOrDefault(x => x.CreatedAtUTC == job.CreatedAtUTC && x.AcademicLevelId == criteria.AcademicLevelId);
                            jobAgain.ElapseTime = timer.Elapsed;
                            jobAgain.EndTimeUTC = DateTime.UtcNow;
                            jobAgain.ResultFilePath = filePath;
                            jobAgain.ResultFileUrl = fileUrl;
                            jobAgain.Remark = remark;
                            confAgain.Value = JsonConvert.SerializeObject(configureAgain);
                            realdb.SaveChangesNoAutoUserIdAndTimestamps();
                        }
                        else
                        {
                            configureAgain = new TuitionFeeCheckingReportJobConfiguration();
                            job.ElapseTime = timer.Elapsed;
                            job.EndTimeUTC = DateTime.UtcNow;
                            job.ResultFilePath = filePath;
                            job.ResultFileUrl = fileUrl;
                            job.Remark = remark;
                            configureAgain.TuitionFeeCheckingReportJobs.Add(job);
                            KeystoneLibrary.Models.DataModels.Configuration configdb = new KeystoneLibrary.Models.DataModels.Configuration()
                            {
                                Key = TuitionFeeJobConfigKey,
                                Value = JsonConvert.SerializeObject(configureAgain),
                            };
                            realdb.Configurations.Add(configdb);
                            realdb.SaveChangesNoAutoUserIdAndTimestamps();
                        }

                        timer.Stop();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + ex.StackTrace);
                }
            });

            _flashMessage.Confirmation("Report is generated");
            return RedirectToAction("Index", criteria);
        }

        private XLWorkbook GenerateWorkBook(TuitionFeeCheckingViewModel result)
        {
            var wb = new XLWorkbook();
            var ws = wb.AddWorksheet();
            int row = 1;
            var column = 1;

            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Cell(row, column++).Value = "Code";
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Cell(row, column++).Value = "Name";
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Cell(row, column++).Value = "Credit";
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Cell(row, column++).Value = "Created At";
            if (result != null && result.Headers != null && result.Headers.Any())
            {
                foreach (var item in result.Headers)
                {
                    ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell(row, column++).Value = $"{item.BatchText}\n{item.StudentFeeTypeNameEn}\n{item.CustomCourseGroupName}";
                }
            }
            row++;
            column = 1;

            if (result != null && result.Results != null && result.Results.Any())
            {
                foreach(var item in result.Results.GroupBy(x => x.CourseId))
                {
                    ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    ws.Cell(row, column++).Value = item.FirstOrDefault().CourseCode;
                    ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    ws.Cell(row, column++).Value = item.FirstOrDefault().CourseName;
                    ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    ws.Cell(row, column++).Value = item.FirstOrDefault().CourseCredit;
                    ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    ws.Cell(row, column++).Value = item.FirstOrDefault().CreatedAtTHText;

                    foreach (var header in result.Headers)
                    {
                        ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Cell(row, column++).Value = item.FirstOrDefault(x => x.Sequence == header.Sequence).ValueText;
                    }

                    row++;
                    column = 1;
                }
            }

            return wb;
        }

        private TuitionFeeCheckingViewModel GetTutionFeeCheckingReport(Criteria criteria, IFeeProvider _feeProvider, ApplicationDbContext dbContext)
        {
            // var course = _db.Courses.SingleOrDefault(x => x.Id == 765);
            // var tuitionfee =_db.TuitionFees.SingleOrDefault(x => x.CourseId == 765);
            // var a = _feeProvider.CalculateTuitionFeeRate(tuitionfee, course, 3, 56);
            // return View();

            var tuitionFeeRates = dbContext.TuitionFeeRates.AsNoTracking()
                                                                 .Include(x => x.StudentFeeType)
                                                                 .Include(x => x.CustomCourseGroup)
                                                                 .OrderBy(x => x.StartedBatch)
                                                                    .ThenBy(x => x.EndedBatch)
                                                                        .ThenBy(x => x.StudentFeeType.NameEn)
                                                                            .ThenBy(x => x.CustomCourseGroup.Name)
                                                                 .Where(x => x.StartedBatch != null
                                                                             && x.StudentFeeTypeId != null)
                                                                 .Select(x => new
                                                                 {
                                                                     StartedBatch = x.StartedBatch,
                                                                     EndedBatch = x.EndedBatch,
                                                                     StudentFeeTypeId = x.StudentFeeTypeId,
                                                                     CustomCourseGroupId = x.CustomCourseGroupId ?? 0,
                                                                     StudentFeeTypeName = x.StudentFeeType.NameEn,
                                                                     CustomCourseGroupName = x.CustomCourseGroup.Name
                                                                 })
                                                                 .ToList();

            var headers = tuitionFeeRates.GroupBy(x => new
            {
                x.StartedBatch,
                x.EndedBatch,
                x.StudentFeeTypeId,
                x.CustomCourseGroupId
            })
                                         .Select((x, y) => new TuitionFeeCheckingBatch
                                         {
                                             Sequence = y,
                                             StartedBatch = x.Key.StartedBatch.Value,
                                             EndedBatch = x.Key.EndedBatch,
                                             StudentFeeTypeId = x.Key.StudentFeeTypeId.Value,
                                             CustomCourseGroupId = x.Key.CustomCourseGroupId,
                                             StudentFeeTypeNameEn = x.FirstOrDefault().StudentFeeTypeName,
                                             CustomCourseGroupName = x.FirstOrDefault().CustomCourseGroupName
                                         })
                                         .ToList();

            var courses = dbContext.Courses.AsNoTracking()
                                     .Where(x => x.AcademicLevelId == criteria.AcademicLevelId
                                                 && (criteria.FacultyId == 0 || x.FacultyId == criteria.FacultyId)
                                                 && x.TransferUniversityId == null
                                                 )
                                     .OrderBy(x => x.Code)
                                     .ToList();

            var tuitionfees = dbContext.TuitionFees.AsNoTracking().ToList();

            var results = new List<TuitionFeeChecking>();
            int total = courses.Count();
            int i = 1;

            foreach (var item in courses)
            {
                foreach (var header in headers)
                {
                    var fee = tuitionfees.FirstOrDefault(x => x.CourseId == item.Id
                                                              && (x.StartedBatch == null || header.StartedBatch >= x.StartedBatch)
                                                              && (x.EndedBatch == null || header.StartedBatch <= x.EndedBatch));
                    results.Add(new TuitionFeeChecking
                    {
                        CourseId = item.Id,
                        CourseCode = item.CodeAndSpecialChar,
                        CreatedAtUtc = item.CreatedAt,
                        CourseName = item.NameEn,
                        CourseCredit = item.CreditText,
                        Sequence = header.Sequence,
                        Value = _feeProvider.CalculateTuitionFeeRate(fee, item, header.StudentFeeTypeId, header.StartedBatch, dbContext)
                    });
                }
                Console.WriteLine("Run:" + i++ + "/" + total);
            }

            var model = new TuitionFeeCheckingViewModel
            {
                Criteria = criteria,
                Headers = headers,
                Results = results
            };
            return model;
        }

        public void CreateSelectList(long academicLevelId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            if (academicLevelId != 0)
            {
                ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
            }
        }
    }
}