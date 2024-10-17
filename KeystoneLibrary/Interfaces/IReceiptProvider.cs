using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.Configurations;
using KeystoneLibrary.Models.Report;
using KeystoneLibrary.Models.USpark;

namespace KeystoneLibrary.Interfaces
{
    public interface IReceiptProvider
    {
        Receipt GetReceiptById(long id);
        List<Receipt> GetReceiptByTerm(Guid studentId, long termId);
        List<Invoice> GetInvoiceByTerm(Guid studentId, long termId);
        Invoice GetInvoice(long id);
        List<InvoiceItem> GetInvoiceItemsForPreview(List<RegistrationCourse> registeringCourses, int registrationRound, bool isCreditNote = false);
        List<InvoiceItem> GetInvoiceItemsForPreview(List<RegistrationCourse> courses, List<MatchCourseTermFee> feeItems);
        long RegenerateInvoices(Guid studentId, long termId, List<RegistrationCourse> dropCourses, List<RegistrationCourse> addCourses, string round);
        Invoice AddInvoice(Guid studentId, long termId, List<RegistrationCourse> registrationCourses, string round, int addDropRound = 0);
        Invoice SimulateAddInvoice(Guid studentId, long termId, List<RegistrationCourse> registrationCourses, string round);
        Invoice SimulateAddInvoiceCreditNote(Guid studentId, long termId, List<RegistrationCourse> registrationCourses);
        void SimulateUpdateTuitionFeeRefund(Invoice dropInvoice, Invoice addInvoice);
        void SimulateGenerateLateRegistrationFeeItem(Invoice invoice, Guid studentId, long termId);
        void SimulateGenerateAddDropFeeItem(Invoice invoice, long termId, int addDropRound);
        void SimulateApplyCreditNote(Invoice invoice, Invoice creditNoteInvoice);
        void SimulateGenerateLatePaymentFeeItem(Invoice invoice, long termId, string type, DateTime today);
        List<RefundDetail> GetInvoiceCourseFeeItem(Guid studentId, long termId, long courseId, long registrationId);
        List<ReceiptItem> GetReceiptItemsByCourseId(long registrationId);
        List<Receipt> GetReceiptNumberByStudentCodeAndTerm(string code, int year, int term);
        List<ReceiptItem> GetTermFeeItemsByStudentAndTermId(Guid studentId, long termId);
        List<RegistrationCourse> GetReceiptsCourses(Guid studentId, long termId);
        string GetAmountByReceiptId(long receiptItemId);
        Receipt GetReceiptCertificateById (long id, string language = "en");
        string GetTotalAmountTh(string money);
        string GetTotalAmountEn(string money);
        FinanceOtherFeeViewModel FinanceOtherFeePreview (long id);
        List<string> GetReceiptCreatedBy();
        ReceiptPDFViewModel GetReceiptPDF(long receiptId, ApplicationUser user);
        Task<bool> GenerateBarCode(Invoice invoice);
        Invoice DropCourses(Guid studentId, long termId, List<RegistrationCourse> registrationCourses, int addDropRound);
        Invoice AddCourses(Guid studentId, long termId, List<RegistrationCourse> registrationCourses, int addDropRound);

        void ApplyCreditNote(Invoice invoice, Invoice creditNoteInvoice);
        void GenerateAddDropFeeItem(Invoice invoice, long termId, int addDropRound);
        InvoiceItem GenerateAddDropFeeItem(long termId, int addDropRound);
        void GenerateLatePaymentFeeItem(Invoice invoice, long termId, string type, DateTime today);
        void GenerateLateRegistrationFeeItem(Invoice invoice, Guid studentId, long termId);
        void UpdateInvoiceReference(Invoice invoice, string studentCode);
        void Checkout(Guid studentId, long termId, List<RegistrationCourse> newCourses, List<RegistrationCourse> deletePaidCourses);
        bool IsLumpsumPayment(Guid studentId);

        // Uspark Api
        Task ReplaceRegistrationCourseFromUspark(Guid studentId, long termId);
        List<Invoice> GetInvoicesByStudentCodeAndTermId(string studentCode, long termId);
        void CancelAddDropInvoices(Guid studentId, long termId);
        void CancelInvoicesWithUpdateRegistrationCourse(List<Guid> studentIdList, long termId);
        AddDropFeeConfiguration GetConfigAddDropFeeCount(long termId);
        LatePaymentConfiguration GetConfigLatePayment(long termId);
        void UpdateTuitionFeeRefund(Invoice dropInvoice, Invoice addInvoice);
        void CalculateRefundReduction(List<InvoiceItem> dropInvoiceItems, List<InvoiceItem> addInvoiceItems);
        InvoiceItem CalculateRefundReduction(List<InvoiceItem> invoiceItems);
        void RegenerateQRCode();


        /// <summary>
        /// Add new invoice for given student
        /// </summary>
        /// <param name="studentCode"></param>
        /// <param name="KSTermId"></param>
        /// <param name="invoiceItems"></param>
        /// <returns></returns>
        Task<Invoice> AddInvoiceV3Async(string studentCode, long KSTermId, IEnumerable<InvoiceItem> invoiceItems);

        void DeleteInvoice(long invoiceId, bool isActive = false, string remark = "");

        /// <summary>
        /// Check out invoice for USpark
        /// </summary>
        /// <param name="studentCode"></param>
        /// <param name="KSTermId"></param>
        /// <param name="invoiceItems"></param>
        /// <returns></returns>
        Task<USparkOrder> CheckoutUSparkInvoiceV3Async(string studentCode, long KSTermId, IEnumerable<InvoiceItem> invoiceItems);

        /// <summary>
        /// Mark expired invoice
        /// </summary>
        /// <param name="studentCode"></param>
        /// <param name="KSTermId"></param>
        bool MarkCancelExpireInvoice(string studentCode, long KSTermId);

        /// <summary>
        /// Get list of invoice for student
        /// </summary>
        /// <param name="studentCode"></param>
        /// <param name="KSTermId"></param>
        /// <returns></returns>
        IEnumerable<USparkOrder> GetStudentNonCancelInvoices(string studentCode);

        /// <summary>
        /// Get student single invoice
        /// </summary>
        /// <param name="studentCode"></param>
        /// <param name="invoiceId"></param>
        /// <returns></returns>
        USparkOrder GetNonCancelStudentInvoiceById(string studentCode, long invoiceId);

        /// <summary>
        /// Get student invoice course details
        /// </summary>
        /// <param name="studentCode"></param>
        /// <param name="invoiceId"></param>
        /// <returns></returns>
        UsparkInvoiceAddDropCourses GetNonCancelStudentInvoiceCoursesDetails(string studentCode, long invoiceId);

        /// <summary>
        /// Update payment status
        /// </summary>
        /// <param name="request"></param>
        Task ProcessInvoicePaymentAsync(InvoiceUpdateIsPaidViewModel request);

        Task<string> ProcessInvoicePaymentManualAsync(FinanceRegistrationFeeViewModel model);

        Task SyncRegistrationCoursePaidStatusAsync(string studentCode, long KSTermId);

        #region Report

        WaitingPaymentWithAddressReportViewModel GetWaitingPaymentWithAddressReport(Criteria criteria);
        RegistrationResultWithAmountAndCreditReportViewModel GetRegistrationResultWithAmountAndCreditReport(Criteria criteria);

        #endregion
    }
}