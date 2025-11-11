using Raylib_cs;

namespace scythe;

internal abstract class raylib_session(int init_width, int init_height, TraceLogLevel trace, params ConfigFlags[] flags) {
    
    protected static int width => Raylib.GetScreenWidth();
    protected static int height => Raylib.GetScreenHeight();
    protected static int target_fps = -1;
    protected static bool close_window = false;

    internal void show() {
        
        Raylib.SetTraceLogLevel(trace);
        
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
        
        Raylib.InitWindow(init_width, init_height, config.mod.name);
        
        Raylib.SetExitKey(KeyboardKey.Null);

        init();
        
        while (!Raylib.IsWindowReady()) Task.Delay(0);
        
        while (!Raylib.WindowShouldClose()) {
            
            loop();

            if (target_fps == -1) target_fps = screen.refresh_rate;
            
            Raylib.SetTargetFPS(target_fps);
            
            Raylib.EndDrawing();

            if (!close_window) continue;
            close_window = false;
            break;
        }
        
        quit();

        Raylib.CloseWindow();
    }

    protected abstract void init();
    protected abstract void loop();
    protected abstract void quit();

    protected void resize_window(int2 size) {
        
        Raylib.SetWindowSize(size.x, size.y);
    }

    protected void center_window() {
        
        Raylib.SetWindowPosition((screen.width - width) / 2, (screen.height - height) / 2);
    }

    protected void clear(color color) {
        
        Raylib.ClearBackground(color.to_raylib());
    }
}
