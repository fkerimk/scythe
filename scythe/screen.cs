using Raylib_cs;

namespace scythe;

#pragma warning disable CS8981 
public static class screen {

    internal static int monitor => Raylib.GetCurrentMonitor();
    internal static int width => Raylib.GetMonitorWidth(monitor);
    internal static int height => Raylib.GetMonitorHeight(monitor);
    internal static int refresh_rate => Raylib.GetMonitorRefreshRate(monitor);
}