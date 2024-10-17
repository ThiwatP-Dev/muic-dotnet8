namespace KeystoneLibrary.Models.DataModels.Logs
{
    public class DataSyncLog
    {
        public long Id { get; set; }
        public string? SyncName { get; set; }
        public string? SyncResult { get; set; }
        public string? Remark { get; set; }
        public DateTime? SyncFinishTimeUtc { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedDateTimeUtc { get; set; } = DateTime.UtcNow;
    }
}
