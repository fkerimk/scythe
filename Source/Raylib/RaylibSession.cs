using Raylib_cs;

internal abstract class RaylibSession(int initWidth, int initHeight, ConfigFlags[] flags, bool? isEditor = null) {
    
    protected static int Width => Raylib.GetScreenWidth();
    protected static int Height => Raylib.GetScreenHeight();
    protected static int TargetFps = -1;
    
    internal void Show() {
    
        if (isEditor == null) Raylib.SetTraceLogLevel(TraceLogLevel.None);
        else if (Enum.TryParse(isEditor.Value ? Config.Editor.RaylibLogLevel : Config.Runtime.RaylibLogLevel, out TraceLogLevel state))
            Raylib.SetTraceLogLevel(state);
        
        Raylib.ClearWindowState(ConfigFlags.VSyncHint);
        Raylib.ClearWindowState(ConfigFlags.FullscreenMode);
        Raylib.ClearWindowState(ConfigFlags.ResizableWindow);
        Raylib.ClearWindowState(ConfigFlags.UndecoratedWindow);
        Raylib.ClearWindowState(ConfigFlags.HiddenWindow);
        Raylib.ClearWindowState(ConfigFlags.MinimizedWindow);
        Raylib.ClearWindowState(ConfigFlags.MaximizedWindow);
        Raylib.ClearWindowState(ConfigFlags.UnfocusedWindow);
        Raylib.ClearWindowState(ConfigFlags.TopmostWindow);
        Raylib.ClearWindowState(ConfigFlags.AlwaysRunWindow);
        Raylib.ClearWindowState(ConfigFlags.TransparentWindow);
        Raylib.ClearWindowState(ConfigFlags.HighDpiWindow);
        Raylib.ClearWindowState(ConfigFlags.MousePassthroughWindow);
        Raylib.ClearWindowState(ConfigFlags.BorderlessWindowMode);
        Raylib.ClearWindowState(ConfigFlags.Msaa4xHint);
        Raylib.ClearWindowState(ConfigFlags.InterlacedHint);

        foreach (var flag in flags)
            Raylib.SetConfigFlags(flag);
    
        Raylib.InitWindow(initWidth, initHeight, Config.Mod.Name);

        if (PathUtil.BestPath("Images/Icon.png", out var iconPath)) {

            var img = Raylib.LoadImage(iconPath);
            Raylib.SetWindowIcon(img);
            Raylib.UnloadImage(img);
        }
        
        Raylib.SetExitKey(KeyboardKey.Null);

        if (!Init()) goto Quit;
        
        while (!Raylib.IsWindowReady()) Task.Delay(0);

        var continueLoop = true;
        
        while (!Raylib.WindowShouldClose() && continueLoop) {
            
            continueLoop = Loop();
            
            if (TargetFps == -1) TargetFps = Screen.RefreshRate;
            
            Raylib.SetTargetFPS(TargetFps);
            
            Raylib.EndDrawing();
        }
        
        Quit:
        Quit();
        Raylib.CloseWindow();
    }

    protected abstract bool Init();
    protected abstract bool Loop();
    protected abstract void Quit();

    protected static void PrepareWindow(float scale = 0.5f, bool maximize = false, bool fullscreen = false, bool borderless = true) {
        
        Raylib.SetWindowSize((int)Math.Floor(Screen.Width * scale), (int)Math.Floor(Screen.Height * scale)); CenterWindow();
        
        if (maximize) Raylib.MaximizeWindow();

        if (!fullscreen) return;
       
        if (borderless) {
                
            Raylib.SetConfigFlags(ConfigFlags.UndecoratedWindow);
            Raylib.SetWindowSize(Raylib.GetMonitorWidth(Raylib.GetCurrentMonitor()), Raylib.GetMonitorHeight(Raylib.GetCurrentMonitor()));
        }
            
        else if (!Raylib.IsWindowFullscreen()) Raylib.ToggleFullscreen();
    }

    protected static void CenterWindow() => Raylib.SetWindowPosition((Screen.Width - Width) / 2, (Screen.Height - Height) / 2);
    protected static void Clear(Color color) => Raylib.ClearBackground(color.ToRaylib());
}
