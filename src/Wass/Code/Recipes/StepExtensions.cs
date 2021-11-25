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
    }
}
