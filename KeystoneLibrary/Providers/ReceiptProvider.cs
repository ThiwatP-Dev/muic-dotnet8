using AutoMapper;
using KeystoneLibrary.Data;
using KeystoneLibrary.Enumeration;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.Fee;
using KeystoneLibrary.Models.DataModels.Scholarship;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using KeystoneLibrary.Models.Report;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Security.Cryptography;
using System.Net;
using KeystoneLibrary.Models.DataModels.Configurations;
using KeystoneLibrary.Models.USpark;
using Microsoft.Extensions.Options;
using KeystoneLibrary.Config;
using KeystoneLibrary.Utility;
using KeystoneLibrary.Extensions;
using KeystoneLibrary.Models.DataModels.Profile;
using Nut;

namespace KeystoneLibrary.Providers
{
    public class ReceiptProvider : BaseProvider, IReceiptProvider
    {
        private IMasterProvider _masterProvider;
        protected IAcademicProvider _academicProvider { get; }
        protected IRegistrationProvider _registrationProvider { get; }
        protected IFeeProvider _feeProvider { get; }
        protected IStudentProvider _studentProvider { get; }
        protected IScholarshipProvider _scholarshipProvider { get; }
        protected readonly KeystoneLibrary.Interfaces.IConfigurationProvider _configurationProvider;
        private readonly PaymentConfiguration _paymentConfig;
        private readonly IHttpClientProxy _httpClientProxy;
        private readonly IUserProvider _userProvider;

        public ReceiptProvider(ApplicationDbContext db,
                               IMapper mapper,
                               IMasterProvider masterProvider,
                               IAcademicProvider academicProvider,
                               IRegistrationProvider registrationProvider,
                               IFeeProvider feeProvider,
                               IScholarshipProvider scholarshipProvider,
                               IConfiguration config,
                               IStudentProvider studentProvider,
                               KeystoneLibrary.Interfaces.IConfigurationProvider configurationProvider,
                               IOptions<PaymentConfiguration> paymentConfigOptions,
                               IUserProvider userProvider,
                               IHttpClientProxy httpClientProxy) : base(config, db, mapper)
        {
            _academicProvider = academicProvider;
            _masterProvider = masterProvider;
            _registrationProvider = registrationProvider;
            _feeProvider = feeProvider;
            _studentProvider = studentProvider;
            _scholarshipProvider = scholarshipProvider;
            _configurationProvider = configurationProvider;
            _paymentConfig = paymentConfigOptions.Value;
            _httpClientProxy = httpClientProxy;
            _userProvider = userProvider;
        }

        public Receipt GetReceiptById(long id)
        {
            var receipt = _db.Receipts.AsNoTracking()
                                      .Include(x => x.Student)
                                          .ThenInclude(x => x.AcademicInformation)
                                          .ThenInclude(x => x.Faculty)
                                      .Include(x => x.Student)
                                          .ThenInclude(x => x.AcademicInformation)
                                          .ThenInclude(x => x.Department)
                                      .Include(x => x.Student)
                                          .ThenInclude(x => x.Title)
                                      .Include(x => x.Student)
                                          .ThenInclude(x => x.Nationality)
                                      .Include(x => x.Invoice)
                                      .Include(x => x.ReceiptItems)
                                          .ThenInclude(x => x.FeeItem)
                                      .Include(x => x.ReceiptItems)
                                          .ThenInclude(x => x.Invoice)
                                      .Include(x => x.ReceiptItems)
                                          .ThenInclude(x => x.InvoiceItem)
                                          .ThenInclude(x => x.Course)
                                      .Include(x => x.ReceiptItems)
                                          .ThenInclude(x => x.InvoiceItem)
                                          .ThenInclude(x => x.Section)
                                      .Include(x => x.Term)
                                      .SingleOrDefault(x => x.Id == id);

            if (receipt.ReceiptItems != null)
            {
                receipt.ReceiptItems = receipt.ReceiptItems.OrderBy(x => x.FeeItemName)
                                                           .ToList();
            }

            if (receipt != null)
            {
                var user = _db.Users.SingleOrDefault(y => y.Id == receipt.PrintedBy);
                var name = user?.FullnameEN?.Trim();
                if (string.IsNullOrEmpty(name))
                {
                    name = user?.UserName;
                }

                receipt.PrintedByFullName = name;


                // To retrieve bank payment info. Same logic from _receiptProvider.GetRegistrationResultWithAmountAndCreditReport tweak
                // Hack to include this into the display info
                if (receipt.InvoiceId != null && receipt.InvoiceId > 0)
                {
                    var invoiceId = receipt.InvoiceId ?? 0;
                    var paymentResponse = _db.BankPaymentResponses.AsNoTracking()
                                                                  .Where(x => ((x.InvoiceId ?? 0) == invoiceId || x.Number == receipt.Invoice.Number)
                                                                      && x.IsPaymentSuccess)
                                                                  .ToList();

                    if (paymentResponse.Any(x => x.RawResponse.Contains("Domestic Transfers")))
                    {
                        receipt.InvoicePaymentMethod = "QR";
                    }
                    //"paymentMethod":"WECHATPAY"
                    else if (paymentResponse.Any(x => x.RawResponse.Contains("\"paymentMethod\":\"WECHATPAY\"")))
                    {
                        receipt.InvoicePaymentMethod = "WeChat";
                    }
                    else if (paymentResponse.Any(x => x.RawResponse.Contains("\"channelCode\":\"PMH\"")))
                    {
                        receipt.InvoicePaymentMethod = "PMH";
                    }
                    else if (paymentResponse.Any(x => x.RawResponse.Contains("\"channelCode\"")))
                    {
                        receipt.InvoicePaymentMethod = "CreditCard";
                        var paymentResponseFirst = paymentResponse.FirstOrDefault(x => x.TotalAmount == receipt.TotalAmount);
                        if (paymentResponseFirst != null)
                        {
                            receipt.ExtraFee = paymentResponseFirst.PaidAmount - (paymentResponseFirst.TotalAmount ?? receipt.TotalAmount);
                            receipt.ReceiptItems.Add(new ReceiptItem
                            {
                                TotalAmount = receipt.ExtraFee,
                                Amount = receipt.ExtraFee,
                                FeeItemId = 999999,
                                FeeItem = new FeeItem
                                {
                                    Id = 999999,
                                    NameEn = "Credit card transaction fee",
                                    NameTh = "ค่าธรรมเนียมธุรกรรมบัตรเครดิต"
                                },
                                InvoiceItem = new InvoiceItem {
                                    TotalAmount = receipt.ExtraFee,
                                    Amount = receipt.ExtraFee,
                                    FeeItem = new FeeItem
                                    {
                                        Id = 999999,
                                        NameEn = "Credit card transaction fee",
                                        NameTh = "ค่าธรรมเนียมธุรกรรมบัตรเครดิต"
                                    }
                                }
                            }) ;
                            receipt.TotalAmount += receipt.ExtraFee;
                            receipt.Amount += receipt.ExtraFee;
                        }

                    }
                    else if (paymentResponse.Any(x => x.RawResponse.Contains("Manual Update")
                                                            || x.RawResponse.Contains("MANUAL TOTAL AMOUNT LESS THAN ZERO"))
                            )
                    {
                        receipt.InvoicePaymentMethod = "Manual Update";
                    }
                    else
                    {
                        receipt.InvoicePaymentMethod = "Unidentified";
                    }
                }
            }

            return receipt;
        }

        public List<Receipt> GetReceiptByTerm(Guid studentId, long termId)
        {
            var receipts = _db.Receipts.Include(x => x.Term)
                                       .Where(x => x.StudentId == studentId
                                                   && x.TermId == termId
                                                   //&& !x.IsCancel
                                                   )
                                       .ToList();
            var createByIds = receipts.Where(x => !string.IsNullOrEmpty(x.CreatedBy)).Select(x => x.CreatedBy).Distinct().ToList();
            var createBys = _userProvider.GetCreatedFullNameByIds(createByIds);
            foreach (var item in receipts)
            {
                item.CreatedByFullNameEn = createBys.SingleOrDefault(x => x.CreatedBy == item.CreatedBy)?.CreatedByFullNameEn ?? "-";
            }
            return receipts;
        }

        public List<Invoice> GetInvoiceByTerm(Guid studentId, long termId)
        {
            var invoices = _db.Invoices.Include(x => x.Term)
                                       .Where(x => x.StudentId == studentId
                                                   && x.TermId == termId
                                                   //&& !x.IsCancel
                                                   )
                                       .ToList();

            var createByIds = invoices.Where(x => !string.IsNullOrEmpty(x.CreatedBy)).Select(x => x.CreatedBy).Distinct().ToList();
            var createBys = _userProvider.GetCreatedFullNameByIds(createByIds);
            foreach (var item in invoices)
            {
                item.CreatedByFullNameEn = createBys.SingleOrDefault(x => x.CreatedBy == item.CreatedBy)?.CreatedByFullNameEn ?? "-";
            }
            return invoices;
        }

        public Invoice GetInvoice(long id)
        {
            var invoice = _db.Invoices.Include(x => x.Student)
                                          .ThenInclude(x => x.AcademicInformation)
                                          .ThenInclude(x => x.Faculty)
                                      .Include(x => x.Student)
                                          .ThenInclude(x => x.AcademicInformation)
                                          .ThenInclude(x => x.Department)
                                      .Include(x => x.Student)
                                          .ThenInclude(x => x.StudentFeeType)
                                      .Include(x => x.Student)
                                          .ThenInclude(x => x.StudentFeeGroup)
                                      .Include(x => x.Student)
                                          .ThenInclude(x => x.AdmissionInformation)
                                          .ThenInclude(x => x.AdmissionType)
                                      .Include(x => x.Student)
                                          .ThenInclude(x => x.Title)
                                      .Include(x => x.Term)
                                      .Include(x => x.InvoiceItems)
                                          .ThenInclude(x => x.FeeItem)
                                      .Include(x => x.InvoiceItems)
                                          .ThenInclude(x => x.Course)
                                      .Include(x => x.InvoiceItems)
                                          .ThenInclude(x => x.Section)
                                      .SingleOrDefault(x => x.Id == id);

            if (invoice.InvoiceItems != null)
            {
                invoice.InvoiceItems = invoice.InvoiceItems.OrderBy(x => x.FeeItemName)
                                                           .ToList();
            }

            if (invoice.Type == "cr")
            {
                var invoiceCreditNoteIds = _db.InvoiceDeductTransactions.Where(x => x.InvoiceCreditNoteId == id
                                                                                    && x.InvoiceId != id
                                                                                    && x.Type == "u")
                                                                        .Select(x => x.InvoiceId)
                                                                        .Distinct()
                                                                        .ToList();
                if (invoiceCreditNoteIds.Any())
                {
                    var invoiceItems = _db.InvoiceItems.Include(x => x.Course)
                                                       .Include(x => x.Section)
                                                       .Include(x => x.FeeItem)
                                                       .Where(x => invoiceCreditNoteIds.Contains(x.InvoiceId))
                                                       .ToList();
                    invoiceItems.Select(x =>
                    {
                        if (x.DiscountAmount > 0)
                        {
                            x.TotalAmount = x.DiscountAmount;
                            x.DiscountAmount = 0;
                        }
                        return x;
                    }).ToList();

                    invoice.InvoiceItems.AddRange(invoiceItems);
                }
            }
            // else
            // {
            //     var invoiceCreditNoteIds = _db.InvoiceDeductTransactions.Where(x => x.InvoiceId == id && x.Type == "u")
            //                                                             .GroupBy(x => x.InvoiceCreditNoteId)
            //                                                             .Select(x => x.First().InvoiceCreditNoteId)
            //                                                             .ToList();
            //     if(invoiceCreditNoteIds.Any())
            //     {
            //         var invoiceItems = _db.InvoiceItems.Include(x => x.Course)
            //                                            .Include(x => x.Section)
            //                                            .Include(x => x.FeeItem)
            //                                            .Where(x => invoiceCreditNoteIds.Contains(x.InvoiceId) 
            //                                                        && x.Type == "cr"
            //                                                        && x.Invoice.Type == "cr")
            //                                            .ToList();

            //         invoice.InvoiceItems.AddRange(invoiceItems);
            //     }
            // }

            if (invoice != null)
            {
                invoice.IsAddDrop = _db.Invoices.AsNoTracking()
                                           .IgnoreQueryFilters()
                                           .Where(x => x.StudentId == invoice.StudentId
                                                           && x.TermId == invoice.TermId
                                                           && !x.IsCancel
                                                           && x.IsActive
                                                           && x.Type != "o")
                                           .OrderBy(x => x.CreatedAt)
                                           .Select(x => x.Id)
                                           .FirstOrDefault() < invoice.Id;

                var isRegisInTime = _db.RegistrationCourses.AsNoTracking()
                                                              .IgnoreQueryFilters()
                                                              .Any(x => x.IsActive
                                                                  && x.StudentId == invoice.StudentId
                                                                  && x.TermId == invoice.TermId
                                                                  && x.CreatedAt <= invoice.Term.FirstRegistrationEndedAt);

                var isRegisLogInTIme = _db.RegistrationLogs.AsNoTracking()
                                                           .IgnoreQueryFilters()
                                                           .Any(x => x.StudentId == invoice.StudentId
                                                                         && x.TermId == invoice.TermId
                                                                         && x.CreatedAt <= invoice.Term.FirstRegistrationEndedAt);

                invoice.IsLateRegis = !( isRegisInTime || isRegisLogInTIme);
            }

            return invoice;
        }

        public Invoice GetLatestInvoice(Guid studentId, long termId)
        {
            var invoice = _db.Invoices.Where(x => x.StudentId == studentId
                                                  && x.TermId == termId
                                                  && x.Type != "o"
                                                  && !x.IsCancel
                                                  && !x.IsPaid).OrderByDescending(x => x.Id).FirstOrDefault();
            return invoice;
        }

        // Registration
        public List<InvoiceItem> GetInvoiceItemsForPreview(List<RegistrationCourse> registeringCourses, int registrationRound, bool isCreditNote = false)
        {
            if (!registeringCourses.Any())
            {
                return new List<InvoiceItem>();
            }

            // Get studentId, and termId by using Tuple<T1, T2>
            (var studentId, var termId) = registeringCourses.Select(x => (x.StudentId, x.TermId))
                                                            .First();
            var student = _studentProvider.GetStudentInformationById(studentId);
            var scholarshipStudent = _scholarshipProvider.GetCurrentScholarshipByTerm(studentId, termId);
            bool isFullAmount = scholarshipStudent?.Scholarship?.IsFullAmount ?? false;
            bool isLumpsumPayment = student.StudentFeeGroup?.IsLumpsumPayment ?? false;
            var scholarshipFeeItems = _feeProvider.GetScholarshipFeeItems(studentId, termId);

            // if already registration and paid, do not plus term fee
            var alreadyRegistration = _db.RegistrationCourses.Any(x => x.StudentId == studentId
                                                                       && x.TermId == termId
                                                                       && x.IsPaid);

            var termFees = new List<TermFee>();
            var termFeeWithDiscount = new List<InvoiceItem>();
            var invoiceItems = new List<InvoiceItem>();
            if (!alreadyRegistration)
            {
                // Get TermFee then compare to ScholarshipFeeItem for the discount (TermFee left join ScholarshipFeeItem)
                termFees = _feeProvider.GetStudentTermFees(studentId, termId);
                if (isFullAmount)
                {
                    termFeeWithDiscount = termFees.Select(x => new InvoiceItem
                    {
                        ScholarshipStudentId = scholarshipStudent?.Id,
                        FeeItemId = x.FeeItemId,
                        FeeItemName = x.FeeItem.NameEn,
                        Amount = x.Amount,
                        ScholarshipAmount = x.Amount,
                        TotalAmount = 0,
                    })
                                                  .ToList();
                }
                else
                {
                    termFeeWithDiscount = GenerateInvoiceItemTermFee(termFees, scholarshipFeeItems, scholarshipStudent?.Id);
                }
            }

            // Get TuitionFee then compare to ScholarshipFeeItem for the discount (TuitionFee left join ScholarshipFeeItem)
            var unPaidCourse = registeringCourses.Where(x => !x.IsPaid || isCreditNote).ToList();
            var tuitionFees = _feeProvider.GetTuitionFees(unPaidCourse,
                                                          student.AcademicInformation.FacultyId,
                                                          student.AcademicInformation.DepartmentId,
                                                          student.AcademicInformation.CurriculumVersion.CurriculumId,
                                                          student.AcademicInformation.CurriculumVersionId,
                                                          student.AcademicInformation.Batch,
                                                          student.StudentFeeTypeId,
                                                          termId);

            var tuitionFeesWithDiscount = new List<InvoiceItem>();
            if (!isLumpsumPayment)
            {
                if (isFullAmount)
                {
                    tuitionFeesWithDiscount = tuitionFees.Select(x => new InvoiceItem
                    {
                        ScholarshipStudentId = scholarshipStudent?.Id,
                        CourseId = x.CourseId,
                        CourseCode = x.CourseCode,
                        CourseName = x.CourseName,
                        CourseAndCredit = x.CourseAndCredit,
                        SectionId = x.SectionId,
                        FeeItemId = x.FeeItemId,
                        FeeItemName = x.FeeItemName,
                        Amount = x.Amount,
                        ScholarshipAmount = x.Amount,
                        TotalAmount = 0,
                    })
                                                        .ToList();
                }
                else
                {
                    tuitionFeesWithDiscount = GenerateInvoiceItemTuitionFee(tuitionFees, scholarshipFeeItems, scholarshipStudent?.Id);
                }
            }

            if (isLumpsumPayment)
            {
                var studentState = _db.StudentStates.SingleOrDefault(x => x.StudentId == student.Id && x.TermId == termId);
                string round = studentState != null && (studentState.State != "REG") && (studentState.State != "PAY_REG") ? "a" : "r";
                if (round == "a")
                {
                    var registeredCourses = _db.RegistrationCourses.Where(x => x.TermId == termId
                                                                    && x.StudentId == studentId
                                                                    && x.Status != "d")
                                                            .ToList();
                    if (registeredCourses.Any())
                    {
                        var tuitionFee = _db.FeeItems.SingleOrDefault(x => x.Code == "001");
                        foreach (var item in registeredCourses)
                        {
                            if (!registeringCourses.Any(x => x.CourseId == item.CourseId
                                                                && (x.SectionId ?? 0) == (item.SectionId ?? 0)
                                                                && x.TermId == item.TermId))
                            {
                                // DROPPING
                                invoiceItems.Add(new InvoiceItem()
                                {
                                    FeeItem = tuitionFee,
                                    FeeItemId = tuitionFee.Id,
                                    FeeItemName = tuitionFee.NameEn,
                                    CourseId = item.CourseId,
                                    CourseCode = item.CourseCode,
                                    SectionId = item.SectionId,
                                    Amount = 0,
                                    ScholarshipAmount = 0,
                                    TotalAmount = 0,
                                    LumpSumAddDrop = "drop"
                                });
                            }
                        }

                        foreach (var item in registeringCourses)
                        {
                            if (!registeredCourses.Any(x => x.CourseId == item.CourseId
                                                        && (x.SectionId ?? 0) == (item.SectionId ?? 0)
                                                        && x.TermId == item.TermId))
                            {
                                // ADDING
                                invoiceItems.Add(new InvoiceItem()
                                {
                                    FeeItem = tuitionFee,
                                    FeeItemId = tuitionFee.Id,
                                    FeeItemName = tuitionFee.NameEn,
                                    CourseId = item.CourseId,
                                    CourseCode = item.CourseCode,
                                    SectionId = item.SectionId,
                                    Amount = 0,
                                    ScholarshipAmount = 0,
                                    TotalAmount = 0,
                                    LumpSumAddDrop = "add"
                                });
                            }
                        }
                    }
                }
            }

            invoiceItems.AddRange(termFeeWithDiscount);
            invoiceItems.AddRange(tuitionFeesWithDiscount);

            return invoiceItems;
        }

        // Match Course Controller
        public List<InvoiceItem> GetInvoiceItemsForPreview(List<RegistrationCourse> courses, List<MatchCourseTermFee> feeItems)
        {
            List<InvoiceItem> items = new List<InvoiceItem>();
            (var studentId, var termId) = courses.Select(x => (x.StudentId, x.TermId))
                                                 .First();
            var scholarshipStudent = _scholarshipProvider.GetCurrentScholarshipByTerm(studentId, termId);
            var scholarshipFeeItems = _feeProvider.GetScholarshipFeeItems(studentId, termId);

            List<TermFee> termFees = feeItems.Select(x => new TermFee
            {
                FeeItemId = x.FeeItemId,
                FeeItem = _db.FeeItems.SingleOrDefault(y => y.Id == x.FeeItemId),
                Amount = x.Amount
            })
                                             .ToList();

            var termFeeWithDiscount = GenerateInvoiceItemTermFee(termFees, scholarshipFeeItems, scholarshipStudent?.Id);

            var student = _studentProvider.GetStudentById(studentId);

            var unPaidCourse = courses.Where(x => !x.IsPaid).ToList();
            var tuitionFees = _feeProvider.GetTuitionFees(unPaidCourse,
                                                          student.AcademicInformation.FacultyId,
                                                          student.AcademicInformation.DepartmentId,
                                                          student.AcademicInformation.CurriculumVersion.CurriculumId,
                                                          student.AcademicInformation.CurriculumVersionId,
                                                          student.AcademicInformation.Batch,
                                                          student.StudentFeeTypeId,
                                                          termId);

            var tuitionFeesWithDiscount = GenerateInvoiceItemTuitionFee(tuitionFees, scholarshipFeeItems, scholarshipStudent?.Id);

            items.AddRange(termFeeWithDiscount.Union(tuitionFeesWithDiscount));

            return items;
        }

        private List<InvoiceItem> GenerateInvoiceItemTermFee(List<TermFee> termFees, List<ScholarshipFeeItem> scholarshipFeeItems, long? scholarshipStudentId)
        {
            var results = (from termFee in termFees
                               //    join scholarship in scholarshipFeeItems on termFee.FeeItemId equals scholarship.FeeItemId into scholarships
                               //    from scholarship in scholarships.DefaultIfEmpty()
                               //    let scholarshipAmount = scholarship != null ? scholarship?.Percentage != 0 ? termFee.Amount * (scholarship?.Percentage ?? 0) / 100 : termFee.Amount - (scholarship?.Amount ?? 0) : 0
                           select new InvoiceItem
                           {
                               //    ScholarshipStudentId = scholarshipAmount > 0 ? scholarshipStudentId : (long?)null,
                               FeeItemId = termFee.FeeItemId,
                               FeeItemName = termFee.FeeItem.NameEn,
                               Amount = termFee.Amount,
                               TotalAmount = termFee.Amount
                               //    ScholarshipAmount = scholarshipAmount,
                               //    TotalAmount = termFee.Amount - scholarshipAmount,
                           }).ToList();
            return results;
        }

        private List<InvoiceItem> GenerateInvoiceItemTuitionFee(List<InvoiceItem> tuitionFees, List<ScholarshipFeeItem> scholarshipFeeItems, long? scholarshipStudentId)
        {
            var results = (from tuitionFee in tuitionFees
                               //    join scholarship in scholarshipFeeItems on tuitionFee.FeeItemId equals scholarship.FeeItemId into scholarships
                               //    from scholarship in scholarships.DefaultIfEmpty()
                               //    let scholarshipAmount = scholarship != null ? scholarship.Percentage != 0 ? tuitionFee.Amount * (scholarship?.Percentage ?? 0) / 100 : tuitionFee.Amount - (scholarship?.Amount ?? 0) : 0
                           select new InvoiceItem
                           {
                               //    ScholarshipStudentId = scholarshipAmount > 0 ? scholarshipStudentId : (long?)null,
                               CourseId = tuitionFee.CourseId,
                               CourseCode = tuitionFee.CourseCode,
                               CourseName = tuitionFee.CourseName,
                               CourseAndCredit = tuitionFee.CourseAndCredit,
                               SectionId = tuitionFee.SectionId,
                               FeeItemId = tuitionFee.FeeItemId,
                               FeeItemName = tuitionFee.FeeItemName,
                               Amount = tuitionFee.Amount,
                               TotalAmount = tuitionFee.Amount
                               //    ScholarshipAmount = scholarshipAmount,
                               //    TotalAmount = tuitionFee.Amount - scholarshipAmount,
                           }).ToList();

            return results;
        }

        public long RegenerateInvoices(Guid studentId, long termId, List<RegistrationCourse> dropCourses, List<RegistrationCourse> addCourses, string round)
        {
            // cancel add drop
            CancelAddDropInvoices(studentId, termId);

            if (round == "r")
            {
                var invoice = AddInvoice(studentId, termId, addCourses, round);
                return invoice.Id;
            }
            else
            {
                var addDropRound = (_db.Invoices.Where(x => x.StudentId == studentId
                                                            && x.TermId == termId
                                                            && x.IsPaid
                                                            && (x.Type == "a" || x.Type == "au" || x.Type == "cr"))
                                                .Max(x => (int?)x.AddDropSequence) ?? 0) + 1;
                Invoice dropInvoice = null;
                if (dropCourses != null && dropCourses.Any())
                {
                    dropInvoice = DropCourses(studentId, termId, dropCourses, addDropRound);
                }

                Invoice addInvoice = null;
                if (addCourses != null && addCourses.Any())
                {
                    addInvoice = AddCourses(studentId, termId, addCourses, addDropRound);
                    return addInvoice.Id;
                }
                else
                {
                    var now = DateTime.Now;
                    var addDropFeeCount = GetConfigAddDropFeeCount(termId);
                    var paymentPeriod = (from slot in _db.Slots
                                         join registrationTerm in _db.RegistrationTerms on slot.RegistrationTermId equals registrationTerm.Id
                                         where registrationTerm.TermId == termId
                                         && registrationTerm.StartedAt <= now
                                         && registrationTerm.EndedAt >= now
                                         && slot.Type == "p"
                                         select slot).FirstOrDefault();
                    if ((addDropFeeCount != null && addDropRound > addDropFeeCount.FreeAddDropCount)
                        || (paymentPeriod != null && now > paymentPeriod.EndedAt))
                    {
                        addInvoice = AddInvoice(studentId, termId, new List<RegistrationCourse>(), "a", addDropRound);
                        return addInvoice.Id;
                    }
                }

                // If there is refund, need to deduct refund percent
                if (dropInvoice != null && dropInvoice.InvoiceItems.Sum(x => x.Amount) > addInvoice.InvoiceItems.Sum(x => x.Amount))
                {
                    // Example
                    // Add 1 course 5000 THB
                    // Drop 2 courses -6000 THB, -7000 THB
                    // Refund total 5000-6000-7000 = -8000 THB
                    // Deduct 800 THB
                    // Each drop item = (6000 - (5000 / 2)) * 10% = 350 THB
                    // and (7000 - (5000 / 2)) * 10% = 450 THB
                    UpdateTuitionFeeRefund(dropInvoice, addInvoice);
                }
            }
            return 0;
        }

        public Invoice AddInvoiceCreditNote(Guid studentId, long termId, List<RegistrationCourse> registrationCourses, int addDropRound = 0)
        {

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var now = DateTime.Today;
                    var invoice = new Invoice();
                    var student = _studentProvider.GetStudentInformationById(studentId);
                    var registrationCourseIds = registrationCourses.Select(x => x.Id);
                    var invoiceItems = _db.InvoiceItems.Include(x => x.Invoice)
                                                    .Where(x => !x.Invoice.IsCancel
                                                                && x.IsPaid
                                                                && x.Invoice.Type != "cr"
                                                                && registrationCourseIds.Contains(x.RegistrationCourseId.Value)).ToList();
                    bool isLumpsumPayment = student.StudentFeeGroup?.IsLumpsumPayment ?? false;
                    var studentState = _db.StudentStates.SingleOrDefault(x => x.StudentId == student.Id && x.TermId == termId);
                    string studentStateRound = studentState != null && (studentState.State != "REG") && (studentState.State != "PAY_REG") ? "a" : "r";

                    if (isLumpsumPayment && studentStateRound == "a")
                    {
                        invoiceItems = new List<InvoiceItem>();
                        var tuitionFee = _db.FeeItems.SingleOrDefault(x => x.Code == "001");
                        foreach (var item in registrationCourses)
                        {
                            invoiceItems.Add(new InvoiceItem()
                            {
                                FeeItem = tuitionFee,
                                FeeItemId = tuitionFee.Id,
                                FeeItemName = tuitionFee.NameEn,
                                CourseId = item.CourseId,
                                CourseCode = item.CourseCode,
                                SectionId = item.SectionId,
                                Amount = 0,
                                ScholarshipAmount = 0,
                                TotalAmount = 0,
                                LumpSumAddDrop = "drop",
                                Type = "cr"
                            });
                        }
                    }

                    if (student != null && invoiceItems.Any())
                    {
                        var today = DateTime.UtcNow;
                        invoice.RunningNumber = _feeProvider.GetNextInvoiceRunningNumber();
                        invoice.Number = _feeProvider.GetFeeInvoiceNumber(invoice.RunningNumber);
                        invoice.Year = DateTime.Today.Year;
                        invoice.TermId = termId;
                        invoice.StudentId = studentId;
                        invoice.Name = student.FullNameEn;
                        invoice.Type = "cr";
                        invoice.AddDropSequence = addDropRound;

                        var address = student.StudentAddresses.FirstOrDefault(x => x.Type == "Current");
                        if (address != null)
                        {
                            invoice.Address = address.AddressEn1;
                            invoice.Address2 = address.AddressEn2;
                            invoice.IsCancel = false;
                        }

                        _db.Invoices.Add(invoice);
                        _db.SaveChanges();

                        if (isLumpsumPayment)
                        {
                            foreach (var item in invoiceItems)
                            {
                                item.InvoiceId = invoice.Id;
                            }
                            _db.InvoiceItems.AddRange(invoiceItems);
                        }
                        else
                        {
                            foreach (var item in invoiceItems)
                            {
                                var registrationCourse = registrationCourses.SingleOrDefault(x => x.CourseId == item.CourseId);
                                if (item.TotalAmount == 0)
                                {
                                    var creditNote = _db.InvoiceDeductTransactions.Where(x => x.InvoiceItemId == item.Id)
                                                                                    .OrderByDescending(x => x.Id)
                                                                                    .FirstOrDefault();
                                    if (creditNote != null)
                                    {
                                        item.Amount = creditNote.Amount;
                                        item.TotalAmount = creditNote.Amount;
                                    }
                                }
                                var invoiceItem = new InvoiceItem
                                {
                                    InvoiceId = invoice.Id,
                                    Type = "cr",
                                    CourseId = item.CourseId == 0 ? null : item.CourseId,
                                    SectionId = registrationCourse?.SectionId,
                                    RegistrationCourseId = registrationCourse == null ? null
                                                                                        : registrationCourse?.Id,
                                    FeeItemId = item.FeeItemId,
                                    FeeItemName = item.FeeItemName,
                                    TaxRate = 0,
                                    IsVAT = false,
                                    IsVATIncluded = false,
                                    Amount = item.Amount,
                                    // ScholarshipAmount = item.ScholarshipAmount,
                                    TotalVATAmount = 0,
                                    TotalAmount = item.TotalAmount,
                                    IsPaid = false,
                                    // ScholarshipStudentId = item.ScholarshipStudentId
                                };

                                _db.InvoiceItems.Add(invoiceItem);
                            }
                        }

                        invoice.Amount = invoiceItems.Sum(x => x.Amount);
                        // invoice.TotalScholarshipAmount = invoiceItems.Sum(x => x.ScholarshipAmount);
                        // invoice.TotalItemsDiscount = invoiceItems.Sum(x => x.DiscountAmount);
                        invoice.TotalVATAmount = 0;
                        invoice.TotalAmount = invoice.Amount - invoice.TotalScholarshipAmount - invoice.TotalItemsDiscount - invoice.TotalDeductAmount - invoice.TotalDiscount;
                        invoice.CreditNoteBalance = invoice.Amount;

                        UpdateInvoiceReference(invoice, student.Code);
                        // DateTime firstRegis = new DateTime(2022, 01, 14);
                        // DateTime addDrop = new DateTime(2022, 01, 21);
                        // invoice.PaymentExpireAt = invoice.Type == "r" ? firstRegis : addDrop;
                        // invoice.Reference2 = _feeProvider.GenerateInvoiceReference2(invoice.CreatedAt, invoice.PaymentExpireAt.Value);
                        // string studentCode = student.Code.Substring(0, 2) + student.Code.Substring(3, 4); // Ignore third digit
                        // invoice.Reference1 = _feeProvider.GenerateInvoiceReference1(studentCode + invoice.Number, invoice.Reference2, invoice.TotalAmount);
                        invoice.Year = today.Year;
                        invoice.Month = today.Month;

                        _db.SaveChanges();

                        transaction.Commit();
                        return invoice;
                    }
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    return null;
                }
            }
            return null;
        }

        public Invoice SimulateAddInvoiceCreditNote(Guid studentId, long termId, List<RegistrationCourse> registrationCourses)
        {
            var now = DateTime.Today;
            var invoice = new Invoice();
            var student = _studentProvider.GetStudentInformationById(studentId);
            var registrationCourseIds = registrationCourses.Select(x => x.Id);
            bool isLumpsumPayment = student.StudentFeeGroup?.IsLumpsumPayment ?? false;
            var studentState = _db.StudentStates.SingleOrDefault(x => x.StudentId == student.Id && x.TermId == termId);
            string studentStateRound = studentState != null && (studentState.State != "REG") && (studentState.State != "PAY_REG") ? "a" : "r";

            if (student != null && registrationCourseIds.Any())
            {
                invoice.Year = DateTime.Today.Year;
                invoice.TermId = termId;
                invoice.StudentId = studentId;
                invoice.Name = student.FullNameEn;
                invoice.Type = "cr";

                invoice.InvoiceItems = new List<InvoiceItem>();
                foreach (var registrationCourse in registrationCourses)
                {
                    var refundInvoiceItem = _db.InvoiceItems.Include(x => x.Invoice)
                                                            .Include(x => x.FeeItem)
                                                            .Where(x => !x.Invoice.IsCancel
                                                                        && x.IsPaid
                                                                        && x.Invoice.Type != "cr"
                                                                        && x.RegistrationCourseId == registrationCourse.Id)
                                                            .OrderByDescending(x => x.Id)
                                                            .FirstOrDefault();
                    if (refundInvoiceItem != null)
                    {
                        if (refundInvoiceItem.TotalAmount == 0)
                        {
                            var creditNote = _db.InvoiceDeductTransactions.Where(x => x.InvoiceItemId == refundInvoiceItem.Id)
                                                                            .OrderByDescending(x => x.Id)
                                                                            .FirstOrDefault();
                            if (creditNote != null)
                            {
                                refundInvoiceItem.Amount = creditNote.Amount;
                                refundInvoiceItem.TotalAmount = creditNote.Amount;
                            }
                        }
                        var invoiceItem = new InvoiceItem
                        {
                            InvoiceId = invoice.Id,
                            Type = "cr",
                            CourseId = registrationCourse.CourseId == 0 ? null : registrationCourse.CourseId,
                            SectionId = registrationCourse?.SectionId,
                            RegistrationCourseId = registrationCourse == null ? null
                                                                                : registrationCourse?.Id,
                            FeeItemId = refundInvoiceItem.FeeItem.Id,
                            FeeItemName = refundInvoiceItem.FeeItem.NameEn,
                            Amount = refundInvoiceItem.Amount,
                            ScholarshipAmount = refundInvoiceItem.ScholarshipAmount,
                            TotalVATAmount = 0,
                            TotalAmount = refundInvoiceItem.TotalAmount,
                            ScholarshipStudentId = refundInvoiceItem.ScholarshipStudentId
                        };

                        invoice.InvoiceItems.Add(invoiceItem);
                    }
                }

                if (isLumpsumPayment && studentStateRound == "a")
                {
                    var tuitionFee = _db.FeeItems.SingleOrDefault(x => x.Code == "001");
                    foreach (var item in registrationCourses)
                    {
                        invoice.InvoiceItems.Add(new InvoiceItem()
                        {
                            FeeItem = tuitionFee,
                            FeeItemId = tuitionFee.Id,
                            FeeItemName = tuitionFee.NameEn,
                            CourseId = item.CourseId,
                            CourseCode = item.CourseCode,
                            SectionId = item.SectionId,
                            Amount = 0,
                            ScholarshipAmount = 0,
                            TotalAmount = 0,
                            LumpSumAddDrop = "drop"
                        });
                    }
                }

                invoice.Amount = invoice.InvoiceItems.Sum(x => x.Amount);
                invoice.TotalScholarshipAmount = invoice.InvoiceItems.Sum(x => x.ScholarshipAmount);
                // invoice.TotalItemsDiscount = invoiceItems.Sum(x => x.DiscountAmount);
                invoice.TotalVATAmount = 0;
                invoice.TotalAmount = invoice.Amount - invoice.TotalScholarshipAmount - invoice.TotalItemsDiscount - invoice.TotalDeductAmount - invoice.TotalDiscount;
                invoice.CreditNoteBalance = invoice.Amount;
            }

            return invoice;
        }

        public Invoice AddInvoice(Guid studentId, long termId, List<RegistrationCourse> registrationCourses, string round, int addDropRound = 0)
        {
            var now = DateTime.Today;
            var invoice = new Invoice();
            var student = _studentProvider.GetStudentInformationById(studentId);
            bool isLumpsumPayment = student.StudentFeeGroup?.IsLumpsumPayment ?? false;
            var invoiceItems = GetInvoiceItemsForPreview(registrationCourses, 0);
            var studentState = _db.StudentStates.SingleOrDefault(x => x.StudentId == student.Id && x.TermId == termId);
            string studentStateRound = studentState != null && (studentState.State != "REG") && (studentState.State != "PAY_REG") ? "a" : "r";

            if (isLumpsumPayment && studentStateRound == "a")
            {
                invoiceItems = new List<InvoiceItem>();
                var tuitionFee = _db.FeeItems.SingleOrDefault(x => x.Code == "001");
                foreach (var item in registrationCourses)
                {
                    invoiceItems.Add(new InvoiceItem()
                    {
                        FeeItem = tuitionFee,
                        FeeItemId = tuitionFee.Id,
                        FeeItemName = tuitionFee.NameEn,
                        CourseId = item.CourseId,
                        CourseCode = item.CourseCode,
                        SectionId = item.SectionId,
                        Amount = 0,
                        ScholarshipAmount = 0,
                        TotalAmount = 0,
                        LumpSumAddDrop = "add"
                    });
                }
            }

            if (student != null && (invoiceItems.Any() || round == "a" || round == "au" || round == "cr"))
            {
                using (var transaction = _db.Database.BeginTransaction())
                {
                    var today = DateTime.UtcNow;
                    invoice.RunningNumber = _feeProvider.GetNextInvoiceRunningNumber();
                    invoice.Number = _feeProvider.GetFeeInvoiceNumber(invoice.RunningNumber);
                    invoice.Year = DateTime.Today.Year;
                    invoice.TermId = termId;
                    invoice.StudentId = studentId;
                    invoice.Name = student.FullNameEn;
                    invoice.Type = round;

                    var address = student.StudentAddresses.FirstOrDefault(x => x.Type == "Current");
                    if (address != null)
                    {
                        invoice.Address = address.AddressEn1;
                        invoice.Address2 = address.AddressEn2;
                        invoice.IsCancel = false;
                    }

                    if (round == "a" || round == "au" || round == "cr")
                    {
                        invoice.AddDropSequence = addDropRound;
                    }

                    if (round == "cr")
                    {
                        invoice.IsPaid = true;
                    }

                    try
                    {
                        _db.Invoices.Add(invoice);
                        _db.SaveChanges();

                        foreach (var item in invoiceItems)
                        {
                            var registrationCourse = registrationCourses.SingleOrDefault(x => x.CourseId == item.CourseId);
                            var invoiceItem = new InvoiceItem
                            {
                                InvoiceId = invoice.Id,
                                Type = round,
                                CourseId = item.CourseId == 0 ? null : item.CourseId,
                                SectionId = registrationCourse?.SectionId,
                                RegistrationCourseId = registrationCourse == null ? null
                                                                                  : registrationCourse?.Id,
                                FeeItemId = item.FeeItemId,
                                FeeItemName = item.FeeItemName,
                                TaxRate = 0,
                                IsVAT = false,
                                IsVATIncluded = false,
                                Amount = item.Amount,
                                ScholarshipAmount = item.ScholarshipAmount,
                                TotalVATAmount = 0,
                                TotalAmount = item.TotalAmount,
                                IsPaid = round == "cr",
                                ScholarshipStudentId = item.ScholarshipStudentId
                            };

                            _db.InvoiceItems.Add(invoiceItem);
                        }

                        invoice.Amount = invoiceItems.Sum(x => x.Amount);
                        invoice.TotalScholarshipAmount = invoiceItems.Sum(x => x.ScholarshipAmount);
                        invoice.TotalItemsDiscount = invoiceItems.Sum(x => x.DiscountAmount);
                        invoice.TotalVATAmount = 0;
                        invoice.TotalAmount = invoice.Amount - invoice.TotalScholarshipAmount - invoice.TotalItemsDiscount - invoice.TotalDeductAmount - invoice.TotalDiscount;
                        if (round == "cr")
                        {
                            invoice.CreditNoteBalance = invoice.Amount;
                        }

                        UpdateInvoiceReference(invoice, student.Code);
                        // DateTime firstRegis = new DateTime(2022, 01, 14);
                        // DateTime addDrop = new DateTime(2022, 01, 21);
                        // invoice.PaymentExpireAt = invoice.Type == "r" ? firstRegis : addDrop;
                        // if(invoice.PaymentExpireAt < DateTime.Today)
                        // {
                        //     invoice.PaymentExpireAt = DateTime.Today;
                        // }
                        // invoice.Reference2 = _feeProvider.GenerateInvoiceReference2(invoice.CreatedAt, invoice.PaymentExpireAt.Value);
                        // string studentCode = student.Code.Substring(0, 2) + student.Code.Substring(3, 4); // Ignore third digit
                        // invoice.Reference1 = _feeProvider.GenerateInvoiceReference1(studentCode + invoice.Number, invoice.Reference2, invoice.TotalAmount);
                        invoice.Year = today.Year;
                        invoice.Month = today.Month;

                        _db.SaveChanges();

                        if (GenerateBarCode(invoice).Result)
                        {

                        }

                        transaction.Commit();
                        return invoice;
                    }
                    catch
                    {
                        transaction.Rollback();
                        return null;
                    }
                }
            }

            return null;
        }

        public Invoice SimulateAddInvoice(Guid studentId, long termId, List<RegistrationCourse> registrationCourses, string round)
        {
            var now = DateTime.Today;
            var invoice = new Invoice();
            var student = _studentProvider.GetStudentInformationById(studentId);
            bool isLumpsumPayment = student.StudentFeeGroup?.IsLumpsumPayment ?? false;
            var invoiceItems = GetInvoiceItemsForPreview(registrationCourses, 0);
            var studentState = _db.StudentStates.SingleOrDefault(x => x.StudentId == student.Id && x.TermId == termId);
            string studentStateRound = studentState != null && (studentState.State != "REG") && (studentState.State != "PAY_REG") ? "a" : "r";

            if (isLumpsumPayment && studentStateRound == "a")
            {
                invoiceItems = new List<InvoiceItem>();
                var tuitionFee = _db.FeeItems.SingleOrDefault(x => x.Code == "001");
                foreach (var item in registrationCourses)
                {
                    invoiceItems.Add(new InvoiceItem()
                    {
                        FeeItem = tuitionFee,
                        FeeItemId = tuitionFee.Id,
                        FeeItemName = tuitionFee.NameEn,
                        CourseId = item.CourseId,
                        CourseCode = item.CourseCode,
                        SectionId = item.SectionId,
                        Amount = 0,
                        ScholarshipAmount = 0,
                        TotalAmount = 0,
                        LumpSumAddDrop = "add"
                    });
                }
            }

            if (student != null && invoiceItems.Any())
            {
                invoice.Year = DateTime.Today.Year;
                invoice.TermId = termId;
                invoice.StudentId = studentId;
                invoice.Name = student.FullNameEn;
                invoice.Type = round;

                invoice.InvoiceItems = new List<InvoiceItem>();
                foreach (var item in invoiceItems)
                {
                    var registrationCourse = registrationCourses.SingleOrDefault(x => x.CourseId == item.CourseId);
                    var invoiceItem = new InvoiceItem
                    {
                        InvoiceId = invoice.Id,
                        CourseId = item.CourseId == 0 ? null : item.CourseId,
                        SectionId = registrationCourse?.SectionId,
                        RegistrationCourseId = registrationCourse == null ? null
                                                                            : registrationCourse?.Id,
                        FeeItemId = item.FeeItemId,
                        FeeItemName = item.FeeItemName,
                        Amount = item.Amount,
                        ScholarshipAmount = item.ScholarshipAmount,
                        TotalVATAmount = 0,
                        TotalAmount = item.TotalAmount,
                        ScholarshipStudentId = item.ScholarshipStudentId,
                        LumpSumAddDrop = item.LumpSumAddDrop
                    };

                    invoice.InvoiceItems.Add(invoiceItem);
                }

                invoice.Amount = invoiceItems.Sum(x => x.Amount);
                invoice.TotalScholarshipAmount = invoiceItems.Sum(x => x.ScholarshipAmount);
                invoice.TotalItemsDiscount = invoiceItems.Sum(x => x.DiscountAmount);
                invoice.TotalVATAmount = 0;
                invoice.TotalAmount = invoice.Amount - invoice.TotalScholarshipAmount - invoice.TotalItemsDiscount - invoice.TotalDeductAmount - invoice.TotalDiscount;

                return invoice;
            }

            return null;
        }

        public List<RefundDetail> GetInvoiceCourseFeeItem(Guid studentId, long termId, long courseId, long registrationId)
        {
            // check registrationcourseId nullable or not?
            var items = _db.ReceiptItems.Include(x => x.InvoiceItem)
                                            .ThenInclude(x => x.Invoice)
                                        .Include(x => x.Receipt)
                                        .Include(x => x.InvoiceItem)
                                            .ThenInclude(x => x.Course)
                                        .Include(x => x.InvoiceItem)
                                            .ThenInclude(x => x.Section)
                                        .Include(x => x.InvoiceItem)
                                            .ThenInclude(x => x.FeeItem)
                                        .Where(x => x.InvoiceItem.CourseId == courseId
                                                    && x.InvoiceItem.RegistrationCourseId == registrationId
                                                    && x.Amount >= 0
                                                    && x.Invoice.StudentId == studentId
                                                    && x.Invoice.TermId == termId
                                                    && !x.Invoice.IsCancel)
                                        .Select(x => _mapper.Map<ReceiptItem, RefundDetail>(x))
                                        .ToList();
            return items;
        }

        public List<ReceiptItem> GetReceiptItemsByCourseId(long registrationId)
        {
            var items = _db.ReceiptItems.Include(x => x.InvoiceItem)
                                            .ThenInclude(x => x.FeeItem)
                                        .Include(x => x.Receipt)
                                        .Where(x => x.InvoiceItem.RegistrationCourseId == registrationId
                                                    && x.Amount >= 0
                                                    && !x.Receipt.IsCancel)
                                        .ToList();
            return items;
        }

        public List<Receipt> GetReceiptNumberByStudentCodeAndTerm(string code, int year, int term)
        {
            var student = _studentProvider.GetStudentByCode(code);
            var studentId = student?.Id ?? new Guid();
            var termId = _academicProvider.GetTermByTermAndYear(student?.AcademicInformation?.AcademicLevelId ?? 0, term, year)?.Id ?? 0;
            var receipts = _db.Receipts.Where(x => x.StudentId == studentId
                                                    && x.TermId == termId
                                                    && !x.IsCancel)
                                       .ToList();
            return receipts;
        }

        public List<ReceiptItem> GetTermFeeItemsByStudentAndTermId(Guid studentId, long termId)
        {
            var items = _db.ReceiptItems.Include(x => x.InvoiceItem)
                                            .ThenInclude(x => x.FeeItem)
                                        .Include(x => x.Receipt)
                                        .Where(x => x.InvoiceItem.CourseId == null
                                                    && x.Amount >= 0
                                                    && x.Receipt.StudentId == studentId
                                                    && x.Receipt.TermId == termId
                                                    && !x.Receipt.IsCancel)
                                        .ToList();
            return items;
        }

        public List<RegistrationCourse> GetReceiptsCourses(Guid studentId, long termId)
        {
            var items = _db.ReceiptItems.Include(x => x.InvoiceItem)
                                            .ThenInclude(x => x.FeeItem)
                                        .Include(x => x.InvoiceItem)
                                            .ThenInclude(x => x.RegistrationCourse)
                                            .ThenInclude(x => x.Course)
                                        .Include(x => x.Receipt)
                                        .Where(x => x.InvoiceItem.CourseId != null
                                                    && x.Amount >= 0
                                                    && x.Receipt.StudentId == studentId
                                                    && x.Receipt.TermId == termId
                                                    && !x.Receipt.IsCancel)
                                        .Select(x => x.InvoiceItem.RegistrationCourse)
                                        .Distinct()
                                        .ToList();
            return items;
        }

        public string GetAmountByReceiptId(long receiptItemId)
        {
            var receipt = _db.ReceiptItems.SingleOrDefault(x => x.Id == receiptItemId);
            return receipt == null ? "0" : receipt.AmountText;
        }

        public Receipt GetReceiptCertificateById(long id, string language = "en")
        {
            var receipt = _db.Receipts.SingleOrDefault(x => x.Id == id);
            receipt.ReceiptItems = (from receiptItem in _db.ReceiptItems
                                    join invoiceItem in _db.InvoiceItems on receiptItem.InvoiceItemId equals invoiceItem.Id
                                    join feeItem in _db.FeeItems on invoiceItem.FeeItemId equals feeItem.Id
                                    where receiptItem.ReceiptId == receipt.Id
                                    group new { receiptItem, feeItem } by new { invoiceItem.FeeItemId } into receiptItemGroup
                                    select new ReceiptItem
                                    {
                                        FeeItemId = receiptItemGroup.Key.FeeItemId,
                                        FeeItemName = language == "en" ? receiptItemGroup.FirstOrDefault().feeItem.NameEn
                                                                              : receiptItemGroup.FirstOrDefault().feeItem.NameTh,
                                        TotalAmount = receiptItemGroup.Sum(x => x.receiptItem.TotalAmount)
                                    }).ToList();
            return receipt;
        }

        public string GetTotalAmountTh(string money)
        {
            string bahtTxt, n, bahtTH = "", minusPrefix = "";
            double amount;
            try { amount = Convert.ToDouble(money); }
            catch { amount = 0; }
            if (amount < 0)
            {
                minusPrefix = "ติดลบ";
                amount = Math.Abs(amount);
            }
            bahtTxt = amount.ToString("####.00");
            string[] num = { "ศูนย์", "หนึ่ง", "สอง", "สาม", "สี่", "ห้า", "หก", "เจ็ด", "แปด", "เก้า", "สิบ" };
            string[] rank = { "", "สิบ", "ร้อย", "พัน", "หมื่น", "แสน", "ล้าน" };
            string[] temp = bahtTxt.Split('.');
            string intVal = temp[0];
            string decVal = temp[1];
            if (Convert.ToDouble(bahtTxt) == 0)
                bahtTH = "ศูนย์บาทถ้วน";
            else
            {
                for (int i = 0; i < intVal.Length; i++)
                {
                    n = intVal.Substring(i, 1);
                    if (n != "0")
                    {
                        if ((i == (intVal.Length - 1)) && (n == "1"))
                            bahtTH += "เอ็ด";
                        else if ((i == (intVal.Length - 2)) && (n == "2"))
                            bahtTH += "ยี่";
                        else if ((i == (intVal.Length - 2)) && (n == "1"))
                            bahtTH += "";
                        else
                            bahtTH += num[Convert.ToInt32(n)];
                        bahtTH += rank[(intVal.Length - i) - 1];
                    }
                }
                bahtTH += "บาท";
                if (decVal == "00")
                    bahtTH += "ถ้วน";
                else
                {
                    for (int i = 0; i < decVal.Length; i++)
                    {
                        n = decVal.Substring(i, 1);
                        if (n != "0")
                        {
                            if ((i == decVal.Length - 1) && (n == "1"))
                                bahtTH += "เอ็ด";
                            else if ((i == (decVal.Length - 2)) && (n == "2"))
                                bahtTH += "ยี่";
                            else if ((i == (decVal.Length - 2)) && (n == "1"))
                                bahtTH += "";
                            else
                                bahtTH += num[Convert.ToInt32(n)];
                            bahtTH += rank[(decVal.Length - i) - 1];
                        }
                    }
                    bahtTH += "สตางค์";
                }
            }
            return minusPrefix + bahtTH;
        }

        public string GetTotalAmountEn(string money)
        {
            var moneySplit = money.Split('.');
            string bahtText = "", satangText = "", moneyText = "", minusText = "";
            if (moneySplit[0].StartsWith("-"))
            {
                minusText = "-";
                moneySplit[0] = moneySplit[0].Substring(1);
            }
            var baht = Int32.Parse(moneySplit[0], NumberStyles.AllowThousands, new CultureInfo("en-au"));

            var satang = Int32.Parse(moneySplit[1]);
            if (satang == 0)
            {
                bahtText = baht.ToText("en") + " baht only";
                moneyText = bahtText;
            }
            else
            {
                bahtText = baht.ToText("en") + " baht";
                satangText = satang.ToText("en") + " satang";
                moneyText = $"{ bahtText } and { satangText }";
            }

            return minusText + moneyText.ToUpper();
        }

        public FinanceOtherFeeViewModel FinanceOtherFeePreview(long id)
        {
            var model = new FinanceOtherFeeViewModel();
            var receipt = GetReceiptById(id);
            var receiptItems = new List<FinanceOtherFeeReceiptItem>();
            model = _mapper.Map<Receipt, FinanceOtherFeeViewModel>(receipt);
            model.CreatedAt = DateTime.Now.ToString(StringFormat.ShortDate);
            if (receipt.ReceiptItems != null)
            {
                foreach (var item in receipt.ReceiptItems)
                {
                    var receiptItem = new FinanceOtherFeeReceiptItem
                    {
                        Name = item.FeeItem.NameEn,
                        Quantity = item.ReceiptItemQuantity,
                        AmountPrice = item.AmountText,
                        TotalAmount = item.TotalAmountText
                    };

                    receiptItems.Add(receiptItem);
                }

                model.ReceiptItems = receiptItems;
            }

            model.TotalAmountTextTh = GetTotalAmountTh(model.TotalAmount);
            return model;
        }

        public List<string> GetReceiptCreatedBy()
        {
            var createdby = _db.Receipts.Select(x => x.CreatedBy)
                                        .Distinct()
                                        .OrderBy(x => x)
                                        .ToList();
            return createdby;
        }

        public ReceiptPDFViewModel GetReceiptPDF(long receiptId, ApplicationUser user)
        {
            ReceiptPDFViewModel model = new ReceiptPDFViewModel();
            var receipt = _db.Receipts.Include(x => x.Student)
                                          .ThenInclude(x => x.AcademicInformation)
                                              .ThenInclude(x => x.Department)
                                      .Include(x => x.Student)
                                          .ThenInclude(x => x.AcademicInformation)
                                              .ThenInclude(x => x.Faculty)
                                      .Include(x => x.ReceiptItems)
                                          .ThenInclude(x => x.InvoiceItem)
                                              .ThenInclude(x => x.FeeItem)
                                      .Include(x => x.ReceiptItems)
                                          .ThenInclude(x => x.InvoiceItem)
                                              .ThenInclude(x => x.Course)
                                      .Include(x => x.ReceiptItems)
                                          .ThenInclude(x => x.InvoiceItem)
                                              .ThenInclude(x => x.Section)
                                      .Include(x => x.Invoice)
                                      .Include(x => x.ReceiptPaymentMethods)
                                          .ThenInclude(x => x.PaymentMethod)
                                      .SingleOrDefault(x => x.Id == receiptId);
            if (receipt != null)
            {
                model = new ReceiptPDFViewModel
                {
                    StudentCode = receipt.Student.Code,
                    StudentFullName = receipt.Student.FullNameEn,
                    FacultyName = receipt.Student.AcademicInformation.Faculty.NameEn,
                    DepartmentName = receipt.Student.AcademicInformation.Department.NameEn,
                    ReceiptNumber = receipt.Number,
                    ReceiptAt = receipt.CreatedAt,
                    InvoiceNumber = receipt.Invoice.Number,

                    ReceiverName = user?.NormalizedUserName,
                    ReceiverFullName = user?.ReportNameEn,
                    ReceiverRoleName = user?.Role,

                    Courses = receipt.ReceiptItems.Where(x => x.InvoiceItem.Course != null)
                                                  .GroupBy(x => new
                                                  {
                                                      x.InvoiceItem.CourseId,
                                                      x.InvoiceItem.SectionId
                                                  })
                                                  .Select(x =>
                                                  {
                                                      var invoiceItem = x.FirstOrDefault().InvoiceItem;
                                                      return new ReceiptPDFCourse
                                                      {
                                                          Code = invoiceItem.Course.Code,
                                                          NameEn = invoiceItem.Course.NameEn,
                                                          Credit = invoiceItem.Course.Credit,
                                                          SectionNumber = invoiceItem.Section?.Number
                                                      };
                                                  })
                                                  .ToList(),
                    Fees = receipt.ReceiptItems.GroupBy(x => x.InvoiceItem.FeeItemId)
                                               .Select(x => new ReceiptPDFFee
                                               {
                                                   NameEn = x.FirstOrDefault().InvoiceItem.FeeItemName,
                                                   Amount = x.Sum(y => y.InvoiceItem.Amount)
                                               })
                                               .ToList(),
                    Payments = receipt.ReceiptPaymentMethods.Select(x => new ReceiptPDFPayment
                    {
                        NameEn = x.PaymentMethod.NameEn,
                        Amount = x.Amount
                    })
                                                            .Union(receipt.ReceiptItems.GroupBy(x => x.ReceiptId)
                                                                                       .Where(x => x.Sum(y => y.ScholarshipAmount) > 0)
                                                                                       .Select(x => new ReceiptPDFPayment
                                                                                       {
                                                                                           NameEn = "Scholarship",
                                                                                           Amount = x.Sum(y => y.ScholarshipAmount)
                                                                                       }))
                                                            .ToList(),
                    TotalAmountText = GetTotalAmountEn(receipt.TotalAmountText)
                };
            }

            return model;
        }

        public async Task<bool> GenerateBarCode(Invoice model)
        {
            var body = new PaymentRequestBodyViewModel
            {
                OrderId = model.Number.ToString(),
                PaymentMethod = "QR",
                PaymentProvider = "SCB",
                ConfigurationKey = "",
                CustomerId = model.StudentId.ToString(),
                Amount = model.Amount,
                Reference1 = model.Reference1,
                Reference2 = model.Reference2,
                Reference3 = model.Reference3
            };

            var bodyJson = JsonConvert.SerializeObject(body);
            var publicKey = "ba7956dd37154957b4ec129937417a5d";
            var secretKey = "Se2w7Ryz3k6eOotWVGLjIpNa8WgJQ0J+ctSCTn5KJKY=";

            var unixEpochTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var nOnce = GenerateRandomString(15);
            var authenticationSignature = $"{publicKey}|{nOnce}|{unixEpochTime}";
            var HMACSHA256 = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
            var byteHMACSHA256 = HMACSHA256.ComputeHash(Encoding.UTF8.GetBytes(authenticationSignature));
            var hasedAuthenticationSignature = Convert.ToBase64String(byteHMACSHA256);
            var authenticationHeader = $"amx {publicKey}:{hasedAuthenticationSignature}:{nOnce}:{unixEpochTime}";

            var url = _config["GenerateBarcodeUrl"];
            using (var client = new HttpClient())
            {
                try

                {
                    var content = new StringContent(bodyJson, Encoding.UTF8, "application/json");

                    var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
                    requestMessage.Headers.Add("Authorization", authenticationHeader);
                    requestMessage.Content = content;

                    var response = await client.SendAsync(requestMessage);
                    var rawResponse = await response.Content.ReadAsStringAsync();
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var barCode = JsonConvert.DeserializeObject<BarcodeViewModel>(rawResponse);

                        var invoice = _db.Invoices.IgnoreQueryFilters()
                                                  .SingleOrDefault(x => x.Number == model.Number);

                        invoice.Barcode = barCode.Value;
                        invoice.Base64Barcode = barCode.Value2;
                        _db.SaveChanges();
                        return false;
                    }
                    else
                    {
                        var error = JsonConvert.DeserializeObject<object>(rawResponse);
                        return false;
                    }
                }
                catch
                {
                    return true;
                }
            }
        }

        private static string GenerateRandomString(int length)
        {
            using (var crypto = new RNGCryptoServiceProvider())
            {
                var bits = (length * 6);
                var byte_size = ((bits + 7) / 8);
                var bytesarray = new byte[byte_size];
                crypto.GetBytes(bytesarray);
                return Convert.ToBase64String(bytesarray);
            }
        }

        public async Task ReplaceRegistrationCourseFromUspark(Guid studentId, long termId)
        {
            // call api get registration 
            var student = _db.Students.SingleOrDefault(x => x.Id == studentId);
            var term = _db.Terms.SingleOrDefault(x => x.Id == termId);
            var addingResults = new List<AddingViewModel>();
            var preregistrations = await _registrationProvider.CallUSparkAPIGetPreregistrations(student.Code, term.AcademicYear, term.AcademicTerm);
            if (preregistrations != null && preregistrations.Preregistrations != null && preregistrations.Preregistrations.Any())
            {
                foreach (var item in preregistrations.Preregistrations)
                {
                    var section = _db.Sections.FirstOrDefault(x => x.Id == item.KSSectionId);
                    if (section != null)
                    {
                        var course = _db.Courses.FirstOrDefault(x => x.Id == section.CourseId);
                        if (course != null)
                        {
                            addingResults.Add(new AddingViewModel
                            {
                                CourseId = course.Id,
                                TermId = termId,
                                CourseCode = course.Code,
                                CourseCodeAndName = course.CodeAndName,
                                SectionId = section.Id,
                                SectionNumber = section.Number,
                                IsPaid = item.IsPaid
                            });
                        }
                    }
                }
            }

            var registrationCourses = addingResults.Select(x => new RegistrationCourse
            {
                StudentId = studentId,
                TermId = termId,
                CourseId = x.CourseId,
                SectionId = x.SectionId,
                IsPaid = x.IsPaid
            })
                                                    .ToList();

            var invoice = _db.Invoices.Include(x => x.InvoiceItems)
                                      .FirstOrDefault(x => !x.IsCancel
                                                           && x.StudentId == studentId
                                                           && x.TermId == termId
                                                           && x.Type == "r"
                                                           && x.InvoiceItems.Any(y => y.RegistrationCourseId != null));

            bool same = invoice != null && invoice.InvoiceItems.Any();
            var invoiceItems = GetInvoiceItemsForPreview(registrationCourses, 0);
            invoiceItems.ForEach(x =>
            {
                x.SectionId = registrationCourses.SingleOrDefault(y => y.CourseId == x.CourseId)?.SectionId;
            });

            if (same)
            {
                foreach (var item in invoiceItems)
                {
                    if (!invoice.InvoiceItems.Any(x => (x.CourseId ?? 0) == (item.CourseId ?? 0)
                                                        && (x.SectionId ?? 0) == (item.SectionId ?? 0)
                                                        && x.FeeItemId == item.FeeItemId
                                                        && x.TotalAmount == item.Amount))
                    {
                        same = false;
                        break;
                    }
                }

                if (same)
                {
                    foreach (var item in invoice.InvoiceItems)
                    {
                        if (!invoiceItems.Any(x => (x.CourseId ?? 0) == (item.CourseId ?? 0)
                                                    && (x.SectionId ?? 0) == (item.SectionId ?? 0)
                                                    && x.FeeItemId == item.FeeItemId
                                                    && x.Amount == item.TotalAmount))
                        {
                            same = false;
                            break;
                        }
                    }
                }
            }

            if (!same)
            {
                // cancel registration
                _registrationProvider.CancelRegistration(studentId, termId);

                if (addingResults != null && addingResults.Any())
                {
                    string round = _registrationProvider.GetStudentState(studentId, termId);
                    List<RegistrationCourse> newCourses = new List<RegistrationCourse>();
                    List<RegistrationCourse> deleteUnpaidCourses = new List<RegistrationCourse>();
                    List<RegistrationCourse> deletePaidCourses = new List<RegistrationCourse>();
                    // insert registration
                    var result = _registrationProvider.ModifyRegistrationCourse(student.Id, termId, round, addingResults, out newCourses, out deleteUnpaidCourses, out deletePaidCourses, "s");

                    // insert invoice
                    AddInvoice(student.Id, termId, result.Where(x => x.Refunds == null).ToList(), "r");
                }
            }
        }

        public List<Invoice> GetInvoicesByStudentCodeAndTermId(string studentCode, long termId)
        {
            //var student = _db.Students.FirstOrDefault(x => x.Code == studentCode);
            // if (_registrationProvider.IsRegistrationPeriod(DateTime.Now, termId))
            // {
            //     ReplaceRegistrationCourseFromUspark(student.Id, termId);
            // }

            var invoices = _db.Invoices.Include(x => x.Student)
                                       .Where(x => x.Student.Code == studentCode
                                                   && x.TermId == termId
                                                   && !x.IsCancel
                                                   && !x.IsPaid)
                                       .OrderByDescending(x => x.CreatedBy)
                                       .ToList();
            return invoices;
        }

        public Invoice DropCourses(Guid studentId, long termId, List<RegistrationCourse> registrationCourses, int addDropRound)
        {
            var invoice = AddInvoiceCreditNote(studentId, termId, registrationCourses, addDropRound);
            _db.InvoiceDeductTransactions.Add(new InvoiceDeductTransaction
            {
                InvoiceCreditNoteId = invoice.Id,
                Type = "n",
                Amount = invoice.TotalAmount,
                Balance = invoice.TotalAmount
            });

            _db.SaveChanges();
            return invoice;
        }

        public void SimulateApplyCreditNote(Invoice invoice, Invoice creditNoteInvoice)
        {
            var invoiceItemsToApply = invoice.InvoiceItems.Where(x => x.Amount >= 0).ToList();
            var totalAmount = invoiceItemsToApply.Sum(x => x.Amount);
            if (totalAmount == 0)
            {
                return;
            }

            foreach (var invoiceItem in invoiceItemsToApply)
            {
                var invoiceItemAmount = Math.Min(creditNoteInvoice.CreditNoteBalance, invoiceItem.Amount);
                // invoiceItem.Amount -= invoiceItemAmount;
                invoiceItem.DiscountAmount = invoiceItemAmount;
                invoiceItem.TotalAmount -= invoiceItemAmount;
                //amount -= invoiceItemAmount;
                creditNoteInvoice.TotalDeductAmount += invoiceItemAmount;
                creditNoteInvoice.CreditNoteBalance -= invoiceItemAmount;
            }

            invoice.Amount = invoice.InvoiceItems.Sum(x => x.Amount);
            invoice.TotalDeductAmount = invoice.InvoiceItems.Sum(x => x.DiscountAmount);
            invoice.TotalAmount = invoice.InvoiceItems.Sum(x => x.TotalAmount);

            creditNoteInvoice.TotalDeductAmount = invoice.InvoiceItems.Sum(x => x.DiscountAmount);
            creditNoteInvoice.TotalAmount += creditNoteInvoice.TotalDeductAmount;
        }

        public void ApplyCreditNote(Invoice invoice, Invoice creditNoteInvoice)
        {
            var invoiceItemsToApply = invoice.InvoiceItems.Where(x => x.Amount >= 0).ToList();
            var totalAmount = invoiceItemsToApply.Sum(x => x.Amount);
            if (totalAmount == 0)
            {
                return;
            }
            var crInvoice = _db.Invoices.SingleOrDefault(x => x.Id == creditNoteInvoice.Id);
            if (crInvoice != null && crInvoice.CreditNoteBalance > 0)
            {
                //var amount = Math.Min(totalAmount, crInvoice.CreditNoteBalance);
                //totalAmount -= amount;

                foreach (var invoiceItem in invoiceItemsToApply)
                {
                    var invoiceItemAmount = Math.Min(crInvoice.CreditNoteBalance, invoiceItem.Amount);
                    // invoiceItem.Amount -= invoiceItemAmount;
                    invoiceItem.DiscountAmount = invoiceItemAmount;
                    invoiceItem.TotalAmount -= invoiceItemAmount;
                    //amount -= invoiceItemAmount;
                    crInvoice.TotalDeductAmount += invoiceItemAmount;
                    crInvoice.CreditNoteBalance -= invoiceItemAmount;


                    _db.InvoiceDeductTransactions.Add(new InvoiceDeductTransaction
                    {
                        InvoiceId = invoice.Id,
                        InvoiceItem = invoiceItem,
                        InvoiceCreditNoteId = crInvoice.Id,
                        Type = "u",
                        Amount = invoiceItemAmount,
                        Balance = crInvoice.CreditNoteBalance
                    });

                    if (invoiceItem.TotalAmount == 0)
                    {
                        invoiceItem.IsPaid = true;

                        var registrationCourse = _db.RegistrationCourses.SingleOrDefault(x => x.Id == invoiceItem.RegistrationCourseId);
                        if (registrationCourse != null)
                        {
                            registrationCourse.IsPaid = true;
                        }
                    }
                }

                invoice.Amount = invoice.InvoiceItems.Where(x => x.Type != "cr").Sum(x => x.Amount);
                invoice.TotalDeductAmount = invoice.InvoiceItems.Sum(x => x.DiscountAmount);
                invoice.TotalAmount = invoice.InvoiceItems.Where(x => x.Type != "cr").Sum(x => x.TotalAmount);
                invoice.IsPaid = invoice.InvoiceItems.All(x => x.IsPaid);

                crInvoice.TotalDeductAmount = invoice.InvoiceItems.Sum(x => x.DiscountAmount);
                crInvoice.TotalAmount += crInvoice.TotalDeductAmount;
                if (crInvoice.TotalAmount == 0)
                {
                    crInvoice.IsPaid = true;
                }

                // Use credit note for refund reduction
                foreach (var invoiceItem in creditNoteInvoice.InvoiceItems.Where(x => x.Type == "rf"))
                {
                    crInvoice.CreditNoteBalance -= invoiceItem.DiscountAmount;
                    _db.InvoiceDeductTransactions.Add(new InvoiceDeductTransaction
                    {
                        InvoiceId = creditNoteInvoice.Id,
                        InvoiceCreditNoteId = creditNoteInvoice.Id,
                        Type = "u",
                        Amount = invoiceItem.DiscountAmount,
                        Balance = crInvoice.CreditNoteBalance
                    });
                }
                _db.SaveChanges();
            }
        }


        public Invoice AddCourses(Guid studentId, long termId, List<RegistrationCourse> registrationCourses, int addDropRound)
        {
            var invoice = AddInvoice(studentId, termId, registrationCourses, "au", addDropRound);
            var totalAmount = invoice.TotalAmount;
            var crInvoices = _db.Invoices.Where(x => x.Type == "cr"
                                                     && x.StudentId == studentId
                                                     && x.CreditNoteBalance > 0)
                                         .ToList();
            foreach (var crInvoice in crInvoices)
            {
                if (totalAmount == 0)
                {
                    break;
                }

                var amount = Math.Min(totalAmount, crInvoice.CreditNoteBalance);

                crInvoice.CreditNoteBalance -= amount;
                totalAmount -= amount;

                _db.InvoiceDeductTransactions.Add(new InvoiceDeductTransaction
                {
                    InvoiceId = invoice.Id,
                    InvoiceCreditNoteId = crInvoice.Id,
                    Type = "u",
                    Amount = amount,
                    Balance = crInvoice.CreditNoteBalance
                });
            }

            _db.SaveChanges();

            return invoice;
        }

        public void CancelAddDropInvoices(Guid studentId, long termId)
        {
            var invoices = _db.Invoices.Include(x => x.InvoiceItems)
                                       .Where(x => x.StudentId == studentId
                                                   && x.TermId == termId
                                                   && !x.IsPaid
                                                   && x.Type != "cr"
                                                   && x.Type != "o")
                                       .ToList();
            foreach (var invoice in invoices)
            {
                invoice.IsCancel = true;
                invoice.IsActive = false;
                foreach (var item in invoice.InvoiceItems)
                {
                    item.Type = "d";
                }

                var crInvoiceIds = _db.InvoiceDeductTransactions.Where(x => x.InvoiceId == invoice.Id)
                                                                .Select(x => x.InvoiceCreditNoteId)
                                                                .Distinct()
                                                                .ToList();
                if (crInvoiceIds != null && crInvoiceIds.Any())
                {
                    var crInvoices = _db.Invoices.Where(x => crInvoiceIds.Contains(x.Id))
                                                 .ToList();
                    foreach (var crInvoice in crInvoices)
                    {
                        crInvoice.IsCancel = true;
                        crInvoice.IsPaid = false;
                        crInvoice.IsActive = false;
                    }

                    var registrationCourseIds = _db.InvoiceItems.Where(x => crInvoiceIds.Contains(x.InvoiceId))
                                                                .Select(x => x.RegistrationCourseId)
                                                                .ToList();
                    _db.RegistrationCourses.Where(x => registrationCourseIds.Contains(x.Id))
                                           .ToList()
                                           .ForEach(x => x.Status = "a");

                    _db.InvoiceDeductTransactions.Where(x => crInvoiceIds.Contains(x.InvoiceCreditNoteId))
                                                 .ToList()
                                                 .ForEach(x => x.IsActive = false);
                }

                var receipts = _db.Receipts
                       .Where(x => x.InvoiceId == invoice.Id).ToList();
                var receiptIds = receipts.Select(x => x.Id).ToList();
                receipts.ForEach(x =>
                {
                    x.IsCancel = true;
                    x.IsActive = false;
                });

                var scholarShips = _db.FinancialTransactions.Where(x => x.ReceiptId.HasValue && receiptIds.Contains(x.ReceiptId.Value)).ToList();
                scholarShips.ForEach(x => x.UsedScholarship = 0m);
            }

            _db.SaveChanges();
        }

        public void CancelInvoicesWithUpdateRegistrationCourse(List<Guid> studentIdList, long termId)
        {
            using (var transaction = _db.Database.BeginTransaction())
            {
                foreach (var studentId in studentIdList)
                {
                    _registrationProvider.DeleteAllNotPaidCourse(studentId, termId);
                    CancelAddDropInvoices(studentId, termId);
                }
                transaction.Commit();
            }
        }

        public LateRegistrationConfiguration GetConfigLateRegistration(long termId)
        {
            var term = _db.Terms.SingleOrDefault(x => x.Id == termId);
            var lateRegistration = _db.LateRegistrationConfigurations.Include(x => x.FromTerm)
                                                                     .Include(x => x.ToTerm)
                                                                     .FirstOrDefault(x => string.Compare(term.TermCompare, x.FromTerm.TermCompare) >= 0
                                                                                          && ((x.ToTermId ?? 0) == 0 || string.Compare(term.TermCompare, x.ToTerm.TermCompare) <= 0));
            return lateRegistration;
        }

        public AddDropFeeConfiguration GetConfigAddDropFeeCount(long termId)
        {
            var term = _db.Terms.SingleOrDefault(x => x.Id == termId);
            var addDropFeeCount = _db.AddDropFeeConfigurations.Include(x => x.FromTerm)
                                                              .Include(x => x.ToTerm)
                                                              .FirstOrDefault(x => string.Compare(term.TermCompare, x.FromTerm.TermCompare) >= 0
                                                                                   && ((x.ToTermId ?? 0) == 0 || string.Compare(term.TermCompare, x.ToTerm.TermCompare) <= 0));
            return addDropFeeCount;
        }

        public LatePaymentConfiguration GetConfigLatePayment(long termId)
        {
            var term = _db.Terms.SingleOrDefault(x => x.Id == termId);
            var latePaymentConfiguration = _db.LatePaymentConfigurations.Include(x => x.FromTerm)
                                                                        .Include(x => x.ToTerm)
                                                                        .FirstOrDefault(x => string.Compare(term.TermCompare, x.FromTerm.TermCompare) >= 0
                                                                                            && ((x.ToTermId ?? 0) == 0 || string.Compare(term.TermCompare, x.ToTerm.TermCompare) <= 0));
            return latePaymentConfiguration;
        }

        public void SimulateUpdateTuitionFeeRefund(Invoice dropInvoice, Invoice addInvoice)
        {
            CalculateRefundReduction(dropInvoice.InvoiceItems, addInvoice.InvoiceItems);
            dropInvoice.Amount = dropInvoice.InvoiceItems.Sum(x => x.Amount);
            dropInvoice.TotalAmount = dropInvoice.InvoiceItems.Sum(x => x.TotalAmount);
        }

        public void UpdateTuitionFeeRefund(Invoice dropInvoice, Invoice addInvoice)
        {
            CalculateRefundReduction(dropInvoice.InvoiceItems, addInvoice.InvoiceItems);
            dropInvoice.Amount = dropInvoice.InvoiceItems.Sum(x => x.Amount);
            dropInvoice.TotalAmount = dropInvoice.InvoiceItems.Sum(x => x.TotalAmount);
            _db.SaveChanges();
        }

        public void CalculateRefundReduction(List<InvoiceItem> dropInvoiceItems, List<InvoiceItem> addInvoiceItems)
        {
            var addAmount = addInvoiceItems == null ? 0 : addInvoiceItems.Sum(x => x.Amount);
            var rate = _configurationProvider.Get<decimal>("TuitionFeeRefundPercent");
            var refundAmount = (dropInvoiceItems.Sum(x => x.Amount) - addAmount) * (100 - rate) / 100;
            var tuitionFeeItem = _db.FeeItems.SingleOrDefault(x => x.Code == "002");
            if (refundAmount > 0)
            {
                dropInvoiceItems.Add(new InvoiceItem()
                {
                    Type = "rf",
                    Amount = -refundAmount,
                    TotalAmount = -refundAmount,
                    FeeItemId = tuitionFeeItem.Id,
                    FeeItem = tuitionFeeItem,
                    IsPaid = true
                });
            }
        }

        public InvoiceItem CalculateRefundReduction(List<InvoiceItem> invoiceItems)
        {
            var addAmount = invoiceItems == null ? 0 : invoiceItems.Where(x => x.Amount > 0).Sum(x => x.Amount);
            var rate = _configurationProvider.Get<decimal>("TuitionFeeRefundPercent");
            var refundAmount = (invoiceItems.Where(x => x.Amount < 0).Sum(x => -x.Amount) - addAmount) * (100 - rate) / 100;
            var tuitionFeeItem = _db.FeeItems.SingleOrDefault(x => x.Code == "002");
            if (refundAmount > 0)
            {
                return new InvoiceItem()
                {
                    Type = "rf",
                    Amount = refundAmount,
                    TotalAmount = refundAmount,
                    FeeItem = tuitionFeeItem,
                    FeeItemId = tuitionFeeItem.Id,
                    FeeItemName = tuitionFeeItem.NameEn,
                    IsPaid = true
                };
            }
            return null;
        }

        private InvoiceItem GenerateLateRegistrationItem(Guid studentId, long termId)
        {
            DateTime date = new DateTime(2022, 01, 10);
            var registrationCourse = _db.RegistrationCourses
                .IgnoreQueryFilters() //temp fix to let it check those RegistrationCourses that we mark is active = 0 in the great 13 Jan Purge
                .Any(x => x.TermId == termId
                    && x.StudentId == studentId
                    && x.CreatedAt < date);

            var invoice = _db.Invoices.Any(x => x.TermId == termId
                                                && x.StudentId == studentId
                                                && x.Type != "o"
                                                && x.IsPaid);

            if (!registrationCourse && !invoice)
            {
                var lateRegistrationConfig = GetConfigLateRegistration(termId);
                var value = _configurationProvider.Get<long>("LateRegistrationFeeItem");
                var lateRegistration = _db.FeeItems.SingleOrDefault(x => x.Id == value);

                return new InvoiceItem()
                {
                    FeeItemId = lateRegistration.Id,
                    FeeItemName = lateRegistration.NameEn,
                    Amount = lateRegistrationConfig.Amount,
                    ScholarshipAmount = 0,
                    TotalAmount = lateRegistrationConfig.Amount,
                };
            }
            return null;
        }

        public void SimulateGenerateAddDropFeeItem(Invoice invoice, long termId, int addDropRound)
        {
            var addDropFeeCount = GetConfigAddDropFeeCount(termId);
            if (addDropFeeCount != null && addDropRound > addDropFeeCount.FreeAddDropCount)
            {
                var value = _configurationProvider.Get<long>("AddDropFeeItem");
                var addDropFee = _db.FeeItems.SingleOrDefault(x => x.Id == value);
                if (addDropFee != null)
                {
                    var invoiceItem = new InvoiceItem()
                    {
                        FeeItemId = addDropFee.Id,
                        FeeItemName = addDropFee.NameEn,
                        Amount = addDropFeeCount.Amount,
                        ScholarshipAmount = 0,
                        TotalAmount = addDropFeeCount.Amount,
                        Type = "a"
                    };
                    invoice.InvoiceItems.Add(invoiceItem);
                    invoice.Amount = invoice.InvoiceItems.Sum(x => x.Amount);
                    invoice.TotalAmount = invoice.InvoiceItems.Sum(x => x.TotalAmount);
                }
            }
        }

        public void GenerateAddDropFeeItem(Invoice invoice, long termId, int addDropRound)
        {
            var addDropFeeCount = GetConfigAddDropFeeCount(termId);
            if (addDropFeeCount != null && addDropRound > addDropFeeCount.FreeAddDropCount)
            {
                var value = _configurationProvider.Get<long>("AddDropFeeItem");
                var addDropFee = _db.FeeItems.SingleOrDefault(x => x.Id == value);
                if (addDropFee != null)
                {
                    var invoiceItem = new InvoiceItem()
                    {
                        FeeItemId = addDropFee.Id,
                        FeeItemName = addDropFee.NameEn,
                        Amount = addDropFeeCount.Amount,
                        ScholarshipAmount = 0,
                        TotalAmount = addDropFeeCount.Amount,
                        Type = "a"
                    };
                    invoice.InvoiceItems.Add(invoiceItem);
                    invoice.Amount = invoice.InvoiceItems.Sum(x => x.Amount);
                    invoice.TotalAmount = invoice.InvoiceItems.Sum(x => x.TotalAmount);
                    _db.SaveChanges();
                }
            }
        }

        public void SimulateGenerateLateRegistrationFeeItem(Invoice invoice, Guid studentId, long termId)
        {
            var lateRegisrationFeeItem = GenerateLateRegistrationItem(studentId, termId);
            if (lateRegisrationFeeItem != null)
            {
                var invoiceItem = new InvoiceItem()
                {
                    FeeItemId = lateRegisrationFeeItem.FeeItemId,
                    FeeItemName = lateRegisrationFeeItem.FeeItemName,
                    Amount = lateRegisrationFeeItem.Amount,
                    ScholarshipAmount = 0,
                    TotalAmount = lateRegisrationFeeItem.Amount,
                    Type = "a"
                };
                invoice.InvoiceItems.Add(invoiceItem);
                invoice.Amount = invoice.InvoiceItems.Sum(x => x.Amount);
                invoice.TotalAmount = invoice.InvoiceItems.Sum(x => x.TotalAmount);
            }
        }

        public void GenerateLateRegistrationFeeItem(Invoice invoice, Guid studentId, long termId)
        {
            var lateRegisrationFeeItem = GenerateLateRegistrationItem(studentId, termId);
            if (lateRegisrationFeeItem != null)
            {
                var invoiceItem = new InvoiceItem()
                {
                    FeeItemId = lateRegisrationFeeItem.FeeItemId,
                    FeeItemName = lateRegisrationFeeItem.FeeItemName,
                    Amount = lateRegisrationFeeItem.Amount,
                    ScholarshipAmount = 0,
                    TotalAmount = lateRegisrationFeeItem.Amount,
                    Type = "a"
                };
                invoice.InvoiceItems.Add(invoiceItem);
                invoice.Amount = invoice.InvoiceItems.Sum(x => x.Amount);
                invoice.TotalAmount = invoice.InvoiceItems.Sum(x => x.TotalAmount);
                _db.SaveChanges();
            }
        }

        public InvoiceItem GenerateAddDropFeeItem(long termId, int addDropRound)
        {
            var addDropFeeCount = GetConfigAddDropFeeCount(termId);
            if (addDropFeeCount != null && addDropRound > addDropFeeCount.FreeAddDropCount)
            {
                var value = _configurationProvider.Get<long>("AddDropFeeItem");
                var addDropFee = _db.FeeItems.SingleOrDefault(x => x.Id == value);
                if (addDropFee != null)
                {
                    return new InvoiceItem()
                    {
                        FeeItemId = addDropFee.Id,
                        FeeItemName = addDropFee.NameEn,
                        Amount = addDropFeeCount.Amount,
                        ScholarshipAmount = 0,
                        TotalAmount = addDropFeeCount.Amount,
                        Type = "a"
                    };
                }
            }
            return null;
        }
        public void SimulateGenerateLatePaymentFeeItem(Invoice invoice, long termId, string type, DateTime today)
        {
            var latePaymentFeeItem = GenerateLatePaymentFeeItem(termId, type, today);
            if (latePaymentFeeItem != null)
            {
                var invoiceItem = new InvoiceItem()
                {
                    FeeItemId = latePaymentFeeItem.FeeItemId,
                    FeeItemName = latePaymentFeeItem.FeeItemName,
                    Amount = latePaymentFeeItem.Amount,
                    ScholarshipAmount = 0,
                    TotalAmount = latePaymentFeeItem.Amount,
                    Type = "a"
                };
                invoice.InvoiceItems.Add(invoiceItem);
                invoice.Amount = invoice.InvoiceItems.Sum(x => x.Amount);
                invoice.TotalAmount = invoice.InvoiceItems.Sum(x => x.TotalAmount);
            }
        }

        public void GenerateLatePaymentFeeItem(Invoice invoice, long termId, string type, DateTime today)
        {
            var latePaymentFeeItem = GenerateLatePaymentFeeItem(termId, type, today);
            if (latePaymentFeeItem != null)
            {
                var invoiceItem = new InvoiceItem()
                {
                    FeeItemId = latePaymentFeeItem.FeeItemId,
                    FeeItemName = latePaymentFeeItem.FeeItemName,
                    Amount = latePaymentFeeItem.Amount,
                    ScholarshipAmount = 0,
                    TotalAmount = latePaymentFeeItem.Amount,
                    Type = "a"
                };
                invoice.InvoiceItems.Add(invoiceItem);
                invoice.Amount = invoice.InvoiceItems.Sum(x => x.Amount);
                invoice.TotalAmount = invoice.InvoiceItems.Sum(x => x.TotalAmount);
                _db.SaveChanges();
            }
        }

        public bool IsLumpsumPayment(Guid studentId)
        {
            var student = _db.Students.SingleOrDefault(x => x.Id == studentId);
            return student.StudentFeeGroup?.IsLumpsumPayment ?? false;
        }

        private InvoiceItem GenerateLatePaymentFeeItem(long termId, string type, DateTime today)
        {
            DateTime regisPaymentDue = new DateTime(2022, 01, 14);
            DateTime addDropPaymentDue = new DateTime(2022, 01, 21);

            var latePaymentConfig = GetConfigLatePayment(termId);
            var value = _configurationProvider.Get<long>("LatePaymentFeeItem");
            var latePaymentFee = _db.FeeItems.SingleOrDefault(x => x.Id == value);
            DateTime compareDate = type == "r" ? regisPaymentDue : addDropPaymentDue;
            var totalDays = today.Subtract(compareDate).Days;

            if (totalDays > 0)
            {
                if (latePaymentConfig.MaximumDays < totalDays)
                {
                    totalDays = latePaymentConfig.MaximumDays;
                }

                var amount = latePaymentConfig.AmountPerDay * totalDays;
                return new InvoiceItem()
                {
                    FeeItemId = latePaymentFee.Id,
                    FeeItemName = latePaymentFee.NameEn,
                    Amount = amount,
                    ScholarshipAmount = 0,
                    TotalAmount = amount,
                    Type = "a"
                };
            }
            return null;
        }

        public void UpdateInvoiceReference(Invoice invoice, string studentCode)
        {
            DateTime firstRegis = new DateTime(2022, 01, 14); // 14
            DateTime addDrop = new DateTime(2022, 01, 21); // 21
            invoice.PaymentExpireAt = invoice.Type == "r" ? firstRegis : addDrop;

            if (invoice.PaymentExpireAt < DateTime.Today)
            {
                //TODO: FIx This same as AddInvoice V3
                var term = _db.Terms.AsNoTracking()
                     .SingleOrDefault(x => x.Id == invoice.TermId);
                if (term != null)
                {
                    var expiryDate = term.LastPaymentEndedAt ?? DateTime.UtcNow.EndOfDay(-1);
                    invoice.PaymentExpireAt = expiryDate;
                }
                else
                {
                    invoice.PaymentExpireAt = new DateTime(2022, 4, 3);
                }
            }
            invoice.Reference2 = _feeProvider.GenerateInvoiceReference2(invoice.CreatedAt, invoice.PaymentExpireAt.Value);
            string studentCodeValue = studentCode.Substring(0, 2) + studentCode.Substring(3, 4); // Ignore third digit
            invoice.Reference1 = _feeProvider.GenerateInvoiceReference1(studentCodeValue + invoice.Number, invoice.Reference2, Math.Abs(invoice.TotalAmount));
            _db.SaveChanges();
        }

        public void Checkout(Guid studentId, long termId, List<RegistrationCourse> newCourses, List<RegistrationCourse> deletePaidCourses)
        {
            var studentState = _db.StudentStates.SingleOrDefault(x => x.StudentId == studentId && x.TermId == termId);
            //string round = studentState != null && (studentState.State == "ADD" || studentState.State == "PAY_ADD") ? "a" : "r";
            string round = studentState != null && (studentState.State != "REG") && (studentState.State != "PAY_REG") ? "a" : "r";

            if (newCourses.Any() || deletePaidCourses.Any())
            {
                var invoiceItems = GetInvoiceItemsForPreview(newCourses, 0);
                bool isSame = IsSameTermAndTuitionFee(studentId, termId, invoiceItems);
                DateTime today = DateTime.Today;
                var latestInvoice = _db.Invoices.FirstOrDefault(x => x.StudentId == studentId
                                                                        && x.TermId == termId
                                                                        && !x.IsPaid
                                                                        && x.Type != "o"
                                                                        && x.Type != "cr");

                if (!isSame || (latestInvoice != null && latestInvoice.PaymentExpireAt < today))
                {
                    var addInvoice = new Invoice();
                    var dropInvoice = new Invoice();

                    if (newCourses.Any())
                    {
                        // Create invoice
                        addInvoice = AddInvoice(studentId, termId, newCourses, round);
                    }

                    if (deletePaidCourses.Any())
                    {
                        dropInvoice = DropCourses(studentId, termId, deletePaidCourses, 0);

                        // If there is refund, need to deduct refund percent
                        if (dropInvoice != null && dropInvoice.InvoiceItems.Sum(x => x.Amount) > (addInvoice.InvoiceItems == null ? 0 : addInvoice.InvoiceItems.Sum(x => x.Amount)))
                        {
                            UpdateTuitionFeeRefund(dropInvoice, addInvoice);
                        }

                        if (addInvoice.InvoiceItems == null)
                        {
                            addInvoice.InvoiceItems = new List<InvoiceItem>();
                        }

                        foreach (var item in dropInvoice.InvoiceItems)
                        {
                            item.Amount = -item.Amount;
                            item.TotalAmount = -item.TotalAmount;
                            //addInvoice.InvoiceItems.Add(item);
                        }
                        dropInvoice.Amount = dropInvoice.InvoiceItems.Sum(x => x.Amount);
                        dropInvoice.TotalAmount = dropInvoice.InvoiceItems.Sum(x => x.TotalAmount);
                        _db.SaveChanges();
                    }
                    try
                    {
                        var student = _db.Students.SingleOrDefault(x => x.Id == studentId);
                        var studentAdmission = _db.AdmissionInformations.Include(x => x.AdmissionType)
                                                                        .FirstOrDefault(x => x.StudentId == student.Id);
                        bool IsSkipPenaltyFee = false;
                        if (studentAdmission != null)
                        {
                            IsSkipPenaltyFee = studentAdmission.AdmissionType.NameEn.Contains("inbound");
                        }

                        if (!IsSkipPenaltyFee)
                        {
                            if (addInvoice.Id > 0)
                            {
                                // Check Late Registration                
                                GenerateLateRegistrationFeeItem(addInvoice, studentId, termId);

                                // Check Add/Drop fee
                                GenerateAddDropFeeItem(addInvoice, termId, 0);

                                // Late Payment 
                                GenerateLatePaymentFeeItem(addInvoice, termId, addInvoice.Type, today);
                            }
                            else if (dropInvoice.Id > 0)
                            {
                                // Check Late Registration                
                                GenerateLateRegistrationFeeItem(dropInvoice, studentId, termId);

                                // Check Add/Drop fee
                                GenerateAddDropFeeItem(dropInvoice, termId, 0);

                                //no late payment fee when registra do the job and it's a drop only
                                //// Late Payment 
                                //GenerateLatePaymentFeeItem(dropInvoice, termId, addInvoice.Type, today);
                            }
                        }
                        if (addInvoice.Id > 0 && addInvoice.InvoiceItems != null && dropInvoice != null && dropInvoice.InvoiceItems != null && dropInvoice.InvoiceItems.Any())
                        {
                            ApplyCreditNote(addInvoice, dropInvoice);
                        }

                        if (addInvoice.Id > 0)
                        {
                            UpdateInvoiceReference(addInvoice, student.Code);
                        }
                        else if (dropInvoice.Id > 0)
                        {
                            UpdateInvoiceReference(dropInvoice, student.Code);
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
            }
        }

        private bool IsSameTermAndTuitionFee(Guid studentId, long termId, List<InvoiceItem> invoiceItems)
        {
            var invoice = _db.Invoices.Include(x => x.InvoiceItems)
                                      .FirstOrDefault(x => !x.IsCancel
                                                           && x.StudentId == studentId
                                                           && x.TermId == termId
                                                           && !x.IsPaid
                                                           && x.Type != "o"
                                                           && x.Type != "cr");

            bool same = invoice != null && invoice.InvoiceItems.Any();
            if (same)
            {
                foreach (var item in invoiceItems)
                {
                    if (!item.SectionId.HasValue || item.SectionId == 0)
                    {
                        continue;
                    }
                    if (!invoice.InvoiceItems.Any(x => (x.CourseId ?? 0) == (item.CourseId ?? 0)
                                                        && (x.SectionId ?? 0) == (item.SectionId ?? 0)
                                                        && x.FeeItemId == item.FeeItemId
                                                        && x.TotalAmount == item.Amount))
                    {
                        same = false;
                        break;
                    }
                }

                if (same)
                {
                    foreach (var item in invoice.InvoiceItems)
                    {
                        if (!item.SectionId.HasValue || item.SectionId == 0)
                        {
                            continue;
                        }
                        if (!invoiceItems.Any(x => (x.CourseId ?? 0) == (item.CourseId ?? 0)
                                                    && (x.SectionId ?? 0) == (item.SectionId ?? 0)
                                                    && x.FeeItemId == item.FeeItemId
                                                    && x.Amount == item.TotalAmount))
                        {
                            same = false;
                            break;
                        }
                    }
                }
            }
            return same;
        }

        private List<AddingViewModel> GenerateAddingResults(long termId, List<RegistrationCourse> registrationCourses)
        {
            var addingResults = new List<AddingViewModel>();
            foreach (var registrationCourse in registrationCourses)
            {
                var addingResult = new AddingViewModel();
                addingResult.TermId = termId;
                addingResult.CourseId = registrationCourse.CourseId;
                addingResult.SectionId = registrationCourse.SectionId.Value;
                addingResults.Add(addingResult);
            }
            return addingResults;
        }

        public void RegenerateQRCode()
        {
            var invoices = _db.Invoices.Include(x => x.Student)
                                       .Where(x => x.Id == 7910).ToList();
            DateTime firstRegis = new DateTime(2022, 01, 14);
            DateTime addDrop = new DateTime(2022, 01, 21);
            int total = invoices.Count();
            int i = 1;
            foreach (var invoice in invoices)
            {
                invoice.PaymentExpireAt = invoice.Type == "r" ? firstRegis : addDrop;
                if (invoice.PaymentExpireAt < DateTime.Today)
                {
                    invoice.PaymentExpireAt = DateTime.Today;
                }
                invoice.Reference2 = _feeProvider.GenerateInvoiceReference2(invoice.CreatedAt, invoice.PaymentExpireAt.Value);
                string studentCode = invoice.Student.Code.Substring(0, 2) + invoice.Student.Code.Substring(3, 4); // Ignore third digit
                invoice.Reference1 = _feeProvider.GenerateInvoiceReference1(studentCode + invoice.Number, invoice.Reference2, invoice.TotalAmount);
                _db.SaveChanges();
                Console.WriteLine(++i + "/" + total);
            }
        }

        public async Task<USparkOrder> CheckoutUSparkInvoiceV3Async(string studentCode, long KSTermId, IEnumerable<InvoiceItem> invoiceItems)
        {
            var invoice = await AddInvoiceV3Async(studentCode, KSTermId, invoiceItems);

            var feeItems = _db.FeeItems.AsNoTracking()
                                       .ToList();

            var response = new USparkOrder
            {
                KSInvoiceID = invoice.Id,
                StudentCode = studentCode,
                KSTermID = KSTermId,
                TotalAmount = invoice.TotalAmount,
                Reference1 = invoice.Reference1,
                Reference2 = invoice.Reference2,
                Number = invoice.Number,
                IsPaid = invoice.IsPaid,
                InvoiceExpiryDate = invoice.PaymentExpireAt.HasValue ? invoice.PaymentExpireAt.Value : default,
                CreatedAt = invoice.CreatedAt,
                OrderDetails = (from item in invoice.InvoiceItems
                                    // invoice item type "CR" = drop course item
                                let itemNameHeader = item.Type == "cr" ? "Refund " : string.Empty
                                join feeItem in feeItems on item.FeeItemId equals feeItem.Id
                                select new USparkOrderDetail
                                {
                                    ItemCode = feeItem.Code,
                                    ItemNameEn = $"{itemNameHeader}{feeItem?.NameEn} {item.CourseAndCredit}",
                                    ItemNameTh = $"{itemNameHeader}{feeItem?.NameTh} {item.CourseAndCredit}",
                                    KSRegistrationCourseId = item.RegistrationCourseId,
                                    KSCourseId = item.CourseId,
                                    KSSectionId = item.SectionId,
                                    Amount = item.TotalAmount,
                                    LumpSumAddDrop = item.LumpSumAddDrop
                                }).ToList()
            };

            return response;
        }

        public async Task<Invoice> AddInvoiceV3Async(string studentCode, long KSTermId, IEnumerable<InvoiceItem> invoiceItems)
        {
            if (invoiceItems == null || !invoiceItems.Any())
            {
                throw new ArgumentException(nameof(invoiceItems));
            }

            var student = _db.Students.AsNoTracking()
                                      .SingleOrDefault(x => x.Code == studentCode);

            var term = _db.Terms.AsNoTracking()
                                .SingleOrDefault(x => x.Id == KSTermId);

            if (student == null)
            {
                throw new ArgumentNullException(nameof(student));
            }

            if (term == null)
            {
                throw new ArgumentNullException(nameof(term));
            }

            var lastInvoice = _db.Invoices.AsNoTracking()
                                          .Where(x => x.Year == DateTime.UtcNow.Year)
                                          .IgnoreQueryFilters()
                                          .OrderByDescending(x => x.RunningNumber)
                                          .FirstOrDefault();

            var runningNumber = lastInvoice == null ? 1
                                                    : lastInvoice.RunningNumber + 1;

            var invoiceNumber = GetFeeInvoiceNumber(runningNumber);

            var totalAmount = invoiceItems.Sum(x => x.TotalAmount);

            // TODO : Expiry Date logic

            // IF INVOICE HAS LATE PAYMENT ONLY ALLOW TIL TODAY
            var latePaymentInvoiceItems = invoiceItems.SingleOrDefault(x => x.Type == "latepayment_fee");

            var registrationLogs = _db.RegistrationLogs.AsNoTracking()
                                                       .Where(x => x.StudentId == student.Id
                                                                   && x.TermId == term.Id)
                                                       .ToList();

            var studentInvoices = _db.Invoices.AsNoTracking()
                                              .Where(x => !x.IsCancel
                                                          && x.StudentId == student.Id
                                                          && x.TermId == term.Id
                                                          && x.Type != "o")
                                              .ToList();

            var isLateRegis = !registrationLogs.Any(x => x.CreatedAt < (term.FirstRegistrationEndedAt ?? DateTime.UtcNow.EndOfDay(-1)));

            var expiryDate = latePaymentInvoiceItems != null ? latePaymentInvoiceItems.LatePaymentExpiryDate.Value
                                                             : isLateRegis || studentInvoices.Any(x => x.IsPaid) ? term.AddDropPaymentEndedAt ?? DateTime.UtcNow.EndOfDay(-1)
                                                                                                                 : term.FirstRegistrationPaymentEndedAt ?? DateTime.UtcNow.EndOfDay(-1);

            var reference2 = GenerateInvoiceReference2(DateTime.UtcNow, expiryDate);
            var reference1 = GenerateInvoiceReference1(studentCode, invoiceNumber, totalAmount, reference2);

            var now = DateTime.UtcNow;

            var invoice = new Invoice
            {
                StudentId = student.Id,
                TermId = term.Id,
                Number = GetFeeInvoiceNumber(runningNumber),
                RunningNumber = runningNumber,
                Reference1 = reference1,
                Reference2 = reference2,
                Year = now.Year,
                Month = now.Month,
                Name = student.FullNameEn,
                IsConfirm = true, // NEW FLOW CONFIRM ON CHECKOUT
                IsCancel = false,
                IsPaid = false,
                IsActive = true,
                Amount = invoiceItems.Sum(x => x.Amount),
                TotalDiscount = decimal.Zero,
                TotalAmount = invoiceItems.Sum(x => x.TotalAmount),
                TotalItemsScholarshipAmount = invoiceItems.Where(x => x.ScholarshipStudentId.HasValue).Sum(x => x.ScholarshipAmount),
                Type = studentInvoices.Any(x => x.IsPaid) ? "a" : "r",
                CreatedAt = now,
                UpdatedAt = now,
                PaymentExpireAt = expiryDate
            };

            var registrationCourseIds = invoiceItems.Where(x => x.RegistrationCourseId.HasValue)
                                                    .Select(x => x.RegistrationCourseId)
                                                    .ToList();

            var registrationCourses = _db.RegistrationCourses.Where(x => registrationCourseIds.Contains(x.Id))
                                                             .ToList();

            if (invoice.TotalAmount <= decimal.Zero)
            {
                invoice.Type = "cr";

                foreach (var registration in registrationCourses)
                {
                    registration.IsPaid = true;
                }
            }

            using (var transaction = _db.Database.BeginTransaction())
            {
                _db.Invoices.Add(invoice);

                foreach (var invoiceItem in invoiceItems)
                {
                    invoiceItem.InvoiceId = invoice.Id;
                }

                _db.InvoiceItems.AddRange(invoiceItems);

                transaction.Commit();
            }

            _db.SaveChanges();

            if (invoice.TotalAmount <= decimal.Zero)
            {
                var mockCallback = new InvoiceUpdateIsPaidViewModel
                {
                    OrderId = invoice.Number,
                    Reference1 = invoice.Reference1,
                    Reference2 = invoice.Reference2,
                    Reference3 = invoice.Reference3,
                    IsPaymentSuccess = false,
                    Amount = invoice.TotalAmount,
                    TransactionId = "AUTOMATED",
                    TransactionDateTime = DateTime.UtcNow.ToString("s"),
                    RawResponseMessage = "MANUAL TOTAL AMOUNT LESS THAN ZERO"
                };

                await ProcessInvoicePaymentAsync(mockCallback);
            }

            return invoice;
        }

        public void DeleteInvoice(long invoiceId, bool isActive = false, string remark = "")
        {
            var model = GetInvoice(invoiceId);
            if (model == null)
            {
                throw new Exception(Message.DataNotFound);
            }

            if (model.IsPaid || model.TotalAmount <= 0)
            {
                throw new Exception(Message.UnableToDelete);
            }

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    model.IsCancel = true;
                    model.IsActive = isActive;
                    model.CancelRemark = remark;

                    var receipts = _db.Receipts
                        .Where(x => x.InvoiceId == model.Id).ToList();
                    var receiptIds = receipts.Select(x => x.Id).ToList();
                    receipts.ForEach(x =>
                    {
                        x.IsCancel = true;
                        x.IsActive = false;
                    });

                    var scholarShips = _db.FinancialTransactions.Where(x => x.ReceiptId.HasValue && receiptIds.Contains(x.ReceiptId.Value)).ToList();
                    scholarShips.ForEach(x => x.UsedScholarship = 0m);

                    _db.SaveChanges();
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw new Exception(Message.UnableToDelete);
                }
            }
        }

        public bool MarkCancelExpireInvoice(string studentCode, long KSTermId)
        {
            var now = DateTime.UtcNow;

            var student = _db.Students.AsNoTracking()
                          .SingleOrDefault(x => x.Code == studentCode);

            var term = _db.Terms.AsNoTracking()
                                .SingleOrDefault(x => x.Id == KSTermId);

            if (student == null)
            {
                throw new ArgumentNullException(nameof(student));
            }

            if (term == null)
            {
                throw new ArgumentNullException(nameof(term));
            }

            // OVER LAST PAYMENT DATE DO NOTHING
            if (!term.LastPaymentEndedAt.HasValue || now > (term.LastPaymentEndedAt ?? DateTime.UtcNow.EndOfDay(-1)))
            {
                return false;
            }

            // CHECK LAST NON-CANCEL INVOICE
            var invoice = _db.Invoices.Where(x => x.StudentId == student.Id
                                                  && x.TermId == term.Id
                                                  && !x.IsCancel
                                                  && x.Type != "o")
                                      .OrderByDescending(x => x.CreatedAt)
                                      .FirstOrDefault();

            if (invoice == null || invoice.IsPaid || invoice.TotalAmount <= decimal.Zero || invoice.PaymentExpireAt > now)
            {
                return false;
            }

            //using (var transaction = _db.Database.BeginTransaction())
            //{
            //    invoice.IsCancel = true;
            //    invoice.UpdatedAt = now;
            //    invoice.UpdatedBy = "EXPIRED INVOICE";

            //    transaction.Commit();
            //}

            //_db.SaveChanges();

            DeleteInvoice(invoice.Id);

            return true;
        }

        public IEnumerable<USparkOrder> GetStudentNonCancelInvoices(string studentCode)
        {
            var student = _db.Students.AsNoTracking()
                                      .SingleOrDefault(x => x.Code == studentCode);

            if (student == null)
            {
                throw new ArgumentNullException(nameof(student));
            }

            var invoicesQuery = _db.Invoices.AsNoTracking()
                                            .IgnoreQueryFilters()
                                            .Include(x => x.InvoiceItems)
                                            .ThenInclude(x => x.Course)
                                            .Where(x => x.StudentId == student.Id
                                                        && !x.IsCancel
                                                        && x.Type != "o");

            var invoices = invoicesQuery.ToList();

            var feeItems = _db.FeeItems.AsNoTracking()
                                       .ToList();


            var sectionIds = invoices.SelectMany(x => x.InvoiceItems)
                                     .Where(x => x.SectionId.HasValue)
                                     .Select(x => x.SectionId.Value)
                                     .Distinct()
                                     .ToList();

            var sections = _db.Sections.AsNoTracking()
                                       .Where(x => sectionIds.Contains(x.Id))
                                       .ToList();


            var response = (from invoice in invoices
                            orderby invoice.CreatedAt descending
                            select new USparkOrder
                            {
                                KSInvoiceID = invoice.Id,
                                KSTermID = invoice.TermId ?? 0,
                                StudentCode = student.Code,
                                Number = invoice.Number,
                                Reference1 = invoice.Reference1,
                                Reference2 = invoice.Reference2,
                                IsPaid = invoice.IsPaid || invoice.Amount <= 0,
                                TotalAmount = invoice.TotalAmount,
                                InvoiceExpiryDate = invoice.PaymentExpireAt ?? new DateTime(),
                                CreatedAt = invoice.CreatedAt,
                                OrderDetails = (from item in invoice.InvoiceItems
                                                let matchingItems = feeItems.SingleOrDefault(x => x.Id == item.FeeItemId)
                                                let sectionNumber = !item.SectionId.HasValue ? string.Empty : $"[{sections.SingleOrDefault(x => x.Id == item.SectionId.Value)?.Number ?? string.Empty}]"
                                                select new USparkOrderDetail
                                                {
                                                    ItemCode = matchingItems?.Code,
                                                    ItemNameEn = (item.ScholarshipStudentId.HasValue ? $"Scholarship for " : string.Empty) +
                                                                 (item.RegistrationCourseId.HasValue ? $"{matchingItems?.NameEn} {item.Course.CodeAndCredit} {sectionNumber}"
                                                                                                     : $"{matchingItems?.NameEn}"),
                                                    ItemNameTh = (item.ScholarshipStudentId.HasValue ? $"ทุนสำหรับ" : string.Empty) +
                                                                 (item.RegistrationCourseId.HasValue ? $"{matchingItems?.NameTh} {item.Course.CodeAndCredit} {sectionNumber}"
                                                                                                     : $"{matchingItems?.NameTh}"),
                                                    KSCourseId = item.CourseId,
                                                    KSSectionId = item.SectionId,
                                                    KSRegistrationCourseId = item.RegistrationCourseId,
                                                    Amount = item.TotalAmount,
                                                    LumpSumAddDrop = item.LumpSumAddDrop
                                                }).ToList()

                            }).ToList();

            return response;
        }

        public USparkOrder GetNonCancelStudentInvoiceById(string studentCode, long invoiceId)
        {
            var student = _db.Students.AsNoTracking()
                                      .Include(x => x.AcademicInformation)
                                      .SingleOrDefault(x => x.Code == studentCode);

            if (student == null)
            {
                throw new ArgumentNullException(nameof(student));
            }

            var invoice = _db.Invoices.AsNoTracking()
                                      .IgnoreQueryFilters()
                                      .Include(x => x.Term)
                                      .Include(x => x.InvoiceItems)
                                      .ThenInclude(x => x.Course)
                                      .SingleOrDefault(x => !x.IsCancel
                                                            && x.StudentId == student.Id
                                                            && x.Id == invoiceId);

            if (invoice == null)
            {
                throw new ArgumentNullException(nameof(invoice));
            }

            var utcNow = DateTime.UtcNow;

            var registrationTerms = _db.RegistrationTerms.AsNoTracking()
                                                         .Where(x => x.TermId == invoice.TermId)
                                                         .ToList();

            var registrationTermIds = registrationTerms.Select(x => x.Id).ToList();

            var termPaymentSlots = !registrationTermIds.Any() ? Enumerable.Empty<Slot>()
                                                              : _db.Slots.AsNoTracking()
                                                                .Include(x => x.RegistrationSlotConditions)
                                                                .Where(x => registrationTermIds.Contains(x.RegistrationTermId)
                                                                            && x.Type == "p"
                                                                            && x.StartedAt < utcNow
                                                                            && utcNow < x.EndedAt)
                                                                .ToList();

            var slotConditionIds = !termPaymentSlots.Any() ? Enumerable.Empty<long>()
                                                       : termPaymentSlots.SelectMany(x => x.RegistrationSlotConditions)
                                                                     .Select(x => x.RegistrationConditionId)
                                                                     .Distinct()
                                                                     .ToList();

            var slotConditions = !slotConditionIds.Any() ? Enumerable.Empty<RegistrationCondition>()
                                                         : _db.RegistrationConditions.AsNoTracking()
                                                                                     .Where(x => slotConditionIds.Contains(x.Id))
                                                                                     .ToList();

            var activeSlot = (from slot in termPaymentSlots
                              let conditionIds = slot.RegistrationSlotConditions.Select(x => x.RegistrationConditionId)
                                                                                .Distinct()
                                                                                .ToList()
                              let conditions = slotConditions.Where(x => conditionIds.Contains(x.Id)).ToList()
                              where IsSlotAvailableForStudent(student, student.AcademicInformation, conditions)
                              orderby slot.StartedAt
                              select slot)
                               .FirstOrDefault();

            var feeItems = _db.FeeItems.AsNoTracking()
                                       .ToList();

            var sectionIds = invoice.InvoiceItems.Where(x => x.SectionId.HasValue)
                                                 .Select(x => x.SectionId.Value)
                                                 .Distinct()
                                                 .ToList();

            var sections = _db.Sections.AsNoTracking()
                                       .Where(x => sectionIds.Contains(x.Id))
                                       .ToList();

            var isAddDropTuition = invoice.Type == "a";

            var isLateRegisApply = false;

            if (!isAddDropTuition)
            {
                var registrationLogs = _db.RegistrationLogs.AsNoTracking()
                                                           .Where(x => x.StudentId == student.Id
                                                                       && x.TermId == invoice.TermId)
                                                           .ToList();

                var lastRegistrationDate = invoice.Term.FirstRegistrationEndedAt ?? DateTime.UtcNow.EndOfDay(-1);

                isLateRegisApply = DateTime.UtcNow > lastRegistrationDate &&
                                       (!registrationLogs.Any()
                                       || !registrationLogs.Where(x => x.CreatedAt < lastRegistrationDate).Any());
            }

            if (invoice.Term != null && invoice.Term.TermTypeId == 2)
            {
                isLateRegisApply = false; //TODO: Summer no late regis
            }

            var response = new USparkOrder
            {
                KSInvoiceID = invoice.Id,
                KSTermID = invoice.TermId ?? default,
                StudentCode = student.Code,
                Number = invoice.Number,
                Reference1 = invoice.Reference1,
                Reference2 = invoice.Reference2,
                IsPaid = invoice.IsPaid || invoice.Amount <= 0,
                TotalAmount = invoice.TotalAmount,
                InvoiceExpiryDate = invoice.PaymentExpireAt ?? DateTime.UtcNow,
                CreatedAt = invoice.CreatedAt,
                OrderDetails = (from item in invoice.InvoiceItems
                                let matchingItems = feeItems.SingleOrDefault(x => x.Id == item.FeeItemId)
                                let sectionNumber = !item.SectionId.HasValue ? string.Empty : $"[{sections.SingleOrDefault(x => x.Id == item.SectionId.Value)?.Number ?? string.Empty}]"
                                select new USparkOrderDetail
                                {
                                    ItemCode = matchingItems?.Code,
                                    ItemNameEn = (item.ScholarshipStudentId.HasValue ? $"Scholarship for " : string.Empty) +
                                                 (item.RegistrationCourseId.HasValue ? $"{matchingItems?.NameEn} {item.Course.CodeAndCredit} {sectionNumber}"
                                                                                     : $"{matchingItems?.NameEn}"),
                                    ItemNameTh = (item.ScholarshipStudentId.HasValue ? $"ทุนสำหรับ" : string.Empty) +
                                                 (item.RegistrationCourseId.HasValue ? $"{matchingItems?.NameTh} {item.Course.CodeAndCredit} {sectionNumber}"
                                                                                     : $"{matchingItems?.NameTh}"),
                                    KSCourseId = item.CourseId,
                                    KSSectionId = item.SectionId,
                                    KSRegistrationCourseId = item.RegistrationCourseId,
                                    Amount = item.TotalAmount,
                                    LumpSumAddDrop = item.LumpSumAddDrop
                                }).ToList(),
                LatePaymentDate = isAddDropTuition || isLateRegisApply ? invoice.Term.AddDropPaymentEndedAt
                                                                       : invoice.Term.FirstRegistrationPaymentEndedAt,
                LastPaymentDate = invoice.Term.LastPaymentEndedAt ?? DateTime.UtcNow,
                PaymentSlot = activeSlot == null ? null
                                                 : new USparkPaymentSlot
                                                 {
                                                     PaymentAvailableFrom = activeSlot.StartedAt,
                                                     PaymentAvailableUntil = activeSlot.EndedAt
                                                 }
            };

            return response;
        }

        private static bool IsSlotAvailableForStudent(Student student, AcademicInformation academicInformation, IEnumerable<RegistrationCondition> conditions)
        {
            if (student == null || academicInformation == null || conditions == null || !conditions.Any())
            {
                return false;
            }

            var result = conditions.Any(x => CalculateSlotConditionScores(student, academicInformation, x) > decimal.Zero);

            return result;
        }

        private static decimal CalculateSlotConditionScores(Student student, AcademicInformation academicInformation, RegistrationCondition condition)
        {
            if (student == null || academicInformation == null || condition == null)
            {
                return -1;
            }

            var scoreWeight = decimal.Zero;

            if (condition.AcademicProgramId.HasValue)
            {
                if (condition.AcademicProgramId.Value != academicInformation.AcademicProgramId)
                    return -1;

                scoreWeight += 1;
            }

            if (condition.FacultyId.HasValue)
            {
                if (condition.FacultyId.Value != academicInformation.FacultyId)
                    return -1;

                scoreWeight += 1;
            }

            if (condition.DepartmentId.HasValue)
            {
                if (condition.DepartmentId.Value != academicInformation.DepartmentId)
                    return -1;

                scoreWeight += 1;
            }

            if (condition.BatchCodeStart.HasValue)
            {
                if (academicInformation.Batch < condition.BatchCodeStart.Value)
                    return -1;

                scoreWeight += 1;
            }

            if (condition.BatchCodeEnd.HasValue)
            {
                if (academicInformation.Batch > condition.BatchCodeEnd.Value)
                    return -1;

                scoreWeight += 1;
            }

            if (condition.LastDigitStart.HasValue)
            {
                var lastDigitSize = condition.LastDigitStart.Value
                                                            .ToString()
                                                            .Count();

                if (lastDigitSize > student.Code.Length)
                {
                    return -1;
                }

                var studentLastDigits = student.Code.Substring(student.Code.Length - lastDigitSize);

                if (!int.TryParse(studentLastDigits, out int studentDigits)
                    || studentDigits < condition.LastDigitStart.Value)
                {
                    return -1;
                }

                scoreWeight += 1;
            }

            if (condition.LastDigitEnd.HasValue)
            {
                var lastDigitSize = condition.LastDigitEnd.Value
                                                            .ToString()
                                                            .Count();

                if (lastDigitSize > student.Code.Length)
                {
                    return -1;
                }

                var studentLastDigits = student.Code.Substring(student.Code.Length - lastDigitSize);

                if (!int.TryParse(studentLastDigits, out int studentDigits)
                    || studentDigits > condition.LastDigitEnd.Value)
                {
                    return -1;
                }

                scoreWeight += 1;
            }

            if (condition.IsGraduating.HasValue)
            {
                if (academicInformation.IsGraduating != condition.IsGraduating.Value)
                {
                    return -1;
                }

                scoreWeight += 1;
            }

            if (condition.IsAthlete.HasValue)
            {
                if (academicInformation.IsAthlete != condition.IsAthlete.Value)
                {
                    return -1;
                }

                scoreWeight += 1;
            }

            if (!string.IsNullOrEmpty(condition.StudentCodes))
            {
                var studentCodes = condition.StudentCode.Split(',').ToList();

                if (!studentCodes.Any(x => x.Trim() == student.Code))
                {
                    return -1;
                }

                scoreWeight += 1;
            }

            if (!string.IsNullOrEmpty(condition.StudentCodeStart))
            {
                if (!int.TryParse(condition.StudentCode, out int startingCode)
                   || int.TryParse(student.Code, out int studentCodeInt)
                   || studentCodeInt < startingCode)
                {
                    return -1;
                }

                scoreWeight += 1;
            }

            if (!string.IsNullOrEmpty(condition.StudentCodeEnd))
            {
                if (!int.TryParse(condition.StudentCodeEnd, out int endingCode)
                   || int.TryParse(student.Code, out int studentCodeInt)
                   || endingCode < studentCodeInt)
                {
                    return -1;
                }

                scoreWeight += 1;
            }

            if (condition.AcademicLevelId.HasValue)
            {
                if (academicInformation.AcademicLevelId != condition.AcademicLevelId.Value)
                {
                    return -1;
                }

                scoreWeight += 1;
            }

            return scoreWeight;
        }

        public UsparkInvoiceAddDropCourses GetNonCancelStudentInvoiceCoursesDetails(string studentCode, long invoiceId)
        {
            var student = _db.Students.AsNoTracking()
                                      .SingleOrDefault(x => x.Code == studentCode);

            if (student == null)
            {
                throw new ArgumentNullException(nameof(student));
            }

            var invoice = _db.Invoices.AsNoTracking()
                                      .IgnoreQueryFilters()
                                      .Include(x => x.InvoiceItems)
                                      .ThenInclude(x => x.RegistrationCourse)
                                      .SingleOrDefault(x => !x.IsCancel
                                                            && x.StudentId == student.Id
                                                            && x.Id == invoiceId);

            if (invoice == null)
            {
                throw new ArgumentNullException(nameof(invoice));
            }

            var feeItems = _db.FeeItems.AsNoTracking()
                                       .ToList();

            var response = new UsparkInvoiceAddDropCourses
            {
                AddCourses = (from item in invoice.InvoiceItems
                              where item.RegistrationCourseId.HasValue
                                    && item.RegistrationCourse.USPreregistrationId.HasValue
                                    && item.Amount > decimal.Zero
                              select item.RegistrationCourse.USPreregistrationId.Value)
                            .ToList(),
                DropCourses = (from item in invoice.InvoiceItems
                               where item.RegistrationCourseId.HasValue
                                     && item.RegistrationCourse.USPreregistrationId.HasValue
                                     && item.Amount < decimal.Zero
                               select item.RegistrationCourse.USPreregistrationId.Value)
                            .ToList()
            };

            return response;
        }

        public async Task SyncRegistrationCoursePaidStatusAsync(string studentCode, long KSTermId)
        {
            var student = _db.Students.AsNoTracking()
                                      .IgnoreQueryFilters()
                                      .SingleOrDefault(x => x.Code == studentCode);

            if (student == null)
            {
                return;
            }

            var paidRegistrationCourseIds = _db.RegistrationCourses.AsNoTracking()
                                                                 .IgnoreQueryFilters()
                                                                 .Where(x => x.Status != "d"
                                                                             && x.StudentId == student.Id
                                                                             && x.TermId == KSTermId
                                                                             && x.IsPaid
                                                                             && x.USPreregistrationId.HasValue)
                                                                 .Select(x => x.USPreregistrationId.Value)
                                                                 .ToList();

            if (!paidRegistrationCourseIds.Any())
            {
                return;
            }

            await UpdateUsparkRegistrationPaidCoursesAsync(paidRegistrationCourseIds);
        }

        public async Task ProcessInvoicePaymentAsync(InvoiceUpdateIsPaidViewModel request)
        {
            var invoice = _db.Invoices.Include(x => x.InvoiceItems)
                                      .SingleOrDefault(x => x.Number == request.OrderId
                                                            && x.Reference1 == request.Reference1
                                                            && x.Reference2 == request.Reference2);

            var markCompletePayment = invoice != null
                                      && invoice.TotalAmount > decimal.Zero
                                      && request.IsPaymentSuccess
                                      && request.Amount >= invoice.TotalAmount;

            var paymentTransaction = new BankPaymentResponse
            {
                InvoiceId = invoice?.Id,
                TotalAmount = invoice?.TotalAmount,
                IsPaymentSuccess = request.IsPaymentSuccess,
                PaidAmount = request.Amount,
                Number = request.OrderId,
                Reference1 = request.Reference1,
                Reference2 = request.Reference2,
                Reference3 = invoice?.Reference3,
                TransactionId = request.TransactionId,
                TransactionDateTime = DateTime.UtcNow,
                RawResponse = request.RawResponseMessage,
                CreatedAt = DateTime.UtcNow.ToString()
            };

            using (var transaction = _db.Database.BeginTransaction())
            {
                _db.BankPaymentResponses.Add(paymentTransaction);
                transaction.Commit();
            }

            _db.SaveChanges();

            if (invoice is null
                || invoice.IsPaid
                || invoice.IsCancel
                || (invoice.Amount > decimal.Zero && !markCompletePayment))
            {
                return;
            }

            var receipt = _mapper.Map<Invoice, Receipt>(invoice);

            receipt.InvoiceId = invoice.Id;

            receipt.Year = DateTime.UtcNow.Year;
            receipt.Month = DateTime.UtcNow.Month;
            receipt.RunningNumber = GetReceiptNextRunningNumber(receipt.Year);
            receipt.Number = GetFeeReceiptNumber(receipt.Year, receipt.Month, receipt.RunningNumber, ReceiptNumberType.REGISTRATION);

            var receiptItems = invoice.InvoiceItems.Select(x =>
                                                            {
                                                                var receiptItem = _mapper.Map<InvoiceItem, ReceiptItem>(x);
                                                                receiptItem.Receipt = receipt;
                                                                receiptItem.RemainingAmount = x.TotalAmount;
                                                                receiptItem.InvoiceItemId = x.Id;
                                                                return receiptItem;
                                                            })
                                                    .ToList();

            var ePaymentMethod = _db.PaymentMethods.AsNoTracking()
                                                   .SingleOrDefault(x => x.NameEn == "E-Payment");

            var receiptPaymentMethod = new ReceiptPaymentMethod
            {
                PaymentMethodId = ePaymentMethod.Id,
                Receipt = receipt,
                Amount = request.Amount
            };

            var scholarShipTransactions = (from item in invoice.InvoiceItems
                                           join receiptItem in receiptItems on item.Id equals receiptItem.InvoiceItemId
                                           where item.ScholarshipStudentId.HasValue
                                           select new FinancialTransaction
                                           {
                                               StudentId = invoice.StudentId ?? Guid.Empty,
                                               StudentScholarshipId = item.ScholarshipStudentId.Value,
                                               TermId = invoice.TermId.Value,
                                               Receipt = receipt,
                                               ReceiptItem = receiptItem,
                                               PaidAt = DateTime.UtcNow,
                                               PersonalPay = receiptItem.TotalAmount,
                                               UsedScholarship = receiptItem.ScholarshipAmount,
                                               Type = "a"
                                           })
                                         .ToList();

            var scholarshipIds = scholarShipTransactions.Select(x => x.StudentScholarshipId)
                                                        .Distinct()
                                                        .ToList();

            var scholarShips = !scholarshipIds.Any() ? Enumerable.Empty<ScholarshipStudent>()
                                                     : _db.ScholarshipStudents.AsNoTracking()
                                                                              .Where(x => scholarshipIds.Contains(x.Id))
                                                                              .Include(x => x.FinancialTransactions)
                                                                              .ToList();

            var scholarShipBalance = (from scholarShip in scholarShips
                                      select new KeyValuePair<long, decimal>(
                                                scholarShip.Id,
                                                scholarShip.LimitedAmount == decimal.Zero ? decimal.MaxValue // UNLIMITED SCHOLAR SHIP
                                                                                          : scholarShip.FinancialTransactions.Any() ? scholarShip.LimitedAmount - scholarShip.FinancialTransactions.Sum(x => x.UsedScholarship)
                                                                                                                                    : scholarShip.LimitedAmount)
                                      ).ToDictionary(x => x.Key, x => x.Value);

            foreach (var transaction in scholarShipTransactions)
            {
                var balance = scholarShipBalance.SingleOrDefault(x => x.Key == transaction.StudentScholarshipId).Value;

                var isUnlimitedScholarship = balance == decimal.MaxValue;

                transaction.Balance = isUnlimitedScholarship ? decimal.Zero
                                                             : balance - transaction.UsedScholarship;

                if (!isUnlimitedScholarship)
                {
                    scholarShipBalance[transaction.StudentScholarshipId] -= transaction.UsedScholarship;
                }
            }

            var registraionCourseIds = invoice.InvoiceItems.Select(x => x.RegistrationCourseId)
                                                           .ToList();

            var registrationCourses = _db.RegistrationCourses.Where(x => registraionCourseIds.Contains(x.Id))
                                                             .ToList();

            using (var transaction = _db.Database.BeginTransaction())
            {
                invoice.IsPaid = markCompletePayment;
                invoice.UpdatedAt = DateTime.UtcNow;

                _db.Receipts.Add(receipt);
                _db.ReceiptItems.AddRange(receiptItems);
                _db.ReceiptPaymentMethods.Add(receiptPaymentMethod);

                _db.FinancialTransactions.AddRange(scholarShipTransactions);

                foreach (var course in registrationCourses)
                {
                    course.IsPaid = true;
                    course.UpdatedAt = DateTime.UtcNow;
                }

                transaction.Commit();
            }

            _db.SaveChanges();

            if (invoice.TotalAmount <= decimal.Zero || markCompletePayment)
            {
                var USparkRegistrationCourseIds = registrationCourses.Where(x => x.USPreregistrationId.HasValue)
                                                                     .Select(x => x.USPreregistrationId.Value)
                                                                     .ToList();

                await UpdateUsparkRegistrationPaidCoursesAsync(USparkRegistrationCourseIds);
            }
        }

        private static string GetFeeInvoiceNumber(int runningNumber)
        {
            var today = DateTime.UtcNow;
            var RunningNumberString = runningNumber.ToString("000000");
            var runningYearString = (today.Year % 100).ToString();
            return $"{ runningYearString }{ RunningNumberString }";
        }

        private static string GenerateInvoiceReference1(string studentCode, string invoiceNumber, decimal totalAmount, string reference2)
        {
            string studentCodeMod = studentCode.Substring(0, 2) + studentCode.Substring(3, 4); // Ignore third digit
            var studentCodeInvoice = studentCodeMod + invoiceNumber;
            var ref1 = "063" + studentCodeInvoice.PadLeft(14, '0') + "1";
            var checkSum = GenerateInvoiceReference1CheckSum(ref1, reference2, Convert.ToInt32(Math.Abs(totalAmount) * 100).ToString());
            var reference1 = "063" + studentCodeInvoice.PadLeft(14, '0') + "1" + checkSum;
            return reference1;
        }

        private static string GenerateInvoiceReference2(DateTime createAt, DateTime expiredAt)
        {
            var reference2 = string.Format("{0:yyyyMMddHHmmss}", createAt) + string.Format("{0:ddMMyy}", expiredAt);
            return reference2;
        }

        private static string GenerateInvoiceReference1CheckSum(string reference1, string reference2, string reference3)
        {
            reference1 = reference1.PadLeft(18, '0');
            reference2 = reference2.PadLeft(20, '0');
            reference3 = reference3.PadLeft(20, '0');

            int checkSum = 0;
            int checkSum2 = 0;
            int checkSum3 = 0;
            for (int i = 0; i < 20; i++)
            {
                if (i % 2 == 0)
                {
                    if (i < 18)
                    {
                        checkSum += (Convert.ToInt32(reference1[i].ToString()) * 3);
                    }
                    checkSum2 += (Convert.ToInt32(reference2[i].ToString()) * 5);
                    checkSum3 += (Convert.ToInt32(reference3[i].ToString()) * 3);
                }
                else
                {
                    if (i < 18)
                    {
                        checkSum += (Convert.ToInt32(reference1[i].ToString()) * 9);
                    }
                    checkSum2 += (Convert.ToInt32(reference2[i].ToString()) * 7);
                    checkSum3 += (Convert.ToInt32(reference3[i].ToString()) * 5);
                }
            }
            var total = checkSum + checkSum2 + checkSum3;

            string result = total.ToString().PadLeft(3, '0');
            return result.Substring(1, 2);
        }

        private int GetReceiptNextRunningNumber(int year)
        {
            var latestReceiptRunningNumber = _db.Receipts.AsNoTracking()
                                                         .IgnoreQueryFilters()
                                                         .Where(x => x.Year == year)
                                                         .OrderByDescending(x => x.RunningNumber)
                                                         .FirstOrDefault()?.RunningNumber;

            return (latestReceiptRunningNumber ?? 0) + 1;
        }

        private string GetFeeReceiptNumber(int year, int month, int runningNumber, ReceiptNumberType type)
        {
            var runningNumberString = runningNumber.ToString("00000");
            var runningYearString = (year % 100).ToString();
            var runningMonthString = month.ToString("00");

            return $"{ ReceiptNumberTypeExtension.ToStringValue(type) }{ runningMonthString }{ runningYearString }{ runningNumberString }";
        }

        private async Task UpdateUsparkRegistrationPaidCoursesAsync(IEnumerable<Guid> USRegistrationCourseIds)
        {
            var body = new StringContent(JsonConvert.SerializeObject(USRegistrationCourseIds), Encoding.UTF8, "application/json");

            var header = new Dictionary<string, string>
            {
                { "x-api-key", _USparkAPIKey}
            };

            var response = await _httpClientProxy.PutAsync(_paymentConfig.UpdateUSPaidRegistrationCourseEndpoint, header, body);
        }

        public async Task<string> ProcessInvoicePaymentManualAsync(FinanceRegistrationFeeViewModel model)
        {
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    // mark paid flag in invoice item
                    var selectedInvoiceItems = model.Invoice.InvoiceItems.Where(x => x.IsChecked)
                                                                            .Select(x => x.Id)
                                                                            .ToList();

                    var invoiceItems = _db.InvoiceItems.Where(x => selectedInvoiceItems.Contains(x.Id))
                                                        .ToList();

                    invoiceItems.Select(x =>
                    {
                        x.IsPaid = true;
                        return x;
                    })
                                .ToList();

                    foreach (var item in invoiceItems)
                    {
                        var registrationCourse = _db.RegistrationCourses.SingleOrDefault(x => x.Id == item.RegistrationCourseId);
                        if (registrationCourse != null)
                        {
                            registrationCourse.IsPaid = true;
                        }
                    }

                    _db.SaveChanges();


                    // Create Receipt
                    var invoice = _masterProvider.GetInvoice(model.InvoiceId);
                    var existReceipt = _db.Receipts.FirstOrDefault(x => x.InvoiceId == invoice.Id);

                    if (existReceipt == null)
                    {
                        var receipt = _mapper.Map<Invoice, Receipt>(invoice);
                        receipt.Year = DateTime.UtcNow.Year;
                        receipt.Month = DateTime.UtcNow.Month;
                        receipt.RunningNumber = _feeProvider.GetNextReceiptRunningNumber();
                        receipt.Number = _feeProvider.GetFeeReceiptNumber(receipt.RunningNumber, ReceiptNumberType.REGISTRATION);
                        _db.Receipts.Add(receipt);
                        _db.SaveChanges();

                        // Create Receipt Item
                        receipt.ReceiptItems = invoiceItems.Select(x => _mapper.Map<InvoiceItem, ReceiptItem>(x))
                                                            .ToList();

                        receipt.ReceiptItems.Select(x =>
                        {
                            x.ReceiptId = receipt.Id;
                            x.RemainingAmount = x.TotalAmount;
                            return x;
                        })
                                            .ToList();

                        _db.ReceiptItems.AddRange(receipt.ReceiptItems);
                        _db.SaveChanges();

                        var paymentTransaction = new BankPaymentResponse
                        {
                            InvoiceId = invoice?.Id,
                            TotalAmount = invoice?.TotalAmount,
                            IsPaymentSuccess = true,
                            PaidAmount = receipt.TotalAmount,
                            Number = invoice.Number,
                            Reference1 = invoice.Reference1,
                            Reference2 = invoice.Reference2,
                            Reference3 = invoice.Reference3,
                            TransactionId = invoice.Id + "",
                            TransactionDateTime = DateTime.UtcNow,
                            RawResponse = $"Manual Update By {model.UpdatedBy}",
                            CreatedAt = DateTime.UtcNow.ToString()
                        };
                        _db.BankPaymentResponses.Add(paymentTransaction);

                        // Add Payment Method
                        model.PaymentMethods.Select(x =>
                        {
                            x.ReceiptId = receipt.Id;
                            return x;
                        })
                                            .ToList();

                        _db.ReceiptPaymentMethods.AddRange(model.PaymentMethods);


                        _scholarshipProvider.CreateTransactionFromReceipt(receipt.Id, model.PaidDate ?? DateTime.UtcNow, "a");

                        if (model.FinancialTransactions != null && model.FinancialTransactions.Any(x => x.StudentScholarshipId > 0))
                        {
                            model.FinancialTransactions = model.FinancialTransactions.Where(x => x.StudentScholarshipId > 0)
                                                                                        .ToList();
                            model.FinancialTransactions.ForEach(x =>
                            {
                                x.StudentId = receipt.StudentId ?? Guid.Empty;
                                x.TermId = receipt.TermId ?? 0;
                                x.ReceiptId = receipt.Id;
                                x.PaidAt = model.PaidDate ?? DateTime.UtcNow;
                                x.PersonalPay = 0;
                                x.Type = "a";
                            });

                            _scholarshipProvider.CreateTransactions(model.FinancialTransactions);

                            foreach (var financialTransaction in model.FinancialTransactions)
                            {
                                receipt.TotalScholarshipAmount += financialTransaction.UsedScholarship;
                                receipt.TotalAmount -= financialTransaction.UsedScholarship;
                            }
                            if (receipt.TotalAmount < 0)
                            {
                                throw new Exception("Use scholarship more than amount");
                            }
                            _db.SaveChanges();
                        }
                    }
                    // mark paid flag in invoice when all items are paid
                    var isAllInvoiceItemPaid = invoice.InvoiceItems.All(x => x.IsPaid);
                    invoice.IsPaid = isAllInvoiceItemPaid;

                    _db.SaveChanges();

                    _feeProvider.UpdateStudentStateByInvoice(invoice);

                    //// Update USpark Order
                    //if (invoice.USOrderId.HasValue)
                    //{
                    //    UpdateUSparkPayment(invoice.USOrderId.Value, receipt.Number, null);
                    //}

                    //if (_registrationProvider.IsRegistrationPeriod(DateTime.Now, model.TermId))
                    //{
                    //    _registrationProvider.CallUSparkAPICheckoutOrder(receipt.Id);
                    //}
                    //_registrationProvider.CallUSparkServiceAPIPaymentConfirm(receipt.Id);

                    var registraionCourseIds = invoice.InvoiceItems.Select(x => x.RegistrationCourseId)
                                                            .ToList();
                    var registrationCourses = _db.RegistrationCourses.Where(x => registraionCourseIds.Contains(x.Id))
                                                            .ToList();
                    var USparkRegistrationCourseIds = registrationCourses.Where(x => x.USPreregistrationId.HasValue)
                                                        .Select(x => x.USPreregistrationId.Value)
                                                        .ToList();

                    await UpdateUsparkRegistrationPaidCoursesAsync(USparkRegistrationCourseIds);

                    transaction.Commit();
                    return "";
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return ex.Message;
                }

            }
        }

        public WaitingPaymentWithAddressReportViewModel GetWaitingPaymentWithAddressReport(Criteria criteria)
        {
            WaitingPaymentWithAddressReportViewModel report = new WaitingPaymentWithAddressReportViewModel();
            report.Criteria = criteria;

            //var unpaidRegistrationCourseQuery = from regisCourse in _db.RegistrationCourses.AsNoTracking()
            //                                                                               .IgnoreQueryFilters()
            //                                                                               .Include(x => x.Student).ThenInclude(x => x.Title)
            //                                                                               .Include(x => x.Student).ThenInclude(x => x.AcademicInformation).ThenInclude(x => x.Department)
            //                                                                               .Include(x => x.Student).ThenInclude(x => x.AcademicInformation).ThenInclude(x => x.Faculty)
            //                                    join invoiceItem in _db.InvoiceItems.AsNoTracking()
            //                                                                        .Include(x => x.Invoice)
            //                                    on new
            //                                    {
            //                                        regisCourse.Id,
            //                                        IsActive = true,
            //                                        IsCancel = false,
            //                                    }
            //                                    equals new
            //                                    {
            //                                        Id = invoiceItem.RegistrationCourseId ?? 0,
            //                                        invoiceItem.Invoice.IsActive,
            //                                        invoiceItem.Invoice.IsCancel,
            //                                    } into invoiceItems
            //                                    from invoiceItem in invoiceItems.DefaultIfEmpty()
            //                                        //join currentaddress in _db.StudentAddresses on new
            //                                        //{
            //                                        //    regisCourse.StudentId,
            //                                        //    Type = "c"
            //                                        //} equals new
            //                                        //{
            //                                        //    currentaddress.StudentId,
            //                                        //    currentaddress.Type
            //                                        //} into currentAddresses
            //                                        //from currentaddress in currentAddresses.DefaultIfEmpty()
            //                                    join currentaddress in _db.ParentInformations on new
            //                                    {
            //                                        regisCourse.StudentId,
            //                                        IsMainParent = true
            //                                    } equals new
            //                                    {
            //                                        currentaddress.StudentId,
            //                                        currentaddress.IsMainParent
            //                                    } into currentAddresses
            //                                    from currentaddress in currentAddresses.DefaultIfEmpty()
            //                                    where regisCourse.IsActive
            //                                        && regisCourse.Status != "d"
            //                                        && !regisCourse.IsPaid
            //                                        && regisCourse.TermId == criteria.TermId
            //                                    select new
            //                                    {
            //                                        RegistrationCourse = regisCourse,
            //                                        InvoiceItem = invoiceItem,
            //                                        Invoice = invoiceItem != null ? invoiceItem.Invoice : null,
            //                                        Student = regisCourse.Student,
            //                                        AcademicInfo = regisCourse.Student != null ? regisCourse.Student.AcademicInformation : null,
            //                                        CurrentAddress = currentaddress,
            //                                        Term = $"{regisCourse.Term.AcademicTerm}/{regisCourse.Term.AcademicYear}",
            //                                        RegistrationCredit = regisCourse.Course.RegistrationCredit,
            //                                    };

            //if (criteria.FacultyId > 0)
            //{
            //    unpaidRegistrationCourseQuery = unpaidRegistrationCourseQuery.Where(x => x.RegistrationCourse.Student.AcademicInformation.FacultyId == criteria.FacultyId);
            //}
            //if (criteria.DepartmentId > 0)
            //{
            //    unpaidRegistrationCourseQuery = unpaidRegistrationCourseQuery.Where(x => x.RegistrationCourse.Student.AcademicInformation.DepartmentId == criteria.DepartmentId);
            //}
            //if (!string.IsNullOrEmpty(criteria.StudentCode))
            //{
            //    unpaidRegistrationCourseQuery = unpaidRegistrationCourseQuery.Where(x => x.RegistrationCourse.Student.Code.StartsWith(criteria.StudentCode));
            //}
            //if (criteria.StartStudentBatch.HasValue)
            //{
            //    unpaidRegistrationCourseQuery = unpaidRegistrationCourseQuery.Where(x => x.RegistrationCourse.Student.AcademicInformation.Batch >= criteria.StartStudentBatch.Value);
            //}
            //if (criteria.EndStudentBatch.HasValue)
            //{
            //    unpaidRegistrationCourseQuery = unpaidRegistrationCourseQuery.Where(x => x.RegistrationCourse.Student.AcademicInformation.Batch <= criteria.EndStudentBatch.Value);
            //}
            //if (criteria.StudentStatuses != null && criteria.StudentStatuses.Any())
            //{
            //    unpaidRegistrationCourseQuery = unpaidRegistrationCourseQuery.Where(x => criteria.StudentStatuses.Contains(x.Student.StudentStatus));
            //}
            //if (criteria.StudentFeeTypeId > 0)
            //{
            //    unpaidRegistrationCourseQuery = unpaidRegistrationCourseQuery.Where(x => x.Student.StudentFeeTypeId == criteria.StudentFeeTypeId);
            //}
            //if (criteria.StudentCodeFrom != null)
            //{
            //    unpaidRegistrationCourseQuery = unpaidRegistrationCourseQuery.Where(x => x.Student.CodeInt >= criteria.StudentCodeFrom);
            //}
            //if (criteria.StudentCodeTo != null)
            //{
            //    unpaidRegistrationCourseQuery = unpaidRegistrationCourseQuery.Where(x => x.Student.CodeInt <= criteria.StudentCodeTo);
            //}
            //if (criteria.ResidentTypeId > 0)
            //{
            //    unpaidRegistrationCourseQuery = unpaidRegistrationCourseQuery.Where(x => x.Student.ResidentTypeId == criteria.ResidentTypeId);
            //}
            //if (criteria.AdmissionTypeId > 0)
            //{
            //    unpaidRegistrationCourseQuery = unpaidRegistrationCourseQuery.Where(x => x.Student.AdmissionInformation.AdmissionTypeId == criteria.AdmissionTypeId);
            //}
            //if (criteria.StudentFeeGroupId > 0)
            //{
            //    unpaidRegistrationCourseQuery = unpaidRegistrationCourseQuery.Where(x => x.Student.StudentFeeGroupId == criteria.StudentFeeGroupId);
            //}

            var relatedRegisCourseQuery = from regisCourse in _db.RegistrationCourses.AsNoTracking()
                                                                                    .IgnoreQueryFilters()
                                                                                    .Include(x => x.Student).ThenInclude(x => x.Title)
                                                                                    .Include(x => x.Student).ThenInclude(x => x.AcademicInformation).ThenInclude(x => x.Department)
                                                                                    .Include(x => x.Student).ThenInclude(x => x.AcademicInformation).ThenInclude(x => x.Faculty)
                                          join currentaddress in _db.ParentInformations on new
                                          {
                                              regisCourse.StudentId,
                                              IsMainParent = true
                                          } equals new
                                          {
                                              currentaddress.StudentId,
                                              currentaddress.IsMainParent
                                          } into currentAddresses
                                          from currentaddress in currentAddresses.DefaultIfEmpty()
                                          where regisCourse.IsActive
                                              && regisCourse.Status != "d"
                                              && !regisCourse.IsPaid
                                              && regisCourse.TermId == criteria.TermId
                                          select new
                                          {
                                              RegistrationCourse = regisCourse,
                                              Student = regisCourse.Student,
                                              AcademicInfo = regisCourse.Student != null ? regisCourse.Student.AcademicInformation : null,
                                              CurrentAddress = currentaddress,
                                              Term = $"{regisCourse.Term.AcademicTerm}/{regisCourse.Term.AcademicYear}",
                                              RegistrationCredit = regisCourse.Course.RegistrationCredit,
                                          };
            if (criteria.FacultyId > 0)
            {
                relatedRegisCourseQuery = relatedRegisCourseQuery.Where(x => x.RegistrationCourse.Student.AcademicInformation.FacultyId == criteria.FacultyId);
            }
            if (criteria.DepartmentId > 0)
            {
                relatedRegisCourseQuery = relatedRegisCourseQuery.Where(x => x.RegistrationCourse.Student.AcademicInformation.DepartmentId == criteria.DepartmentId);
            }
            if (!string.IsNullOrEmpty(criteria.StudentCode))
            {
                relatedRegisCourseQuery = relatedRegisCourseQuery.Where(x => x.RegistrationCourse.Student.Code.StartsWith(criteria.StudentCode));
            }
            if (criteria.StartStudentBatch.HasValue)
            {
                relatedRegisCourseQuery = relatedRegisCourseQuery.Where(x => x.RegistrationCourse.Student.AcademicInformation.Batch >= criteria.StartStudentBatch.Value);
            }
            if (criteria.EndStudentBatch.HasValue)
            {
                relatedRegisCourseQuery = relatedRegisCourseQuery.Where(x => x.RegistrationCourse.Student.AcademicInformation.Batch <= criteria.EndStudentBatch.Value);
            }
            if (criteria.StudentStatuses != null && criteria.StudentStatuses.Any())
            {
                relatedRegisCourseQuery = relatedRegisCourseQuery.Where(x => criteria.StudentStatuses.Contains(x.Student.StudentStatus));
            }
            if (criteria.StudentFeeTypeId > 0)
            {
                relatedRegisCourseQuery = relatedRegisCourseQuery.Where(x => x.Student.StudentFeeTypeId == criteria.StudentFeeTypeId);
            }
            if (criteria.StudentCodeFrom != null)
            {
                relatedRegisCourseQuery = relatedRegisCourseQuery.Where(x => x.Student.CodeInt >= criteria.StudentCodeFrom);
            }
            if (criteria.StudentCodeTo != null)
            {
                relatedRegisCourseQuery = relatedRegisCourseQuery.Where(x => x.Student.CodeInt <= criteria.StudentCodeTo);
            }
            if (criteria.ResidentTypeId > 0)
            {
                relatedRegisCourseQuery = relatedRegisCourseQuery.Where(x => x.Student.ResidentTypeId == criteria.ResidentTypeId);
            }
            if (criteria.AdmissionTypeId > 0)
            {
                relatedRegisCourseQuery = relatedRegisCourseQuery.Where(x => x.Student.AdmissionInformation.AdmissionTypeId == criteria.AdmissionTypeId);
            }
            if (criteria.StudentFeeGroupId > 0)
            {
                relatedRegisCourseQuery = relatedRegisCourseQuery.Where(x => x.Student.StudentFeeGroupId == criteria.StudentFeeGroupId);
            }
            var relatedRegisCourseList = relatedRegisCourseQuery.ToList();

            var relatedInvoices = (from invoiceItem in _db.InvoiceItems.AsNoTracking()
                                                                       .Include(x => x.Invoice)
                                   where invoiceItem.Invoice.IsActive
                                            && !invoiceItem.Invoice.IsCancel
                                            && invoiceItem.Invoice.TermId == criteria.TermId
                                            && invoiceItem.RegistrationCourseId != null
                                   select invoiceItem
                                  ).ToList();


            var unpaidRegistrationCourseQuery = from regisCourse in relatedRegisCourseList
                                                join invoiceItem in relatedInvoices
                                                 on new
                                                 {
                                                     regisCourse.RegistrationCourse.Id,
                                                     IsActive = true,
                                                     IsCancel = false,
                                                 }
                                                 equals new
                                                 {
                                                     Id = invoiceItem.RegistrationCourseId ?? 0,
                                                     invoiceItem.Invoice.IsActive,
                                                     invoiceItem.Invoice.IsCancel,
                                                 }
                                                 into invoiceItems
                                                from invoiceItem in invoiceItems.DefaultIfEmpty()
                                                select new
                                                {
                                                    RegistrationCourse = regisCourse.RegistrationCourse,
                                                    InvoiceItem = invoiceItem,
                                                    Invoice = invoiceItem != null ? invoiceItem.Invoice : null,
                                                    Student = regisCourse.Student,
                                                    AcademicInfo = regisCourse.AcademicInfo,
                                                    CurrentAddress = regisCourse.CurrentAddress,
                                                    Term = regisCourse.Term,
                                                    RegistrationCredit = regisCourse.RegistrationCredit,
                                                };

            var waitingPaymentWithAddressReportItems = new List<WaitingPaymentWithAddressReportItemViewModel>();
            foreach (var x in unpaidRegistrationCourseQuery.ToList())
            {
                var waitingPaymentWithAddressReportItem = new WaitingPaymentWithAddressReportItemViewModel();
                //waitingPaymentWithAddressReportItem.AddressEn1 = x.CurrentAddress != null ? x.CurrentAddress.AddressEn1 : "";
                //waitingPaymentWithAddressReportItem.AddressEn2 = x.CurrentAddress != null ? x.CurrentAddress.AddressEn2 : "";
                //waitingPaymentWithAddressReportItem.HouseNumber = x.CurrentAddress != null ? x.CurrentAddress.HouseNumber : "";
                //waitingPaymentWithAddressReportItem.RoadEn = x.CurrentAddress != null ? x.CurrentAddress.RoadEn : "";
                //waitingPaymentWithAddressReportItem.SoiEn = x.CurrentAddress != null ? x.CurrentAddress.SoiEn : "";

                waitingPaymentWithAddressReportItem.AddressEn1 = x.CurrentAddress != null ? x.CurrentAddress.AddressEn : "";                                

                waitingPaymentWithAddressReportItem.CityEn = x.CurrentAddress != null ? (x.CurrentAddress.City != null ? x.CurrentAddress.City.NameEn : "") : "";
                waitingPaymentWithAddressReportItem.ConfirmStatus = x.InvoiceItem != null;
                waitingPaymentWithAddressReportItem.CountryEn = x.CurrentAddress != null ? (x.CurrentAddress.Country != null ? x.CurrentAddress.Country.NameEn : "") : "";
                waitingPaymentWithAddressReportItem.Department = x.AcademicInfo != null ? (x.AcademicInfo.Department != null ? x.AcademicInfo.Department.Code : "") : "";
                waitingPaymentWithAddressReportItem.DistrictEn = x.CurrentAddress != null ? (x.CurrentAddress.District != null ? x.CurrentAddress.District.NameEn : "") : "";
                waitingPaymentWithAddressReportItem.Email = x.Student.Email;
                waitingPaymentWithAddressReportItem.Email2 = x.Student.Email2;
                waitingPaymentWithAddressReportItem.Facebook = x.Student.Facebook;
                waitingPaymentWithAddressReportItem.Faculty = x.AcademicInfo != null ? (x.AcademicInfo.Faculty != null ? x.AcademicInfo.Faculty.CodeAndName : "") : "";
                waitingPaymentWithAddressReportItem.InvoiceId = x.InvoiceItem != null ? x.InvoiceItem.InvoiceId : 0;
                waitingPaymentWithAddressReportItem.InvoiceNumber = x.InvoiceItem != null ? (x.InvoiceItem.Invoice != null ? x.InvoiceItem.Invoice.Number : "") : "";
                waitingPaymentWithAddressReportItem.InvoiceTotalAmount = x.InvoiceItem != null ? (x.InvoiceItem.Invoice != null ? x.InvoiceItem.Invoice.TotalAmount : 0) : 0;
                waitingPaymentWithAddressReportItem.InvoiceType = x.InvoiceItem != null ? (x.InvoiceItem.Invoice != null ? x.InvoiceItem.Invoice.TypeText : "") : "";
                waitingPaymentWithAddressReportItem.Line = x.Student.Line;
                waitingPaymentWithAddressReportItem.OtherContact = x.Student.OtherContact;
                waitingPaymentWithAddressReportItem.PaidStatus = x.RegistrationCourse.IsPaid;
                waitingPaymentWithAddressReportItem.PersonalEmail = x.Student.PersonalEmail;
                waitingPaymentWithAddressReportItem.PersonalEmail2 = x.Student.PersonalEmail2;
                waitingPaymentWithAddressReportItem.ProvinceEn = x.CurrentAddress != null ? (x.CurrentAddress.Province != null ? x.CurrentAddress.Province.NameEn : "") : "";
                waitingPaymentWithAddressReportItem.StateEn = x.CurrentAddress != null ? (x.CurrentAddress.State != null ? x.CurrentAddress.State.NameEn : "") : "";
                waitingPaymentWithAddressReportItem.StudentCode = x.Student.Code;
                waitingPaymentWithAddressReportItem.StudentName = x.Student.FullNameEn;
                waitingPaymentWithAddressReportItem.StudentStatus = x.Student.StudentStatusText;
                waitingPaymentWithAddressReportItem.SubdistrictEn = x.CurrentAddress != null ? (x.CurrentAddress.Subdistrict != null ? x.CurrentAddress.Subdistrict.NameEn : "") : "";
                waitingPaymentWithAddressReportItem.TelephoneNumber1 = x.Student.TelephoneNumber1;
                waitingPaymentWithAddressReportItem.TelephoneNumber2 = x.Student.TelephoneNumber2;
                waitingPaymentWithAddressReportItem.TelephoneNumber3 = x.Student.TelephoneNumber3;
                waitingPaymentWithAddressReportItem.Term = x.Term;
                waitingPaymentWithAddressReportItem.TotalAmount = x.InvoiceItem != null ? x.InvoiceItem.TotalAmount : 0;
                waitingPaymentWithAddressReportItem.TotalRelatedCredit = x.RegistrationCredit;
                waitingPaymentWithAddressReportItem.ZipCode = x.CurrentAddress != null ? x.CurrentAddress.ZipCode : "";

                waitingPaymentWithAddressReportItems.Add(waitingPaymentWithAddressReportItem);
            }

            #region invoice
            //find invoice without registration course

           
            var alreadyExistInvoice = waitingPaymentWithAddressReportItems.Where(x => x.InvoiceId > 0)
                                                                          .Select(x => x.InvoiceId)
                                                                          .Distinct()
                                                                          .ToList();
            var unpaidOthersInvoices = from invoice in _db.Invoices.AsNoTracking()
                                                                   .IgnoreQueryFilters()
                                                                   .Include(x => x.Term)
                                                                   .Include(x => x.Student).ThenInclude(x => x.Title)
                                                                   .Include(x => x.Student).ThenInclude(x => x.AcademicInformation).ThenInclude(x => x.Department)
                                                                   .Include(x => x.Student).ThenInclude(x => x.AcademicInformation).ThenInclude(x => x.Faculty)
                                                                   .Include(x => x.InvoiceItems)
                                                    //join currentaddress in _db.StudentAddresses on new
                                                    //{
                                                    //    regisCourse.StudentId,
                                                    //    Type = "c"
                                                    //} equals new
                                                    //{
                                                    //    currentaddress.StudentId,
                                                    //    currentaddress.Type
                                                    //} into currentAddresses
                                                    //from currentaddress in currentAddresses.DefaultIfEmpty()
                                        join currentaddress in _db.ParentInformations on new
                                        {
                                            StudentId = invoice.StudentId.Value,
                                            IsMainParent = true
                                        } equals new
                                        {
                                            currentaddress.StudentId,
                                            currentaddress.IsMainParent
                                        } into currentAddresses
                                        from currentaddress in currentAddresses.DefaultIfEmpty()
                                        where invoice.IsActive
                                            && !invoice.IsCancel
                                            && !invoice.IsPaid
                                            && invoice.TermId == criteria.TermId
                                            && invoice.TotalAmount > 0
                                            && !alreadyExistInvoice.Contains(invoice.Id)
                                        select new
                                        {
                                            Invoice = invoice,
                                            Student = invoice.Student,
                                            AcademicInfo = invoice.Student != null ? invoice.Student.AcademicInformation : null,
                                            CurrentAddress = currentaddress,
                                            Term = $"{invoice.Term.AcademicTerm}/{invoice.Term.AcademicYear}",
                                            RegistrationCredit = 0,
                                        };
            if (criteria.FacultyId > 0)
            {
                unpaidOthersInvoices = unpaidOthersInvoices.Where(x => x.Student.AcademicInformation.FacultyId == criteria.FacultyId);
            }
            if (criteria.DepartmentId > 0)
            {
                unpaidOthersInvoices = unpaidOthersInvoices.Where(x => x.Student.AcademicInformation.DepartmentId == criteria.DepartmentId);
            }
            if (!string.IsNullOrEmpty(criteria.StudentCode))
            {
                unpaidOthersInvoices = unpaidOthersInvoices.Where(x => x.Student.Code.StartsWith(criteria.StudentCode));
            }
            if (criteria.StartStudentBatch.HasValue)
            {
                unpaidOthersInvoices = unpaidOthersInvoices.Where(x => x.Student.AcademicInformation.Batch >= criteria.StartStudentBatch.Value);
            }
            if (criteria.EndStudentBatch.HasValue)
            {
                unpaidOthersInvoices = unpaidOthersInvoices.Where(x => x.Student.AcademicInformation.Batch <= criteria.EndStudentBatch.Value);
            }
            if (criteria.StudentStatuses != null && criteria.StudentStatuses.Any())
            {
                unpaidOthersInvoices = unpaidOthersInvoices.Where(x => criteria.StudentStatuses.Contains(x.Student.StudentStatus));
            }
            if (criteria.StudentFeeTypeId > 0)
            {
                unpaidOthersInvoices = unpaidOthersInvoices.Where(x => x.Student.StudentFeeTypeId == criteria.StudentFeeTypeId);
            }
            if (criteria.StudentCodeFrom != null)
            {
                unpaidOthersInvoices = unpaidOthersInvoices.Where(x => x.Student.CodeInt >= criteria.StudentCodeFrom);
            }
            if (criteria.StudentCodeTo != null)
            {
                unpaidOthersInvoices = unpaidOthersInvoices.Where(x => x.Student.CodeInt <= criteria.StudentCodeTo);
            }
            if (criteria.ResidentTypeId > 0)
            {
                unpaidOthersInvoices = unpaidOthersInvoices.Where(x => x.Student.ResidentTypeId == criteria.ResidentTypeId);
            }
            if (criteria.AdmissionTypeId > 0)
            {
                unpaidOthersInvoices = unpaidOthersInvoices.Where(x => x.Student.AdmissionInformation.AdmissionTypeId == criteria.AdmissionTypeId);
            }
            if (criteria.StudentFeeGroupId > 0)
            {
                unpaidOthersInvoices = unpaidOthersInvoices.Where(x => x.Student.StudentFeeGroupId == criteria.StudentFeeGroupId);
            }

            foreach (var x in unpaidOthersInvoices.ToList())
            {
                var waitingPaymentWithAddressReportItem = new WaitingPaymentWithAddressReportItemViewModel();
                //waitingPaymentWithAddressReportItem.AddressEn1 = x.CurrentAddress != null ? x.CurrentAddress.AddressEn1 : "";
                //waitingPaymentWithAddressReportItem.AddressEn2 = x.CurrentAddress != null ? x.CurrentAddress.AddressEn2 : "";
                //waitingPaymentWithAddressReportItem.HouseNumber = x.CurrentAddress != null ? x.CurrentAddress.HouseNumber : "";
                //waitingPaymentWithAddressReportItem.RoadEn = x.CurrentAddress != null ? x.CurrentAddress.RoadEn : "";
                //waitingPaymentWithAddressReportItem.SoiEn = x.CurrentAddress != null ? x.CurrentAddress.SoiEn : "";

                waitingPaymentWithAddressReportItem.AddressEn1 = x.CurrentAddress != null ? x.CurrentAddress.AddressEn : "";

                waitingPaymentWithAddressReportItem.CityEn = x.CurrentAddress != null ? (x.CurrentAddress.City != null ? x.CurrentAddress.City.NameEn : "") : "";
                waitingPaymentWithAddressReportItem.ConfirmStatus = true;
                waitingPaymentWithAddressReportItem.CountryEn = x.CurrentAddress != null ? (x.CurrentAddress.Country != null ? x.CurrentAddress.Country.NameEn : "") : "";
                waitingPaymentWithAddressReportItem.Department = x.AcademicInfo != null ? (x.AcademicInfo.Department != null ? x.AcademicInfo.Department.Code : "") : "";
                waitingPaymentWithAddressReportItem.DistrictEn = x.CurrentAddress != null ? (x.CurrentAddress.District != null ? x.CurrentAddress.District.NameEn : "") : "";
                waitingPaymentWithAddressReportItem.Email = x.Student.Email;
                waitingPaymentWithAddressReportItem.Email2 = x.Student.Email2;
                waitingPaymentWithAddressReportItem.Facebook = x.Student.Facebook;
                waitingPaymentWithAddressReportItem.Faculty = x.AcademicInfo != null ? (x.AcademicInfo.Faculty != null ? x.AcademicInfo.Faculty.CodeAndName : "") : "";
                waitingPaymentWithAddressReportItem.InvoiceId =  x.Invoice.Id;
                waitingPaymentWithAddressReportItem.InvoiceNumber = x.Invoice.Number;
                waitingPaymentWithAddressReportItem.InvoiceTotalAmount = x.Invoice.TotalAmount;
                waitingPaymentWithAddressReportItem.InvoiceType = x.Invoice.TypeText;
                waitingPaymentWithAddressReportItem.Line = x.Student.Line;
                waitingPaymentWithAddressReportItem.OtherContact = x.Student.OtherContact;
                waitingPaymentWithAddressReportItem.PaidStatus = x.Invoice.IsPaid;
                waitingPaymentWithAddressReportItem.PersonalEmail = x.Student.PersonalEmail;
                waitingPaymentWithAddressReportItem.PersonalEmail2 = x.Student.PersonalEmail2;
                waitingPaymentWithAddressReportItem.ProvinceEn = x.CurrentAddress != null ? (x.CurrentAddress.Province != null ? x.CurrentAddress.Province.NameEn : "") : "";
                waitingPaymentWithAddressReportItem.StateEn = x.CurrentAddress != null ? (x.CurrentAddress.State != null ? x.CurrentAddress.State.NameEn : "") : "";
                waitingPaymentWithAddressReportItem.StudentCode = x.Student.Code;
                waitingPaymentWithAddressReportItem.StudentName = x.Student.FullNameEn;
                waitingPaymentWithAddressReportItem.StudentStatus = x.Student.StudentStatusText;
                waitingPaymentWithAddressReportItem.SubdistrictEn = x.CurrentAddress != null ? (x.CurrentAddress.Subdistrict != null ? x.CurrentAddress.Subdistrict.NameEn : "") : "";
                waitingPaymentWithAddressReportItem.TelephoneNumber1 = x.Student.TelephoneNumber1;
                waitingPaymentWithAddressReportItem.TelephoneNumber2 = x.Student.TelephoneNumber2;
                waitingPaymentWithAddressReportItem.TelephoneNumber3 = x.Student.TelephoneNumber3;
                waitingPaymentWithAddressReportItem.Term = x.Term;
                waitingPaymentWithAddressReportItem.TotalAmount = x.Invoice.TotalAmount;
                waitingPaymentWithAddressReportItem.TotalRelatedCredit = x.RegistrationCredit;
                waitingPaymentWithAddressReportItem.ZipCode = x.CurrentAddress != null ? x.CurrentAddress.ZipCode : "";

                waitingPaymentWithAddressReportItems.Add(waitingPaymentWithAddressReportItem);
            }
            #endregion

            //filter more
            if (!string.IsNullOrEmpty(criteria.InvoiceType) && !"All".Equals(criteria.InvoiceType))
            {
                if (Invoice.TYPE_REGISTRATION.Equals(criteria.InvoiceType))
                {
                    waitingPaymentWithAddressReportItems = waitingPaymentWithAddressReportItems.Where(x => Invoice.TYPE_REGISTRATION.Equals(x.InvoiceType))
                                                                                               .ToList();
                }
                else if (Invoice.TYPE_ADD_DROP.Equals(criteria.InvoiceType))
                {
                    waitingPaymentWithAddressReportItems = waitingPaymentWithAddressReportItems.Where(x => Invoice.TYPE_ADD_DROP.Equals(x.InvoiceType))
                                                                                               .ToList();
                }
                else
                {
                    waitingPaymentWithAddressReportItems = waitingPaymentWithAddressReportItems.Where(x => string.IsNullOrEmpty(x.InvoiceType))
                                                                                               .ToList();
                }
            }

            report.ReportItems = (from item in waitingPaymentWithAddressReportItems
                                  group item by new
                                  {
                                      item.Term,
                                      item.StudentCode,
                                      item.StudentName,
                                      item.StudentStatus,
                                      item.Faculty,
                                      item.Department,

                                      item.ConfirmStatus,
                                      item.PaidStatus,
                                      item.InvoiceType,
                                      item.InvoiceNumber,
                                      item.InvoiceTotalAmount,

                                      item.AddressEn1,
                                      item.AddressEn2,
                                      item.CityEn,
                                      item.CountryEn,
                                      item.DistrictEn,
                                      item.Email,
                                      item.Email2,
                                      item.Facebook,
                                      item.HouseNumber,
                                      item.Line,
                                      item.OtherContact,
                                      item.PersonalEmail,
                                      item.PersonalEmail2,
                                      item.ProvinceEn,
                                      item.RoadEn,
                                      item.SoiEn,
                                      item.StateEn,
                                      item.SubdistrictEn,
                                      item.TelephoneNumber,
                                      item.TelephoneNumber1,
                                      item.TelephoneNumber2,
                                      item.TelephoneNumber3,
                                      item.ZipCode
                                  } into itemGroup
                                  select new WaitingPaymentWithAddressReportItemViewModel()
                                  {
                                      AddressEn1 = itemGroup.Key.AddressEn1,
                                      AddressEn2 = itemGroup.Key.AddressEn2,
                                      CityEn = itemGroup.Key.CityEn,
                                      ConfirmStatus = itemGroup.Key.ConfirmStatus,
                                      CountryEn = itemGroup.Key.CountryEn,
                                      Department = itemGroup.Key.Department,
                                      DistrictEn = itemGroup.Key.DistrictEn,
                                      Email = itemGroup.Key.Email,
                                      Email2 = itemGroup.Key.Email2,
                                      Facebook = itemGroup.Key.Facebook,
                                      Faculty = itemGroup.Key.Faculty,
                                      HouseNumber = itemGroup.Key.HouseNumber,
                                      InvoiceType = itemGroup.Key.InvoiceType,
                                      InvoiceNumber = itemGroup.Key.InvoiceNumber,
                                      InvoiceTotalAmount = itemGroup.Key.InvoiceTotalAmount,
                                      Line = itemGroup.Key.Line,
                                      OtherContact = itemGroup.Key.OtherContact,
                                      PaidStatus = itemGroup.Key.PaidStatus,
                                      PersonalEmail = itemGroup.Key.PersonalEmail,
                                      PersonalEmail2 = itemGroup.Key.PersonalEmail2,
                                      ProvinceEn = itemGroup.Key.ProvinceEn,
                                      RoadEn = itemGroup.Key.RoadEn,
                                      SoiEn = itemGroup.Key.SoiEn,
                                      StateEn = itemGroup.Key.StateEn,
                                      StudentCode = itemGroup.Key.StudentCode,
                                      StudentName = itemGroup.Key.StudentName,
                                      StudentStatus = itemGroup.Key.StudentStatus,
                                      SubdistrictEn = itemGroup.Key.SubdistrictEn,
                                      TelephoneNumber = itemGroup.Key.TelephoneNumber,
                                      TelephoneNumber1 = itemGroup.Key.TelephoneNumber1,
                                      TelephoneNumber2 = itemGroup.Key.TelephoneNumber2,
                                      TelephoneNumber3 = itemGroup.Key.TelephoneNumber3,
                                      Term = itemGroup.Key.Term,
                                      TotalAmount = itemGroup.Sum(x => x.TotalAmount),
                                      TotalRelatedCredit = itemGroup.Sum(x => x.TotalRelatedCredit),
                                      ZipCode = itemGroup.Key.ZipCode,
                                  }).OrderBy(x => x.Term).ThenBy(x => x.StudentCode).ToList();

            if (criteria.CreditFrom != null && criteria.CreditFrom > 0)
            {
                report.ReportItems = report.ReportItems.Where(x => x.TotalRelatedCredit >= criteria.CreditFrom).ToList();
            }
            if (criteria.CreditTo != null && criteria.CreditTo > 0)
            {
                report.ReportItems = report.ReportItems.Where(x => x.TotalRelatedCredit <= criteria.CreditTo).ToList();
            }

            return report;
        }

        public static readonly int SPECIAL_FEE_TYPE_ID_SCHOLARSHIP = -2;
        public RegistrationResultWithAmountAndCreditReportViewModel GetRegistrationResultWithAmountAndCreditReport(Criteria criteria)
        {
            RegistrationResultWithAmountAndCreditReportViewModel report = new RegistrationResultWithAmountAndCreditReportViewModel();
            report.Criteria = criteria;

            var registrationCourseQuery = _db.RegistrationCourses.AsNoTracking()
                                                                 .IgnoreQueryFilters()
                                                                 .Where(x => x.IsActive
                                                                                && x.TermId == criteria.TermId
                                                                                && !x.IsTransferCourse
                                                                       );
            if (criteria.FacultyId > 0)
            {
                registrationCourseQuery = registrationCourseQuery.Where(x => x.Student.AcademicInformation.FacultyId == criteria.FacultyId);
            }
            if (criteria.DepartmentId > 0)
            {
                registrationCourseQuery = registrationCourseQuery.Where(x => x.Student.AcademicInformation.DepartmentId == criteria.DepartmentId);
            }
            if (criteria.DepartmentIds != null && criteria.DepartmentIds.Any())
            {
                registrationCourseQuery = registrationCourseQuery.Where(x => criteria.DepartmentIds.Contains(x.Student.AcademicInformation.DepartmentId ?? 0));
            }
            if (!string.IsNullOrEmpty(criteria.StudentCode))
            {
                registrationCourseQuery = registrationCourseQuery.Where(x => x.Student.Code.StartsWith(criteria.StudentCode));
            }
            if (criteria.StartStudentBatch.HasValue)
            {
                registrationCourseQuery = registrationCourseQuery.Where(x => x.Student.AcademicInformation.Batch >= criteria.StartStudentBatch.Value);
            }
            if (criteria.EndStudentBatch.HasValue)
            {
                registrationCourseQuery = registrationCourseQuery.Where(x => x.Student.AcademicInformation.Batch <= criteria.EndStudentBatch.Value);
            }
            if (criteria.StudentStatuses != null && criteria.StudentStatuses.Any())
            {
                registrationCourseQuery = registrationCourseQuery.Where(x => criteria.StudentStatuses.Contains(x.Student.StudentStatus));
            }            
            if (criteria.StudentFeeTypeId > 0)
            {
                registrationCourseQuery = registrationCourseQuery.Where(x => x.Student.StudentFeeTypeId == criteria.StudentFeeTypeId);
            }
            if (criteria.StudentFeeTypeIds != null && criteria.StudentFeeTypeIds.Any())
            {
                registrationCourseQuery = registrationCourseQuery.Where(x => criteria.StudentFeeTypeIds.Contains(x.Student.StudentFeeTypeId));
            }
            if (criteria.StudentCodeFrom != null)
            {
                registrationCourseQuery = registrationCourseQuery.Where( x => x.Student.CodeInt >= criteria.StudentCodeFrom);
            }               
            if (criteria.StudentCodeTo != null)
            {
                registrationCourseQuery = registrationCourseQuery.Where( x => x.Student.CodeInt <= criteria.StudentCodeTo);
            }
            if (criteria.ResidentTypeId > 0)
            {
                registrationCourseQuery = registrationCourseQuery.Where(x => x.Student.ResidentTypeId == criteria.ResidentTypeId);
            }
            if (criteria.ResidentTypeIds != null && criteria.ResidentTypeIds.Any())
            {
                registrationCourseQuery = registrationCourseQuery.Where(x => criteria.ResidentTypeIds.Contains(x.Student.ResidentTypeId));
            }
            if (criteria.AdmissionTypeId > 0)
            {
                registrationCourseQuery = registrationCourseQuery.Where(x => x.Student.AdmissionInformation.AdmissionTypeId == criteria.AdmissionTypeId);
            }
            if (criteria.AdmissionTypeIds != null && criteria.AdmissionTypeIds.Any())
            {
                registrationCourseQuery = registrationCourseQuery.Where(x => criteria.AdmissionTypeIds.Contains(x.Student.AdmissionInformation.AdmissionTypeId ?? 0));
            }
            if (criteria.StudentFeeGroupId > 0)
            {
                registrationCourseQuery = registrationCourseQuery.Where(x => x.Student.StudentFeeGroupId == criteria.StudentFeeGroupId);
            }
            if (criteria.StudentFeeGroupIds != null && criteria.StudentFeeGroupIds.Any())
            {
                registrationCourseQuery = registrationCourseQuery.Where(x => criteria.StudentFeeGroupIds.Contains(x.Student.StudentFeeGroupId ?? 0));
            }
            if (criteria.TermIds != null && criteria.TermIds.Any())
            {
                registrationCourseQuery = registrationCourseQuery.Where(x => criteria.TermIds.Contains(x.Student.AdmissionInformation.AdmissionTermId ?? 0));
            }

            var allRegistrationCourseList = registrationCourseQuery.ToList();

            var registrationCourseList = allRegistrationCourseList.Where(x => x.Status != "d")
                                                                  .ToList();

            var termIdList = allRegistrationCourseList.Select(x => x.TermId)
                                                      .Distinct()
                                                      .ToList();
            var termList = _db.Terms.AsNoTracking()
                                    .IgnoreQueryFilters()
                                    .Where(x => termIdList.Contains(x.Id))
                                    .ToList();
            var courseIdList = allRegistrationCourseList.Select(x => x.CourseId)
                                                        .Distinct()
                                                        .ToList();
            var courseList = _db.Courses.AsNoTracking()
                                        .IgnoreQueryFilters()
                                        .Where(x => courseIdList.Contains(x.Id))
                                        .ToList();

            var studentIdList = allRegistrationCourseList.Select(x => x.StudentId)
                                                         .Distinct()
                                                         .ToList();

            var studentList = _db.Students.AsNoTracking()
                                          .IgnoreQueryFilters()
                                          .Include(x => x.AcademicInformation)
                                              .ThenInclude(x => x.Department)
                                          .Include(x => x.AcademicInformation)
                                              .ThenInclude(x => x.Faculty)
                                          .Include(x => x.Title)
                                          .Include(x => x.Nationality)
                                          .Include(x => x.StudentFeeType)
                                          .Where(x => studentIdList.Contains(x.Id))
                                          .ToList();

            var advisorList = _db.AdvisorStudents.AsNoTracking()
                                                 .Include(x => x.Instructor)
                                                     .ThenInclude(x => x.Title)
                                                 .Where(x =>  x.IsActive
                                                                  && studentIdList.Contains(x.StudentId))
                                                 .ToList();

            var relatedInvoicesList = _db.Invoices.AsNoTracking()
                                                  .IgnoreQueryFilters()
                                                  .Include(x => x.InvoiceItems)
                                                  .Where(x => x.IsActive
                                                                  && !x.IsCancel
                                                                  && x.TermId == criteria.TermId
                                                                  && studentIdList.Contains(x.StudentId.Value))
                                                  .ToList();

            //TODO: logic to mark all these invoice 

            var invoiceIdList = relatedInvoicesList.Select(x => x.Id)
                                                   .ToList();
            var registrationCourseIdList = registrationCourseList.Select(x => x.Id)
                                                                .ToList();

            var relatedInvoiceItemsList = _db.InvoiceItems.AsNoTracking()
                                                          .IgnoreQueryFilters()
                                                          .Where(x => invoiceIdList.Contains(x.InvoiceId))
                                                          .ToList();

            var registrationCourseRelatedInvoiceItemsList = relatedInvoiceItemsList.Where(x => registrationCourseIdList.Contains(x.RegistrationCourseId ?? 0))
                                                                                   .ToList();
            if(criteria.IsConvertScholarAsAnotherFeeType)
            {
                registrationCourseRelatedInvoiceItemsList = registrationCourseRelatedInvoiceItemsList.Select(x =>
                {
                    if (x.ScholarshipStudentId.HasValue && x.ScholarshipStudentId > 0 )
                    {
                        x.FeeItemId = SPECIAL_FEE_TYPE_ID_SCHOLARSHIP; //hardcoded id for display on report
                    }
                    return x;
                }).ToList();
            }

            var reportItems = (from registrationCourse in registrationCourseList
                               join student in studentList on registrationCourse.StudentId equals student.Id
                               join term in termList on registrationCourse.TermId equals term.Id
                               join course in courseList on registrationCourse.CourseId equals course.Id
                               join invoiceItem in registrationCourseRelatedInvoiceItemsList on registrationCourse.Id equals invoiceItem.RegistrationCourseId into invoiceItems
                               from invoiceItem in invoiceItems.DefaultIfEmpty()
                               join invoice in relatedInvoicesList on invoiceItem?.InvoiceId equals invoice.Id into invoices
                               from invoice in invoices.DefaultIfEmpty()
                               join advisor in advisorList on registrationCourse.StudentId equals advisor.StudentId into advisors
                               from advisor in advisors.DefaultIfEmpty()
                               //where invoiceItem != null
                               group new
                               {
                                   registrationCourse,
                                   invoiceItem,
                                   course
                               }
                               by new
                               {
                                   student,
                                   invoice,
                                   //registrationCourse.IsPaid,
                                   IsPaid = invoice?.IsPaid ?? false,
                                   term,
                                   advisor
                               }
                               into g
                               select new RegistrationResultWithAmountAndCreditReportItemViewModel
                               {
                                   AdvisorFirstName = g.Key.advisor?.Instructor?.FirstNameEn ?? "",
                                   AdvisorLastName = g.Key.advisor?.Instructor?.LastNameEn ?? "",
                                   AdvisorTitle = g.Key.advisor?.Instructor?.Title?.NameEn ?? "",
                                   ConfirmStatus = g.Any(x => x.invoiceItem != null),
                                   Department = g.Key.student?.AcademicInformation?.Department?.Code ?? "",
                                   Faculty = g.Key.student?.AcademicInformation?.Faculty?.ShortNameEn ?? "",
                                   FirstRegistrationDate = g.Min(x => x.registrationCourse.CreatedAt),
                                   InvoiceId = g.Key.invoice?.Id ?? 0,
                                   InvoiceNumber = g.Key.invoice?.Number ?? "",
                                   InvoiceType = g.Key.invoice?.TypeText ?? "",
                                   InvoiceDateTime = g.Key.invoice?.CreatedAt ?? null,
                                   Items = g.Key.invoice != null ? (from item in g.Key.invoice.InvoiceItems
                                                                    group item by new
                                                                    {
                                                                        item.FeeItemId,
                                                                        item.FeeItemName,
                                                                        item.ScholarshipStudentId
                                                                    } into itemGroup
                                                                    select new RegistrationResultWithAmountAndCreditReportItemViewModel.Item
                                                                    {
                                                                        FeeItemId = (criteria.IsConvertScholarAsAnotherFeeType && (itemGroup.Key.ScholarshipStudentId ?? 0) > 0) ?
                                                                                        SPECIAL_FEE_TYPE_ID_SCHOLARSHIP :
                                                                                        itemGroup.Key.FeeItemId,
                                                                        FeeName = itemGroup.Key.FeeItemName,
                                                                        Amount = itemGroup.Sum(x => x.Amount - x.ScholarshipAmount),
                                                                        DiscountAmount = itemGroup.Sum(x => x.DiscountAmount),
                                                                        TotalAmount = itemGroup.Sum(x => x.TotalAmount)
                                                                    }).ToList()
                                                                  : new List<RegistrationResultWithAmountAndCreditReportItemViewModel.Item>(),
                                   IsPaid = g.Key.IsPaid,
                                   LastPaymentDate = g.Key.IsPaid && g.Key.invoice != null ? g.Key.invoice.UpdatedAt : null,
                                   StudentId = g.Key.student?.Id ?? Guid.Empty,
                                   StudentBatch = g.Key.student?.AcademicInformation?.Batch ?? 0,
                                   StudentCode = g.Key.student.Code,
                                   StudentCitizenNumber = g.Key.student.CitizenNumber,
                                   StudentTitleEn = g.Key.student.Title?.NameEn ?? "",
                                   StudentFirstNameEn = g.Key.student.FirstNameEn,
                                   StudentLastNameEn = g.Key.student.LastNameEn,
                                   StudentTitleTh = g.Key.student.Title?.NameTh ?? "",
                                   StudentFirstNameTh = g.Key.student.FirstNameTh,
                                   StudentLastNameTh = g.Key.student.LastNameTh,
                                   StudentNationality = g.Key.student.Nationality?.NameEn ?? "",
                                   StudentFeeType = g.Key.student.StudentFeeType?.NameEn ?? "",
                                   StudentTelephoneNumber = g.Key.student.TelephoneNumber1 ?? "",
                                   StudentStatus = g.Key.student.StudentStatusText,
                                   Term = g.Key.term.TermText,
                                   TotalAmount = g.Key.invoice?.TotalAmount ?? 0,
                                   TotalRelatedCourse = g.Select(x => x.course.Id).Distinct().Count(),
                                   TotalRelatedCredit = g.Select(x => new
                                                                      {
                                                                          x.course.Id,
                                                                          x.course.RegistrationCredit
                                   }   
                                                                ).Distinct().Sum(x => x.RegistrationCredit)
                               }).ToList();

            #region GET Invoice not have registration course [or only deleted course]
            //
            var reportItemsInvoiceIds = reportItems.Where(x => x.InvoiceId > 0)
                                                   .Select(x => x.InvoiceId)
                                                   .Distinct()
                                                   .ToList();
            var noRegistrationCourseInvoices = relatedInvoicesList.Where(x => !reportItemsInvoiceIds.Contains(x.Id))
                                                                  .ToList();

            var moreReportItems = (from invoice in noRegistrationCourseInvoices
                                   join student in studentList on invoice.StudentId equals student.Id
                                   join term in termList on invoice.TermId equals term.Id
                                   join advisor in advisorList on invoice.StudentId equals advisor.StudentId into advisors
                                   from advisor in advisors.DefaultIfEmpty()
                                   select new RegistrationResultWithAmountAndCreditReportItemViewModel
                                   {
                                       AdvisorFirstName = advisor?.Instructor?.FirstNameEn ?? "",
                                       AdvisorLastName = advisor?.Instructor?.LastNameEn ?? "",
                                       AdvisorTitle = advisor?.Instructor?.Title?.NameEn ?? "",
                                       ConfirmStatus = true,
                                       Department = student.AcademicInformation?.Department?.Code ?? "",
                                       Faculty = student.AcademicInformation?.Faculty?.ShortNameEn ?? "",
                                       FirstRegistrationDate = null,
                                       InvoiceId = invoice.Id,
                                       InvoiceNumber = invoice.Number,
                                       InvoiceType = invoice.TypeText,
                                       InvoiceDateTime = invoice.CreatedAt,
                                       Items =  (from item in invoice.InvoiceItems
                                                 group item by new
                                                 {
                                                     item.FeeItemId,
                                                     item.FeeItemName,
                                                     item.ScholarshipStudentId
                                                 } into itemGroup
                                                 select new RegistrationResultWithAmountAndCreditReportItemViewModel.Item
                                                 {
                                                     FeeItemId = (criteria.IsConvertScholarAsAnotherFeeType && (itemGroup.Key.ScholarshipStudentId ?? 0) > 0) ? 
                                                                    SPECIAL_FEE_TYPE_ID_SCHOLARSHIP : 
                                                                    itemGroup.Key.FeeItemId,
                                                     FeeName = itemGroup.Key.FeeItemName,
                                                     Amount = itemGroup.Sum(x => x.Amount - x.ScholarshipAmount),
                                                     DiscountAmount = itemGroup.Sum(x => x.DiscountAmount),
                                                     TotalAmount = itemGroup.Sum(x => x.TotalAmount)
                                                 }).ToList(),
                                       IsPaid = invoice.IsPaid, 
                                       LastPaymentDate = invoice.IsPaid ? invoice.UpdatedAt : null,
                                       StudentId = student?.Id ?? Guid.Empty,
                                       StudentBatch = student.AcademicInformation?.Batch ?? 0,
                                       StudentCode = student.Code,
                                       StudentCitizenNumber = student.CitizenNumber,
                                       StudentTitleEn = student.Title?.NameEn ?? "",
                                       StudentFirstNameEn = student.FirstNameEn,
                                       StudentLastNameEn = student.LastNameEn,
                                       StudentTitleTh = student.Title?.NameTh ?? "",
                                       StudentFirstNameTh = student.FirstNameTh,
                                       StudentLastNameTh = student.LastNameTh,
                                       StudentNationality = student.Nationality?.NameEn ?? "",
                                       StudentFeeType = student.StudentFeeType?.NameEn ?? "",
                                       StudentStatus = student.StudentStatusText,
                                       StudentTelephoneNumber = student.TelephoneNumber1 ?? "",
                                       Term = term.TermText,
                                       TotalAmount = invoice.TotalAmount,
                                       TotalRelatedCourse = 0,
                                       TotalRelatedCredit = 0
                                  }).ToList();
            reportItems.AddRange(moreReportItems);
            #endregion


            #region GET Unconfirm Drop only 
            var existedUnconfirmedStudentId = reportItems.Where(x => x.InvoiceTypeText == Invoice.TYPE_UNCONFIRM)
                                             .Select(x => x.StudentId)
                                             .ToList();
            var registrationCourseDropList = allRegistrationCourseList.Where(x => x.Status == "d"
                                                                                      && x.IsPaid
                                                                                      && !existedUnconfirmedStudentId.Contains(x.StudentId)
                                                                            )
                                                                      .ToList();
            var registrationCourseDropIdList = registrationCourseDropList.Select(x => x.Id)
                                                                         .ToList();
            var registrationCourseDropRelatedInvoiceItemsList = relatedInvoiceItemsList.Where(x => registrationCourseDropIdList.Contains(x.RegistrationCourseId ?? 0))
                                                                                       .ToList();
            if (criteria.IsConvertScholarAsAnotherFeeType)
            {
                registrationCourseDropRelatedInvoiceItemsList = registrationCourseDropRelatedInvoiceItemsList.Select(x =>
                {
                    if (x.ScholarshipStudentId.HasValue && x.ScholarshipStudentId > 0)
                    {
                        x.FeeItemId = SPECIAL_FEE_TYPE_ID_SCHOLARSHIP; //hardcoded id for display on report
                    }
                    return x;
                }).ToList();
            }

            var dropReportItems = (from registrationCourse in registrationCourseDropList
                                   join student in studentList on registrationCourse.StudentId equals student.Id
                                   join term in termList on registrationCourse.TermId equals term.Id
                                   join course in courseList on registrationCourse.CourseId equals course.Id
                                   join invoiceItem in registrationCourseDropRelatedInvoiceItemsList on
                                        new
                                        {
                                            RegistrationCourseId = registrationCourse.Id,
                                            RefundInvoiceItem = true
                                        } 
                                        equals 
                                        new
                                        {
                                            RegistrationCourseId = invoiceItem.RegistrationCourseId ?? 0,
                                            RefundInvoiceItem = invoiceItem.TotalAmount < 0
                                        } into invoiceItems
                                   from invoiceItem in invoiceItems.DefaultIfEmpty()
                                   join invoice in relatedInvoicesList on invoiceItem?.InvoiceId equals invoice.Id into invoices
                                   from invoice in invoices.DefaultIfEmpty()
                                   join advisor in advisorList on registrationCourse.StudentId equals advisor.StudentId into advisors
                                   from advisor in advisors.DefaultIfEmpty()
                                   where invoiceItem == null
                                   group new
                                   {
                                       registrationCourse,
                                       invoiceItem,
                                       course
                                   }
                                   by new
                                   {
                                       student,
                                       invoice,
                                       //registrationCourse.IsPaid,
                                       IsPaid = invoice?.IsPaid ?? false,
                                       term,
                                       advisor
                                   }
                                   into g
                                   select new RegistrationResultWithAmountAndCreditReportItemViewModel
                                   {
                                       AdvisorFirstName = g.Key.advisor?.Instructor?.FirstNameEn ?? "",
                                       AdvisorLastName = g.Key.advisor?.Instructor?.LastNameEn ?? "",
                                       AdvisorTitle = g.Key.advisor?.Instructor?.Title?.NameEn ?? "",
                                       ConfirmStatus = g.Any(x => x.invoiceItem != null),
                                       Department = g.Key.student?.AcademicInformation?.Department?.Code ?? "",
                                       Faculty = g.Key.student?.AcademicInformation?.Faculty?.ShortNameEn ?? "",
                                       FirstRegistrationDate = g.Min(x => x.registrationCourse.CreatedAt),
                                       InvoiceId = g.Key.invoice?.Id ?? 0,
                                       InvoiceNumber = g.Key.invoice?.Number ?? "",
                                       InvoiceType = g.Key.invoice?.TypeText ?? "",
                                       InvoiceDateTime = g.Key.invoice?.CreatedAt ?? null,
                                       Items = g.Key.invoice != null ? (from item in g.Key.invoice.InvoiceItems
                                                                        group item by new
                                                                        {
                                                                            item.FeeItemId,
                                                                            item.FeeItemName,
                                                                            item.ScholarshipStudentId
                                                                        } into itemGroup
                                                                        select new RegistrationResultWithAmountAndCreditReportItemViewModel.Item
                                                                        {
                                                                            FeeItemId = (criteria.IsConvertScholarAsAnotherFeeType && (itemGroup.Key.ScholarshipStudentId ?? 0) > 0) ?
                                                                                            SPECIAL_FEE_TYPE_ID_SCHOLARSHIP :
                                                                                            itemGroup.Key.FeeItemId,
                                                                            FeeName = itemGroup.Key.FeeItemName,
                                                                            Amount = itemGroup.Sum(x => x.Amount - x.ScholarshipAmount),
                                                                            DiscountAmount = itemGroup.Sum(x => x.DiscountAmount),
                                                                            TotalAmount = itemGroup.Sum(x => x.TotalAmount)
                                                                        }).ToList()
                                                                      : new List<RegistrationResultWithAmountAndCreditReportItemViewModel.Item>(),
                                       IsPaid = g.Key.IsPaid,
                                       LastPaymentDate = g.Key.IsPaid && g.Key.invoice != null ? g.Key.invoice.UpdatedAt : null,
                                       StudentId = g.Key.student?.Id ?? Guid.Empty,
                                       StudentBatch = g.Key.student?.AcademicInformation?.Batch ?? 0,
                                       StudentCode = g.Key.student.Code,
                                       StudentCitizenNumber = g.Key.student.CitizenNumber,
                                       StudentTitleEn = g.Key.student.Title?.NameEn ?? "",
                                       StudentFirstNameEn = g.Key.student.FirstNameEn,
                                       StudentLastNameEn = g.Key.student.LastNameEn,
                                       StudentTitleTh = g.Key.student.Title?.NameTh ?? "",
                                       StudentFirstNameTh = g.Key.student.FirstNameTh,
                                       StudentLastNameTh = g.Key.student.LastNameTh,
                                       StudentNationality = g.Key.student.Nationality?.NameEn ?? "",
                                       StudentFeeType = g.Key.student.StudentFeeType?.NameEn ?? "",
                                       StudentTelephoneNumber = g.Key.student.TelephoneNumber1 ?? "",
                                       StudentStatus = g.Key.student.StudentStatusText,
                                       Term = g.Key.term.TermText,
                                       TotalAmount = g.Key.invoice?.TotalAmount ?? 0,
                                       TotalRelatedCourse = 0, // g.Select(x => x.course.Id).Distinct().Count(),
                                       TotalRelatedCredit = 0, /*g.Select(x => new
                                       {
                                           x.course.Id,
                                           x.course.RegistrationCredit
                                       }
                                                                    ).Distinct().Sum(x => x.RegistrationCredit)
                                       */
                                   }).ToList();

            reportItems.AddRange(dropReportItems);
            #endregion

            //filter more
            if (!criteria.IsGroupStudent && !string.IsNullOrEmpty(criteria.InvoiceType) && !"All".Equals(criteria.InvoiceType))
            {
                if (Invoice.TYPE_REGISTRATION.Equals(criteria.InvoiceType))
                {
                    reportItems = reportItems.Where(x => Invoice.TYPE_REGISTRATION.Equals(x.InvoiceType))
                                             .ToList();
                }
                else if (Invoice.TYPE_ADD_DROP.Equals(criteria.InvoiceType))
                {
                    reportItems = reportItems.Where(x => Invoice.TYPE_ADD_DROP.Equals(x.InvoiceType))
                                             .ToList();
                }
                else
                {
                    reportItems = reportItems.Where(x => string.IsNullOrEmpty(x.InvoiceType))
                                             .ToList();
                }
            }

            if (criteria.ReceiptDateFrom.HasValue)
            {
                reportItems = reportItems.Where(x => x.InvoiceDateTime.HasValue && x.InvoiceDateTime >= criteria.ReceiptDateFrom).ToList();
            }
            if (criteria.ReceiptDateTo.HasValue)
            {
                reportItems = reportItems.Where(x => x.InvoiceDateTime.HasValue && x.InvoiceDateTime < criteria.ReceiptDateTo.Value.Date.AddDays(1)).ToList();
            }

            if (criteria.PaidDateFrom.HasValue)
            {
                reportItems = reportItems.Where(x => x.LastPaymentDate.HasValue && x.LastPaymentDate >= criteria.PaidDateFrom).ToList();
            }
            if (criteria.PaidDateTo.HasValue)
            {
                reportItems = reportItems.Where(x => x.LastPaymentDate.HasValue && x.LastPaymentDate < criteria.PaidDateTo.Value.Date.AddDays(1)).ToList();
            }

            if (!string.IsNullOrEmpty(criteria.InvoiceRefundType) && "All" != criteria.InvoiceRefundType)
            {
                if (Invoice.REFUND_TYPE_NON_REFUND.Equals(criteria.InvoiceRefundType))
                {
                    reportItems = reportItems.Where(x => x.TotalAmount > 0).ToList();
                }
                else if (Invoice.REFUND_TYPE_REFUND.Equals(criteria.InvoiceRefundType))
                {
                    reportItems = reportItems.Where(x => x.TotalAmount < 0).ToList();
                }
                else if (Invoice.REFUND_TYPE_BALANCE.Equals(criteria.InvoiceRefundType))
                {
                    reportItems = reportItems.Where(x => x.TotalAmount == 0).ToList();
                }
            }

            // Payment Method
            if (criteria.IsQueryPaymentMethod)
            {
                var invIds = reportItems.Select(x => x.InvoiceId).Distinct().ToList();
                var invCode = reportItems.Select(x => x.InvoiceNumber).Distinct().ToList();
                var allPaymentTransaction = _db.BankPaymentResponses.AsNoTracking()
                                                                    .Where(x => (invIds.Contains(x.InvoiceId ?? 0) || invCode.Contains(x.Number))
                                                                        && x.IsPaymentSuccess)
                                                                    .ToList();
                foreach (var record in reportItems)
                {
                    if (!record.IsPaid || record.InvoiceId < 1)
                    {
                        continue;
                    }
                    var paymentResponse = allPaymentTransaction.Where(x => x.InvoiceId == record.InvoiceId || x.Number == record.InvoiceNumber);
                    if (paymentResponse.Any(x => x.RawResponse.Contains("Domestic Transfers")))
                    {
                        record.InvoicePaymentMethod = "QR";
                    }
                    //"paymentMethod":"WECHATPAY"
                    else if (paymentResponse.Any(x => x.RawResponse.Contains("\"paymentMethod\":\"WECHATPAY\"")))
                    {
                        record.InvoicePaymentMethod = "WeChat";
                    }
                    else if (paymentResponse.Any(x => x.RawResponse.Contains("\"channelCode\":\"PMH\"")))
                    {
                        record.InvoicePaymentMethod = "PMH";
                    }
                    else if (paymentResponse.Any(x => x.RawResponse.Contains("\"channelCode\"")))
                    {
                        record.InvoicePaymentMethod = "CreditCard";
                    }
                    else if (paymentResponse.Any(x => x.RawResponse.Contains("Manual Update") 
                                                         || x.RawResponse.Contains("MANUAL TOTAL AMOUNT LESS THAN ZERO"))
                            )
                    {
                        record.InvoicePaymentMethod = "Manual Update";
                    }
                    else
                    {
                        record.InvoicePaymentMethod = "Unidentified";
                    }
                }
            }

            //Receipt
            if (criteria.IsQueryReceipt)
            {
                var invIds = reportItems.Select(x => x.InvoiceId).Distinct().ToList();
                var allReceipt = _db.Receipts.IgnoreQueryFilters()
                                             .AsNoTracking()
                                             .Where(x => invIds.Contains(x.InvoiceId ?? 0)
                                                && x.IsActive
                                                && !x.IsCancel)
                                             .ToList();
                foreach (var record in reportItems)
                {
                    if (!record.IsPaid || record.InvoiceId < 1)
                    {
                        continue;
                    }
                    var receicpt = allReceipt.FirstOrDefault(x => x.InvoiceId == record.InvoiceId);
                    if (receicpt != null)
                    {
                        record.ReceiptNumber = receicpt.Number;
                        record.ReceiptDateTime = receicpt.CreatedAt;
                    }
                }
            }

            if (criteria.IsGroupStudent)
            {
                reportItems = reportItems.Select(x =>
                {
                    x.IsPaid = x.TotalAmount <= 0 && x.InvoiceTypeText != Invoice.TYPE_UNCONFIRM ? true : x.IsPaid;
                    return x;
                }).ToList();
                var groupItems = from item in reportItems
                                 group item by new
                                 {
                                     item.StudentBatch,
                                     item.StudentCitizenNumber,
                                     item.StudentCode,
                                     item.StudentFeeType,
                                     item.StudentFirstNameEn,
                                     item.StudentFirstNameTh,
                                     item.StudentLastNameEn,
                                     item.StudentLastNameTh,
                                     item.StudentNationality,
                                     item.StudentStatus,
                                     item.StudentTitleEn,
                                     item.StudentTitleTh,
                                     item.AdvisorFirstName,
                                     item.AdvisorLastName,
                                     item.AdvisorTitle,
                                     item.Department,
                                     item.Faculty,
                                     item.Term,
                                     //item.IsPaid
                                 } into itemGroup
                                 select new RegistrationResultWithAmountAndCreditReportItemViewModel
                                 {
                                     AdvisorFirstName = itemGroup.Key.AdvisorFirstName,
                                     AdvisorLastName = itemGroup.Key.AdvisorLastName,
                                     AdvisorTitle = itemGroup.Key.AdvisorTitle,
                                     ConfirmStatus = itemGroup.Min(x => x.ConfirmStatus),
                                     Department = itemGroup.Key.Department,
                                     Faculty = itemGroup.Key.Faculty,
                                     FirstRegistrationDate = itemGroup.Min(x => x.FirstRegistrationDate),
                                     InvoiceId = 0,
                                     InvoiceNumber = itemGroup.Select(x => x.InvoiceNumber).Aggregate((current, next) => current + ", " + next),
                                     InvoiceType = itemGroup.Any(x => x.InvoiceId > 0) ? Invoice.TYPE_REGISTRATION : "",
                                     InvoiceDateTime = itemGroup.Min(x => x.InvoiceDateTime),
                                     //IsPaid = itemGroup.Min(x => x.IsPaid), //only group should use this
                                     //IsPaid = itemGroup.Min(x => x.TotalAmount <= 0 ? true : x.IsPaid), //hack for p'trong to not see, refund/balance as unpaid
                                     IsPaid = itemGroup.Any(x => x.IsPaid),
                                     //IsPaid = itemGroup.Key.IsPaid,
                                     Items = itemGroup.SelectMany(x => x.Items).ToList(),
                                     LastPaymentDate = itemGroup.Max(x => x.LastPaymentDate),
                                     StudentBatch = itemGroup.Key.StudentBatch,
                                     StudentCitizenNumber = itemGroup.Key.StudentCitizenNumber,
                                     StudentCode = itemGroup.Key.StudentCode,
                                     StudentFeeType = itemGroup.Key.StudentFeeType,
                                     StudentFirstNameEn = itemGroup.Key.StudentFirstNameEn,
                                     StudentFirstNameTh = itemGroup.Key.StudentFirstNameTh,
                                     StudentLastNameEn = itemGroup.Key.StudentLastNameEn,
                                     StudentLastNameTh = itemGroup.Key.StudentLastNameTh,
                                     StudentNationality = itemGroup.Key.StudentNationality,
                                     StudentStatus = itemGroup.Key.StudentStatus,
                                     StudentTitleEn = itemGroup.Key.StudentTitleEn,
                                     StudentTitleTh = itemGroup.Key.StudentTitleTh,
                                     Term = itemGroup.Key.Term,
                                     TotalAmount = itemGroup.Sum(x => x.TotalAmount),
                                     TotalRelatedCourse = itemGroup.Sum(x => x.TotalRelatedCourse),
                                     TotalRelatedCredit = itemGroup.Sum(x => x.TotalRelatedCredit)
                                 };
                report.ReportItems = groupItems.OrderBy(x => x.Term).ThenBy(x => x.StudentCode).ToList();
            } 
            else {
                report.ReportItems = reportItems.OrderBy(x => x.Term).ThenBy(x => x.StudentCode).ToList();
            }

            //filter more similar with above but delay to do it here
            if (criteria.IsGroupStudent && !string.IsNullOrEmpty(criteria.InvoiceType) && !"All".Equals(criteria.InvoiceType))
            {
                if (Invoice.TYPE_REGISTRATION.Equals(criteria.InvoiceType))
                {
                    report.ReportItems = report.ReportItems.Where(x => Invoice.TYPE_REGISTRATION.Equals(x.InvoiceType))
                                                           .ToList();
                }
                else if (Invoice.TYPE_ADD_DROP.Equals(criteria.InvoiceType))
                {
                    report.ReportItems = report.ReportItems.Where(x => Invoice.TYPE_ADD_DROP.Equals(x.InvoiceType))
                                                           .ToList();
                }
                else
                {
                    report.ReportItems = report.ReportItems.Where(x => string.IsNullOrEmpty(x.InvoiceType))
                                                           .ToList();
                }
            }

            if (!string.IsNullOrEmpty(criteria.IsPaidAdmissionFee))
            {
                if (bool.Parse(criteria.IsPaidAdmissionFee))
                {
                    report.ReportItems = report.ReportItems.Where(x => x.IsPaid)
                                                           .ToList();
                }
                else
                {
                    report.ReportItems = report.ReportItems.Where(x => !x.IsPaid)
                                                           .ToList();
                }
            }

            if (criteria.CreditFrom != null && criteria.CreditFrom > 0)
            {
                report.ReportItems = report.ReportItems.Where(x => x.TotalRelatedCredit >= criteria.CreditFrom).ToList();
            }            
            if (criteria.CreditTo != null && criteria.CreditTo > 0)
            {
                report.ReportItems = report.ReportItems.Where(x => x.TotalRelatedCredit <= criteria.CreditTo).ToList();
            }

            return report;
        }
    }
}