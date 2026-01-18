using Raylib_cs;

internal static class Window {
    
    public static int Width => Raylib.GetScreenWidth();
    public static int Height => Raylib.GetScreenHeight();
    
    public static void UpdateFps() {
        
        var targetFps = CommandLine.Editor ? 
            Config.Editor.FpsLock :
            Config.Runtime.FpsLock;
        
        if (targetFps == -1)
            targetFps = Screen.RefreshRate;
        
        Raylib.SetTargetFPS(targetFps);
    }

    public static void Clear(ScytheColor scytheColor) => Raylib.ClearBackground(scytheColor.ToRaylib());

    private static void CenterWindow() => Raylib.SetWindowPosition((Screen.Width - Width) / 2, (Screen.Height - Height) / 2);
    
    public static void Show(
        
        int width = 0,
        int height = 0,
        float scale = 0.5f,
        bool maximize = false,
        bool fullscreen = false,
        bool borderless = true,
        string title = "SCYTHE",
        bool isSplash = false,
        params ConfigFlags[] flags
    ) {
        
        // Close old window
        if (Raylib.IsWindowReady()) Raylib.CloseWindow();

        // Flags
        Flags.Set(flags);
        
        // Logging
        if (isSplash) Raylib.SetTraceLogLevel(TraceLogLevel.None);
        
        else if (Enum.TryParse(CommandLine.Editor ? Config.Editor.RaylibLogLevel : Config.Runtime.RaylibLogLevel, out TraceLogLevel state))
            Raylib.SetTraceLogLevel(state);
        
        // New window     
        Raylib.InitWindow(0, 0, title);
        
        if (width == 0) width = Screen.Width;
        if (height == 0) height = Screen.Height;
        width = (int)Math.Floor(width * scale);
        height = (int)Math.Floor(height * scale);
        
        Raylib.SetWindowSize(width, height);
        
        Raylib.SetExitKey(KeyboardKey.Null);

        CenterWindow();
        
        // Fullscreen & maximizing 
        if (fullscreen) {
            
            if (borderless) {
                
                Flags.Add(ConfigFlags.UndecoratedWindow);
                Raylib.SetWindowSize(Raylib.GetMonitorWidth(Raylib.GetCurrentMonitor()), Raylib.GetMonitorHeight(Raylib.GetCurrentMonitor()));
                CenterWindow();
            }
            
            else if (!Raylib.IsWindowFullscreen()) Raylib.ToggleFullscreen();
            
        } else if (maximize) Raylib.MaximizeWindow();
        
        // Window icon
        if (PathUtil.BestPath("Images/Icon.png", out var iconPath)) {

            var img = Raylib.LoadImage(iconPath);
            Raylib.SetWindowIcon(img);
            Raylib.UnloadImage(img);
        }
        
        //while (!Raylib.IsWindowReady()) Task.Delay(0);
    }
}
