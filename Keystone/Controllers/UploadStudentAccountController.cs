using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;

namespace Keystone.Controllers
{
    [PermissionAuthorize("UploadStudentAccount", PolicyGenerator.Write)]
    public class UploadStudentAccountController : BaseController
    {
        public UploadStudentAccountController(ApplicationDbContext db,
                                              IFlashMessage flashMessage) : base(db, flashMessage) { }

        public IActionResult Index()
        {            
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(IFormFile uploadFile)
        {
            var model = new List<UploadStudentAccountViewModel>();
            var extensions = new List<string>{ ".xlsx", ".xls" };
            if (string.IsNullOrEmpty(uploadFile.FileName)
                || !extensions.Contains(Path.GetExtension(uploadFile.FileName)))
            {
                _flashMessage.Danger("Invalid file.");
                return View(model);
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
                        var branches = _db.BankBranches;
                        for (int i = 0; i < result.Tables.Count; i++)
                        {
                            for (int j = 0; j < result.Tables[i].Rows.Count; j++)
                            {
                                var record = new UploadStudentAccountViewModel
                                             {
                                                 StudentCode = result.Tables[i].Rows[j][0]?.ToString(),
                                                 BankBranchCode = result.Tables[i].Rows[j][1]?.ToString(),
                                                 BankAccount = result.Tables[i].Rows[j][2]?.ToString()
                                             };

                                var student = students.SingleOrDefault(x => x.Code == record.StudentCode);
                                var bank = branches.FirstOrDefault(x => x.Code == record.BankBranchCode);
                                if (student != null && !string.IsNullOrEmpty(record.BankAccount))
                                {
                                    student.BankBranchId = bank?.Id;
                                    student.BankAccount = record.BankAccount;

                                    record.StudentFullName = student.FullNameEn;
                                    record.BankAbbreviation = bank?.Abbreviation;
                                    record.IsUploadSuccess = true;
                                }
                                else
                                {
                                    record.StudentFullName = student?.FullNameEn;
                                    record.BankAbbreviation = bank?.Abbreviation;
                                    record.IsUploadSuccess = false;
                                }

                                model.Add(record);
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

            model = model.OrderBy(x => x.StudentCode)
                         .ToList();
            return View(model);
        }
    }
}