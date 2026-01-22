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
}