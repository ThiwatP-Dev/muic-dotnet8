using AutoMapper;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace KeystoneLibrary.Providers
{
    public class RegistrationCalculationProvider : BaseProvider, IRegistrationCalculationProvider
    {
        protected readonly IStudentProvider _studentProvider;
        protected readonly IRegistrationProvider _registrationProvider;
        protected readonly IReceiptProvider _receiptProvider;
        protected readonly IFeeProvider _feeProvider;

        public RegistrationCalculationProvider(ApplicationDbContext db,
                                                   IMapper mapper,
                                                   IStudentProvider studentProvider,
                                                   IConfiguration config,
                                                   IRegistrationProvider registrationProvider,
                                                   IReceiptProvider receiptProvider,
                                                   IFeeProvider feeProvider) : base(config, db, mapper)
                        {
            _studentProvider = studentProvider;
            _registrationProvider = registrationProvider;
            _receiptProvider = receiptProvider;
            _feeProvider = feeProvider;
        }

        public async Task<BatchRegistrationConfirmJobDetail> ConfirmInvoice(string studentCode, long academicLevelId, long termId, string userId, bool isFromRegistration, bool isSyncWithUSpark)
        {
            BatchRegistrationConfirmJobDetail result = new BatchRegistrationConfirmJobDetail()
            {
                StudentCode = studentCode,
                AcademicLevelId = academicLevelId,
                TermId = termId,
                StartSyncWithUSparkDateTimeUtc = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId,
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = userId,
                IsActive = true 
            };
            try
            {
                result.Result = "Start Process";
                //var student = _studentProvider.GetStudentInformationByCode(studentCode);
                var student = _db.Students.AsNoTracking()
                                          .IgnoreQueryFilters()
                                          .SingleOrDefault(x => x.Code == studentCode
                                                                   && x.StudentStatus != "a");

                var currentSections = _db.RegistrationCourses.AsNoTracking()
                                                             .Where(x => x.TermId == termId
                                                                         && x.StudentId == student.Id
                                                                         && x.Status != "d")
                                                             .Select(x => x.SectionId)
                                                             .OrderBy(x => x)
                                                             .ToList();
                if (isSyncWithUSpark)
                {
                    try
                    {
                        result.Result = "Sync with USpark";
                        await _registrationProvider.GetRegistrationCourseFromUspark(student.Id, termId, userId);
                        result.SyncWithUSparkRemark = "Success";
                        result.Result = "Sync with USpark Success";
                    }
                    catch (Exception withUSparkException)
                    {
                        result.SyncWithUSparkRemark = withUSparkException.Message;

                        throw new Exception("No change made. Problem retrieve data...");
                    }
                    finally
                    {
                        result.FinishSyncWithUSparkDateTimeUtc = DateTime.UtcNow;
                    }
                }
                else
                {
                    result.Result = "Skip Sync with USpark";
                    result.SyncWithUSparkRemark = "Skip";
                    result.FinishSyncWithUSparkDateTimeUtc = DateTime.UtcNow;
                }

                if (isFromRegistration)
                {
                    result.Result = "Check displayed sections with sections after sync";
                    var updatedSections = _db.RegistrationCourses.AsNoTracking()
                                                            .Where(x => x.TermId == termId
                                                                        && x.StudentId == student.Id
                                                                        && x.Status != "d")
                                                            .Select(x => x.SectionId)
                                                            .OrderBy(x => x)
                                                            .ToList();

                    if (!currentSections.SequenceEqual(updatedSections))
                    {
                        throw new Exception("Student section has been updated, please refresh page and check again");
                    }
                }
                result.Result = "CalculateTuitionFee";
                var courseTuition = _feeProvider.CalculateTuitionFeeV3(student.Code, termId)
                                                .ToList();
                
                result.Result = "CalculatePenaltyFee";
                var penaltyFee = _feeProvider.CalculatePenaltyFee(student.Code, termId, courseTuition)
                                             .ToList();

                result.Result = "Combine all fee";
                var invoiceItems = courseTuition.Concat(penaltyFee).ToList();

                if (invoiceItems.Any())
                {
                    result.Result = "Checkout Invoice";
                    var invoice = await _receiptProvider.CheckoutUSparkInvoiceV3Async(student.Code, termId, invoiceItems);
                    result.Result = "Confirm on behalf of student successful. Invoice is created";
                }
                else
                {
                    result.Result = "No change made.";
                }

                result.IsSuccess = true;
            }
            catch (Exception e)
            {
                result.IsSuccess = false;
                result.Result += " - Error:" + e.Message;
            }
            finally
            {
                result.FinishProcessDateTimeUtc = DateTime.UtcNow;
            }
            return result;
        }
    }
}
