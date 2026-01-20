internal static class SortUtil {
    
    public static int NaturalCompare(string? x, string? y) {
        
        switch (x) {
            
            case null when y == null: return 0;
            case null: return -1;
        }

        if (y == null) return 1;

        // Fallback to standard comparison for cross-platform compatibility
        return string.Compare(x, y, StringComparison.OrdinalIgnoreCase);
    }
}
