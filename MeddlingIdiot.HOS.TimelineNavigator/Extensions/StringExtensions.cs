using System.Text.RegularExpressions;

namespace MeddlingIdiot.HOS.TimelineNavigator.Extensions
{
    public static class StringExtensions
    {
        public static DateTimeOffset? ToDateTimeOffsetOrNull(this string value)
        {
            if (DateTimeOffset.TryParse(value, out var result))
            {
                return result;
            }

            return null;
        }

        public static DateTime? ToDateTimeOrNull(this string value)
        {
            if (DateTime.TryParse(value, out var result))
            {
                return result;
            }

            return null;
        }
        public static double? ToDoubleOrNull(this string value)
        {
            if (double.TryParse(value, out var result))
            {
                return result;
            }

            return null;
        }

        public static int? ToIntOrNull(this string value)
        {
            if (int.TryParse(value, out var result))
            {
                return result;
            }

            return null;
        }

        public static int ToIntOrDefault(this string value, int defaultValue)
        {
            if (int.TryParse(value, out var result))
            {
                return result;
            }

            return defaultValue;
        }

        public static string? StripLineEndChars(this string? s)
        {
            if (s == null)
            {
                return null;
            }
            string replacement = Regex.Replace(s, @"\t|\n|\r", "");
            return replacement;
        }
    }
}
