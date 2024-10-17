using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Scholarship;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    [PermissionAuthorize("ImportScholarshipStudent", PolicyGenerator.Write)]
    public class ImportScholarshipStudentController : BaseController
    {
        protected readonly IFeeProvider _feeProvider;
        protected readonly IScholarshipProvider _scholarshipProvider;
        protected readonly IDateTimeProvider _dateTimeProvider;

        public ImportScholarshipStudentController(ApplicationDbContext db,
                                                  IFeeProvider feeProvider,
                                                  IScholarshipProvider scholarshipProvider,
                                                  ISelectListProvider selectListProvider,
                                                  IDateTimeProvider dateTimeProvider,
                                                  IFlashMessage flashMessage) : base(db, flashMessage, selectListProvider)
        {
            _feeProvider = feeProvider;
            _scholarshipProvider = scholarshipProvider;
            _dateTimeProvider = dateTimeProvider;
        }
        
        public IActionResult Index()
        {          
            CreateSelectList();  
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(IFormFile uploadFile, string Date, long ScholarshipTypeId = 0, long ScholarshipId = 0 ,long paymentMethodId = 0)
        {
            CreateSelectList();
            var model = new ImportScholarshipStudentViewModel
            {
                Success = new List<ImportScholarshipStudentSuccessDetail>(),
                Fail = new List<ImportScholarshipStudentFailDetail>()
            };
            
            if (ScholarshipTypeId == 0 || ScholarshipId == 0 || string.IsNullOrEmpty(Date))
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }
            
            var extensions = new List<string>{ ".xlsx", ".xls" };
            if (string.IsNullOrEmpty(uploadFile?.FileName)
                || !extensions.Contains(Path.GetExtension(uploadFile.FileName)))
            {
                _flashMessage.Danger("Invalid file.");
                return View();
            }
            model.FileName = uploadFile?.FileName;
            model.ReferenceDate = _dateTimeProvider.ConvertStringToDateTime(Date);
            model.Scholarship = _db.Scholarships.SingleOrDefault(x => x.Id == ScholarshipId);
            model.ScholarshipType = _db.ScholarshipTypes.SingleOrDefault(x => x.Id == ScholarshipTypeId);

            try
            {                

                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                using (var stream = uploadFile.OpenReadStream())
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                                                      {
                                                          ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                                                                                      {
                                                                                          UseHeaderRow = true
                                                                                      }
                                                      });
                        var students = _db.Students.Include(x => x.AcademicInformation);
                        var terms = _db.Terms.IgnoreQueryFilters();
                        
                        for (int i = 0; i < result.Tables.Count; i++)
                        {
                            for (int j = 0; j < result.Tables[i].Rows.Count; j++)
                            {
                                string studentCode = result.Tables[i].Rows[j][0]?.ToString();
                                int effectivedTerm = Convert.ToInt16(result.Tables[i].Rows[j][1]);
                                int effectivedYear = Convert.ToInt16(result.Tables[i].Rows[j][2]);
                                int expiredTerm = Convert.ToInt16(result.Tables[i].Rows[j][3]);
                                int expiredYear = Convert.ToInt16(result.Tables[i].Rows[j][4]);
                                decimal amount = Convert.ToDecimal(result.Tables[i].Rows[j][5]);
                                string remark = result.Tables[i].Rows[j][6]?.ToString();

                                var student = students.SingleOrDefault(x => x.Code == studentCode);

                                if (student == null)
                                {
                                    model.Fail.Add(new ImportScholarshipStudentFailDetail
                                                   {
                                                       StudentCode = studentCode,
                                                       StudentFullName = student?.FullNameEn,
                                                       EffectiveTerm = effectivedTerm,
                                                       EffectiveYear = effectivedYear,
                                                       ExpireTerm = expiredTerm,
                                                       ExpireYear = expiredYear,
                                                       Remark = remark,
                                                       Amount = amount,
                                                       Comment = "Invalid student code",
                                                   });
                                }
                                else
                                {
                                    var EffectivedTerm = terms.SingleOrDefault(x => x.AcademicTerm == effectivedTerm 
                                                                                    && x.AcademicYear == effectivedYear 
                                                                                    && x.AcademicLevelId == student.AcademicInformation.AcademicLevelId);
                                    var ExpiredTerm = terms.SingleOrDefault(x => x.AcademicTerm == expiredTerm 
                                                                                 && x.AcademicYear == expiredYear
                                                                                 && x.AcademicLevelId == student.AcademicInformation.AcademicLevelId);

                                    if( ExpiredTerm == null || EffectivedTerm == null )
                                    {
                                        model.Fail.Add(new ImportScholarshipStudentFailDetail
                                                       {
                                                           StudentCode = studentCode,
                                                           StudentFullName = student?.FullNameEn,
                                                           EffectiveTerm = effectivedTerm,
                                                           EffectiveYear = effectivedYear,
                                                           ExpireTerm = expiredTerm,
                                                           ExpireYear = expiredYear,
                                                           Remark = remark,
                                                           Amount = amount,
                                                           Comment = "Invalid term or year"
                                                       });
                                    }
                                    else
                                    {
                                        model.Success.Add(new ImportScholarshipStudentSuccessDetail
                                                          {
                                                              StudentCode = student.Code,
                                                              StudentFullName = student.FullNameEn,
                                                              EffectivedTermText = EffectivedTerm.TermText,
                                                              ExpiredTermText = ExpiredTerm.TermText,
                                                              Remark = remark,
                                                              Amount = amount,
                                                              EffectiveTermId = EffectivedTerm.Id,
                                                              ExpireTermId = ExpiredTerm.Id
                                                          });
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToEdit);
                return View(model);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Save(ImportScholarshipStudentViewModel model)
        {
            var result = new List<ScholarshipStudent>();
            
            var students = _db.Students;

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    foreach(var item in model.Success)
                    {
                        var studentId = students.SingleOrDefault(x => x.Code == item.StudentCode).Id;  
                        if (!_db.ScholarshipStudents.Any(x => x.StudentId == studentId
                                                              && x.ScholarshipId == model.Scholarship.Id))     
                        {
                            result.Add(new ScholarshipStudent
                                       {
                                           StudentId = studentId,
                                           ScholarshipId = model.Scholarship.Id,
                                           EffectivedTermId = item.EffectiveTermId,
                                           ExpiredTermId = item.ExpireTermId,
                                           Remark = item.Remark,
                                           LimitedAmount = item.Amount,
                                           ReferenceDate = model.ReferenceDate,
                                       });
                        }
                    }
 
                    _db.ScholarshipStudents.AddRange(result);
                    _db.SaveChanges();
                    
                    transaction.Commit();
                    _flashMessage.Confirmation(Message.SaveSucceed);

                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    transaction.Rollback();
                    _flashMessage.Danger(Message.UnableToCreate);

                    return View();
                }
            }
        }

        private void CreateSelectList()
        {
            ViewBag.ScholarshipTypes = _selectListProvider.GetScholarshipTypes();
            ViewBag.Scholarships = _selectListProvider.GetScholarships();
        }
    }
}