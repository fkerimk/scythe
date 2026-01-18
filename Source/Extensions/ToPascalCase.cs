using System.Globalization;
using System.Text.RegularExpressions;

internal static partial class Extensions {

    public static string ToPascalCase(this string input) {
        
        if (string.IsNullOrWhiteSpace(input)) return input;

        var result = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input.ToLower());
        return ToPascalCaseRegex().Replace(result, "");
    }

    [GeneratedRegex(@"\s+")] private static partial Regex ToPascalCaseRegex();
}