namespace KeystoneLibrary.Interfaces
{
    public interface IDateTimeProvider
    {
        DateTime? ConvertStringToDateTime(string dateTime);
        TimeSpan? ConvertStringToTime(string time);
        DateTime? ConvertFromUtcToSE(DateTime? date);
    }
}