using Raylib_cs;

namespace scythe;

internal abstract class raylib_session {
    
    protected static int width => Raylib.GetScreenWidth();
    protected static int height => Raylib.GetScreenHeight();

    internal virtual unsafe void show(int init_width, int init_height, TraceLogLevel log_level, params ConfigFlags[] flags) {
        
        Raylib.SetTraceLogLevel(log_level);
        
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
        
        Raylib.InitWindow(init_width, init_height, "SCYTHE");
        
        Raylib.SetExitKey(KeyboardKey.Null);

        draw();
    }

    protected abstract unsafe void draw();

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
