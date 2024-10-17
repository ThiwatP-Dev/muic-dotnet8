using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;

namespace KeystoneLibrary.Interfaces
{
    public interface IPrintingLogProvider
    {
        List<PrintingLog> GetPrintingLogs(Criteria criteria);
    }
}