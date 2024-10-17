using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using System.Globalization;

namespace KeystoneLibrary.Providers
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime? ConvertStringToDateTime(string dateTime)
        {
            DateTime  convertedDate; 
            var canConvert = DateTime.TryParseExact(dateTime, StringFormat.ShortDate,
                                                    CultureInfo.InvariantCulture, DateTimeStyles.None, out convertedDate);
            
            return canConvert ? convertedDate : null;
        }

        public TimeSpan? ConvertStringToTime(string time)
        {
            TimeSpan  convertedTime; 
            var canConvert = TimeSpan.TryParseExact(time, StringFormat.TimeSpan, 
                                                    CultureInfo.InvariantCulture, TimeSpanStyles.None, out convertedTime);
            return canConvert ? convertedTime : null;
        }

        public DateTime? ConvertFromUtcToSE(DateTime? date)//SE Asia Standard Time
        {
            if(date == null)
            {
                return date;
            }
            
            return date?.AddHours(7);
        }
    }
}