using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;

namespace KeystoneLibrary.Providers
{
    public class PrintingLogProvider : IPrintingLogProvider
    {
        protected readonly ApplicationDbContext _db;
        protected readonly IDateTimeProvider _dateTimeProvider;

        public PrintingLogProvider(ApplicationDbContext db,
                                   IDateTimeProvider dateTimeProvider) 
        {
            _db = db;
            _dateTimeProvider = dateTimeProvider;
        }

        public List<PrintingLog> GetPrintingLogs(Criteria criteria)
        {
            DateTime? startedAt = _dateTimeProvider.ConvertStringToDateTime(criteria.StartedAt);
            DateTime? endedAt = _dateTimeProvider.ConvertStringToDateTime(criteria.EndedAt);

            var printingLogs = _db.PrintingLogs.Where(x => ((string.IsNullOrEmpty(criteria.Code))
                                                            || criteria.Code == x.ReferenceNumber)
                                                           && (criteria.Gender == null
                                                               || x.Gender == criteria.Gender)
                                                           && (startedAt == null
                                                               ||  x.PrintedAt >= startedAt)
                                                           && (endedAt == null
                                                               || x.PrintedAt <= endedAt)
                                                           && (string.IsNullOrEmpty(criteria.Language)
                                                               || x.Language == criteria.Language)
                                                           && (string.IsNullOrEmpty(criteria.UrgentStatus)
                                                               || x.IsUrgent.ToString() == criteria.UrgentStatus)
                                                           && (string.IsNullOrEmpty(criteria.PrintStatus)
                                                               || (criteria.PrintStatus == "r" && x.PrintedAt == null)
                                                               || (criteria.PrintStatus == "p" && x.PrintedAt != null))
                                                           && (string.IsNullOrEmpty(criteria.PaidStatus)
                                                               || x.IsPaid.ToString() == criteria.PaidStatus))
                                               .ToList();
            return printingLogs;
        }
    }
}