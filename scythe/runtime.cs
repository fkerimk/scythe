using Raylib_cs;

namespace scythe;

#pragma warning disable CS8981
internal class runtime() : raylib_session(1, 1, TraceLogLevel.Warning, ConfigFlags.Msaa4xHint, ConfigFlags.AlwaysRunWindow, ConfigFlags.ResizableWindow) {

    protected override void init() {
        
        resize_window(new(screen.width / 2, screen.height / 2));
        center_window();
        
        core.init(false);
    }

    protected override void loop() {
        
        target_fps = config.runtime.fps_lock;
        
        if (cam.main == null) return;

        Raylib.BeginDrawing();
            
        clear(colors.game);
            
        cam.main.start_rendering();
            
        core.loop_3d(false);
            
        cam.main.stop_rendering();
            
        core.loop_ui(false);

        if (config.runtime.draw_fps) Raylib.DrawText($"{Raylib.GetFPS()}", 10, 10, 20, colors.primary.to_raylib());
    }

    protected override void quit() {
        
        core.quit();
    }
}