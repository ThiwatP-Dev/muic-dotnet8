using AutoMapper;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Profile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;
using Microsoft.AspNetCore.Authorization;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.USpark;
using KeystoneLibrary.Models.ViewModel;

namespace Keystone.Controllers
{
    [AllowAnonymous]
    public class USparkCalculateFeeController : BaseController
    {
        protected readonly IStudentProvider _studentProvider;
        protected readonly IAdmissionProvider _admissionProvider;
        protected readonly ICacheProvider _cacheProvider;
        protected readonly IAcademicProvider _academicProvider;
        protected readonly IDateTimeProvider _dateTimeProvider;
        protected readonly IReceiptProvider _receiptProvider;
        protected readonly IFeeProvider _feeProvider;
        protected readonly IRegistrationProvider _registrationProvider;

        public USparkCalculateFeeController(ApplicationDbContext db,
                                          IFlashMessage flashMessage,
                                          IMapper mapper,
                                          IStudentProvider studentProvider,
                                          IAdmissionProvider admissionProvider,
                                          ICacheProvider cacheProvider,
                                          IAcademicProvider academicProvider,
                                          IDateTimeProvider dateTimeProvider,
                                          IReceiptProvider receiptProvider,
                                          IFeeProvider feeProvider,
                                          IRegistrationProvider registrationProvider) : base(db, flashMessage, mapper)
        {
            _studentProvider = studentProvider;
            _admissionProvider = admissionProvider;
            _cacheProvider = cacheProvider;
            _academicProvider = academicProvider;
            _dateTimeProvider = dateTimeProvider;
            _receiptProvider = receiptProvider;
            _feeProvider = feeProvider;
            _registrationProvider = registrationProvider;
        }


        [HttpPost]
        public IActionResult UpdatePayment(long KSInvoiceId)
        {
            var result = new USparkCalculateFeeResponse();
            try
            {
                var returnMessage = _feeProvider.GenerateReceiptAfterPayingInUSpark(KSInvoiceId);
                if (string.IsNullOrEmpty(returnMessage))
                {
                    result.Message = "Success";
                    return StatusCode(200, result);
                }
                else
                {
                    result.Message = returnMessage;
                    return StatusCode(400, result);
                }
            }
            catch (Exception e)
            {
                return StatusCode(500, e);
            }
        }

        public IActionResult Checkout(string studentCode, string ksSectionIds, long KSTermId, int addDropCount)
        {
            var result = new USparkCalculateFeeResponse();
            var student = _studentProvider.GetStudentInformationByCode(studentCode);
            bool isLumpsumPayment = student.StudentFeeGroup?.IsLumpsumPayment ?? false;
            var term = _db.Terms.SingleOrDefault(x => x.Id == KSTermId);
            if (student == null)
            {
                result.Message = "Student not found";
                return StatusCode(400, result);
            }
            else if (term == null)
            {
                result.Message = "Term not found";
                return StatusCode(400, result);
            }
            else if (String.IsNullOrEmpty(ksSectionIds))
            {
                result.Message = "Sections not specified";
                return StatusCode(400, result);
            }
            else
            {
                var studentState = _db.StudentStates.SingleOrDefault(x => x.StudentId == student.Id && x.TermId == KSTermId);
                string round = studentState != null && (studentState.State != "REG") && (studentState.State != "PAY_REG") ? "a" : "r";
                var US_RegistrationCourses = GenerateRegistrationCoursesFromAPI(student.Id, KSTermId, ksSectionIds, round);
                if (US_RegistrationCourses.Any())
                {
                    var invoiceItems = _receiptProvider.GetInvoiceItemsForPreview(US_RegistrationCourses, 0);
                    bool isSame = IsSameTermAndTuitionFee(student.Id, KSTermId, invoiceItems, US_RegistrationCourses);
                    DateTime today = DateTime.Today;
                    var latestInvoice = _db.Invoices.FirstOrDefault(x => x.StudentId == student.Id
                                                                         && x.TermId == KSTermId
                                                                         && !x.IsPaid
                                                                         && x.Type != "o"
                                                                         && x.Type != "cr");

                    if (!isSame || (latestInvoice != null && latestInvoice.PaymentExpireAt < today))
                    {
                        // Update Registration Course
                        var addingResults = GenerateAddingResults(KSTermId, US_RegistrationCourses);
                        var newCourses = new List<RegistrationCourse>();
                        var deleteUnpaidCourses = new List<RegistrationCourse>();
                        var deletePaidCourses = new List<RegistrationCourse>();

                        if (!isLumpsumPayment)
                        {
                            // Cancel active invoice
                            _receiptProvider.CancelAddDropInvoices(student.Id, KSTermId);
                        }

                        var modifyCourseResult = _registrationProvider.ModifyRegistrationCourse(student.Id, KSTermId, round, addingResults, out newCourses, out deleteUnpaidCourses, out deletePaidCourses, "r");
                        var addInvoice = new Invoice();
                        var dropInvoice = new Invoice();
                        var dropOrder = new USparkOrder();

                        if (newCourses.Any())
                        {
                            // Create invoice
                            addInvoice = _receiptProvider.AddInvoice(student.Id, KSTermId, newCourses, round);
                        }

                        if (deletePaidCourses.Any())
                        {
                            dropInvoice = _receiptProvider.DropCourses(student.Id, KSTermId, deletePaidCourses, addDropCount);

                            // If there is refund, need to deduct refund percent
                            if (dropInvoice != null && dropInvoice.InvoiceItems.Sum(x => x.Amount) > (addInvoice.InvoiceItems == null ? 0 : addInvoice.InvoiceItems.Sum(x => x.Amount)))
                            {
                                _receiptProvider.UpdateTuitionFeeRefund(dropInvoice, addInvoice);
                            }

                            if (addInvoice.InvoiceItems == null)
                            {
                                addInvoice.InvoiceItems = new List<InvoiceItem>();
                            }

                            foreach (var item in dropInvoice.InvoiceItems)
                            {
                                item.Amount = -item.Amount;
                                item.TotalAmount = -item.TotalAmount;
                            }
                            dropInvoice.Amount = dropInvoice.InvoiceItems.Sum(x => x.Amount);
                            dropInvoice.TotalAmount = dropInvoice.InvoiceItems.Sum(x => x.TotalAmount);
                            _db.SaveChanges();
                        }

                        try
                        {
                            bool IsSkipPenaltyFee = false;
                            if (student.AdmissionInformation.AdmissionType != null)
                            {
                                IsSkipPenaltyFee = student.AdmissionInformation.AdmissionType.NameEn.Contains("inbound");
                            }
                            if (!IsSkipPenaltyFee)
                            {
                                if (addInvoice.Id > 0)
                                {
                                    // Check Late Registration
                                    _receiptProvider.GenerateLateRegistrationFeeItem(addInvoice, student.Id, KSTermId);

                                    // Check Add/Drop fee
                                    _receiptProvider.GenerateAddDropFeeItem(addInvoice, KSTermId, addDropCount);

                                    // Late Payment
                                    _receiptProvider.GenerateLatePaymentFeeItem(addInvoice, KSTermId, addInvoice.Type, today);

                                }
                                else if (dropInvoice.Id > 0)
                                {
                                    // Check Late Registration
                                    _receiptProvider.GenerateLateRegistrationFeeItem(dropInvoice, student.Id, KSTermId);

                                    // Check Add/Drop fee
                                    _receiptProvider.GenerateAddDropFeeItem(dropInvoice, KSTermId, addDropCount);

                                    //no late payment fee when drop only
                                    //// Late Payment
                                    //_receiptProvider.GenerateLatePaymentFeeItem(dropInvoice, KSTermId, addInvoice.Type, today);
                                }
                            }

                            if (addInvoice.Id > 0 && addInvoice.InvoiceItems != null && dropInvoice != null && dropInvoice.InvoiceItems != null && dropInvoice.InvoiceItems.Any())
                            {
                                _receiptProvider.ApplyCreditNote(addInvoice, dropInvoice);
                            }

                            if (addInvoice.Id > 0)
                            {
                                _receiptProvider.UpdateInvoiceReference(addInvoice, student.Code);
                            }
                            else if (dropInvoice.Id > 0)
                            {
                                _receiptProvider.UpdateInvoiceReference(dropInvoice, student.Code);
                            }

                            //Add drop invoiceitems to add invoice to send to Uspark
                            if (isLumpsumPayment)
                            {
                                if (addInvoice != null && addInvoice.InvoiceItems != null && addInvoice.InvoiceItems.Any())
                                {
                                    foreach (var item in addInvoice.InvoiceItems)
                                    {
                                        item.LumpSumAddDrop = "add";
                                    }
                                }
                                if (dropInvoice != null && dropInvoice.InvoiceItems != null && dropInvoice.InvoiceItems.Any())
                                {
                                    foreach (var item in dropInvoice.InvoiceItems)
                                    {
                                        item.LumpSumAddDrop = "drop";
                                    }
                                }
                            }
                            var order = _registrationProvider.GenerateUSparkOrderFromInvoice(studentCode, KSTermId, addInvoice, dropInvoice);
                            result.Result = order;
                            return StatusCode(200, result);
                        }
                        catch (Exception e)
                        {
                            return StatusCode(500, e);
                        }
                    }
                    else
                    {
                        try
                        {
                            result = GetExistingInvoiceForResponse(student, KSTermId, US_RegistrationCourses);
                            return StatusCode(200, result);
                        }
                        catch (Exception e)
                        {
                            return StatusCode(500, e);
                        }
                    }
                }
                else
                {
                    result.Message = "Courses and sections not found.";
                    return StatusCode(400, result);
                }
            }
        }

        public IActionResult CalculateV2(string studentCode, string ksSectionIds, long KSTermId, int addDropCount)
        {
            var result = new USparkCalculateFeeResponse();
            var student = _db.Students.SingleOrDefault(x => x.Code == studentCode);
            var term = _db.Terms.SingleOrDefault(x => x.Id == KSTermId);
            if (student == null)
            {
                result.Message = "Student not found";
                return StatusCode(400, result);
            }
            else if (term == null)
            {
                result.Message = "Term not found";
                return StatusCode(400, result);
            }
            else if (String.IsNullOrEmpty(ksSectionIds))
            {
                result.Message = "Sections not specified";
                return StatusCode(400, result);
            }
            else
            {
                var studentState = _db.StudentStates.SingleOrDefault(x => x.StudentId == student.Id && x.TermId == KSTermId);
                string round = studentState != null && (studentState.State != "REG") && (studentState.State != "PAY_REG") ? "a" : "r";

                var US_RegistrationCourses = GenerateRegistrationCoursesFromAPI(student.Id, KSTermId, ksSectionIds, round);
                if (US_RegistrationCourses.Any())
                {
                    var invoiceItems = _receiptProvider.GetInvoiceItemsForPreview(US_RegistrationCourses, 0);
                    bool isSame = IsSameTermAndTuitionFee(student.Id, KSTermId, invoiceItems, US_RegistrationCourses);
                    DateTime today = DateTime.Today;
                    var latestInvoice = _db.Invoices.FirstOrDefault(x => x.StudentId == student.Id
                                                                         && x.TermId == KSTermId
                                                                         && !x.IsPaid
                                                                         && x.Type != "o"
                                                                         && x.Type != "cr");

                    if (!isSame || (latestInvoice != null && latestInvoice.PaymentExpireAt < today))
                    {
                        // Update Registration Course
                        var addingResults = GenerateAddingResults(KSTermId, US_RegistrationCourses);
                        var newCourses = new List<RegistrationCourse>();
                        var deletePaidCourses = new List<RegistrationCourse>();
                        _registrationProvider.SimulateModifyRegistrationCourse(student.Id, KSTermId, addingResults, out newCourses, out deletePaidCourses);

                        if (!newCourses.Any() && !deletePaidCourses.Any())
                        {
                            result = GetSimulateExistingInvoiceForResponse(student, KSTermId, US_RegistrationCourses);
                            return StatusCode(200, result);
                        }

                        var addInvoice = new Invoice();
                        var dropInvoice = new Invoice();
                        var dropOrder = new USparkOrder();

                        if (newCourses.Any())
                        {
                            // Create invoice
                            addInvoice = _receiptProvider.SimulateAddInvoice(student.Id, KSTermId, newCourses, round);
                        }

                        if (deletePaidCourses.Any())
                        {
                            dropInvoice = _receiptProvider.SimulateAddInvoiceCreditNote(student.Id, KSTermId, deletePaidCourses);

                            // If there is refund, need to deduct refund percent
                            if (dropInvoice != null && dropInvoice.InvoiceItems.Sum(x => x.Amount) > (addInvoice.InvoiceItems == null ? 0 : addInvoice.InvoiceItems.Sum(x => x.Amount)))
                            {
                                _receiptProvider.SimulateUpdateTuitionFeeRefund(dropInvoice, addInvoice);
                            }

                            if (addInvoice.InvoiceItems == null)
                            {
                                addInvoice.InvoiceItems = new List<InvoiceItem>();
                            }

                            foreach (var item in dropInvoice.InvoiceItems)
                            {
                                item.Amount = -item.Amount;
                                item.TotalAmount = -item.TotalAmount;
                            }
                            dropInvoice.Amount = dropInvoice.InvoiceItems.Sum(x => x.Amount);
                            dropInvoice.TotalAmount = dropInvoice.InvoiceItems.Sum(x => x.TotalAmount);
                        }

                        try
                        {
                            bool IsSkipPenaltyFee = false;
                            if (student.AdmissionInformation.AdmissionType != null)
                            {
                                IsSkipPenaltyFee = student.AdmissionInformation.AdmissionType.NameEn.Contains("inbound");
                            }
                            if (!IsSkipPenaltyFee)
                            {
                                if (addInvoice.InvoiceItems != null && addInvoice.InvoiceItems.Count > 0)
                                {
                                    // Check Late Registration
                                    _receiptProvider.SimulateGenerateLateRegistrationFeeItem(addInvoice, student.Id, KSTermId);

                                    // Check Add/Drop fee
                                    _receiptProvider.SimulateGenerateAddDropFeeItem(addInvoice, KSTermId, addDropCount);

                                    // Late Payment
                                    _receiptProvider.SimulateGenerateLatePaymentFeeItem(addInvoice, KSTermId, addInvoice.Type, today);
                                }
                                else if (dropInvoice.InvoiceItems != null && dropInvoice.InvoiceItems.Count > 0)
                                {
                                    // Check Late Registration
                                    _receiptProvider.SimulateGenerateLateRegistrationFeeItem(dropInvoice, student.Id, KSTermId);

                                    // Check Add/Drop fee
                                    _receiptProvider.SimulateGenerateAddDropFeeItem(dropInvoice, KSTermId, addDropCount);

                                    //no late payment fee if drop only
                                    //// Late Payment
                                    //_receiptProvider.SimulateGenerateLatePaymentFeeItem(dropInvoice, KSTermId, addInvoice.Type, today);
                                }
                            }

                            if (addInvoice.InvoiceItems != null
                                && addInvoice.InvoiceItems.Count > 0
                                && dropInvoice.InvoiceItems != null
                                && dropInvoice.InvoiceItems.Count > 0)
                            {
                                _receiptProvider.SimulateApplyCreditNote(addInvoice, dropInvoice);
                            }

                            //Add drop invoiceitems to add invoice to send to Uspark
                            var order = _registrationProvider.SimulateGenerateUSparkOrderFromInvoice(studentCode, KSTermId, addInvoice, dropInvoice);
                            result.Result = order;
                            return StatusCode(200, result);
                        }
                        catch (Exception e)
                        {
                            return StatusCode(500, e);
                        }
                    }
                    else
                    {
                        try
                        {
                            result = GetSimulateExistingInvoiceForResponse(student, KSTermId, US_RegistrationCourses);
                            return StatusCode(200, result);
                        }
                        catch (Exception e)
                        {
                            return StatusCode(500, e);
                        }
                    }
                }
                else
                {
                    result.Message = "Courses and sections not found.";
                    return StatusCode(400, result);
                }
            }
        }

        [HttpPost]
        public IActionResult CalculateV3([FromBody] USparkCalculateTuitionRequestViewModel request)
        {
            _registrationProvider.UpsertRegistrationCourses(request.Preregistrations, request.StudentCode, request.KSTermId, request.StudentCode);
            _registrationProvider.UpsertRegistrationLog(request.RegistrationLogs, request.StudentCode, request.KSTermId);

            var response = _feeProvider.GetUsparkTuitionFeeResponse(request.StudentCode, request.KSTermId);

            return StatusCode(200, response);
        }

        [HttpGet("students/{studentCode}/pending/courses")]
        public IActionResult CalculateV3Courses(string studentCode, long ksTermId)
        {
            var item = _feeProvider.GetUsparkCourseChanges(studentCode, ksTermId);

            return StatusCode(200, item);
        }

        [HttpPost]
        public async Task<IActionResult> CheckOutV3([FromBody] USparkCalculateTuitionRequestViewModel request)
        {
            _registrationProvider.UpsertRegistrationCourses(request.Preregistrations, request.StudentCode, request.KSTermId, request.StudentCode);
            _registrationProvider.UpsertRegistrationLog(request.RegistrationLogs, request.StudentCode, request.KSTermId);

            var courseTuition = _feeProvider.CalculateTuitionFeeV3(request.StudentCode, request.KSTermId)
                                            .ToList();

            // First payment require at least one courses
            if(courseTuition.Any() && courseTuition.Any(x => x.Type == "fee"))
            {
                // allow if there's one-time-fee
                if(!courseTuition.Any(x => x.Type == "one-time-fee")
                    && !courseTuition.Any(x => x.RegistrationCourseId.HasValue))
                {
                    return StatusCode(409, "Student must select at least one course for first registration.");
                }
            }

            var penaltyFee = _feeProvider.CalculatePenaltyFee(request.StudentCode, request.KSTermId, courseTuition)
                                         .ToList();

            var invoiceItems = courseTuition.Concat(penaltyFee).ToList();

            var invoice = await _receiptProvider.CheckoutUSparkInvoiceV3Async(request.StudentCode, request.KSTermId, invoiceItems);

            return StatusCode(200, invoice);
        }

        [HttpGet("students/{studentCode}/invoices")]
        public async Task<IActionResult> GetInvoiceList(string studentCode)
        {
            // GET INVOICE LISTS
            var invoices = _receiptProvider.GetStudentNonCancelInvoices(studentCode).ToList();

            if (!invoices.Any())
            {
                return StatusCode(204);
            }

            var expirePendingInvoices = invoices.Where(x => !x.IsPaid
                                                              && x.TotalAmount > decimal.Zero
                                                              && x.InvoiceExpiryDate < DateTime.UtcNow)
                                                .ToList();

            var expiredTerms = expirePendingInvoices.Select(x => x.KSTermID)
                                                    .Distinct()
                                                    .ToList();

            foreach(var termId in expiredTerms)
            {
                var isLastInvoiceExpired = _receiptProvider.MarkCancelExpireInvoice(studentCode, termId);

                if (isLastInvoiceExpired)
                {
                    var request = new USparkCalculateTuitionRequestViewModel
                    {
                        StudentCode = studentCode,
                        KSTermId = termId
                    };

                    await CheckOutV3(request);
                }
            }

            // RE-GET INVOICES INCASE OF HAVING EXPIRED INVOICES
            invoices = expiredTerms.Any() ? _receiptProvider.GetStudentNonCancelInvoices(studentCode).ToList()
                                          : invoices;

            if (invoices.Any())
            {
                try
                {
                    var lastTermId = invoices.Max(x => x.KSTermID);

                    await _receiptProvider.SyncRegistrationCoursePaidStatusAsync(studentCode, lastTermId);
                }
                catch (Exception)
                {
                    // TODO : LOG?
                }
            }

            return StatusCode(200, invoices);
        }

        [HttpGet("students/{studentCode}/invoices/{invoiceId}")]
        public IActionResult GetInvoice(string studentCode, long invoiceId)
        {
            var invoice = _receiptProvider.GetNonCancelStudentInvoiceById(studentCode, invoiceId);

            return StatusCode(200, invoice);
        }

        [HttpGet("students/{studentCode}/invoices/{invoiceId}/courses")]
        public IActionResult GetInvoiceCourses(string studentCode, long invoiceId)
        {
            var invoiceCourses = _receiptProvider.GetNonCancelStudentInvoiceCoursesDetails(studentCode, invoiceId);

            return StatusCode(200, invoiceCourses);
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePaymentV3([FromBody] InvoiceUpdateIsPaidViewModel request)
        {
            await _receiptProvider.ProcessInvoicePaymentAsync(request);

            return StatusCode(200);
        }

        public IActionResult GetLatestOrder(string studentCode, long KSTermId)
        {
            var result = new USparkCalculateFeeResponse();
            var student = _db.Students.SingleOrDefault(x => x.Code == studentCode);
            var term = _db.Terms.SingleOrDefault(x => x.Id == KSTermId);
            if (student == null)
            {
                result.Message = "Student not found";
                return StatusCode(400, result);
            }
            else if (term == null)
            {
                result.Message = "Term not found";
                return StatusCode(400, result);
            }
            else
            {

                var invoice = _db.Invoices.Include(x => x.InvoiceItems)
                                        .Where(x => !x.IsCancel
                                                    && x.StudentId == student.Id
                                                    && x.TermId == KSTermId
                                                    && !x.IsPaid
                                                    && x.Type != "o"
                                                    && x.Type != "cr").FirstOrDefault();
                if (invoice != null)
                {
                    List<InvoiceItem> invoiceItems = new List<InvoiceItem>();
                    var creditNoteTransactions = _db.InvoiceDeductTransactions.Include(x => x.InvoiceItem)
                                                                                .ThenInclude(x => x.FeeItem)
                                                                            .Where(x => x.InvoiceId == invoice.Id
                                                                                            && x.Amount > 0
                                                                                            && x.InvoiceItemId != null)
                                                                            .Select(x => new
                                                                            {
                                                                                x.InvoiceItem,
                                                                                x.InvoiceItem.FeeItem
                                                                            }).ToList();
                    if (creditNoteTransactions != null)
                    {
                        foreach (var creditNoteTransaction in creditNoteTransactions)
                        {
                            invoiceItems.Add(new InvoiceItem()
                            {
                                Amount = -creditNoteTransaction.InvoiceItem.DiscountAmount,
                                TotalAmount = -creditNoteTransaction.InvoiceItem.DiscountAmount,
                                DiscountAmount = 0,
                                FeeItemId = creditNoteTransaction.FeeItem.Id,
                                FeeItem = creditNoteTransaction.FeeItem,
                                RegistrationCourseId = creditNoteTransaction.InvoiceItem.RegistrationCourseId,
                                CourseId = creditNoteTransaction.InvoiceItem.CourseId,
                                SectionId = creditNoteTransaction.InvoiceItem.SectionId
                            });
                        }
                        invoice.InvoiceItems.AddRange(invoiceItems);
                    }

                    var order = _registrationProvider.GenerateUSparkOrderFromInvoice(student.Code, KSTermId, invoice, null);
                    result.Result = order;
                    result.Message = "Same invoice.";
                    return StatusCode(200, result);
                }
                else
                {
                    var invoiceCreditNote = _db.Invoices.Include(x => x.InvoiceItems)
                                                        .Where(x => !x.IsCancel
                                                                            && x.StudentId == student.Id
                                                                            && x.TermId == KSTermId
                                                                            && x.Type == "cr")
                                                        .OrderByDescending(x => x.Id)
                                                        .FirstOrDefault();

                    if (invoiceCreditNote != null)
                    {
                        if (invoiceCreditNote.IsPaid || invoiceCreditNote.IsConfirm)
                        {
                            result.Result = null;
                            result.Message = "Invoice is up to date.";
                            return StatusCode(200, result);
                        }
                        else
                        {
                            var creditNoteTransactions = _db.InvoiceDeductTransactions.Include(x => x.InvoiceItem)
                                                                                    .Where(x => x.InvoiceCreditNoteId == invoiceCreditNote.Id
                                                                                                && x.Amount > 0
                                                                                                && x.InvoiceItemId != null)
                                                                                    .Select(x => x.InvoiceItem).ToList();
                            if (creditNoteTransactions != null && creditNoteTransactions.Any())
                            {
                                invoiceCreditNote.InvoiceItems.AddRange(creditNoteTransactions);
                                invoiceCreditNote.TotalDeductAmount = invoiceCreditNote.InvoiceItems.Sum(x => x.DiscountAmount);
                                invoiceCreditNote.TotalAmount += invoiceCreditNote.TotalDeductAmount;
                            }

                            var order = _registrationProvider.GenerateUSparkOrderFromInvoice(student.Code, KSTermId, invoiceCreditNote, null);

                            if (!order.OrderDetails.Any())
                            {
                                result.Result = null;
                            }
                            else
                            {
                                result.Result = order;
                            }
                            result.Message = "Invoice is up to date.";
                            return StatusCode(200, result);
                        }
                    }
                    else
                    {
                        var paidInvoice = _db.Invoices.Include(x => x.InvoiceItems)
                                                    .Where(x => !x.IsCancel
                                                                && x.StudentId == student.Id
                                                                && x.TermId == KSTermId
                                                                && x.IsPaid
                                                                && x.Type != "o"
                                                                && x.Type != "cr")
                                                    .OrderByDescending(x => x.Id)
                                                    .FirstOrDefault();
                        if (paidInvoice != null)
                        {
                            var order = _registrationProvider.SimulateGenerateUSparkOrderFromInvoice(student.Code, KSTermId, paidInvoice, null);
                            result.Result = order;
                            result.Message = "Invoice is up to date.";
                            return StatusCode(200, result);
                        }
                        else
                        {
                            result.Result = null;
                            result.Message = "Invoice is up to date.";
                            return StatusCode(200, result);
                        }
                    }
                }
            }
        }

        private USparkCalculateFeeResponse GetSimulateExistingInvoiceForResponse(Student student, long KSTermId, List<RegistrationCourse> US_RegistrationCourses)
        {
            var result = new USparkCalculateFeeResponse();
            var invoice = _db.Invoices.Include(x => x.InvoiceItems)
                                      .Where(x => !x.IsCancel
                                                  && x.StudentId == student.Id
                                                  && x.TermId == KSTermId
                                                  && !x.IsPaid
                                                  && x.Type != "o"
                                                  && x.Type != "cr").FirstOrDefault();
            if (invoice != null)
            {
                List<InvoiceItem> invoiceItems = new List<InvoiceItem>();
                var creditNoteTransactions = _db.InvoiceDeductTransactions.Include(x => x.InvoiceItem)
                                                                            .ThenInclude(x => x.FeeItem)
                                                                          .Where(x => x.InvoiceId == invoice.Id
                                                                                        && x.Amount > 0
                                                                                        && x.InvoiceItemId != null)
                                                                          .Select(x => new
                                                                          {
                                                                              x.InvoiceItem,
                                                                              x.InvoiceItem.FeeItem
                                                                          }).ToList();
                if (creditNoteTransactions != null)
                {
                    foreach (var creditNoteTransaction in creditNoteTransactions)
                    {
                        invoiceItems.Add(new InvoiceItem()
                        {
                            Amount = -creditNoteTransaction.InvoiceItem.DiscountAmount,
                            TotalAmount = -creditNoteTransaction.InvoiceItem.DiscountAmount,
                            DiscountAmount = 0,
                            FeeItemId = creditNoteTransaction.FeeItem.Id,
                            FeeItem = creditNoteTransaction.FeeItem,
                            RegistrationCourseId = creditNoteTransaction.InvoiceItem.RegistrationCourseId,
                            CourseId = creditNoteTransaction.InvoiceItem.CourseId,
                            SectionId = creditNoteTransaction.InvoiceItem.SectionId
                        });
                    }
                    invoice.InvoiceItems.AddRange(invoiceItems);
                }

                var order = _registrationProvider.SimulateGenerateUSparkOrderFromInvoice(student.Code, KSTermId, invoice, null);
                result.Result = order;
                result.Message = "Same invoice.";
                return result;
            }
            else
            {
                var invoiceCreditNote = _db.Invoices.Include(x => x.InvoiceItems)
                                                    .Where(x => !x.IsCancel
                                                                && x.StudentId == student.Id
                                                                && x.TermId == KSTermId
                                                                && x.Type == "cr")
                                                    .OrderByDescending(x => x.Id)
                                                    .FirstOrDefault();

                if (invoiceCreditNote != null)
                {
                    if (invoiceCreditNote.IsPaid || invoiceCreditNote.IsConfirm)
                    {
                        result.Result = null;
                        result.Message = "Invoice is up to date.";
                        return result;
                    }
                    else
                    {
                        var creditNoteTransactions = _db.InvoiceDeductTransactions.Include(x => x.InvoiceItem)
                                                                                .Where(x => x.InvoiceCreditNoteId == invoiceCreditNote.Id
                                                                                            && x.Amount > 0
                                                                                            && x.InvoiceItemId != null)
                                                                                .Select(x => x.InvoiceItem).ToList();
                        if (creditNoteTransactions != null && creditNoteTransactions.Any())
                        {
                            invoiceCreditNote.InvoiceItems.AddRange(creditNoteTransactions);
                            invoiceCreditNote.TotalDeductAmount = invoiceCreditNote.InvoiceItems.Sum(x => x.DiscountAmount);
                            invoiceCreditNote.TotalAmount += invoiceCreditNote.TotalDeductAmount;
                        }

                        var order = _registrationProvider.SimulateGenerateUSparkOrderFromInvoice(student.Code, KSTermId, invoiceCreditNote, null);

                        if (!order.OrderDetails.Any())
                        {
                            result.Result = null;
                        }
                        else
                        {
                            result.Result = order;
                        }
                        result.Message = "Invoice is up to date.";
                        return result;
                    }
                }
                else
                {
                    var paidInvoice = _db.Invoices.Include(x => x.InvoiceItems)
                                                  .Where(x => !x.IsCancel
                                                              && x.StudentId == student.Id
                                                              && x.TermId == KSTermId
                                                              && x.IsPaid
                                                              && x.Type != "o"
                                                              && x.Type != "cr")
                                                  .OrderByDescending(x => x.Id)
                                                  .FirstOrDefault();
                    if (paidInvoice != null)
                    {
                        var order = _registrationProvider.SimulateGenerateUSparkOrderFromInvoice(student.Code, KSTermId, paidInvoice, null);
                        result.Result = order;
                        result.Message = "Invoice is up to date.";
                        return result;
                    }
                    else
                    {
                        result.Result = null;
                        result.Message = "Invoice is up to date.";
                        return result;
                    }
                }
            }
        }

        private USparkCalculateFeeResponse GetExistingInvoiceForResponse(Student student, long KSTermId, List<RegistrationCourse> US_RegistrationCourses)
        {
            var result = new USparkCalculateFeeResponse();
            var invoice = _db.Invoices.Include(x => x.InvoiceItems)
                                      .Where(x => !x.IsCancel
                                                  && x.StudentId == student.Id
                                                  && x.TermId == KSTermId
                                                  && !x.IsPaid
                                                  && x.Type != "o"
                                                  && x.Type != "cr").FirstOrDefault();
            if (invoice != null)
            {
                List<InvoiceItem> invoiceItems = new List<InvoiceItem>();
                var creditNoteTransactions = _db.InvoiceDeductTransactions.Include(x => x.InvoiceItem)
                                                                            .ThenInclude(x => x.FeeItem)
                                                                          .Where(x => x.InvoiceId == invoice.Id
                                                                                        && x.Amount > 0
                                                                                        && x.InvoiceItemId != null)
                                                                          .Select(x => new
                                                                          {
                                                                              x.InvoiceItem,
                                                                              x.InvoiceItem.FeeItem
                                                                          }).ToList();
                if (creditNoteTransactions != null)
                {
                    foreach (var creditNoteTransaction in creditNoteTransactions)
                    {
                        invoiceItems.Add(new InvoiceItem()
                        {
                            Amount = -creditNoteTransaction.InvoiceItem.DiscountAmount,
                            TotalAmount = -creditNoteTransaction.InvoiceItem.DiscountAmount,
                            DiscountAmount = 0,
                            FeeItemId = creditNoteTransaction.FeeItem.Id,
                            FeeItem = creditNoteTransaction.FeeItem,
                            RegistrationCourseId = creditNoteTransaction.InvoiceItem.RegistrationCourseId,
                            CourseId = creditNoteTransaction.InvoiceItem.CourseId,
                            SectionId = creditNoteTransaction.InvoiceItem.SectionId
                        });
                    }
                    invoice.InvoiceItems.AddRange(invoiceItems);
                }

                var order = _registrationProvider.GenerateUSparkOrderFromInvoice(student.Code, KSTermId, invoice, null);
                result.Result = order;
                result.Message = "Same invoice.";
                return result;
            }
            else
            {
                var invoiceCreditNote = _db.Invoices.Include(x => x.InvoiceItems)
                                                    .Where(x => !x.IsCancel
                                                                        && x.StudentId == student.Id
                                                                        && x.TermId == KSTermId
                                                                        && x.Type == "cr")
                                                    .OrderByDescending(x => x.Id)
                                                    .FirstOrDefault();

                if (invoiceCreditNote != null)
                {
                    if (invoiceCreditNote.IsPaid || invoiceCreditNote.IsConfirm)
                    {
                        result.Result = null;
                        result.Message = "Invoice is up to date.";
                        return result;
                    }
                    else
                    {
                        var creditNoteTransactions = _db.InvoiceDeductTransactions.Include(x => x.InvoiceItem)
                                                                                  .Where(x => x.InvoiceCreditNoteId == invoiceCreditNote.Id
                                                                                              && x.Amount > 0
                                                                                              && x.InvoiceItemId != null)
                                                                                  .Select(x => x.InvoiceItem).ToList();
                        if (creditNoteTransactions != null && creditNoteTransactions.Any())
                        {
                            invoiceCreditNote.InvoiceItems.AddRange(creditNoteTransactions);
                            invoiceCreditNote.TotalDeductAmount = invoiceCreditNote.InvoiceItems.Sum(x => x.DiscountAmount);
                            invoiceCreditNote.TotalAmount += invoiceCreditNote.TotalDeductAmount;
                        }

                        var order = _registrationProvider.GenerateUSparkOrderFromInvoice(student.Code, KSTermId, invoiceCreditNote, null);

                        if (!order.OrderDetails.Any())
                        {
                            result.Result = null;
                        }
                        else
                        {
                            result.Result = order;
                        }
                        result.Message = "Invoice is up to date.";
                        return result;
                    }
                }
                else
                {
                    var paidInvoice = _db.Invoices.Include(x => x.InvoiceItems)
                                                  .Where(x => !x.IsCancel
                                                              && x.StudentId == student.Id
                                                              && x.TermId == KSTermId
                                                              && x.IsPaid
                                                              && x.Type != "o"
                                                              && x.Type != "cr")
                                                  .OrderByDescending(x => x.Id)
                                                  .FirstOrDefault();
                    if (paidInvoice != null)
                    {
                        var order = _registrationProvider.SimulateGenerateUSparkOrderFromInvoice(student.Code, KSTermId, paidInvoice, null);
                        result.Result = order;
                        result.Message = "Invoice is up to date.";
                        return result;
                    }
                    else
                    {
                        result.Result = null;
                        result.Message = "Invoice is up to date.";
                        return result;
                    }
                }

                // var invoiceforResponse = new Invoice();
                // invoiceforResponse.InvoiceItems = new List<InvoiceItem>();
                // var registrationCourses = _db.RegistrationCourses.Where(x => x.TermId == KSTermId
                //                                                         && x.StudentId == student.Id
                //                                                         && US_RegistrationCourses.Select(y => y.SectionId).Contains(x.SectionId)
                //                                                         && x.Status != "d"
                //                                                     )
                //                                                 .ToList();
                // foreach (var registrationCourse in registrationCourses)
                // {
                //     if (registrationCourse.Status == "a")
                //     {
                //         var addInvoiceItem = (from invoiceItem in _db.InvoiceItems
                //                                 join invoice in _db.Invoices on invoiceItem.InvoiceId equals invoice.Id
                //                                 join invoiceDeduct in _db.InvoiceDeductTransactions on invoiceItem.Id equals invoiceDeduct.InvoiceItemId into invoiceDeductTmp
                //                                 from invoiceDeduct in invoiceDeductTmp.DefaultIfEmpty()
                //                                 where invoiceItem.RegistrationCourseId == registrationCourse.Id
                //                                       && !invoice.IsCancel
                //                                 select new { invoiceItem, invoiceDeduct, invoice })
                //                                 .FirstOrDefault();
                //         if (addInvoiceItem != null)
                //         {
                //             if (!invoiceforResponse.InvoiceItems.Any())
                //             {
                //                 invoiceforResponse = addInvoiceItem.invoice;
                //             }
                //             else
                //             {
                //                 invoiceforResponse.InvoiceItems.Add(addInvoiceItem.invoiceItem);
                //             }

                //             if (addInvoiceItem.invoiceDeduct != null)
                //             {
                //                 addInvoiceItem.invoiceItem.Amount += addInvoiceItem.invoiceDeduct.Amount;
                //                 addInvoiceItem.invoiceItem.TotalAmount += addInvoiceItem.invoiceDeduct.Amount;

                //                 var deductInvoiceItems = (from invoice in _db.Invoices
                //                                             join invoiceItem in _db.InvoiceItems on invoice.Id equals invoiceItem.InvoiceId
                //                                             where invoice.Id == addInvoiceItem.invoiceDeduct.InvoiceCreditNoteId
                //                                                   && !invoice.IsCancel
                //                                             select invoiceItem).ToList();
                //                 invoiceforResponse.InvoiceItems.AddRange(deductInvoiceItems.Where(x => !invoiceforResponse.InvoiceItems.Select(y => y.Id).Contains(x.Id)));
                //             }
                //         }

                //         var otherInvoiceItems = (from invoice in _db.Invoices
                //                             join invoiceItem in _db.InvoiceItems on invoice.Id equals invoiceItem.InvoiceId
                //                             where invoice.Id == addInvoiceItem.invoiceItem.InvoiceId
                //                                   && invoiceItem.Id != addInvoiceItem.invoiceItem.Id
                //                                   && !invoice.IsCancel
                //                             select invoiceItem).ToList();
                //         invoiceforResponse.InvoiceItems.AddRange(otherInvoiceItems.Where(x => !invoiceforResponse.InvoiceItems.Select(y => y.Id).Contains(x.Id)));
                //     }
                // }

                // var order = _registrationProvider.GenerateUSparkOrderFromInvoice(student.Code, KSTermId, invoiceforResponse, null);
                // result.Result = order;
                // result.Message = "Same invoice.";
                // return result;
            }
        }

        private List<RegistrationCourse> GenerateRegistrationCoursesFromAPI(Guid studentId, long termId, string ksSectionIds, string round)
        {
            var registrationCourses = new List<RegistrationCourse>();
            var split = ksSectionIds.Split(',');
            foreach (var ksSectionId in split)
            {
                var section = _db.Sections.FirstOrDefault(x => x.Id == Convert.ToInt64(ksSectionId));
                if (section != null)
                {
                    var registrationCourse = new RegistrationCourse();
                    registrationCourse.StudentId = studentId;
                    registrationCourse.TermId = termId;
                    registrationCourse.CourseId = section.CourseId;
                    registrationCourse.SectionId = section.Id;
                    registrationCourse.Status = round;
                    registrationCourses.Add(registrationCourse);
                }
            }
            return registrationCourses;
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

        private bool IsSameTermAndTuitionFee(Guid studentId, long termId, List<InvoiceItem> invoiceItems, List<RegistrationCourse> apiRegistrationCourses)
        {
            var student = _studentProvider.GetStudentInformationById(studentId);
            bool isLumpsumPayment = student.StudentFeeGroup?.IsLumpsumPayment ?? false;
            var invoices = _db.Invoices.Include(x => x.InvoiceItems)
                                      .Where(x => !x.IsCancel
                                                           && x.StudentId == studentId
                                                           && x.TermId == termId
                                                           //&& !x.IsPaid
                                                           && x.Type != "o"
                                                           && x.Type != "cr").ToList();
            if (isLumpsumPayment)
            {
                var registeredCourses = _db.RegistrationCourses.Where(x => x.TermId == termId
                                                                    && x.StudentId == studentId
                                                                    && x.Status != "d")
                                                            .ToList();
                bool same = registeredCourses.Any();
                if (same)
                {
                    foreach (var item in registeredCourses)
                    {
                        if (!apiRegistrationCourses.Any(x => x.CourseId == item.CourseId
                                                            && (x.SectionId ?? 0) == (item.SectionId ?? 0)
                                                            && x.TermId == item.TermId))
                        {
                            same = false;
                            break;
                        }
                    }

                    if (same)
                    {
                        foreach (var item in apiRegistrationCourses)
                        {
                            if (!registeredCourses.Any(x => x.CourseId == item.CourseId
                                                        && (x.SectionId ?? 0) == (item.SectionId ?? 0)
                                                        && x.TermId == item.TermId))
                            {
                                same = false;
                                break;
                            }
                        }
                    }
                }
                return same;
            }
            else if (invoices.Any() && invoices.Count == 1)
            {
                var invoice = invoices.FirstOrDefault();
                var same = invoice != null && invoice.InvoiceItems.Any();
                if (same)
                {
                    foreach (var item in invoiceItems.Where(x => x.CourseId != null))
                    {
                        if (!item.SectionId.HasValue || item.SectionId == 0)
                        {
                            continue;
                        }
                        if (!invoice.InvoiceItems.Any(x => (x.CourseId ?? 0) == (item.CourseId ?? 0)
                                                            && (x.SectionId ?? 0) == (item.SectionId ?? 0)
                                                            && x.FeeItemId == item.FeeItemId
                                                            && x.TotalAmount == item.Amount
                                                            && x.IsPaid == item.IsPaid))
                        {
                            same = false;
                            break;
                        }
                    }

                    if (same)
                    {
                        foreach (var item in invoice.InvoiceItems.Where(x => x.CourseId != null))
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
            else if (!invoices.Any())
            {
                return false;
            }
            else
            {
                var registeredCourses = _db.RegistrationCourses.Where(x => x.TermId == termId
                                                                    && x.StudentId == studentId
                                                                    && x.Status != "d")
                                                            .ToList();
                bool same = registeredCourses.Any();
                if (same)
                {
                    foreach (var item in registeredCourses)
                    {
                        if (!apiRegistrationCourses.Any(x => x.CourseId == item.CourseId
                                                            && (x.SectionId ?? 0) == (item.SectionId ?? 0)
                                                            && x.TermId == item.TermId))
                        {
                            same = false;
                            break;
                        }
                    }

                    if (same)
                    {
                        foreach (var item in apiRegistrationCourses)
                        {
                            if (!registeredCourses.Any(x => x.CourseId == item.CourseId
                                                        && (x.SectionId ?? 0) == (item.SectionId ?? 0)
                                                        && x.TermId == item.TermId))
                            {
                                same = false;
                                break;
                            }
                        }
                    }
                }
                return same;
            }
        }

        public IActionResult Calculate(string studentCode, string ksSectionIds, long KSTermId, int addDropCount, bool isRegistrationPeriod = true, bool isOnlyCalculate = true)
        {
            var result = new USparkCalculateFeeResponse();
            var student = _db.Students.SingleOrDefault(x => x.Code == studentCode);
            var term = _db.Terms.SingleOrDefault(x => x.Id == KSTermId);
            if (student == null)
            {
                result.Message = "Student not found";
                return StatusCode(400, result);
            }
            else if (term == null)
            {
                result.Message = "Term not found";
                return StatusCode(400, result);
            }
            else if (String.IsNullOrEmpty(ksSectionIds))
            {
                result.Message = "Sections not specified";
                return StatusCode(400, result);
            }
            else
            {
                var studentState = _db.StudentStates.SingleOrDefault(x => x.StudentId == student.Id && x.TermId == KSTermId);
                string round = studentState != null && (studentState.State == "ADD" || studentState.State == "PAY_ADD") ? "a" : "r";
                var US_RegistrationCourses = GenerateRegistrationCoursesFromAPI(student.Id, KSTermId, ksSectionIds, round);
                if (US_RegistrationCourses.Any())
                {
                    var invoiceItems = _receiptProvider.GetInvoiceItemsForPreview(US_RegistrationCourses, 0);
                    bool isSame = IsSameTermAndTuitionFee(student.Id, KSTermId, invoiceItems, US_RegistrationCourses);
                    if (!isSame)
                    {
                        if (studentState.State == "REG" || studentState.State == "PAY_REG")
                        {
                            var alreadyRegistration = _db.RegistrationCourses.Any(x => x.StudentId == student.Id
                                                                                    && x.TermId == KSTermId
                                                                                    && x.IsPaid);
                            if (alreadyRegistration && !invoiceItems.Any())
                            {
                                result.Message = "Already registered for this term.";
                                return StatusCode(200, result);
                            }
                            else
                            {
                                var invoice = new Invoice();
                                invoice.TermId = KSTermId;
                                invoice.StudentId = student.Id;
                                invoice.InvoiceItems = invoiceItems;
                                var feeItems = _db.FeeItems.Where(x => invoiceItems.Select(y => y.FeeItemId).Contains(x.Id)).ToList();
                                foreach (var item in invoice.InvoiceItems)
                                {
                                    var feeItem = feeItems.SingleOrDefault(x => x.Id == item.FeeItemId);
                                    item.FeeItem = feeItem;
                                }
                                try
                                {
                                    var order = _registrationProvider.GenerateUSparkOrderFromInvoice(studentCode, KSTermId, invoice, null);
                                    result.Result = order;
                                    return StatusCode(200, result);
                                }
                                catch (Exception e)
                                {
                                    return StatusCode(500, e);
                                }
                            }
                        }
                        else
                        {
                            var invoiceForResponse = new Invoice();
                            invoiceForResponse.InvoiceItems = new List<InvoiceItem>();
                            var currentRegisteredCourses = _db.RegistrationCourses.Where(x => x.StudentId == student.Id
                                                                                        && x.TermId == KSTermId
                                                                                        && x.Status != "d")
                                                                                   .ToList();
                            var currentInvoiceItems = (from invoiceItem in _db.InvoiceItems
                                                       join invoiceDeduct in _db.InvoiceDeductTransactions on invoiceItem.Id equals invoiceDeduct.InvoiceItemId into invoiceDeductTmp
                                                       from invoiceDeduct in invoiceDeductTmp.DefaultIfEmpty()
                                                       where invoiceItem.RegistrationCourseId.HasValue
                                                        && currentRegisteredCourses.Select(y => y.Id).Contains(invoiceItem.RegistrationCourseId.Value)
                                                       select new { invoiceItem, invoiceDeduct });
                            foreach (var currentRegisteredCourse in currentRegisteredCourses)
                            {
                                if (!US_RegistrationCourses.Select(x => x.SectionId).Contains(currentRegisteredCourse.SectionId))
                                {
                                    //Add
                                    var currentInvoiceItem = currentInvoiceItems.SingleOrDefault(x => x.invoiceItem.RegistrationCourseId == currentRegisteredCourse.Id);
                                    currentInvoiceItem.invoiceItem.Amount += currentInvoiceItem.invoiceDeduct.Amount;
                                    currentInvoiceItem.invoiceItem.TotalAmount += currentInvoiceItem.invoiceDeduct.Amount;
                                    currentInvoiceItem.invoiceItem.Amount = -currentInvoiceItem.invoiceItem.Amount;
                                    currentInvoiceItem.invoiceItem.TotalAmount = -currentInvoiceItem.invoiceItem.TotalAmount;
                                    invoiceForResponse.InvoiceItems.Add(currentInvoiceItem.invoiceItem);
                                }
                            }

                            foreach (var US_RegistrationCourse in US_RegistrationCourses)
                            {
                                if (!currentRegisteredCourses.Select(x => x.SectionId).Contains(US_RegistrationCourse.SectionId))
                                {
                                    //Drop
                                    invoiceForResponse.InvoiceItems.Add(invoiceItems.SingleOrDefault(x => x.SectionId == US_RegistrationCourse.SectionId));
                                }
                            }

                            var refundDeductionInvoiceItem = _receiptProvider.CalculateRefundReduction(invoiceForResponse.InvoiceItems);
                            if (refundDeductionInvoiceItem != null)
                            {
                                invoiceForResponse.InvoiceItems.Add(refundDeductionInvoiceItem);
                            }

                            var addDropFeeInvoiceItem = _receiptProvider.GenerateAddDropFeeItem(KSTermId, addDropCount);
                            if (addDropFeeInvoiceItem != null)
                            {
                                invoiceForResponse.InvoiceItems.Add(addDropFeeInvoiceItem);
                            }
                            try
                            {
                                invoiceForResponse.Amount = invoiceForResponse.InvoiceItems.Sum(x => x.Amount);
                                invoiceForResponse.TotalAmount = invoiceForResponse.InvoiceItems.Sum(x => x.TotalAmount);
                                var order = _registrationProvider.GenerateUSparkOrderFromInvoice(studentCode, KSTermId, invoiceForResponse, null);
                                result.Result = order;
                                return StatusCode(200, result);
                            }
                            catch (Exception e)
                            {
                                return StatusCode(500, e);
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            result = GetExistingInvoiceForResponse(student, KSTermId, US_RegistrationCourses);
                            return StatusCode(200, result);
                        }
                        catch (Exception e)
                        {
                            return StatusCode(500, e);
                        }
                    }
                }
                else
                {
                    result.Message = "Courses and sections not found";
                    return StatusCode(400, result);
                }
            }
        }

        public IActionResult AddDrop(string studentCode, string ksSectionIds, long ksTermId, bool isOnlyCalculate = true)
        {
            var result = new USparkAddDropResponse();
            var student = _db.Students.SingleOrDefault(x => x.Code == studentCode);
            var term = _db.Terms.SingleOrDefault(x => x.Id == ksTermId);
            if (student == null)
            {
                result.Message = "Student not found";
                return StatusCode(400, result);
            }
            else if (term == null)
            {
                result.Message = "Term not found";
                return StatusCode(400, result);
            }
            else if (String.IsNullOrEmpty(ksSectionIds))
            {
                result.Message = "Sections not specified";
                return StatusCode(400, result);
            }
            else
            {
                var registrationCourses = new List<RegistrationCourse>();
                var split = ksSectionIds.Split(',');
                foreach (var ksSectionId in split)
                {
                    var section = _db.Sections.Include(x => x.Course).SingleOrDefault(x => x.Id == Convert.ToInt64(ksSectionId));
                    if (section != null)
                    {
                        registrationCourses.Add(new RegistrationCourse
                        {
                            StudentId = student.Id,
                            TermId = ksTermId,
                            CourseId = section.CourseId,
                            SectionId = section?.Id ?? 0,
                            Course = section.Course,
                            Section = section
                        });
                    }
                }

                if (!registrationCourses.Any())
                {
                    result.Message = "Courses and sections not found";
                    return StatusCode(400, result);
                }

                if (isOnlyCalculate)
                {
                    var studentRegistrationCourses = _db.RegistrationCourses.Include(x => x.Section)
                                                                                .ThenInclude(x => x.SectionDetails)
                                                                                .ThenInclude(x => x.Room)
                                                                            .Include(x => x.Course)
                                                                            .Where(x => x.StudentId == student.Id && x.TermId == ksTermId).ToList();
                    var studentInvoices = _db.Invoices.Include(x => x.InvoiceItems).Where(x => x.StudentId == student.Id && x.TermId == ksTermId).ToList();
                    foreach (var invoice in studentInvoices.Where(x => !x.IsPaid && (x.Type == "au" || x.Type == "cr")))
                    {
                        foreach (var item in invoice.InvoiceItems)
                        {
                            var course = studentRegistrationCourses.SingleOrDefault(x => x.Id == item.RegistrationCourseId);
                            if (course != null)
                            {
                                course.Status = invoice.Type == "au" ? "d" : "a";
                            }
                        }
                    }
                    studentInvoices.RemoveAll(x => !x.IsPaid && (x.Type == "au" || x.Type == "cr"));

                    var drops = new List<RegistrationCourse>();
                    var adds = new List<RegistrationCourse>();
                    var activeRegistrations = studentRegistrationCourses.Where(x => x.Status != "d").ToList();

                    var same = activeRegistrations != null && activeRegistrations.Any();
                    if (same)
                    {
                        drops = activeRegistrations.Where(x => !registrationCourses.Any(y => x.CourseId == y.CourseId
                                                                                            && (x.SectionId ?? 0) == (y.SectionId ?? 0)))
                                                .ToList();
                        adds = registrationCourses.Where(x => !activeRegistrations.Any(y => x.CourseId == y.CourseId
                                                                                            && (x.SectionId ?? 0) == (y.SectionId ?? 0)))
                                                .ToList();

                        same = (drops == null || !drops.Any()) && (adds == null || !adds.Any());
                    }

                    if (!same)
                    {
                        var addCourses = new List<RegistrationCourse>();
                        var dropCourses = new List<RegistrationCourse>();
                        foreach (var courseFromAPI in registrationCourses)
                        {
                            if (studentRegistrationCourses.Select(x => x.CourseId).Contains(courseFromAPI.CourseId))
                            {
                                // Same
                                studentRegistrationCourses.RemoveAll(x => x.CourseId == courseFromAPI.CourseId);
                            }
                            else
                            {
                                // Add
                                addCourses.Add(new RegistrationCourse()
                                {
                                    StudentId = student.Id,
                                    TermId = ksTermId,
                                    CourseId = courseFromAPI.CourseId,
                                    SectionId = courseFromAPI.SectionId,
                                    IsPaid = false,
                                    IsLock = false,
                                    IsSurveyed = false,
                                    Status = "a",
                                    IsActive = true,
                                });
                            }
                        }

                        //Drop
                        if (studentRegistrationCourses.Any())
                        {
                            foreach (var course in studentRegistrationCourses)
                            {
                                dropCourses.Add(new RegistrationCourse()
                                {
                                    StudentId = student.Id,
                                    TermId = ksTermId,
                                    CourseId = course.CourseId,
                                    SectionId = course.SectionId,
                                    IsPaid = false,
                                    IsLock = false,
                                    IsSurveyed = false,
                                    Status = "a",
                                    IsActive = true,
                                });
                            }
                        }

                        var addCourseInvoiceItems = _receiptProvider.GetInvoiceItemsForPreview(addCourses, 0, false).Where(x => x.CourseId.HasValue && addCourses.Select(y => y.CourseId).Contains(x.CourseId.Value)).ToList();
                        var dropCourseInvoiceItems = _receiptProvider.GetInvoiceItemsForPreview(dropCourses, 0, false).Where(x => x.CourseId.HasValue && dropCourses.Select(y => y.CourseId).Contains(x.CourseId.Value)).ToList();
                        if (dropCourseInvoiceItems.Sum(x => x.Amount) > addCourseInvoiceItems.Sum(x => x.Amount))
                        {
                            //Deduct 10%
                            _receiptProvider.CalculateRefundReduction(dropCourseInvoiceItems, addCourseInvoiceItems);
                        }

                        //Calculuate add/drop count
                        var addDropRound = (_db.Invoices.Where(x => x.StudentId == student.Id
                                                                    && x.TermId == ksTermId
                                                                    && x.IsPaid
                                                                    && (x.Type == "cr" || x.Type == "au"))
                                                        .Max(x => (int?)x.AddDropSequence) ?? 0) + 1;
                        var addDropFeeCount = _receiptProvider.GetConfigAddDropFeeCount(ksTermId);
                        if (addDropFeeCount != null && addDropRound > addDropFeeCount.FreeAddDropCount)
                        {
                            var value = _configurationProvider.Get<long>("AddDropFeeItem");
                            var addDropFee = _db.FeeItems.SingleOrDefault(x => x.Id == value);
                            if (addDropFee != null)
                            {
                                addCourseInvoiceItems.Add(new InvoiceItem
                                {
                                    FeeItemId = addDropFee.Id,
                                    FeeItemName = addDropFee.NameEn,
                                    Amount = addDropFeeCount.Amount,
                                    ScholarshipAmount = 0,
                                    TotalAmount = addDropFeeCount.Amount,
                                });
                            }
                        }

                        result.Result = new List<USparkOrder>();
                        if (addCourseInvoiceItems.Any())
                        {
                            var addOrder = new USparkOrder();
                            addOrder.OrderDetails = new List<USparkOrderDetail>();
                            addOrder.KSTermID = ksTermId;
                            addOrder.StudentCode = studentCode;
                            addOrder.TotalAmount = addCourseInvoiceItems.Sum(x => x.Amount);

                            var feeItems = _db.FeeItems.Where(x => addCourseInvoiceItems.Select(y => y.FeeItemId).Contains(x.Id)).ToList();
                            foreach (var item in addCourseInvoiceItems)
                            {
                                var registrationCourse = registrationCourses.SingleOrDefault(x => x.CourseId == item.CourseId);
                                var feeItem = feeItems.SingleOrDefault(x => x.Id == item.FeeItemId);
                                addOrder.OrderDetails.Add(new USparkOrderDetail()
                                {
                                    Amount = item.Amount,
                                    ItemCode = feeItem.Code,
                                    ItemNameEn = feeItem.NameEn,
                                    ItemNameTh = feeItem.NameTh,
                                });
                            }

                            result.Result.Add(addOrder);
                        }
                        if (dropCourseInvoiceItems.Any())
                        {
                            var dropOrder = new USparkOrder();
                            dropOrder.OrderDetails = new List<USparkOrderDetail>();
                            dropOrder.KSTermID = ksTermId;
                            dropOrder.StudentCode = studentCode;
                            dropOrder.TotalAmount = -dropCourseInvoiceItems.Sum(x => x.Amount);
                            var feeItems = _db.FeeItems.Where(x => dropCourseInvoiceItems.Select(y => y.FeeItemId).Contains(x.Id)).ToList();
                            foreach (var item in dropCourseInvoiceItems)
                            {
                                var registrationCourse = registrationCourses.SingleOrDefault(x => x.CourseId == item.CourseId);
                                var feeItem = feeItems.SingleOrDefault(x => x.Id == item.FeeItemId);
                                dropOrder.OrderDetails.Add(new USparkOrderDetail()
                                {
                                    Amount = -item.Amount,
                                    ItemCode = feeItem.Code,
                                    ItemNameEn = feeItem.NameEn,
                                    ItemNameTh = feeItem.NameTh,
                                });
                            }
                            result.Result.Add(dropOrder);
                        }
                        return StatusCode(200, result);
                    }
                    else
                    {
                        result.Message = "Same course.";
                        return StatusCode(200, result);
                    }
                }
                else
                {
                    // cancel add drop
                    _receiptProvider.CancelAddDropInvoices(student.Id, ksTermId);

                    var drops = new List<RegistrationCourse>();
                    var adds = new List<RegistrationCourse>();
                    var activeRegistrations = _registrationProvider.GetActiveRegistrationCourses(student.Id, ksTermId);

                    var same = activeRegistrations != null && activeRegistrations.Any();
                    if (same)
                    {
                        drops = activeRegistrations.Where(x => !registrationCourses.Any(y => x.CourseId == y.CourseId
                                                                                            && (x.SectionId ?? 0) == (y.SectionId ?? 0)))
                                                .ToList();
                        adds = registrationCourses.Where(x => !activeRegistrations.Any(y => x.CourseId == y.CourseId
                                                                                            && (x.SectionId ?? 0) == (y.SectionId ?? 0)))
                                                .ToList();

                        same = (drops == null || !drops.Any()) && (adds == null || !adds.Any());
                    }

                    if (!same)
                    {
                        // mapping adding result
                        var addingResults = registrationCourses.Select(x => new AddingViewModel
                        {
                            RegistrationCourseId = _db.RegistrationCourses.FirstOrDefault(y => y.StudentId == student.Id
                                                                                            && y.CourseId == x.CourseId
                                                                                            && y.SectionId == x.SectionId
                                                                                            && y.Status != "d")?.Id ?? 0,
                            CourseId = x.CourseId,
                            CourseCode = x.Course.Code,
                            SectionId = x.SectionId ?? 0,
                            SectionNumber = x.Section?.Number
                        })
                                                            .ToList();
                        // add
                        List<RegistrationCourse> newCourses = new List<RegistrationCourse>();
                        List<RegistrationCourse> deleteUnpaidCourses = new List<RegistrationCourse>();
                        List<RegistrationCourse> deletePaidCourses = new List<RegistrationCourse>();
                        var addCourses = _registrationProvider.ModifyRegistrationCourse(student.Id, ksTermId, "a", addingResults, out newCourses, out deleteUnpaidCourses, out deletePaidCourses, "s");
                        var addDropRound = (_db.Invoices.Where(x => x.StudentId == student.Id
                                                                    && x.TermId == ksTermId
                                                                    && x.IsPaid
                                                                    && (x.Type == "cr" || x.Type == "au"))
                                                        .Max(x => (int?)x.AddDropSequence) ?? 0) + 1;

                        Invoice dropInvoice = null;
                        if (drops != null && drops.Any())
                        {
                            dropInvoice = _receiptProvider.DropCourses(student.Id, ksTermId, drops, addDropRound);
                        }

                        Invoice addInvoice = null;
                        if (addCourses != null && addCourses.Any())
                        {
                            addInvoice = _receiptProvider.AddCourses(student.Id, ksTermId, addCourses, addDropRound);
                        }
                        else
                        {
                            var now = DateTime.Now;
                            var addDropFeeCount = _receiptProvider.GetConfigAddDropFeeCount(ksTermId);
                            var paymentPeriod = (from slot in _db.Slots
                                                 join registrationTerm in _db.RegistrationTerms on slot.RegistrationTermId equals registrationTerm.Id
                                                 where registrationTerm.TermId == ksTermId
                                                 && registrationTerm.StartedAt <= now
                                                 && registrationTerm.EndedAt >= now
                                                 && slot.Type == "p"
                                                 select slot).FirstOrDefault();
                            if ((addDropFeeCount != null && addDropRound > addDropFeeCount.FreeAddDropCount)
                                || (paymentPeriod != null && now > paymentPeriod.EndedAt))
                            {
                                addInvoice = _receiptProvider.AddInvoice(student.Id, ksTermId, new List<RegistrationCourse>(), "au", addDropRound);
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
                            _receiptProvider.UpdateTuitionFeeRefund(dropInvoice, addInvoice);
                        }

                        var orders = new List<USparkOrder>();
                        if (addInvoice != null)
                        {
                            var order = new USparkOrder();
                            order.Reference1 = addInvoice.Reference1;
                            order.Reference2 = addInvoice.Reference2;
                            order.KSTermID = ksTermId;
                            order.StudentCode = studentCode;
                            order.TotalAmount = addInvoice.InvoiceItems.Sum(x => x.Amount);
                            order.Number = addInvoice.Number;
                            order.ReferenceNumber = addInvoice.RunningNumber;
                            order.OrderDetails = new List<USparkOrderDetail>();
                            order.CreatedAt = addInvoice.CreatedAt;
                            order.InvoiceExpiryDate = addInvoice.PaymentExpireAt ?? DateTime.Today.AddDays(15);
                            order.IsPaid = addInvoice.IsPaid;
                            order.KSInvoiceID = addInvoice.Id;

                            var feeItems = _db.FeeItems.Where(x => addInvoice.InvoiceItems.Select(y => y.FeeItemId).Contains(x.Id)).ToList();
                            foreach (var invoiceItem in addInvoice.InvoiceItems)
                            {
                                var feeItem = feeItems.SingleOrDefault(x => x.Id == invoiceItem.FeeItemId);
                                order.OrderDetails.Add(new USparkOrderDetail()
                                {
                                    Amount = invoiceItem.Amount,
                                    ItemCode = feeItem.Code,
                                    ItemNameEn = feeItem.NameEn,
                                    ItemNameTh = feeItem.NameTh,
                                });
                            }

                            orders.Add(order);
                        }

                        if (dropInvoice != null)
                        {
                            var order = new USparkOrder();
                            // order.Reference1 = dropInvoice.Reference1;
                            // order.Reference2 = dropInvoice.Reference2;
                            order.KSTermID = ksTermId;
                            order.StudentCode = studentCode;
                            order.TotalAmount = -dropInvoice.InvoiceItems.Sum(x => x.Amount);
                            // order.Number = dropInvoice.Number;
                            // order.ReferenceNumber = dropInvoice.RunningNumber;
                            order.OrderDetails = new List<USparkOrderDetail>();
                            // order.PaymentStartAt = dropInvoice.CreatedAt;
                            // order.PaymentEndAt = dropInvoice.PaymentExpireAt;
                            // order.IsPaid = dropInvoice.IsPaid;
                            order.KSInvoiceID = dropInvoice.Id;

                            var feeItems = _db.FeeItems.Where(x => dropInvoice.InvoiceItems.Select(y => y.FeeItemId).Contains(x.Id)).ToList();
                            foreach (var invoiceItem in dropInvoice.InvoiceItems)
                            {
                                var feeItem = feeItems.SingleOrDefault(x => x.Id == invoiceItem.FeeItemId);
                                order.OrderDetails.Add(new USparkOrderDetail()
                                {
                                    Amount = -invoiceItem.Amount,
                                    ItemCode = feeItem.Code,
                                    ItemNameEn = feeItem.NameEn,
                                    ItemNameTh = feeItem.NameTh,
                                });
                            }

                            orders.Add(order);
                        }

                        result.Result = orders;
                        return StatusCode(200, result);
                    }
                    else
                    {
                        result.Message = "Same";
                        return StatusCode(400, result);
                    }
                }
            }
        }
    }
}
