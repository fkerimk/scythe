using Raylib_cs;

internal static class Screen {

    internal static int Monitor => Raylib.GetCurrentMonitor();
    internal static int Width => Raylib.GetMonitorWidth(Monitor);
    internal static int Height => Raylib.GetMonitorHeight(Monitor);
    internal static int RefreshRate => Raylib.GetMonitorRefreshRate(Monitor);
}