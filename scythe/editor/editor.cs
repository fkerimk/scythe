using System.Numerics;
using System.Runtime.InteropServices;
using ImGuiNET;
using Raylib_cs;
using rlImGui_cs;

namespace scythe;

#pragma warning disable CS8981
internal class editor : raylib_session {
    
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
        var cam = new cam();
        var freecam = new freecam(cam);
        
        fonts.load(imgui);
        
        var level_3d = new level_3d();
        var level_browser = new level_browser(core.level);
        
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
            core.loop_3d();
            
            // draw grid
            var grid = new grid(cam);
            grid.draw();
            
            // stop camera
            cam.stop_rendering();
            
            // run ui
            core.loop_ui();
            
            Raylib.DrawFPS(10, 10);

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
            
            // draw viewport
            level_3d.draw();
            
            // draw level browser
            level_browser.draw();
            
            // stop imgui
            ImGui.PopFont();
            style.pop();
            rlImGui.End();
            
            // stop raylib
            Raylib.EndDrawing();
        }
        
        Raylib.CloseWindow();
    }
}