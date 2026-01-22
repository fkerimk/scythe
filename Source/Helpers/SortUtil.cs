internal static class SortUtil {
    
    public static int NaturalCompare(string? x, string? y) {
        
        return x switch {
            
            null when y == null => 0,
            null => -1,
            
            _ => y == null ? 1 : string.Compare(x, y, StringComparison.OrdinalIgnoreCase)
        };
    }
}
