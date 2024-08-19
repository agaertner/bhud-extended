using System.Globalization;
using System.Text.RegularExpressions;

namespace Blish_HUD.Extended
{
    internal static class StringExtensions
    {
        public static string ToTitleCase(this string title)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(title.ToLower());
        }

        public static string SplitCamelCase(this string input)
        {
            return Regex.Replace(input, "([A-Z])", " $1", RegexOptions.Compiled).Trim();
        }

        public static string ReplaceWhitespace(this string input, string replacement)
        {
            return Regex.Replace(input, @"\s+", replacement);
        }

        public static string GetTextBetweenTags(this string input, string tagName)
        {
            var match = Regex.Match(input, $"<{tagName}>(.*?)</{tagName}>");
            return match.Success && match.Groups.Count > 1 ? match.Groups[1].Value : string.Empty;
        }
    }
}
