using System.Globalization;

namespace Wass.Code.Recipes
{
    internal static class StepExtensions
    {
        public static bool IsEqualTo(this string value, string target) => string.Compare(value, target, true, CultureInfo.InvariantCulture) == 0;
    }
}
