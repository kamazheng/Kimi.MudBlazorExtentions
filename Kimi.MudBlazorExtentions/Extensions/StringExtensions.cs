using System.Globalization;

internal static class StringExtensions
{
    public static string SplitAndCapitalize(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var result = System.Text.RegularExpressions.Regex.Replace(input, "([a-z])([A-Z])", "$1 $2");
        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(result);
    }
}