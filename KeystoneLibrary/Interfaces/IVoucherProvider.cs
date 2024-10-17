using KeystoneLibrary.Models.DataModels.Scholarship;

namespace KeystoneLibrary.Interfaces
{
    public interface IVoucherProvider
    {
        Voucher GetVoucherById(long id);
        bool UpdateVoucherModel(Voucher voucher);
        List<VoucherLog> GetVoucherLogs(long id);
        List<Voucher> GetVoucherByStudentId(Guid studentId);
    }
}