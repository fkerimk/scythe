using System.Runtime.InteropServices;
using ImGuiNET;
using Raylib_cs;
using rlImGui_cs;

namespace scythe;

#pragma warning disable CS8981
internal class editor : raylib_session {

    public static cam? cam;
    
    protected override unsafe void draw() {
        
        resize_window(new(screen.width / 2, screen.height / 2));
        center_window();
        
        // imgui setup
        rlImGui.Setup(true, true);
        
        var imgui = ImGui.GetIO();
        imgui.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
        //imgui.NativePtr->IniFilename = (byte*)IntPtr.Zero.ToPointer();
        imgui.NativePtr->IniFilename = (byte*)Marshal.StringToHGlobalAnsi("layout/user.ini").ToPointer();
        
        // generic initials
        var core = new core();
        cam = new();
        var freecam = new freecam(cam);
        fonts.load(imgui);
        var level_3d = new level_3d();
        var level_browser = new level_browser(core.level);
        var object_browser = new object_browser();
        
        while (!Raylib.WindowShouldClose()) {
            
            Raylib.SetTargetFPS(screen.refresh_rate);

            // reload viewport render
            if (level_3d.tex_size != level_3d.tex_temp) {
                
                Raylib.UnloadRenderTexture(level_3d.rt);
                level_3d.rt = Raylib.LoadRenderTexture((int)level_3d.tex_size.X, (int)level_3d.tex_size.Y);
                level_3d.tex_temp = level_3d.tex_size;
            }
            
            // render core on viewport
            Raylib.BeginTextureMode(level_3d.rt);
            
            clear(colors.game);

            // start camera
            freecam.loop(level_3d);
        
            cam.start_rendering();
            
            // run 3d
            core.loop_3d(true);
            
            // draw grid
            var grid = new grid(cam);
            grid.draw();
            
            // run ui
            core.loop_ui(true);
            
            // run editor
            core.loop_editor(level_3d);
            
            // stop camera
            cam.stop_rendering();
            
            Raylib.DrawText($"{Raylib.GetFPS()}", 10, 10, 20, colors.primary.to_raylib());

            Raylib.EndTextureMode();
            
            // draw raylib
            Raylib.BeginDrawing();
            clear(new(0, 0, 0));
            
            // draw imgui
            rlImGui.Begin();
            style.push();
            ImGui.PushFont(fonts.montserrat_regular);
            ImGui.DockSpaceOverViewport(ImGui.GetMainViewport().ID);
            imgui.MouseDoubleClickTime = 0.2f; // Saniye cinsinden (varsayılan 0.3)
            //imgui.MouseDoubleClickMaxDist = 6.0f; // Pixel cinsinden
            
            // draw elements
            level_3d.draw();
            level_browser.draw();
            object_browser.obj = level_browser.selected_object;
            object_browser.draw();
            
            // stop imgui
            ImGui.PopFont();
            style.pop();
            rlImGui.End();
            
            // stop raylib
            Raylib.EndDrawing();
        }
        
        core.quit();
        
        Raylib.CloseWindow();
    }
}