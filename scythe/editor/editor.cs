using System.Runtime.InteropServices;
using ImGuiNET;
using Raylib_cs;
using rlImGui_cs;

namespace scythe;

#pragma warning disable CS8981
internal static class editor {

    internal static unsafe void show() {
        
        // ----------------------------------------------------------------
        // raylib setup
        // ----------------------------------------------------------------
        Raylib.SetTraceLogLevel(TraceLogLevel.Warning);

        Raylib.SetConfigFlags(ConfigFlags.Msaa4xHint);
        Raylib.SetConfigFlags(ConfigFlags.AlwaysRunWindow);
        Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);
        
        Raylib.InitWindow(1, 1, "SCYTHE");
        
        Raylib.ClearWindowState(ConfigFlags.UndecoratedWindow);
        
        var width = screen.width / 2;
        var height = screen.height / 2;
        
        Raylib.SetWindowSize(width, height);
        Raylib.SetWindowPosition((screen.width - width) / 2, (screen.height - height) / 2);
        
        Raylib.SetTargetFPS(screen.refresh_rate);
        
        Raylib.SetExitKey(KeyboardKey.Null);
        
        // ----------------------------------------------------------------
        // imgui setup
        // ----------------------------------------------------------------
        rlImGui.Setup(true, true);
        
        var imgui = ImGui.GetIO();
        imgui.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
        //imgui.NativePtr->IniFilename = (byte*)IntPtr.Zero.ToPointer();
        imgui.NativePtr->IniFilename = (byte*)Marshal.StringToHGlobalAnsi("layout/user.ini").ToPointer();
        
        // ----------------------------------------------------------------
        // generic initials
        // ----------------------------------------------------------------
        core.init();
        freecam.init();
        
        fonts.load(imgui);
        viewport.init();

        while (!Raylib.WindowShouldClose()) {

            // reload viewport render
            if (viewport.tex_size != viewport.tex_temp) {
                
                Raylib.UnloadRenderTexture(viewport.rt);
                viewport.rt = Raylib.LoadRenderTexture((int)viewport.tex_size.X, (int)viewport.tex_size.Y);
                viewport.tex_temp = viewport.tex_size;
            }
            
            // render core on viewport
            Raylib.BeginTextureMode(viewport.rt);
            
            Raylib.ClearBackground(colors.game);

            freecam.update(viewport.pos.to_int2() + viewport.size.to_int2() / 2);
        
            Raylib.BeginMode3D(cam.current);
            
            core.loop_3d();
            
            grid.draw();
            
            Raylib.EndMode3D();
            
            core.loop_ui();
            
            Raylib.DrawFPS(10, 10);

            Raylib.EndTextureMode();
            
            // draw raylib
            Raylib.BeginDrawing();
            Raylib.ClearBackground(colors.back);
            
            // draw imgui
            rlImGui.Begin();
            style.push();
            ImGui.PushFont(fonts.montserrat_regular);
            ImGui.DockSpaceOverViewport(ImGui.GetMainViewport().ID);
            
            // draw viewport
            viewport.draw();
            
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