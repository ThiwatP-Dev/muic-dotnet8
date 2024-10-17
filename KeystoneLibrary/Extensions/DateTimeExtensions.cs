namespace KeystoneLibrary.Extensions
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Return new datetime instance with time set to 23:59:59
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static DateTime EndOfDay(this DateTime input, int hourDiff = 0)
        {
            return new DateTime(input.Year, input.Month, input.Day, 23, 59, 59).AddHours(hourDiff);
        }
    }
}