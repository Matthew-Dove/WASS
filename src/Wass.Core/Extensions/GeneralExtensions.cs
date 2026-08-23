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

        public static bool In<T>(this T source, ReadOnlySpan<T> values) where T : IEquatable<T> => values.Contains(source);

        public static bool In<T>(this T source, params T[] values) where T : IEquatable<T>
        {
            var comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < values.Length; i++)
            {
                if (comparer.Equals(source, values[i]))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool In<T>(this T source, IEnumerable<T> values) where T : IEquatable<T>
        {
            var comparer = EqualityComparer<T>.Default;
            foreach (var value in values)
            {
                if (comparer.Equals(source, value))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
