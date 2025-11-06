using ImGuiNET;
using Raylib_cs;
using rlImGui_cs;

namespace scythe;

#pragma warning disable CS8981
internal class runtime : raylib_session {

    private cam cam;
    
    protected override unsafe void draw() {
        
        resize_window(new(screen.width / 2, screen.height / 2));
        center_window();
        
        // generic initials
        var core = new core();
        cam = new();
        //var freecam = new freecam(cam);
        
        while (!Raylib.WindowShouldClose()) {
            
            Raylib.SetTargetFPS(screen.refresh_rate);

            Raylib.BeginDrawing();
            
            clear(colors.game);
            
            // start camera
            //freecam.loop(level_3d);
        
            cam.start_rendering();
            
            Raylib.BeginMode3D(cam.rl_cam);
            
            core.loop_3d(false);
            
            Raylib.EndMode3D();
            
            core.loop_ui(false);
            
            cam.stop_rendering();
            
            // Raylib.DrawText($"{Raylib.GetFPS()}", 10, 10, 20, colors.primary.to_raylib());
            
            // stop raylib
            Raylib.EndDrawing();
        }
        
        core.quit();
        
        Raylib.CloseWindow();
    }

}