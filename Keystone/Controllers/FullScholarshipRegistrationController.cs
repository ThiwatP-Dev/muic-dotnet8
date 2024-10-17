using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;
using KeystoneLibrary.Models.DataModels;
using Newtonsoft.Json;
using Keystone.Permission;

namespace Keystone.Controllers
{
    [PermissionAuthorize("FullScholarshipRegistration", "")]
    public class FullScholarshipRegistrationController : BaseController
    {
        protected readonly IScholarshipProvider _scholarshipProvider;
        protected readonly IStudentProvider _studentProvider;
        protected readonly IFeeProvider _feeProvider;

        public FullScholarshipRegistrationController(ApplicationDbContext db, 
                                                     IFlashMessage flashMessage, 
                                                     IScholarshipProvider scholarshipProvider,
                                                     IStudentProvider studentProvider,
                                                     IFeeProvider feeProvider,
                                                     ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider)
        {
            _scholarshipProvider = scholarshipProvider;
            _studentProvider = studentProvider;
            _feeProvider = feeProvider;
        }
        
        public IActionResult Index(Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId);
            if (criteria.AcademicLevelId == 0 || criteria.TermId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var scholarships = from item in _db.InvoiceItems
                               join scholarshipStudent in _db.ScholarshipStudents.Include(x => x.Scholarship) 
                               on item.ScholarshipStudentId equals scholarshipStudent.Id
                               where scholarshipStudent.Scholarship.IsFullAmount
                                     && (criteria.ScholarshipId == 0 
                                         || scholarshipStudent.ScholarshipId == criteria.ScholarshipId)
                               group scholarshipStudent.Scholarship
                               by new 
                                  { 
                                      item.InvoiceId, 
                                      scholarshipStudent.Id
                                  } 
                               into g
                               select new 
                                      { 
                                          g.Key.InvoiceId,
                                          ScholarshipStudentId = g.Key.Id,
                                          ScholarShipName = g.FirstOrDefault().NameEn
                                      };

            var results = (from invoice in _db.Invoices
                           join scholarship in scholarships on invoice.Id equals scholarship.InvoiceId
                           where invoice.TermId == criteria.TermId
                                 && (string.IsNullOrEmpty(criteria.Code)
                                     || invoice.Student.Code.StartsWith(criteria.Code))
                                 && (string.IsNullOrEmpty(criteria.FirstName)
                                     || invoice.Student.FirstNameEn.StartsWith(criteria.FirstName))
                           group new {
                                          invoice.Id,
                                          StudentCode = invoice.Student.Code,
                                          StudentFullName = invoice.Student.FullNameEn,
                                          scholarship.ScholarShipName,
                                          invoice.TotalAmount
                                     }
                           by new {
                                       StudentId = invoice.StudentId ?? Guid.Empty,
                                       scholarship.ScholarshipStudentId
                                  }
                           into g
                           select new FullScholarshipRegistrationResultViewModel
                                  {
                                      StudentId = g.Key.StudentId,
                                      StudentCode = g.FirstOrDefault().StudentCode,
                                      StudentFullName = g.FirstOrDefault().StudentFullName,
                                      ScholarshipStudentId = g.Key.ScholarshipStudentId,
                                      ScholarshipName = g.FirstOrDefault().ScholarShipName,
                                      Balance = 0,
                                      TotalAmount = g.Sum(x => x.TotalAmount),
                                      InvoiceIds = JsonConvert.SerializeObject(g.Select(x => x.Id)) ?? "[]"
                                  }).ToList();

            results.ForEach(x => x.Balance = _scholarshipProvider.GetScholarshipBalance(x.ScholarshipStudentId ?? 0));

            var model = new FullScholarshipRegistrationViewModel
                        {
                            Criteria = criteria,
                            Results = results
                        };

            return View(model);
        }

        public ActionResult Details(string invoiceIds, Guid studentId, long scholarshipStudentId, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var _invoiceIds = JsonConvert.DeserializeObject<List<long>>(invoiceIds);
            var student = _studentProvider.GetStudentById(studentId);
            var scholarshipStudent = _scholarshipProvider.GetScholarshipStudentById(scholarshipStudentId);
            var invoices = Find(_invoiceIds);
            var model = new FullScholarshipRegistrationResultViewModel
                        {
                            StudentCode = student?.Code,
                            StudentFullName = student?.FullNameEn,
                            ScholarshipName = scholarshipStudent?.Scholarship?.NameEn,
                            Balance = _scholarshipProvider.GetScholarshipBalance(scholarshipStudentId),
                            InvoiceIds = invoiceIds,
                            Invoices = invoices,
                            TotalAmount = invoices.Sum(x => x.TotalAmount)
                        };

            return View(model);
        }

        [PermissionAuthorize("FullScholarshipRegistration", PolicyGenerator.Write)]
        public ActionResult Paid(string invoiceIds, string returnUrl)
        {
            foreach (var id in JsonConvert.DeserializeObject<List<long>>(invoiceIds))
            {
                var invoice = _db.Invoices.SingleOrDefault(x => x.Id == id
                                                                && !x.IsPaid);
                if (invoice != null)
                {
                    FinanceRegistrationFeeViewModel viewModel = new FinanceRegistrationFeeViewModel
                                                                {
                                                                    PaidDate = DateTime.UtcNow,
                                                                    InvoiceId = invoice.Id
                                                                };
                                    
                    viewModel.Invoice = _feeProvider.GetRegistrationFeeInvoice(invoice.Id);
                    viewModel.Invoice.InvoiceItems.ForEach(x => x.IsChecked = true);

                    viewModel.PaymentMethods = new List<ReceiptPaymentMethod>();

                    //TODO: Change ReceiptProvider.ProcessPaymentManualAsync
                    //_feeProvider.SaveFinanceRegistrationFeeReceipt(viewModel, out string errMsg);
                }
            }

            _flashMessage.Confirmation(Message.SaveSucceed);
            return Redirect(returnUrl);
        }

        private List<Invoice> Find(List<long> invoiceIds)
        {
            return _db.Invoices.Include(x => x.InvoiceItems)
                                    .ThenInclude(x => x.FeeItem)
                               .Include(x => x.InvoiceItems)
                                    .ThenInclude(x => x.Section)
                               .Include(x => x.InvoiceItems)
                                    .ThenInclude(x => x.Course)
                               .Where(x => invoiceIds.Contains(x.Id))
                               .ToList();
        }

        private void CreateSelectList(long academicLevelId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Scholarships = _selectListProvider.GetFullAmountScholarship();
            if (academicLevelId != 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            }
        }
    }
}