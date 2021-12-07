using System.Globalization;

namespace Wass.Code.Recipes
{
    internal static class StepExtensions
    {
        public static bool IsEqualTo(this string value, string target) => string.Compare(value, target, true, CultureInfo.InvariantCulture) == 0;

        public static bool IsEqualTo(this string value, string targetA, string targetB)
        {
            return
                string.Compare(value, targetA, true, CultureInfo.InvariantCulture) == 0 ||
                string.Compare(value, targetB, true, CultureInfo.InvariantCulture) == 0;
        }

        public static bool IsEqualTo(this string value, string targetA, string targetB, string targetC)
        {
            return
                string.Compare(value, targetA, true, CultureInfo.InvariantCulture) == 0 ||
                string.Compare(value, targetB, true, CultureInfo.InvariantCulture) == 0 ||
                string.Compare(value, targetC, true, CultureInfo.InvariantCulture) == 0;
        }

        public static bool IsEqualTo(this string value, string targetA, string targetB, string targetC, string targetD)
        {
            return
                string.Compare(value, targetA, true, CultureInfo.InvariantCulture) == 0 ||
                string.Compare(value, targetB, true, CultureInfo.InvariantCulture) == 0 ||
                string.Compare(value, targetC, true, CultureInfo.InvariantCulture) == 0 ||
                string.Compare(value, targetD, true, CultureInfo.InvariantCulture) == 0;
        }

        public static bool IsEqualTo(this string value, params string[] targets)
        {
            for (int i = 0; i < targets.Length; i++)
            {
                if (string.Compare(value, targets[i], true, CultureInfo.InvariantCulture) == 0) return true;
            }
            return false;
        }

        public static string Guard(this string value)
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentException("Argument cannot be null, or empty.");
            return value;
        }

        public static string Guard(this string value, string name)
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentException("Argument cannot be null, or empty.", name);
            return value;
        }

        public static T Guard<T>(this T obj) where T : class
        {
            if (obj == null) throw new ArgumentNullException();
            return obj;
        }

        public static T Guard<T>(this T obj, string name) where T : class
        {
            if (obj == null) throw new ArgumentNullException(name);
            return obj;
        }
    }
}
