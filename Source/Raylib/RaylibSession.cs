using Raylib_cs;

internal abstract class RaylibSession(int initWidth, int initHeight, params ConfigFlags[] flags) {
    
    protected static int Width => Raylib.GetScreenWidth();
    protected static int Height => Raylib.GetScreenHeight();
    protected static int TargetFps = -1;
    protected static bool CloseWindow;

    internal void Show() {
        
        if (Enum.TryParse(Config.Raylib.TraceLogLevel, out TraceLogLevel state))
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
        
        Raylib.SetExitKey(KeyboardKey.Null);

        Init();
        
        while (!Raylib.IsWindowReady()) Task.Delay(0);
        
        while (!Raylib.WindowShouldClose()) {
            
            Loop();

            if (TargetFps == -1) TargetFps = Screen.RefreshRate;
            
            Raylib.SetTargetFPS(TargetFps);
            
            Raylib.EndDrawing();

            if (!CloseWindow) continue;
            CloseWindow = false;
            break;
        }
        
        Quit();

        Raylib.CloseWindow();
    }

    protected abstract void Init();
    protected abstract void Loop();
    protected abstract void Quit();

    protected void resize_window(int2 size) {
        
        Raylib.SetWindowSize(size.x, size.y);
    }

    protected void center_window() {
        
        Raylib.SetWindowPosition((Screen.Width - Width) / 2, (Screen.Height - Height) / 2);
    }

    protected void clear(Color color) {
        
        Raylib.ClearBackground(color.to_raylib());
    }
}
