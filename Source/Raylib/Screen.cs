using static Raylib_cs.Raylib;

internal static class Screen {
    
    private static int Monitor => GetCurrentMonitor();
    internal static int Width => GetMonitorWidth(Monitor);
    internal static int Height => GetMonitorHeight(Monitor);
    internal static int RefreshRate => GetMonitorRefreshRate(Monitor);
}