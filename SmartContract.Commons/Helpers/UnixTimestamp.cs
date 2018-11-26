using System;

namespace SmartContract.Commons.Helpers
{
    public class UnixTimestamp
    {
        public static long ToUnixTimestamp(DateTime date)
        {
            return ((DateTimeOffset)date).ToUnixTimeSeconds();
        }

        public static DateTime FromUnixTimestamp(long unixTimestamp)
        {
            return DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).DateTime;
        }

        public static long ConvertToMiliseconds(long unixTimestamp)
        {
            return DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).ToUnixTimeMilliseconds();
        }


        public static string Iso8061DateFromUnixTimestamp(long unixTimestamp)
        {
            return DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).DateTime.ToString("yyyy-MM-ddTHH:mm:ss");
        }

        public static long GetCurrentEpoch()
        {
            return ToUnixTimestamp(DateTime.UtcNow);
        }

        public static long GetChartDataBeginTime(long from, long interval)
        {
            return (long)Math.Floor((double)from / interval) * interval;
        }
    }
}