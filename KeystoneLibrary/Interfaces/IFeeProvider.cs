using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.Fee;
using KeystoneLibrary.Models.DataModels.Scholarship;
using KeystoneLibrary.Models.Report;
using KeystoneLibrary.Enumeration;
using KeystoneLibrary.Models.USpark;
using KeystoneLibrary.Data;

namespace KeystoneLibrary.Interfaces
{
    public interface IFeeProvider
    {
        List<TermFee> GetTermFees();
        List<TermFee> GetStudentTermFees(Guid studentId);
        List<TermFee> GetStudentTermFees(Guid studentId, long termId);
        List<InvoiceItem> GetTuitionFees(List<RegistrationCourse> registeringCourses,
                                         long facultyId,
                                         long? departmentId,
                                         long curriculumId,
                                         long? curriculumVersionId,
                                         int batch,
                                         long studentFeeTypeId,
                                         long termId);
        List<ScholarshipFeeItem> GetScholarshipFeeItems(Guid studentId, long termId);
        TermFeeModalViewModel GetTermFeeViewModel(long termFeeId);
        List<TermFeeSimulateItemViewModel> GetStudentTermFees(long studentFeeGroupId, int batch, long academicLevelId, long termId, int numberOfTerms, long termTypeId);
        List<StudentFeeGroup> GetStudentFeeGroups(long academicLevelId, long facultyId, long departmentId, long curriculumId,
                                                  long curriculumVersionId, long nationalityId, int batch, long studentGroupId,
                                                  long studentFeeTypeId);
        FeeItem GetFeeItem(long feeItemId);
        bool SaveFinanceOtherFeeInvoiceAndReceipt(FinanceOtherFeeFormModel model);
        string GenerateReceiptAfterPayingInUSpark(long invoiceId);
        RegistrationFeeInvoice GetRegistrationFeeInvoice(long invoiceId);

        // Tuition Fee 
        TuitionFee GetTuitionFee(long academicLevelId, long courseId, long tuitionFeeId);
        CourseTuitionFeeViewModel GetCourseTuitionFee(long academicLevelId, long courseId);
        TuitionFeeType GetTuitionFeeType(long? id);
        TuitionFeeRate GetTuitionFeeRate(long? id);
        TuitionFeeFormula GetTuitionFeeFormula(long? id);
        bool IsTuitionFeeRateOverlapBatch(TuitionFeeRate model);
        void UpdateUSparkPayment(long USOrderId, string receiptNumber, string receiptUrl);
        decimal CalculateTuitionFeeRate(TuitionFee tuitionFee, Course course,long studentFeeTypeId, int batch);
        decimal CalculateTuitionFeeRate(TuitionFee tuitionFee, Course course, long studentFeeTypeId, int batch, ApplicationDbContext db);

        // MUIC Report
        FeeReportViewModel GetFeeReportPivotByStudent(Criteria criteria);
        FeeReportViewModel GetFeeReportPivotByStudentFacultyDepartment(Criteria criteria);
        FeeReportViewModel GetFeeReportPivotReceiptOnlyByStudentFacultyDepartment(Criteria criteria);
        FeeReportViewModel GetFeeReportPivotByDepartment(Criteria criteria);

        // MUIC Running number
        string GetInvoiceReceiptNumber(long termId, int runningNumber);
        string GetFeeReceiptNumber(int runningNumber, ReceiptNumberType type);
        string GetFeeInvoiceNumber(int runningNumber);
        int GetNextInvoiceRunningNumber();
        int GetNextReceiptRunningNumber();
        string GenerateInvoiceReference1(string invoiceNumber, string reference2, decimal totalAmount);
        string GenerateInvoiceReference2(DateTime createAt, DateTime expiredAt);

        /// <summary>
        /// Calculate tuition fee for USpark
        /// </summary>
        /// <param name="studentCode"></param>
        /// <param name="ksTermId"></param>
        /// <returns></returns>
        USparkCalculateFeeResponse GetUsparkTuitionFeeResponse(string studentCode, long ksTermId);

        /// <summary>
        /// Get list of current course change
        /// </summary>
        /// <param name="studentCode"></param>
        /// <param name="ksTermId"></param>
        /// <returns></returns>
        UsparkInvoiceAddDropCourses GetUsparkCourseChanges(string studentCode, long ksTermId);

        /// <summary>
        /// Calculate invoice item
        /// </summary>
        /// <param name="studentCode"></param>
        /// <param name="ksTermId"></param>
        /// <param name="KSSectionIds"></param>
        /// <returns></returns>
        IEnumerable<InvoiceItem> CalculateTuitionFeeV3(string studentCode, long ksTermId);

        /// <summary>
        /// Calculate penalty item
        /// </summary>
        /// <param name="studentCode"></param>
        /// <param name="ksTermId"></param>
        /// <param name="courseInvoiceItems"></param>
        /// <returns></returns>
        IEnumerable<InvoiceItem> CalculatePenaltyFee(string studentCode, long ksTermId, IEnumerable<InvoiceItem> courseInvoiceItems);

        void UpdateStudentStateByInvoice(Invoice invoice);
    }
}