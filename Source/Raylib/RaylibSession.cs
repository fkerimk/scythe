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
        
        Raylib.SetExitKey(KeyboardKey.Null);

        Init();
        
        while (!Raylib.IsWindowReady()) Task.Delay(0);

        bool continueLoop = true;
        
        while (!Raylib.WindowShouldClose() && continueLoop) {
            
            continueLoop = Loop();

            if (TargetFps == -1) TargetFps = Screen.RefreshRate;
            
            Raylib.SetTargetFPS(TargetFps);
            
            Raylib.EndDrawing();
        }
        
        Quit();

        Raylib.CloseWindow();
    }

    protected abstract void Init();
    protected abstract bool Loop();
    protected abstract void Quit();

    protected void ResizeWindow(int2 size) {
        
        Raylib.SetWindowSize(size.x, size.y);
    }

    protected void CenterWindow() {
        
        Raylib.SetWindowPosition((Screen.Width - Width) / 2, (Screen.Height - Height) / 2);
    }

    protected void Clear(Color color) {
        
        Raylib.ClearBackground(color.ToRaylib());
    }
}
