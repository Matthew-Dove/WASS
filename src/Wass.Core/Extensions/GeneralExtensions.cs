using System.Globalization;

namespace Wass.Core
{
    public static class GeneralExtensions
    {
        /// <summary>Short iso version ("yyyy-MM-ddTHH:mm:ssZ"): e.g. "2024-05-03T14:36:56Z".</summary>
        public static string ToIso8601(this DateTime utc) => utc.ToString("s", CultureInfo.InvariantCulture) + "Z";

        /// <summary>Convert a datetime in UTC timezone, to Sydney timezone.</summary>
        public static DateTime ToSydneyTime(this DateTime utc) => TimeZoneInfo.ConvertTimeFromUtc(utc, _sydney);
        private static readonly TimeZoneInfo _sydney = TimeZoneInfo.FindSystemTimeZoneById("Australia/Sydney");
    }
}
