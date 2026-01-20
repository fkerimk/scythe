using System.Runtime.InteropServices;

internal static class SortUtil {
    
    [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern int StrCmpLogicalW(string x, string y);

    public static int NaturalCompare(string? x, string? y) {
        
        switch (x) {
            
            case null when y == null:return 0;
            case null: return -1;
        }

        if (y == null) return 1;

        return OperatingSystem.IsWindows() ? StrCmpLogicalW(x, y) :
            // Simple fallback for Linux
            string.Compare(x, y, StringComparison.OrdinalIgnoreCase);
    }
}
