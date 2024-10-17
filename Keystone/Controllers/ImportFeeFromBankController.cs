using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;

namespace Keystone.Controllers
{
    public class ImportFeeFromBankController : BaseController
    {
        protected readonly IFeeProvider _feeProvider;

        public ImportFeeFromBankController(ApplicationDbContext db,
                                           IFeeProvider feeProvider,
                                           ISelectListProvider selectListProvider,
                                           IFlashMessage flashMessage) : base(db, flashMessage, selectListProvider)
        {
            _feeProvider = feeProvider;
        }
        
        public IActionResult Index()
        {          
            CreateSelectList();  
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(IFormFile uploadFile, long? paymentMethodId)
        {
            CreateSelectList();
            var model = new ImportFeeFromBankViewModel
            {
                Success = new List<ImportFeeFromBankSuccessDetail>(),
                Fail = new List<ImportFeeFromBankFailDetail>()
            };
            
            var extensions = new List<string>{ ".xlsx", ".xls" };
            if (string.IsNullOrEmpty(uploadFile?.FileName)
                || !extensions.Contains(Path.GetExtension(uploadFile.FileName)))
            {
                _flashMessage.Danger("Invalid file.");
                return View();
            }

            if ((paymentMethodId ?? 0) == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

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

                        var students = _db.Students;
                        var invoices = _db.Invoices;
                        var invoiceItems = _db.InvoiceItems;
                        var receipts = _db.Receipts;
                        for (int i = 0; i < result.Tables.Count; i++)
                        {
                            for (int j = 0; j < result.Tables[i].Rows.Count; j++)
                            {
                                string studentCode = result.Tables[i].Rows[j][0]?.ToString();
                                string invoiceNumber = result.Tables[i].Rows[j][1]?.ToString();
                                decimal totalAmount = Convert.ToDecimal(result.Tables[i].Rows[j][2]);
                                DateTime paidDate = Convert.ToDateTime(result.Tables[i].Rows[j][3]);

                                var student = students.SingleOrDefault(x => x.Code == studentCode);
                                var invoice = invoices.FirstOrDefault(x => x.StudentId == student.Id
                                                                           && !x.IsPaid
                                                                           && x.TotalAmount == totalAmount);
                                if (student == null || invoice == null)
                                {
                                    model.Fail.Add(new ImportFeeFromBankFailDetail
                                                   {
                                                       StudentCode = studentCode,
                                                       StudentFullName = student.FullNameEn,
                                                       Amount = totalAmount,
                                                       PaidDate = paidDate
                                                   });
                                }
                                else
                                {
                                    FinanceRegistrationFeeViewModel viewModel = new FinanceRegistrationFeeViewModel
                                                                                {
                                                                                    PaidDate = paidDate,
                                                                                    InvoiceId = invoice.Id
                                                                                };
                                    
                                    viewModel.Invoice = _feeProvider.GetRegistrationFeeInvoice(invoice.Id);
                                    viewModel.Invoice.InvoiceItems.Select(x => {
                                                                                   x.IsChecked = true;
                                                                                   return x;
                                                                               }).ToList();

                                    viewModel.PaymentMethods = new List<ReceiptPaymentMethod>
                                                               {
                                                                   new ReceiptPaymentMethod
                                                                   {
                                                                       PaymentMethodId = paymentMethodId ?? 0,
                                                                       Amount = invoice.TotalAmount
                                                                   }
                                                               };

                                    //TODO: Change ReceiptProvider.ProcessPaymentManualAsync
                                    //_feeProvider.SaveFinanceRegistrationFeeReceipt(viewModel, out string errMsg);

                                    var receipt = receipts.FirstOrDefault(x => x.InvoiceId == invoice.Id);
                                    model.Success.Add(new ImportFeeFromBankSuccessDetail
                                                      {
                                                          StudentCode = studentCode,
                                                          InvoiceNumber = invoiceNumber,
                                                          ReceiptNumber = receipt?.Number
                                                      });
                                }
                            }
                        }

                        _db.SaveChanges();
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

        private void CreateSelectList()
        {
            ViewBag.PaymentMethods = _selectListProvider.GetPaymentMethods();
        }
    }
}