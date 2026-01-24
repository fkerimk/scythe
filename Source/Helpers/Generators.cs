using System.Text.RegularExpressions;

// ReSharper disable PossibleMultipleEnumeration
internal static class Generators {

    public static string AvailableName(string input, IEnumerable<string?> names) {
        
        var output = input = Regex.Replace(input, @"\s\d+$", "");
    
        var i = 0;

        while (names.Contains(output)) {

            i++;
            output = $"{input} {i}";
        }

        return output;
    }

    public static string SplitCamelCase(string input) {
        
        if (string.IsNullOrEmpty(input)) return input;
        
        // Convert snake_case to Space Case first
        input = input.Replace("_", " ");
        
        // Split CamelCase
        var output = Regex.Replace(input, @"([A-Z])", " $1").Trim();
        
        // Capitalize first letter if needed
        if (char.IsLower(output[0])) output = char.ToUpper(output[0]) + output.Substring(1);

        return output;
    }
}