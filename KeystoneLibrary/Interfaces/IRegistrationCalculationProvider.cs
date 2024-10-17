using KeystoneLibrary.Models.DataModels;

namespace KeystoneLibrary.Interfaces
{
    public interface IRegistrationCalculationProvider
    {
        Task<BatchRegistrationConfirmJobDetail> ConfirmInvoice(string strudentCode, long academicLevelId, long termId, string userId, bool isFromRegistration, bool isSyncWithUSpark);
    }
}
